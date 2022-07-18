using System;
using PX.Data;
using PX.Objects.SO;

namespace VFCustomizations.DAC
{
    [Serializable]
    [PXCacheName("LUMVFApisetupResult")]
    public class LUMVFApisetupResult : IBqlTable
    {
        #region Selected
        [PXBool()]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected { get; set; }
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
        #endregion

        #region ShipmentNbr
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Shipment Nbr", Visible = false, Enabled = false)]
        [PXDefault(typeof(SOShipment.shipmentNbr))]
        public virtual string ShipmentNbr { get; set; }
        public abstract class shipmentNbr : PX.Data.BQL.BqlString.Field<shipmentNbr> { }
        #endregion

        #region ShipmentType
        [PXDBString(1, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Shipment Type", Visible = false, Enabled = false)]
        [PXDefault(typeof(SOShipment.shipmentType))]
        public virtual string ShipmentType { get; set; }
        public abstract class shipmentType : PX.Data.BQL.BqlString.Field<shipmentType> { }
        #endregion

        #region LineNbr
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Line Nbr", Visible = false, Enabled = false)]
        [PXDefault]
        public virtual int? LineNbr { get; set; }
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        #endregion

        #region JobNo
        [PXDBString(10, IsFixed = true, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Job No")]
        [PXDefault]
        public virtual string JobNo { get; set; }
        public abstract class jobNo : PX.Data.BQL.BqlString.Field<jobNo> { }
        #endregion

        #region StartDateTime
        [PXDBDateAndTime()]
        [PXUIField(DisplayName = "Start DateTime")]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        public virtual DateTime? StartDateTime { get; set; }
        public abstract class startDateTime : PX.Data.BQL.BqlDateTime.Field<startDateTime> { }
        #endregion

        #region FinishDateTime
        [PXDBDateAndTime()]
        [PXUIField(DisplayName = "Finish DateTime")]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        public virtual DateTime? FinishDateTime { get; set; }
        public abstract class finishDateTime : PX.Data.BQL.BqlDateTime.Field<finishDateTime> { }
        #endregion

        #region TerminalID
        [PXDBString(10, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Terminal ID")]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        public virtual string TerminalID { get; set; }
        public abstract class terminalID : PX.Data.BQL.BqlString.Field<terminalID> { }
        #endregion

        #region SerialNo
        [PXDBString(30, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Serial Number")]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        public virtual string SerialNo { get; set; }
        public abstract class serialNo : PX.Data.BQL.BqlString.Field<serialNo> { }
        #endregion

        #region SetupReason
        [PXDBString(30, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Setup Reason")]
        public virtual string SetupReason { get; set; }
        public abstract class setupReason : PX.Data.BQL.BqlString.Field<setupReason> { }
        #endregion

        #region IsProcessed
        [PXDBBool()]
        [PXUIField(DisplayName = "Is Processed",Enabled = false)]
        public virtual bool? IsProcessed { get; set; }
        public abstract class isProcessed : PX.Data.BQL.BqlBool.Field<isProcessed> { }
        #endregion

        #region ProcessedDateTime
        [PXDBDateAndTime()]
        [PXUIField(DisplayName = "Processed Date Time", Enabled = false)]
        public virtual DateTime? ProcessedDateTime { get; set; }
        public abstract class processedDateTime : PX.Data.BQL.BqlDateTime.Field<processedDateTime> { }
        #endregion

        #region NoteID
        [PXNote()]
        public virtual Guid? NoteID { get; set; }
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
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