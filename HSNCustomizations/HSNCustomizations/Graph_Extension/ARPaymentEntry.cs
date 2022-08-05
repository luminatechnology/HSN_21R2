using System.Collections;
using System.Collections.Generic;
using HSNCustomizations.DAC;
using HSNCustomizations.DAC_Extension;
using PX.Data;
using PX.Data.BQL.Fluent;

namespace PX.Objects.AR
{
    public class ARPaymentEntry_Extension : PXGraphExtension<ARPaymentEntry>
    {
        public override void Initialize()
        {
            base.Initialize();
            Base.report.AddMenuAction(PrepaymentInvoice);
        }

        #region Action
        public PXAction<ARPayment> PrepaymentInvoice;
        [PXButton]
        [PXUIField(DisplayName = "Print Prepayment Invoice", Enabled = true, MapEnableRights = PXCacheRights.Select)]
        protected virtual IEnumerable prepaymentInvoice(PXAdapter adapter)
        {
            if (Base.Document.Current != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["DocType"] = Base.Document.Current.DocType;
                parameters["RefNbr"] = Base.Document.Current.RefNbr;
                throw new PXReportRequiredException(parameters, "LM602000", "Report LM602000");
            }
            return adapter.Get();
        }
        #endregion

        #region Cache Attached 
        [PXRemoveBaseAttribute(typeof(ARPaymentType.NumberingAttribute))]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [HSNCustomizations.Descriptor.ARPymtNumbering()]
        protected void _(Events.CacheAttached<ARPayment.refNbr> e) { }
        #endregion

        #region Events

        public virtual void _(Events.RowSelected<ARPayment> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            LUMHSNSetup hSNSetup = SelectFrom<LUMHSNSetup>.View.Select(Base);
            // [Phase II - SCB Refund]
            PXUIFieldAttribute.SetVisible<ARPaymentExt.usrSCBPaymentRefundExported>(e.Cache, null, hSNSetup?.EnableSCBPaymentFile ?? false);
            PXUIFieldAttribute.SetVisible<ARPaymentExt.usrSCBPaymentRefundDateTime>(e.Cache, null, hSNSetup?.EnableSCBPaymentFile ?? false);
        }

        #endregion
    }
}