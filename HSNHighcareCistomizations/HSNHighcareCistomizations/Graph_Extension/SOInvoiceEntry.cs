using HSNHighcareCistomizations.Descriptor;
using PX.Data;
using PX.Objects.AR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.SO
{
    public class SOInvoiceEntryHighcareExt : PXGraphExtension<SOInvoiceEntry>
    {
        public const string Check = "Check";
        public const string Redeem = "Redeem";
        public const string CouponValidError = "Coupon is Invalid, please check";
        public const string CouponUpdateError = "Update Coupon failed, please check";

        #region Override Method/Action
        public delegate IEnumerable ReleaseDelegate(PXAdapter adapter);
        [PXOverride]
        public virtual IEnumerable Release(PXAdapter adapter, ReleaseDelegate baseMethod)
        {
            try
            {
                var result = baseMethod(adapter);
                // [Highcare] - Call Highcare API to redeem the discount code after release invoice
                ProcessDiscountCode(SOInvoiceEntryHighcareExt.Redeem);
                Base.Save.Press();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Event

        public virtual void _(Events.RowPersisting<ARInvoiceDiscountDetail> e)
        {
            // [Highcare] - Call Highcare API to check the available of the discount code
            ProcessDiscountCode(SOInvoiceEntryHighcareExt.Check);
        }

        #endregion

        #region Method

        /// <summary> Check or Update Highcare Discount Code </summary>
        public virtual void ProcessDiscountCode(string processType)
        {
            // 找Cache 與 Delete 的差集資料
            var cuurentDiscountLst = Base.ARDiscountDetails.View.Cache.Cached.RowCast<ARInvoiceDiscountDetail>().Except(Base.ARDiscountDetails.View.Cache.Deleted.RowCast<ARInvoiceDiscountDetail>());
            if (cuurentDiscountLst.Count() > 0)
            {
                var docNote = string.Empty;
                var errorMsg = string.Empty;
                var isValid = true;
                HighcareHelper helper = new HighcareHelper();
                helper.GetAccessToken();
                var customerInfo = Customer.PK.Find(Base, Base.Document.Current?.CustomerID);
                foreach (var discountLine in cuurentDiscountLst)
                {
                    var discountInfo = DiscountSequence.PK.Find(Base, discountLine?.DiscountID, discountLine?.DiscountSequenceID);
                    // Discount Code contains "Highcare" then do validation
                    if (discountInfo?.Description.ToUpper().Contains("HIGHCARE") ?? false)
                    {
                        switch (processType)
                        {
                            // Valid Discount Code
                            case SOInvoiceEntryHighcareExt.Check:
                                if (!helper.ValidHihgcareDiscountCoupon(customerInfo?.AcctCD, discountLine?.ExtDiscCode))
                                {
                                    isValid = false;
                                    errorMsg = CouponValidError;
                                    Base.ARDiscountDetails.Cache.RaiseExceptionHandling<ARInvoiceDiscountDetail.extDiscCode>(discountLine, discountLine?.ExtDiscCode,
                                        new PXSetPropertyException("Coupon is Invalid", PXErrorLevel.Error));
                                }
                                break;
                            // Redeem Discount Code
                            case SOInvoiceEntryHighcareExt.Redeem:
                                var redeemResult = helper.RedeemHighcareDiscountCoupon(discountLine?.ExtDiscCode);
                                docNote += $"Code:{discountLine?.ExtDiscCode} Result: {(redeemResult ? "Success" : "Failed")}";
                                if (!redeemResult)
                                {
                                    isValid = false;
                                    errorMsg = CouponValidError;
                                }
                                break;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(docNote))
                    PXNoteAttribute.SetNote(Base.Document.Cache, Base.Document.Current, docNote);
                if (!isValid)
                    throw new PXException(errorMsg);
            }
        }

        #endregion
    }
}
