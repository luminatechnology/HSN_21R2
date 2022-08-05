using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.GL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.CM;

namespace HSNCustomizations.DAC
{
	[Serializable]
	[PXCacheName("LumProcessSCBPaymentRefundFilter")]
	public class LumProcessSCBPaymentRefundFilter : IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[PXDefault(typeof(AccessInfo.branchID))]
		[Branch(Visible = true, Enabled = true)]
		public virtual Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion

		#region PayTypeID
		public abstract class payTypeID : PX.Data.BQL.BqlString.Field<payTypeID> { }
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("TT")]
		//[PXDefault(typeof(Search<Location.paymentMethodID,
		//					Where<Location.bAccountID, Equal<Current<APPayment.vendorID>>, And<Location.locationID, Equal<Current<APPayment.vendorLocationID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Payment Method", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID,
						  Where<PaymentMethod.useForAP, Equal<True>,
							And<PaymentMethod.isActive, Equal<True>>>>))]
		[PXForeignReference(typeof(Field<payTypeID>.IsRelatedTo<PaymentMethod.paymentMethodID>))]
		public virtual string PayTypeID { get; set; }
		#endregion

		#region PayAccountID
		public abstract class payAccountID : PX.Data.BQL.BqlInt.Field<payAccountID> { }
		protected Int32? _PayAccountID;
		//[PXDefault(typeof(Coalesce<Search2<Location.cashAccountID,
		//					InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<Location.cashAccountID>,
		//						And<PaymentMethodAccount.paymentMethodID, Equal<Location.vPaymentMethodID>,
		//							And<PaymentMethodAccount.useForAP, Equal<True>>>>>,
		//					Where<Location.bAccountID, Equal<Current<APPayment.vendorID>>,
		//						And<Location.locationID, Equal<Current<APPayment.vendorLocationID>>,
		//							And<Location.vPaymentMethodID, Equal<Current<APPayment.paymentMethodID>>>>>>,
		//	Search2<PaymentMethodAccount.cashAccountID, InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<PaymentMethodAccount.cashAccountID>>>,
		//		Where<PaymentMethodAccount.paymentMethodID, Equal<Current<APPayment.paymentMethodID>>,
		//			And<CashAccount.branchID, Equal<Current<APPayment.branchID>>,
		//				And<PaymentMethodAccount.useForAP, Equal<True>, And<PaymentMethodAccount.aPIsDefault, Equal<True>>>>>>>),
		//	PersistingCheck = PXPersistingCheck.Nothing)]
		[CashAccount(typeof(ARPayment.branchID), typeof(Search2<CashAccount.cashAccountID,
							InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>>>,
							Where2<Match<Current<AccessInfo.userName>>,
								And<PaymentMethodAccount.paymentMethodID, Equal<Current<payTypeID>>,
								And<PaymentMethodAccount.useForAP, Equal<True>,
									And<Where<CashAccount.clearingAccount, Equal<False>, Or<Current<ARPayment.docType>, In3<ARDocType.refund, ARDocType.voidRefund>>>>>>>>),
			Visibility = PXUIVisibility.Visible, SuppressCurrencyValidation = true)]
		[PXDefault("SCB")]
		public virtual Int32? PayAccountID
		{
			get
			{
				return this._PayAccountID;
			}
			set
			{
				this._PayAccountID = value;
			}
		}
		#endregion


		#region Application Date
		public abstract class adjDate : PX.Data.BQL.BqlDateTime.Field<adjDate> { }
		protected DateTime? _AdjDate;
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Application Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? AdjDate
		{
			get
			{
				return this._AdjDate;
			}
			set
			{
				this._AdjDate = value;
			}
		}
		#endregion
	}
}
