using System;
using HSNCustomizations.Descriptor;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.FS;

namespace HSNCustomizations.DAC
{
    [Serializable]
    [PXCacheName("LUMWarrantyHistory")]
    public class LUMWarrantyHistory : IBqlTable
    {
        #region SMEquipmentID
        [PXDBInt(IsKey = true)]
        [PXDefault(typeof(FSEquipment.SMequipmentID))]
        [PXUIField(DisplayName = "SMEquipment ID", Visible = false)]
        public virtual int? SMEquipmentID { get; set; }
        public abstract class sMEquipmentID : PX.Data.BQL.BqlInt.Field<sMEquipmentID> { }
        #endregion

        #region LineNbr
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Line Nbr", Visible = false)]
        [PXDefault]
        public virtual int? LineNbr { get; set; }
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        #endregion

        #region WarrantyMonths
        [PXDBInt()]
        [PXUIField(DisplayName = "Warranty Months")]
        [PXDefault(0)]
        public virtual int? WarrantyMonths { get; set; }
        public abstract class warrantyMonths : PX.Data.BQL.BqlInt.Field<warrantyMonths> { }
        #endregion

        #region WarrantySerialNbr
        [PXDBString(50, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Warranty Serial Nbr")]
        [PXDefault]
        public virtual string WarrantySerialNbr { get; set; }
        public abstract class warrantySerialNbr : PX.Data.BQL.BqlString.Field<warrantySerialNbr> { }
        #endregion

        #region WarrantyStartDate
        [PXDBDate()]
        [PXUIField(DisplayName = "Warranty Start Date", Enabled = false)]
        [PXDefault(typeof(AccessInfo.businessDate))]
        public virtual DateTime? WarrantyStartDate { get; set; }
        public abstract class warrantyStartDate : PX.Data.BQL.BqlDateTime.Field<warrantyStartDate> { }
        #endregion

        #region WarrantyEndDate
        [PXDBDate()]
        [PXUIField(DisplayName = "Warranty End Date")]
        [PXDefault]
        public virtual DateTime? WarrantyEndDate { get; set; }
        public abstract class warrantyEndDate : PX.Data.BQL.BqlDateTime.Field<warrantyEndDate> { }
        #endregion

        #region Component
        [PXDBString(255, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Component")]
        [LUMCSAttributeListAttribute("CMPNT")]
        public virtual string Component { get; set; }
        public abstract class component : PX.Data.BQL.BqlString.Field<component> { }
        #endregion

        #region Remark
        [PXDBString(500, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Remark")]
        public virtual string Remark { get; set; }
        public abstract class remark : PX.Data.BQL.BqlString.Field<remark> { }
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