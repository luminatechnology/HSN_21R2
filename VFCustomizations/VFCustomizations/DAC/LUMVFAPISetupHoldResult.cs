using System;
using PX.Data;
using PX.Objects.SO;

namespace VFCustomizations.DAC
{
    [Serializable]
    [PXCacheName("LUMVFAPISetupHoldResult")]
    public class LUMVFAPISetupHoldResult : IBqlTable
    {
        #region Selected
        [PXBool()]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected { get; set; }
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
        #endregion

        #region ShipmentNbr
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Shipment Nbr", Visible = false)]
        [PXDefault(typeof(SOShipment.shipmentNbr))]
        public virtual string ShipmentNbr { get; set; }
        public abstract class shipmentNbr : PX.Data.BQL.BqlString.Field<shipmentNbr> { }
        #endregion

        #region ShipmentType
        [PXDBString(1, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Shipment Type", Visible = false)]
        [PXDefault(typeof(SOShipment.shipmentType))]
        public virtual string ShipmentType { get; set; }
        public abstract class shipmentType : PX.Data.BQL.BqlString.Field<shipmentType> { }
        #endregion

        #region LineNbr
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Line Nbr", Visible = false)]
        [PXDefault]
        public virtual int? LineNbr { get; set; }
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        #endregion

        #region JobNo
        [PXDBString(16, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "JobNo")]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        public virtual string JobNo { get; set; }
        public abstract class jobNo : PX.Data.BQL.BqlString.Field<jobNo> { }
        #endregion

        #region PreviousCommitDate
        [PXDBDateAndTime()]
        [PXUIField(DisplayName = "Previous CommitDate")]
        [PXDefault]
        public virtual DateTime? PreviousCommitDate { get; set; }
        public abstract class previousCommitDate : PX.Data.BQL.BqlDateTime.Field<previousCommitDate> { }
        #endregion

        #region CommitDate
        [PXDBDateAndTime()]
        [PXUIField(DisplayName = "CommitDate")]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        public virtual DateTime? CommitDate { get; set; }
        public abstract class commitDate : PX.Data.BQL.BqlDateTime.Field<commitDate> { }
        #endregion

        #region HoldReason
        [PXDBString(30, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Hold Reason")]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        public virtual string HoldReason { get; set; }
        public abstract class holdReason : PX.Data.BQL.BqlString.Field<holdReason> { }
        #endregion

        #region HoldDate
        [PXDBDate()]
        [PXUIField(DisplayName = "Hold Date")]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        public virtual DateTime? HoldDate { get; set; }
        public abstract class holdDate : PX.Data.BQL.BqlDateTime.Field<holdDate> { }
        #endregion

        #region HoldSatus
        [PXDBString(30, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Hold Satus")]
        public virtual string HoldSatus { get; set; }
        public abstract class holdSatus : PX.Data.BQL.BqlString.Field<holdSatus> { }
        #endregion

        #region IncidentCatalogName
        [PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "IncidentCatalog Name")]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        public virtual string IncidentCatalogName { get; set; }
        public abstract class incidentCatalogName : PX.Data.BQL.BqlString.Field<incidentCatalogName> { }
        #endregion

        #region Processed
        [PXDBBool()]
        [PXUIField(DisplayName = "Processed", Enabled = false)]
        public virtual bool? Processed { get; set; }
        public abstract class processed : PX.Data.BQL.BqlBool.Field<processed> { }
        #endregion

        #region ProcessedDateTime
        [PXDBDate()]
        [PXUIField(DisplayName = "Processed Date Time", Enabled = false)]
        public virtual DateTime? ProcessedDateTime { get; set; }
        public abstract class processedDateTime : PX.Data.BQL.BqlDateTime.Field<processedDateTime> { }
        #endregion

        #region Noteid
        [PXNote()]
        public virtual Guid? Noteid { get; set; }
        public abstract class noteid : PX.Data.BQL.BqlGuid.Field<noteid> { }
        #endregion

        #region CreatedByID
        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID { get; set; }
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        #endregion

        #region CreatedByScreenID
        [PXDBCreatedByScreenID()]
        public virtual string CreatedByScreenID { get; set; }
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
        #endregion

        #region CreatedDateTime
        [PXDBCreatedDateTime()]
        public virtual DateTime? CreatedDateTime { get; set; }
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
        #endregion

        #region LastModifiedByID
        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID { get; set; }
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
        #endregion

        #region LastModifiedByScreenID
        [PXDBLastModifiedByScreenID()]
        public virtual string LastModifiedByScreenID { get; set; }
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
        #endregion

        #region LastModifiedDateTime
        [PXDBLastModifiedDateTime()]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
        #endregion

        #region Tstamp
        [PXDBTimestamp()]
        [PXUIField(DisplayName = "Tstamp")]
        public virtual byte[] Tstamp { get; set; }
        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
        #endregion
    }
}