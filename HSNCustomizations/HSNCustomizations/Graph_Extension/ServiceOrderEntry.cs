﻿using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;
using System.Linq;
using System.Collections.Generic;
using HSNCustomizations.DAC;
using HSNCustomizations.Descriptor;
using PX.SM;
using PX.Common;
using System.Collections;

namespace PX.Objects.FS
{
    public class ServiceOrderEntry_Extension : PXGraphExtension<ServiceOrderEntry>
    {
        #region Selects
        public SelectFrom<INRegister>.Where<INRegister.docType.IsIn<INDocType.transfer, INDocType.receipt>
                                            .And<INRegisterExt.usrSrvOrdType.IsEqual<FSServiceOrder.srvOrdType.FromCurrent>
                                                 .And<INRegisterExt.usrSORefNbr.IsEqual<FSServiceOrder.refNbr.FromCurrent>>>>.View INRegisterView;

        public SelectFrom<LUMSrvEventHistory>.Where<LUMSrvEventHistory.srvOrdType.IsEqual<FSServiceOrder.srvOrdType.FromCurrent>
                                                    .And<LUMSrvEventHistory.sORefNbr.IsEqual<FSServiceOrder.refNbr.FromCurrent>>>.View EventHistory;

        public CRActivityListReadonly<FSAppointment> Activities;
        #endregion

        #region Override Method
        public override void Initialize()
        {
            base.Initialize();
            FSWorkflowStageHandler.InitStageList();
            AddAllStageButton();
        }
        #endregion

        #region Delegate Method
        public delegate void PersistDelegate();
        [PXOverride]
        public void Persist(PersistDelegate baseMethod)
        {
            if (Base.ServiceOrderRecords.Current != null &&
               (SelectFrom<FSSrvOrdType>.Where<FSSrvOrdType.srvOrdType.IsEqual<P.AsString>>.View.Select(Base, Base.ServiceOrderRecords.Current.SrvOrdType).TopFirst?.GetExtension<FSSrvOrdTypeExt>().UsrEnableEquipmentMandatory ?? false))
            {
                VerifyEquipmentIDMandatory();
            }

            if (Base.ServiceOrderRecords.Current != null &&
                Base.ServiceOrderRecords.Current.Status != FSAppointment.status.Values.Closed &&
                SelectFrom<LUMHSNSetup>.View.Select(Base).TopFirst?.EnableHeaderNoteSync == true)
            {
                AppointmentEntry_Extension.SyncNoteApptOrSrvOrd(Base, typeof(FSServiceOrder), typeof(FSAppointment));
            }

            var isNewData = Base.ServiceOrderRecords.Cache.Inserted.RowCast<FSServiceOrder>().Count() > 0;
            // Check Status is Dirty
            var statusDirtyResult = CheckStatusIsDirty(Base.ServiceOrderRecords.Current);
            // Check Stage is Dirty
            var wfStageDirtyResult = CheckWFStageIsDirty(Base.ServiceOrderRecords.Current);
            // Set UsrLastSatusModDate if Stage is dirty
            if (wfStageDirtyResult.IsDirty)
                Base.ServiceOrderRecords.Current.GetExtension<FSServiceOrderExt>().UsrLastSatusModDate = PXTimeZoneInfo.Now;
            baseMethod();
            try
            {
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    FSWorkflowStageHandler.srvEntry = Base;
                    FSWorkflowStageHandler.InitStageList();

                    // insert log if status is change
                    if (statusDirtyResult.IsDirty && !string.IsNullOrEmpty(statusDirtyResult.oldValue))
                        FSWorkflowStageHandler.InsertEventHistoryForStatus(nameof(ServiceOrderEntry), statusDirtyResult.oldValue, statusDirtyResult.newValue);

                    LUMAutoWorkflowStage autoWFStage = new LUMAutoWorkflowStage();

                    // New Data
                    if (isNewData)
                        autoWFStage = LUMAutoWorkflowStage.PK.Find(Base, Base.ServiceOrderRecords.Current.SrvOrdType, nameof(WFRule.OPEN01));
                    // Manual Chagne Stage
                    else if (wfStageDirtyResult.IsDirty && wfStageDirtyResult.oldValue.HasValue && wfStageDirtyResult.newValue.HasValue)
                        autoWFStage = new LUMAutoWorkflowStage()
                        {
                            SrvOrdType = Base.ServiceOrderRecords.Current.SrvOrdType,
                            WFRule = "MANUAL",
                            Active = true,
                            CurrentStage = wfStageDirtyResult.oldValue,
                            NextStage = wfStageDirtyResult.newValue,
                            Descr = "Manual change Stage"
                        };
                    // Workflow
                    else
                        autoWFStage = FSWorkflowStageHandler.AutoWFStageRule(nameof(ServiceOrderEntry));
                    if (autoWFStage != null && autoWFStage.Active == true)
                        FSWorkflowStageHandler.UpdateWFStageID(nameof(ServiceOrderEntry), autoWFStage);

                    baseMethod();
                    ts.Complete();
                }
            }
            catch (PXException)
            {
                throw;
            }
        }
        #endregion

        #region Override DAC
        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Contact")]
        [PXSelector(typeof(
                   SelectFrom<Contact>
                   .InnerJoin<BAccount>.On<Contact.bAccountID.IsEqual<BAccount.bAccountID>>
                   .Where<Contact.contactType.IsNotEqual<ContactTypesAttribute.bAccountProperty>
                     .And<BAccount.type.IsEqual<BAccountType.customerType>.Or<BAccount.type.IsEqual<BAccountType.prospectType>.Or<BAccount.type.IsEqual<BAccountType.combinedType>>>>
                     .And<BAccount.bAccountID.IsEqual<FSServiceOrder.customerID.FromCurrent>.Or<FSServiceOrder.customerID.FromCurrent.IsEqual<Null>>>>
                   .SearchFor<Contact.contactID>),
           typeof(Contact.contactID),
           typeof(ContactExtensions.usrLocationID),
           typeof(Contact.displayName),
           typeof(Contact.fullName),
           typeof(Contact.title),
           typeof(Contact.eMail),
           typeof(Contact.phone1),
           typeof(Contact.contactType),
           DescriptionField = typeof(Contact.displayName))]
        [PXMergeAttributes(Method = MergeMethod.Replace)]
        public virtual void _(Events.CacheAttached<FSServiceOrder.contactID> e) { }
        #endregion

        #region Event Handlers
        protected void _(Events.RowSelected<FSServiceOrder> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            LUMHSNSetup hSNSetup = SelectFrom<LUMHSNSetup>.View.Select(Base);

            bool activeWFStageCtrl = hSNSetup?.EnableWFStageCtrlInAppt == true;

            lumStages.SetVisible(activeWFStageCtrl);

            Activities.AllowSelect = hSNSetup?.DispApptActiviteInSrvOrd ?? false;
            EventHistory.AllowSelect = activeWFStageCtrl;
            INRegisterView.AllowSelect = hSNSetup?.EnablePartReqInAppt == true;

            SettingStageButton();
        }

        protected void _(Events.RowUpdated<FSServiceOrder> e, PXRowUpdated baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            if (e.OldRow.ContactID != e.Row.ContactID)
            {
                SetSrvContactInfo(Base.ServiceOrder_Contact.Cache, e.Row.ContactID, e.Row.ServiceOrderContactID);
            }
        }

        protected void _(Events.RowSelected<FSSODet> e, PXRowSelected baseHandler)
        {
            // [Phase - II] Add new Field in Equipment and Appointment
            baseHandler?.Invoke(e.Cache, e.Args);
            var setup = SelectFrom<LUMHSNSetup>.View.Select(Base).TopFirst;
            if (e.Row != null && (setup?.EnableEquipmentModel ?? false))
            {
                var equipmentInfo = FSEquipment.PK.Find(Base, ((FSSODet)e.Row)?.SMEquipmentID);
                PXUIFieldAttribute.SetVisible<FSSODetExt.usrEquipmentModel>(e.Cache, null, setup?.EnableEquipmentModel ?? false);
                PXUIFieldAttribute.SetVisible<FSSODetExt.usrRegistrationNbr>(e.Cache, null, setup?.EnableEquipmentModel ?? false);

                Base.ServiceOrderDetails.SetValueExt<FSSODetExt.usrEquipmentModel>(e.Row, equipmentInfo.GetExtension<FSEquipmentExtension>()?.UsrEquipmentModel);
                Base.ServiceOrderDetails.SetValueExt<FSSODetExt.usrRegistrationNbr>(e.Row, equipmentInfo?.RegistrationNbr);
                Base.ServiceOrderDetails.SetValueExt<FSSODetExt.usrEquipSerialNbr>(e.Row, equipmentInfo?.SerialNumber);
            }
        }

        protected void _(Events.FieldUpdated<FSSODet.SMequipmentID> e, PXFieldUpdated baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);
            // [Phase - II] Add new Field in Equipment and Appointment
            var setup = SelectFrom<LUMHSNSetup>.View.Select(Base).TopFirst;
            if ((setup?.EnableEquipmentModel ?? false) && e.Row != null)
            {
                var row = (FSSODet)e.Row;
                var equipmentInfo = FSEquipment.PK.Find(Base, ((FSSODet)e.Row)?.SMEquipmentID);
                Base.ServiceOrderDetails.SetValueExt<FSSODetExt.usrEquipmentModel>(row, equipmentInfo.GetExtension<FSEquipmentExtension>()?.UsrEquipmentModel);
                Base.ServiceOrderDetails.SetValueExt<FSSODetExt.usrRegistrationNbr>(row, equipmentInfo?.RegistrationNbr);
                Base.ServiceOrderDetails.SetValueExt<FSSODetExt.usrEquipSerialNbr>(row, equipmentInfo?.SerialNumber);
            }
            // [Phase - II] Add a New Tab: Service Contract in Equipment & New Field in Appointment
            if ((setup?.EnableOverrideWarranty ?? false) && e.Row != null)
            {
                var row = (FSSODet)e.Row;
                var contractInfo = PXSelectJoin<FSServiceContract,
                                    InnerJoin<FSContractPeriodDet, On<FSServiceContract.serviceContractID, Equal<FSContractPeriodDet.serviceContractID>>>,
                                    Where<FSContractPeriodDet.SMequipmentID, Equal<P.AsString>>,
                                    OrderBy<Desc<FSServiceContract.endDate>>>.Select(Base, row.SMEquipmentID).RowCast<FSServiceContract>();
                // PX.Objects.FS.FSServiceContract. EndDate for this Target Equipment ID >= FSAppointment. ExecutionDate AND PX.Objects.FS.FSServiceContract.Status is “Active”
                if (contractInfo.Any(x => x.EndDate >= Base.ServiceOrderRecords.Current?.OrderDate && x.Status == "A") && !(row.Warranty ?? false))
                    Base.ServiceOrderDetails.SetValueExt<FSSODet.warranty>(row, true);
            }
        }

        public void _(Events.FieldUpdated<FSSODet.warranty> e, PXFieldUpdated baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);
            var setup = SelectFrom<LUMHSNSetup>.View.Select(Base).TopFirst;
            // [Phase - II] Add a New Tab: Service Contract in Equipment & New Field in Appointment
            if ((setup?.EnableOverrideWarranty ?? false) && e.Row != null && (bool?)e.NewValue == false)
            {
                var row = (FSSODet)e.Row;
                var contractInfo = PXSelectJoin<FSServiceContract,
                                    InnerJoin<FSContractPeriodDet, On<FSServiceContract.serviceContractID, Equal<FSContractPeriodDet.serviceContractID>>>,
                                    Where<FSContractPeriodDet.SMequipmentID, Equal<P.AsString>>,
                                    OrderBy<Desc<FSServiceContract.endDate>>>.Select(Base, row.SMEquipmentID).RowCast<FSServiceContract>();
                // PX.Objects.FS.FSServiceContract. EndDate for this Target Equipment ID >= FSSODet. OrderDate AND PX.Objects.FS.FSServiceContract.Status is “Active”
                if (contractInfo.Any(x => x.EndDate >= Base.ServiceOrderRecords.Current?.OrderDate && x.Status == "A") && !(row.Warranty ?? false))
                    Base.ServiceOrderDetails.SetValueExt<FSSODet.warranty>(row, true);
            }
        }

        #endregion

        #region Action
        public PXMenuAction<FSServiceOrder> lumStages;
        [PXUIField(DisplayName = "STAGES", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(MenuAutoOpen = true, CommitChanges = true)]
        public virtual void LumStages() { }

        /// <summary>
        /// [Taiwan-Phase2] 修復改了Service Order Details中 CuryUnitPrice後，Create Appointment CuryUntiPrice會不正確問題
        /// </summary>
        /// <param name="adapter"></param>
        /// <returns></returns>
        [PXUIField(DisplayName = "Create Appointment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
        public virtual IEnumerable ScheduleAppointment(PXAdapter adapter)
        {
            try
            {
                Base.ScheduleAppointment(adapter);
            }
            catch (PXRedirectRequiredException ex)
            {
                var graphAppointmentEntry = ex.Graph as AppointmentEntry;
                var srvDet = Base.ServiceOrderDetails.Cache.Cached.RowCast<FSSODet>();
                foreach (FSAppointmentDet item in graphAppointmentEntry.AppointmentDetails.Cache.Cached.RowCast<FSAppointmentDet>())
                {
                    // 找出Service order Det & Appointment Det 對應的那筆資料
                    var currentDetailLine = srvDet.FirstOrDefault(x => x.InventoryID == item.InventoryID && x.LineNbr == item.OrigLineNbr);
                    if (currentDetailLine != null && currentDetailLine?.CuryUnitPrice != item?.CuryUnitPrice)
                    {
                        graphAppointmentEntry.AppointmentDetails.SetValueExt<FSAppointmentDet.manualPrice>(item, true);
                        graphAppointmentEntry.AppointmentDetails.SetValueExt<FSAppointmentDet.curyUnitPrice>(item, currentDetailLine?.CuryUnitPrice);
                    }

                    // [Phase II - Staff Selection for Customer Locations]
                    graphAppointmentEntry.GetExtension<AppointmentEntry_Extension>().InsertStaffManual(graphAppointmentEntry, item);
                }
                throw new PXRedirectRequiredException(graphAppointmentEntry, null);
            }
            return adapter.Get<FSServiceOrder>().ToList();
        }

        #endregion

        #region Methods
        /// <summary>Check Status Is Drity </summary>
        public (bool IsDirty, string oldValue, string newValue) CheckStatusIsDirty(FSServiceOrder row)
        {
            if (row == null)
                return (false, string.Empty, string.Empty);

            var oldVale = SelectFrom<FSServiceOrder>
                             .Where<FSServiceOrder.srvOrdType.IsEqual<P.AsString>
                                 .And<FSServiceOrder.refNbr.IsEqual<P.AsString>>>
                              .View.Select(new PXGraph(), row.SrvOrdType, row.RefNbr)
                              .RowCast<FSServiceOrder>()?.FirstOrDefault()?.Status;
            var newValue = row.Status;

            return (!string.IsNullOrEmpty(oldVale) && oldVale != newValue, oldVale, newValue);
        }

        /// <summary>Check Stage Is Dirty </summary>
        public (bool IsDirty, int? oldValue, int? newValue) CheckWFStageIsDirty(FSServiceOrder row)
        {
            if (row == null)
                return (false, null, null);

            var oldVale = SelectFrom<FSServiceOrder>
                             .Where<FSServiceOrder.srvOrdType.IsEqual<P.AsString>
                                 .And<FSServiceOrder.refNbr.IsEqual<P.AsString>>>
                              .View.Select(new PXGraph(), row.SrvOrdType, row.RefNbr)
                              .RowCast<FSServiceOrder>()?.FirstOrDefault()?.WFStageID;
            var newValue = row.WFStageID;

            return (oldVale.HasValue && oldVale != newValue, oldVale, newValue);
        }

        /// <summary> Add All Stage Button </summary>
        public void AddAllStageButton()
        {
            var primatryView = Base.ServiceOrderRecords.Cache.GetItemType();
            var list = FSWorkflowStageHandler.stageList.Select(x => new { x.WFStageID, x.WFStageCD }).Distinct();
            var actionLst = new List<PXAction>();
            foreach (var item in list)
            {
                var temp = PXNamedAction.AddAction(Base, primatryView, item.WFStageCD, item.WFStageCD,
                    adapter =>
                    {
                        var row = Base.ServiceOrderRecords.Current;
                        if (row != null)
                        {
                            var srvOrderData = FSSrvOrdType.PK.Find(new PXGraph(), row.SrvOrdType);
                            var stageList = FSWorkflowStageHandler.stageList.Where(x => x.WFID == srvOrderData.SrvOrdTypeID);
                            var currStageIDByType = stageList.Where(x => x.WFStageCD == item.WFStageCD).FirstOrDefault().WFStageID;
                            Base.ServiceOrderRecords.Cache.SetValueExt<FSServiceOrder.wFStageID>(Base.ServiceOrderRecords.Current, currStageIDByType);
                            Base.ServiceOrderRecords.Cache.MarkUpdated(Base.ServiceOrderRecords.Current);
                            Base.ServiceOrderRecords.Update(Base.ServiceOrderRecords.Current);
                            Base.Persist();
                        }
                        return adapter.Get();
                    },
                    new PXEventSubscriberAttribute[] { new PXButtonAttribute() { CommitChanges = true, MenuAutoOpen = false } }
                );
                actionLst.Add(temp);
            }
            foreach (var act in actionLst)
            {
                act.SetDisplayOnMainToolbar(false);
                this.lumStages.AddMenuAction(act);
            }
        }

        /// <summary> Setting Stage Button Status </summary>
        public void SettingStageButton()
        {
            var row = Base.ServiceOrderRecords.Current;
            var isAdmin = SelectFrom<UsersInRoles>
                             .Where<UsersInRoles.rolename.IsEqual<P.AsString>
                                   .And<UsersInRoles.username.IsEqual<P.AsString>>>
                             .View.Select(Base, "Administrator", PXAccess.GetUserName())
                             .Count > 0;


            if (row != null && !string.IsNullOrEmpty(row.SrvOrdType))
            {
                List<PXResult<LumStageControl>> lists = SelectFrom<LumStageControl>.Where<LumStageControl.srvOrdType.IsEqual<P.AsString>
                                                                                          .And<LumStageControl.currentStage.IsEqual<P.AsInt>>>
                                                                                   .View.Select(Base, row.SrvOrdType, row.WFStageID).ToList();

                var btn = this.lumStages.GetState(null) as PXButtonState;

                if (btn.Menus != null)
                {
                    foreach (ButtonMenu btnMenu in btn.Menus)
                    {
                        var isVisible = lists.Exists(x => (!(x.GetItem<LumStageControl>().AdminOnly ?? false) || ((x.GetItem<LumStageControl>().AdminOnly ?? false) && isAdmin ? true : false)) && FSWorkflowStageHandler.GetStageName(x.GetItem<LumStageControl>().ToStage) == btnMenu.Command);
                        this.lumStages.SetVisible(btnMenu.Command, isVisible);
                    }
                }
            }
        }

        public void VerifyEquipmentIDMandatory()
        {
            var details = Base.ServiceOrderDetails.Select();
            foreach (FSSODet item in details)
            {
                if (item.LineType == "SERVI" && !item.SMEquipmentID.HasValue)
                    throw new PXException("Target Equipment ID cannot be blank for service");
            }
        }

        #endregion

        #region Static Methods
        /// <summary>
        /// Spec [Design Concept - Service Order Enhancement-V2.1] 1.10.4 Override Company Name and Attention
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="contactID"></param>
        /// <param name="srvOrdContactID"></param>
        public static void SetSrvContactInfo(PXCache cache, int? contactID, int? srvOrdContactID)
        {
            var contact = cache.Cached.RowCast<FSContact>().Where(x => x.ContactID == srvOrdContactID).FirstOrDefault();

            foreach (CSAnswers row in SelectFrom<CSAnswers>.InnerJoin<Contact>.On<CSAnswers.refNoteID.IsEqual<Contact.noteID>>
                                                           .Where<Contact.contactID.IsEqual<@P.AsInt>>.View.Select(cache.Graph, contactID))
            {
                switch (row.AttributeID)
                {
                    case HSNMessages.CompanyName_Attr:
                        contact.FullName = row.Value;
                        break;

                    case HSNMessages.Attention_Attr:
                        contact.Attention = row.Value;
                        break;
                }
            }

            cache.MarkUpdated(contact);
        }
        #endregion
    }
}