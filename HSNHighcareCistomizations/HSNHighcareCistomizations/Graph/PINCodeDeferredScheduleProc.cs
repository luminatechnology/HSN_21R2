﻿using HSNHighcareCistomizations.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.IN;
using PX.Data.BQL;
using PX.Objects.CS;
using PX.Objects.DR;
using PX.Objects.CR;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.SO;
using PX.Objects.FS;

namespace HSNHighcareCistomizations.Graph
{
    public class PINCodeDeferredScheduleProc : PXGraph<PINCodeDeferredScheduleProc>
    {

        [InjectDependency]
        public IFinPeriodRepository FinPeriodRepository { get; set; }

        public PXCancel<LUMCustomerPINCode> Cancel;
        public PXProcessingJoin<LUMCustomerPINCode,
                                InnerJoin<LUMPINCodeMapping, On<LUMCustomerPINCode.pin, Equal<LUMPINCodeMapping.pin>>,
                                InnerJoin<Customer, On<LUMCustomerPINCode.bAccountID, Equal<Customer.bAccountID>>>>,
                                Where<LUMCustomerPINCode.scheduleNbr.IsNull>> ProcessList;

        public PINCodeDeferredScheduleProc()
            => ProcessList.SetProcessDelegate(GoProcess);

        #region Static Method

        public static void GoProcess(List<LUMCustomerPINCode> list)
            => PXGraph.CreateInstance<PINCodeDeferredScheduleProc>().CreateDeferralSchedule(list);
        #endregion

        #region Method
        public virtual void CreateDeferralSchedule(List<LUMCustomerPINCode> list)
        {
            if (list.Count <= 0)
                return;
            PXLongOperation.StartOperation(this, delegate ()
            {
                foreach (var item in list)
                {
                    try
                    {
                        // Find PIN Code Serial Nbr.
                        var serialNbr = LUMPINCodeMapping.PK.Find(this, item.Pin)?.SerialNbr;
                        if (string.IsNullOrEmpty(serialNbr))
                            throw new PXException("can not find serial Nbr.");
                        // Find SOShipment by Serial Nbr.
                        var soshipLine = SelectFrom<SOShipLine>
                                         .InnerJoin<SOShipLineSplit>.On<SOShipLine.shipmentNbr.IsEqual<SOShipLineSplit.shipmentNbr>
                                                                    .And<SOShipLine.lineNbr.IsEqual<SOShipLineSplit.lineNbr>>>
                                         .Where<SOShipLineSplit.lotSerialNbr.IsEqual<P.AsString>>
                                         .View.Select(this, serialNbr).RowCast<SOShipLine>().FirstOrDefault();
                        var apptLine = SelectFrom<FSAppointmentDet>
                                      .InnerJoin<FSApptLineSplit>.On<FSAppointmentDet.srvOrdType.IsEqual<FSApptLineSplit.srvOrdType>
                                                                .And<FSAppointmentDet.refNbr.IsEqual<FSApptLineSplit.apptNbr>>
                                                                .And<FSAppointmentDet.lineNbr.IsEqual<FSApptLineSplit.lineNbr>>>
                                      .Where<FSApptLineSplit.lotSerialNbr.IsEqual<P.AsString>>
                                      .View.Select(this, serialNbr).TopFirst;

                        if (soshipLine == null && apptLine == null)
                            throw new PXException("can not find SOShipment/Apppointment!!");

                        if (soshipLine != null)
                        {
                            #region Create Deferred Revenue By SOShipment
                            // Find SOLine by SOShipment
                            var soline = SOLine.PK.Find(this, soshipLine?.OrigOrderType, soshipLine?.OrigOrderNbr, soshipLine?.OrigLineNbr);
                            if (soline == null)
                                throw new PXException("can not find SOLine!!");
                            // Find Invoice
                            var invoice = SelectFrom<SOOrderShipment>
                                     .Where<SOOrderShipment.orderType.IsEqual<P.AsString>
                                       .And<SOOrderShipment.orderNbr.IsEqual<P.AsString>>
                                       .And<SOOrderShipment.shipmentNbr.IsEqual<P.AsString>>>
                                     .View.Select(this, soline.OrderType, soline.OrderNbr, soshipLine.ShipmentNbr).TopFirst;

                            var scheduleNbr = CreateDeferredRevenue(item, soline?.CuryLineAmt / soline?.OrderQty);

                            // setting CustomerPIN Code DeferralSchedule
                            PXUpdate<Set<LUMCustomerPINCode.scheduleNbr, Required<LUMCustomerPINCode.scheduleNbr>,
                                     Set<LUMCustomerPINCode.sOOrderNbr, Required<LUMCustomerPINCode.sOOrderNbr>,
                                     Set<LUMCustomerPINCode.invoiceNbr, Required<LUMCustomerPINCode.invoiceNbr>>>>,
                                     LUMCustomerPINCode,
                                     Where<LUMCustomerPINCode.bAccountID, Equal<P.AsInt>,
                                       And<LUMCustomerPINCode.pin, Equal<P.AsString>,
                                       And<LUMCustomerPINCode.cPriceClassID, Equal<P.AsString>>>>>
                            .Update(this, scheduleNbr, soline?.OrderNbr, invoice?.InvoiceNbr, item.BAccountID, item.Pin, item.CPriceClassID);
                            #endregion
                        }
                        // 
                        else
                        {
                            #region Create Deferred Revenue By Appointment
                            // Find Invoice
                            var invoice = SelectFrom<ARInvoice>
                                         .InnerJoin<FSBillHistory>.On<ARInvoice.docType.IsEqual<FSBillHistory.childDocType>
                                                                 .And<ARInvoice.refNbr.IsEqual<FSBillHistory.childRefNbr>>>
                                         .Where<FSBillHistory.srvOrdType.IsEqual<P.AsString>
                                           .And<FSBillHistory.appointmentRefNbr.IsEqual<P.AsString>>>
                                         .View.Select(this, apptLine.SrvOrdType, apptLine.RefNbr).TopFirst;

                            var scheduleNbr = CreateDeferredRevenue(item, apptLine.CuryBillableTranAmt ?? 0);
                            // setting CustomerPIN Code DeferralSchedule
                            PXUpdate<Set<LUMCustomerPINCode.scheduleNbr, Required<LUMCustomerPINCode.scheduleNbr>,
                                     Set<LUMCustomerPINCode.serviceOrderNbr, Required<LUMCustomerPINCode.serviceOrderNbr>,
                                     Set<LUMCustomerPINCode.invoiceNbr, Required<LUMCustomerPINCode.invoiceNbr>>>>,
                                     LUMCustomerPINCode,
                                     Where<LUMCustomerPINCode.bAccountID, Equal<P.AsInt>,
                                       And<LUMCustomerPINCode.pin, Equal<P.AsString>,
                                       And<LUMCustomerPINCode.cPriceClassID, Equal<P.AsString>>>>>
                            .Update(this, scheduleNbr, apptLine?.OrigSrvOrdNbr, invoice?.RefNbr, item.BAccountID, item.Pin, item.CPriceClassID);
                            #endregion
                        }

                        this.Actions.PressSave();
                    }
                    catch (Exception ex)
                    {
                        PXProcessing.SetError<LUMCustomerPINCode>(ex.Message);
                    }
                }
            });
        }

        public string CreateDeferredRevenue(LUMCustomerPINCode item, decimal? totalAmt)
        {
            FinPeriod finPeriod = FinPeriodRepository.GetFinPeriodByDate(DateTime.Now, PXAccess.GetParentOrganizationID(PXAccess.GetBranchID()));
            var scope = SelectFrom<LUMServiceScopeHeader>
                        .Where<LUMServiceScopeHeader.cPriceClassID.IsEqual<P.AsString>>
                        .View.Select(this, item.CPriceClassID).RowCast<LUMServiceScopeHeader>().FirstOrDefault();
            var acctInfo = BAccount2.PK.Find(this, item.BAccountID.Value);
            var itemInfo = InventoryItem.PK.Find(this, scope.InventoryID);
            if (scope == null)
                throw new PXException("please maintain Service Scope");
            // Create Draft Schedule Graph
            var graph = PXGraph.CreateInstance<DraftScheduleMaint>();

            #region Create document
            // Create document
            var draftDoc = graph.Schedule.Insert((DRSchedule)graph.Schedule.Cache.CreateInstance());
            graph.Schedule.Cache.SetValue<DRSchedule.bAccountID>(draftDoc, item.BAccountID);
            graph.Schedule.Cache.SetValueExt<DRSchedule.bAccountLocID>(draftDoc, acctInfo?.DefLocationID);
            graph.Schedule.Cache.SetValueExt<DRSchedule.termStartDate>(draftDoc, item.StartDate);
            graph.Schedule.Cache.SetValueExt<DRSchedule.termEndDate>(draftDoc, item.EndDate);
            #endregion

            #region Create Components
            // Create Components
            var component = graph.Components.Insert((DRScheduleDetail)graph.Components.Cache.CreateInstance());
            graph.Components.Cache.SetValueExt<DRScheduleDetail.accountID>(component, itemInfo.DeferralAcctID);
            graph.Components.Cache.SetValueExt<DRScheduleDetail.subID>(component, itemInfo.DeferralSubID);
            graph.Components.Cache.SetValueExt<DRScheduleDetail.componentID>(component, scope.InventoryID);
            graph.Components.Cache.SetValueExt<DRScheduleDetail.defCode>(component, scope.DefCode);
            graph.Components.Cache.SetValueExt<DRScheduleDetail.totalAmt>(component, totalAmt ?? 0);
            graph.Components.Cache.SetValueExt<DRScheduleDetail.branchID>(component, PXAccess.GetBranchID());
            component.FinPeriodID = finPeriod.FinPeriodID;
            #endregion

            // Generate Transactions
            graph.generateTransactions.Press();
            graph.Save.Press();
            graph.release.Press();

            return graph.Schedule.Current.ScheduleNbr;
        }
        #endregion
    }
}
