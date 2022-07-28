using HSNCustomizations.DAC;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.CM;
using System.Collections;
using HSNCustomizations.DAC_Extension;
using System.IO;
using PX.Data.BQL.Fluent;
using PX.Objects.CA;
using PX.Data.BQL;
using PX.Objects.CR;
using PX.Data.Licensing;
using Address = PX.Objects.CR.Address;

namespace HSNCustomizations.Graph
{
    public class ProcessSCBPaymentRefundMaint : PXGraph<ProcessSCBPaymentRefundMaint>
    {
        public PXFilter<LumProcessSCBPaymentRefundFilter> Filter;
        public PXCancel<LumProcessSCBPaymentRefundFilter> Cancel;

        [PXFilterable]
        public PXFilteredProcessing<ARPayment, LumProcessSCBPaymentRefundFilter,
            Where<ARPayment.status, Equal<ARDocStatus.closed>,
              And<ARPayment.docType, Equal<ARPaymentType.refund>,
              And<ARPayment.adjDate, Equal<Current<LumProcessSCBPaymentRefundFilter.adjDate>>,
              And<ARPayment.cashAccountID, Equal<Current<LumProcessSCBPaymentRefundFilter.payAccountID>>,
              And<ARPayment.paymentMethodID, Equal<Current<LumProcessSCBPaymentRefundFilter.payTypeID>>>>>>>,
            OrderBy<Desc<ARPayment.refNbr>>> ARPaymentList;

        public SelectFrom<Customer>.Where<Customer.bAccountID.IsEqual<Customer.bAccountID.AsOptional>>.View CustomerView;

        public ProcessSCBPaymentRefundMaint()
        {
            ARPaymentList.SetProcessVisible(false);
            ARPaymentList.SetProcessDelegate(list => DownlodARPayments(list));
        }

        public IEnumerable arpaymentList()
        {
            foreach (PXResult<ARPayment> doc in PXSelect<ARPayment,
                     Where<ARPayment.status, Equal<ARDocStatus.closed>,
                       And<ARPayment.docType, Equal<ARPaymentType.refund>,
                       And<ARPayment.adjDate, Equal<Current<LumProcessSCBPaymentRefundFilter.adjDate>>,
                       And<ARPayment.cashAccountID, Equal<Current<LumProcessSCBPaymentRefundFilter.payAccountID>>,
                       And<ARPayment.paymentMethodID, Equal<Current<LumProcessSCBPaymentRefundFilter.payTypeID>>>>>>>,
                     OrderBy<Desc<ARPayment.refNbr>>>.Select(this))
            {
                ARPayment row = (ARPayment)doc;
                row.GetExtension<ARPaymentExt>().UsrBankAccnamattributes = (string)((this.ARPaymentList.Cache.GetValueExt(row, PX.Objects.CS.Messages.Attribute + "BANKACCNAM") as PXFieldState)?.Value);
                row.GetExtension<ARPaymentExt>().UsrBankSwiftAttributes = (string)((this.ARPaymentList.Cache.GetValueExt(row, PX.Objects.CS.Messages.Attribute + "BANKSWIFT") as PXFieldState)?.Value);
                row.GetExtension<ARPaymentExt>().UsrBankAccNbrttributes = (string)((this.ARPaymentList.Cache.GetValueExt(row, PX.Objects.CS.Messages.Attribute + "BANKACCNBR") as PXFieldState)?.Value);
                if (!(row.GetExtension<ARPaymentExt>().UsrSCBPaymentRefundExported ?? false))
                    yield return row;
            }
        }

		#region Method

		public void DownlodARPayments(IEnumerable<ARPayment> arPaymentLists)
		{
			try
			{
				using (MemoryStream stream = new MemoryStream())
				{
					using (StreamWriter sw = new StreamWriter(stream, Encoding.ASCII))
					{
						string fileName = DateTime.Now.ToString("yyyyMMddHHmm") + "-Customer Refund.txt";

						foreach (ARPayment arLine in arPaymentLists)
						{
                            var currentBranch = SelectFrom<Branch>.Where<Branch.branchID.IsEqual<@P.AsInt>>.View.Select(this, arLine.BranchID).TopFirst;
                            var ExtRefNbr = SelectFrom<CashAccount>.Where<CashAccount.cashAccountID.IsEqual<@P.AsInt>>.View.Select(this, arLine.CashAccountID).TopFirst?.ExtRefNbr;
                            var CompanyInfo = SelectFrom<BAccount2>.Where<BAccount2.bAccountID.IsEqual<@P.AsInt>>.View.Select(this, currentBranch?.BAccountID).TopFirst;
                            var CompanyAddress = SelectFrom<Address>.
                                                    Where<Address.bAccountID.IsEqual<@P.AsInt>>.
                                                    View.Select(this, CompanyInfo.BAccountID).TopFirst;
                            var customerInfo = CustomerView.Select(arLine.CustomerID).TopFirst;
                            var customerAddress = Address.PK.Find(this, customerInfo?.DefAddressID);
                            string[] content = new string[114];
                            // 1.PLG
                            content[0] = "PLG";
                            // 3.If CashAccount.ExtRefNbr<> blank then CashAccount.ExtRefNbr Else '312143582508'
                            content[2] = string.IsNullOrEmpty(ExtRefNbr) ? "312143582508" : ExtRefNbr;
                            // 4.ARPayment.curyID
                            content[3] = arLine.CuryID;
                            // 5.ARPayment.CuryOrigDocAmt
                            content[5] = arLine.CuryOrigDocAmt?.ToString();
                            // 7.ARPayment.adjDate
                            content[6] = arLine.AdjDate?.ToString("yyyy/MM/dd");
                            // 8.ARPayment.RefNbr
                            content[8] = arLine.RefNbr;
                            // 14.Left(Companies.AccontName, 30)
                            content[13] = CompanyInfo?.AcctName?.Length >= 30 ? CompanyInfo?.AcctName?.Substring(0,30) : CompanyInfo?.AcctName;
                            // 15.Left(CompaniesAddress.Addressline1,30)
                            content[14] = CompanyAddress?.AddressLine1?.Length >= 30 ? CompanyAddress?.AddressLine1?.Substring(0,30) : CompanyAddress?.AddressLine1;
                            // 16.Left(CompaniesAddress.Addressline2 + ‘, ’+ Postalcode,30)
                            content[15] = (CompanyAddress?.AddressLine2 + "," + CompanyAddress?.PostalCode)?.Length >= 30 ? (CompanyAddress?.AddressLine2 + "," + CompanyAddress?.PostalCode)?.Substring(0,30) : CompanyAddress?.AddressLine2 + "," + CompanyAddress?.PostalCode;
                            // 17.Left(CompaniesAddress.City+’, ‘+State,30)
                            content[16] = (CompanyAddress?.City + "," + CompanyAddress?.State)?.Length >= 30 ? (CompanyAddress?.City + "," + CompanyAddress?.State)?.Substring(0,30) : CompanyAddress?.City + "," + CompanyAddress?.State;
                            // 20.Left(ARPayment.atttribute BANKACCNAM, 30)
                            var BANKACCNAM = arLine.GetExtension<ARPaymentExt>()?.UsrBankAccnamattributes;
                            content[19] = BANKACCNAM?.Length >= 30 ? BANKACCNAM?.Substring(0,30) : BANKACCNAM;
                            // 21.Left(ARPayment.atttribute DESCRIPTN,30)
                            var DESCRIPTN = (string)((this.ARPaymentList.Cache.GetValueExt(arLine, PX.Objects.CS.Messages.Attribute + "DESCRIPTN") as PXFieldState)?.Value);
                            content[20] = DESCRIPTN?.Length >= 30 ? DESCRIPTN?.Substring(0,30) : DESCRIPTN;
                            // 22.Left(Customer.atttribute BANKACTML,30)
                            var BANKACTML = (string)((this.CustomerView.Cache.GetValueExt(customerInfo, PX.Objects.CS.Messages.Attribute + "BANKACTML") as PXFieldState)?.Value);
                            content[21] = BANKACTML?.Length >= 30 ? BANKACTML?.Substring(0,30) : BANKACTML;
                            // 23.Left(ARPayment.Customer.Address.State,30)
                            content[22] = customerAddress?.State?.Length >= 30 ? customerAddress?.State?.Substring(0,30) : customerAddress?.State;
                            // 25.ARPayment.atttribute BANKACCNBR
                            content[24] = arLine.GetExtension<ARPaymentExt>().UsrBankAccNbrttributes;
                            // 32.ARPayment.atttribute BANKSWIFT
                            content[31] = arLine.GetExtension<ARPaymentExt>().UsrBankSwiftAttributes;

                            sw.WriteLine(string.Join("|",content));
                            //Update UsrSCBPaymentExported and UsrSCBPaymentDateTime
                            arLine.GetExtension<ARPaymentExt>().UsrSCBPaymentRefundExported = true;
                            arLine.GetExtension<ARPaymentExt>().UsrSCBPaymentRefundDateTime = DateTime.Now;

							this.Caches[typeof(ARPayment)].Update(arLine);
						}

						this.Actions.PressSave();
						sw.Close();

						// Redirect browser to file created in memory on server
						throw new PXRedirectToFileException(new PX.SM.FileInfo(Guid.NewGuid(), fileName, null, stream.ToArray(), string.Empty), true);
					}
				}
			}
			catch (PXException ex)
			{
				PXProcessing<APPayment>.SetError(ex);
                this.ARPaymentList.View.RequestRefresh();
				throw;
			}
		}

		#endregion
	}
}
