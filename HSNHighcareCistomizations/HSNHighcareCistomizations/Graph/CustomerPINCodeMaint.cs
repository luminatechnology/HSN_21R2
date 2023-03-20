using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Objects.CR;
using System.Threading.Tasks;
using HSNHighcareCistomizations.DAC;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Data.EP;
using PX.Objects.DR;
using PX.Data.BQL;
using PX.Objects.IN;
using System.Collections;
using PX.Objects.SO;
using PX.Objects.FS;
using HSNCustomizations.DAC;

namespace HSNHighcareCistomizations.Graph
{
    public class CustomerPINCodeMaint : PXGraph<CustomerPINCodeMaint>
    {
        public PXSave<CustomerPINCodeFilter> Save;
        public PXCancel<CustomerPINCodeFilter> Cancel;

        public PXFilter<CustomerPINCodeFilter> Filter;

        public SelectFrom<LUMCustomerPINCode>
               .Where<LUMCustomerPINCode.bAccountID.IsEqual<CustomerPINCodeFilter.bAccountID.FromCurrent>>.View Transaction;

        public SelectFrom<LUMHSNSetup>.View HSNSetup;

        public PXAction<LUMCustomerPINCode> viewDefSchedule;
        [PXButton]
        [PXUIField(Visible = false)]
        public virtual IEnumerable ViewDefSchedule(PXAdapter adapter)
        {
            var row = this.Transaction.Current;
            var graph = PXGraph.CreateInstance<DraftScheduleMaint>();
            graph.Schedule.Current = SelectFrom<DRSchedule>
                                     .Where<DRSchedule.scheduleNbr.IsEqual<P.AsString>>
                                     .View.Select(this, row.ScheduleNbr);
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
            return adapter.Get();
        }

        public PXAction<LUMCustomerPINCode> viewSalesOrder;
        [PXButton]
        [PXUIField(Visible = false)]
        public virtual IEnumerable ViewSalesOrder(PXAdapter adapter)
        {
            var row = this.Transaction.Current;
            var graph = PXGraph.CreateInstance<SOOrderEntry>();
            graph.Document.Current = SelectFrom<SOOrder>
                                     .Where<SOOrder.orderType.IsEqual<P.AsString>
                                       .And<SOOrder.orderNbr.IsEqual<P.AsString>>>
                                     .View.Select(this, "SO", row.SOOrderNbr);
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
            return adapter.Get();
        }

        public PXAction<LUMCustomerPINCode> viewInvoice;
        [PXButton]
        [PXUIField(Visible = false)]
        public virtual IEnumerable ViewInvoice(PXAdapter adapter)
        {
            var row = this.Transaction.Current;
            var graph = PXGraph.CreateInstance<SOInvoiceEntry>();
            graph.Document.Current = SelectFrom<ARInvoice>
                                     .Where<ARInvoice.docType.IsEqual<P.AsString>
                                       .And<ARInvoice.refNbr.IsEqual<P.AsString>>>
                                     .View.Select(this, "INV", row.InvoiceNbr);
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
            return adapter.Get();
        }

        public PXAction<LUMCustomerPINCode> viewSrvOrder;
        [PXButton]
        [PXUIField(Visible = false)]
        public virtual IEnumerable ViewSrvOrder(PXAdapter adapter)
        {
            var row = this.Transaction.Current;
            var graph = PXGraph.CreateInstance<ServiceOrderEntry>();
            graph.ServiceOrderRecords.Current = SelectFrom<FSServiceOrder>
                                               .Where<FSServiceOrder.refNbr.IsEqual<P.AsString>>
                                               .View.Select(this, row.ServiceOrderNbr);
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
            return adapter.Get();
        }

        public virtual void _(Events.RowSelected<LUMCustomerPINCode> e)
        {
            var setup = HSNSetup.Select();
            if (e.Row != null)
            {
                this.Transaction.Cache.SetValueExt<LUMCustomerPINCode.serialNbr>(e.Row, LUMPINCodeMapping.PK.Find(this, e.Row.Pin)?.SerialNbr);
                PXUIFieldAttribute.SetEnabled<LUMCustomerPINCode.startDate>(e.Cache, null, setup.TopFirst?.GetExtension<LUMHSNSetupExtension>()?.EnableOverridePINCodetDate ?? false);
            }
        }

        public virtual void _(Events.RowPersisting<LUMCustomerPINCode> e)
        {
            if (e.Row is LUMCustomerPINCode row && row != null && this.Filter.Current != null && e.Operation == PXDBOperation.Insert)
            {
                row.BAccountID = this.Filter.Current.BAccountID;
                if (!row.StartDate.HasValue || !row.EndDate.HasValue)
                {
                    row.StartDate = DateTime.Now;
                    row.EndDate = DateTime.Now.AddYears(1).AddDays(-1);
                }
                // 讓API能夠override active屬性
                if (!(row?.IsActive ?? false))
                    row.IsActive = Accessinfo.BusinessDate?.Date >= row.StartDate?.Date && Accessinfo.BusinessDate?.Date <= row.EndDate?.Date;
            }
        }
    }

    public class HighcareAttr : PX.Data.BQL.BqlString.Constant<HighcareAttr>
    {
        public HighcareAttr() : base("HIGHCARE") { }
    }

    [Serializable]
    [PXHidden]
    public class CustomerPINCodeFilter : IBqlTable
    {
        [Customer(DisplayName = "Customer ID")]
        public virtual int? BAccountID { get; set; }
        public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
    }
}
