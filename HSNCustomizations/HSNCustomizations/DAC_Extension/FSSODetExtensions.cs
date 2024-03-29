using PX.Data;

namespace PX.Objects.FS
{
    public class FSSODetExt : PXCacheExtension<PX.Objects.FS.FSSODet>
    {
        #region Unbound Fields

        #region UsrDummyRMAReq
        [PXString]
        [PXUIField(DisplayName = "", Enabled = false, Visible = false)]
        [PXUnboundDefault(typeof(Search2<CS.CSAnswers.value, InnerJoin<IN.InventoryItem, On<IN.InventoryItem.noteID, Equal<CS.CSAnswers.refNoteID>,
                                                                                            And<CS.CSAnswers.attributeID, Equal<AppointmentEntry_Extension.rMAReqAttrID>>>>,
                                                             Where<IN.InventoryItem.inventoryID, Equal<Current<FSSODet.inventoryID>>>>),
                   PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<FSSODet.inventoryID>))]
        public virtual string UsrDummyRMAReq { get; set; }
        public abstract class usrDummyRMAReq : PX.Data.BQL.BqlString.Field<usrDummyRMAReq> { }
        #endregion

        #region UsrEquipSerialNbr
        [PXString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Target Serial Nbr.", Visible = false, Enabled = false)]
        [PXDBScalar(typeof(Search<FSEquipment.serialNumber, Where<FSEquipment.SMequipmentID, Equal<FSSODet.SMequipmentID>>>))]
        public virtual string UsrEquipSerialNbr { get; set; }
        public abstract class usrEquipSerialNbr : PX.Data.BQL.BqlString.Field<usrEquipSerialNbr> { }
        #endregion

        #region UsrEquipmentModel
        // [Phase - II] Add new Field in Equipment and Appointment
        [PXString(500, IsUnicode = true)]
        [PXUIField(DisplayName = "Equipment Model", Visible = false, Enabled = false)]
        public virtual string UsrEquipmentModel { get; set; }
        public abstract class usrEquipmentModel : PX.Data.BQL.BqlString.Field<usrEquipmentModel> { }
        #endregion

        #region UsrRegistrationNbr
        // [Phase - II] Add new Field in Equipment and Appointment
        [PXString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Registration Nbr.", Visible = false, Enabled = false)]
        public virtual string UsrRegistrationNbr { get; set; }
        public abstract class usrRegistrationNbr : PX.Data.BQL.BqlString.Field<usrRegistrationNbr> { }
        #endregion

        #endregion

        #region UsrRMARequired
        [PXDBBool()]
        [PXUIField(DisplayName = "RMA Required", Enabled = false)]
        [PXDefault(typeof(IIf<Where<FSSODetExt.usrDummyRMAReq, Equal<CS.string1>>, True, False>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<FSSODetExt.usrDummyRMAReq>))]
        public virtual bool? UsrRMARequired { get; set; }
        public abstract class usrRMARequired : PX.Data.BQL.BqlBool.Field<usrRMARequired> { }
        #endregion
    }
}