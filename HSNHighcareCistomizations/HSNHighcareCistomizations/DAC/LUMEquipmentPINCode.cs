using System;
using PX.Data;

namespace HSNHighcareCistomizations.DAC
{
    [Serializable]
    [PXCacheName("LUMEquipmentPINCode")]
    public class LUMEquipmentPINCode : IBqlTable
    {

        public static class FK
        {
            public class Equipment : PX.Objects.FS.FSEquipment.PK.ForeignKeyOf<LUMEquipmentPINCode>.By<sMEquipmentID> { }
        }
        #region SMEquipmentID
        [PXDBInt(IsKey = true)]
        [PXDefault(typeof(PX.Objects.FS.FSEquipment.SMequipmentID))]
        [PXParent(typeof(FK.Equipment))]
        [PXUIField(DisplayName = "SMEquipmentID")]
        public virtual int? SMEquipmentID { get; set; }
        public abstract class sMEquipmentID : PX.Data.BQL.BqlInt.Field<sMEquipmentID> { }
        #endregion

        #region Pincode
        [PXDBString(100, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Pincode")]
        public virtual string Pincode { get; set; }
        public abstract class pincode : PX.Data.BQL.BqlString.Field<pincode> { }
        #endregion

        #region HighcarePriceClass
        [PXString]
        [PXDefault(typeof(Search<LUMCustomerPINCode.cPriceClassID,
                           Where<LUMCustomerPINCode.pin, Equal<Current<LUMEquipmentPINCode.pincode>>,
                             And<LUMCustomerPINCode.bAccountID, Equal<Current<PX.Objects.FS.FSEquipment.ownerID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Highcare Price Class", Enabled = false)]
        public virtual string CPriceClassID { get; set; }
        public abstract class cPriceClassID : PX.Data.BQL.BqlString.Field<cPriceClassID> { }
        #endregion

        #region IsActive
        [PXBool]
        [PXDefault]
        [PXUIField(DisplayName = "Active", Enabled = false)]
        public virtual bool? IsActive { get; set; }
        public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
        #endregion

        #region StartDate
        [PXDate()]
        [PXDefault]
        [PXUIField(DisplayName = "Start Date", Enabled = false)]
        public virtual DateTime? StartDate { get; set; }
        public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
        #endregion

        #region EndDate
        [PXDate]
        [PXDefault]
        [PXUIField(DisplayName = "End Date", Enabled = false)]
        public virtual DateTime? EndDate { get; set; }
        public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
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