using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using HSNCustomizations.DAC;
using HSNCustomizations.Descriptor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using System;

namespace PX.Objects.FS
{
    public class SMEquipmentMaint_Extension : PXGraphExtension<SMEquipmentMaint>
    {

        [PXCacheName("WarrantyHistory")]
        public SelectFrom<LUMWarrantyHistory>
              .Where<LUMWarrantyHistory.sMEquipmentID.IsEqual<FSEquipment.SMequipmentID.FromCurrent>>
              .View WarrantyHistory;

        // [Phase - II] Add a New Tab: Service Contract in Equipment & New Field in Appointment
        public PXSelectJoinGroupBy<FSServiceContract,
                InnerJoin<FSContractPeriodDet, On<FSServiceContract.serviceContractID, Equal<FSContractPeriodDet.serviceContractID>>>,
                Where<FSContractPeriodDet.SMequipmentID, Equal<Current<FSEquipment.SMequipmentID>>>,
                Aggregate<GroupBy<FSServiceContract.serviceContractID, Max<FSServiceContract.serviceContractID>>>,
                OrderBy<Desc<FSServiceContract.endDate>>>
                ServiceContractMappingList;

        public override void Initialize()
        {
            base.Initialize();
            this.ServiceContractMappingList.AllowDelete = ServiceContractMappingList.AllowInsert = ServiceContractMappingList.AllowUpdate = false;
        }

        #region Event Handlers

        public virtual void _(Events.RowSelected<FSEquipment> e, PXRowSelected baseHandler)
        {
            // [Phase - II] Add new Field in Equipment and Appointment
            baseHandler?.Invoke(e.Cache, e.Args);
            var setup = SelectFrom<LUMHSNSetup>.View.Select(Base).TopFirst;
            PXUIFieldAttribute.SetVisible<FSEquipmentExtension.usrEquipmentModel>(e.Cache, null, setup?.EnableEquipmentModel ?? false);
        }

        protected void _(Events.RowPersisting<FSEquipment> e, PXRowPersisting baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            var row = e.Row as FSEquipment;

            if (row != null && (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update))
            {
                bool duplicate = SelectFrom<FSEquipment>.Where<FSEquipment.serialNumber.IsEqual<@P.AsString>
                                                          .And<FSEquipment.equipmentTypeID.IsEqual<@P.AsInt>>
                                                          .And<FSEquipment.refNbr.IsNotEqual<@P.AsString>>>.View.Select(new PXGraph(), row.SerialNumber ?? string.Empty, row.EquipmentTypeID, row.RefNbr).Count > 0;

                if (!string.IsNullOrEmpty(row.SerialNumber) && SelectFrom<LUMHSNSetup>.View.Select(Base).TopFirst?.EnableUniqSerialNbrByEquipType == true && duplicate == true)
                {
                    e.Cache.RaiseExceptionHandling<FSEquipment.serialNumber>(row, row.SerialNumber, new PXSetPropertyException(HSNMessages.NonUniqueSerNbr));
                }
            }
        }

        /// <summary>
        /// Field Defaulting - LUMWarrantyHistory.lineNbr
        /// </summary>
        /// <param name="e"></param>
        public virtual void _(Events.FieldDefaulting<LUMWarrantyHistory.lineNbr> e)
        {
            var currentList = this.WarrantyHistory.Select().RowCast<LUMWarrantyHistory>();
            var maxLineNbr = currentList.Count() == 0 ? 0 : currentList.Max(x => x?.LineNbr ?? 0);
            e.NewValue = maxLineNbr + 1;
        }

        /// <summary>
        /// Field Updated - LUMWarrantyHistory.warrantyStartDate
        /// </summary>
        /// <param name="e"></param>
        public virtual void _(Events.FieldUpdated<LUMWarrantyHistory.warrantyStartDate> e)
        {
            if (e.NewValue != null)
            {
                object newValue;
                e.Cache.RaiseFieldDefaulting<LUMWarrantyHistory.warrantyEndDate>(e.Row, out newValue);
                e.Cache.SetValueExt<LUMWarrantyHistory.warrantyEndDate>(e.Row, newValue);
            }
        }

        /// <summary>
        /// Field Defaulting - LUMWarrantyHistory.warrantyEndDate
        /// </summary>
        /// <param name="e"></param>
        public virtual void _(Events.FieldDefaulting<LUMWarrantyHistory.warrantyEndDate> e)
        {
            var row = e.Row as LUMWarrantyHistory;
            if (Base.IsContractBasedAPI)
                e.NewValue = (row?.WarrantyStartDate ?? DateTime.Now).AddMonths(row?.WarrantyMonths ?? 0);

            else
                e.NewValue = (row?.WarrantyStartDate ?? PXContext.GetBusinessDate()).Value.AddMonths(row?.WarrantyMonths ?? 0);
        }

        /// <summary>
        /// Field Updated - LUMWarrantyHistory.warrantyMonths
        /// </summary>
        /// <param name="e"></param>
        public virtual void _(Events.FieldUpdated<LUMWarrantyHistory.warrantyMonths> e)
        {
            if (e.NewValue != null)
            {
                object newValue;
                e.Cache.RaiseFieldDefaulting<LUMWarrantyHistory.warrantyEndDate>(e.Row, out newValue);
                e.Cache.SetValueExt<LUMWarrantyHistory.warrantyEndDate>(e.Row, newValue);
            }
        }

        #endregion
    }
}