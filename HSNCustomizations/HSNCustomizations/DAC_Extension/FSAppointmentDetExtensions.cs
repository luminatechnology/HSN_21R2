using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;

namespace PX.Objects.FS
{
    public class FSAppointmentDetExt : PXCacheExtension<PX.Objects.FS.FSAppointmentDet>
    {
        #region Unbound Fields

        #region UsrDummyRMAReq
        [PXString]
        [PXUIField(Enabled = false, Visible = false)]
        [PXUnboundDefault(typeof(Search2<CS.CSAnswers.value, InnerJoin<IN.InventoryItem, On<IN.InventoryItem.noteID, Equal<CS.CSAnswers.refNoteID>,
                                                                                            And<CS.CSAnswers.attributeID, Equal<AppointmentEntry_Extension.rMAReqAttrID>>>>,
                                                             Where<IN.InventoryItem.inventoryID, Equal<Current<FSAppointmentDet.inventoryID>>>>),
                   PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<FSAppointmentDet.inventoryID>))]
        public virtual string UsrDummyRMAReq { get; set; }
        public abstract class usrDummyRMAReq : PX.Data.BQL.BqlString.Field<usrDummyRMAReq> { }
        #endregion

        #region UsrEquipSerialNbr
        [PXString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Target Serial Nbr.", Visible = false, Enabled = false)]
        [PXDBScalar(typeof(Search<FSEquipment.serialNumber, Where<FSEquipment.SMequipmentID, Equal<FSAppointmentDet.SMequipmentID>>>))]
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

        #region UsrEquipAttrAssetNbr
        public const string AssetNbr = "ASSETNBR";
        public class ASSETNBR_Attr : PX.Data.BQL.BqlString.Constant<ASSETNBR_Attr>
        {
            public ASSETNBR_Attr() : base(AssetNbr) { }
        }

        [PXString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "°]²£½s¸¹", Enabled = false)]
        [PXUnboundDefault(typeof(SelectFrom<CSAnswers>.InnerJoin<FSEquipment>.On<FSEquipment.noteID.IsEqual<CSAnswers.refNoteID>
                                                                          .And<CSAnswers.attributeID.IsEqual<ASSETNBR_Attr>>>
                                               .Where<FSEquipment.SMequipmentID.IsEqual<FSAppointmentDet.SMequipmentID.FromCurrent>>
                                               .SearchFor<CSAnswers.value>), 
                   PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBScalar(typeof(SelectFrom<CSAnswers>.InnerJoin<FSEquipment>.On<FSEquipment.noteID.IsEqual<CSAnswers.refNoteID>
                                                                           .And<CSAnswers.attributeID.IsEqual<ASSETNBR_Attr>>>
                                                .Where<FSEquipment.SMequipmentID.IsEqual<FSAppointmentDet.SMequipmentID>>.SearchFor<CSAnswers.value>))]
        [PXFormula(typeof(Default<FSAppointmentDet.SMequipmentID>))]
        public virtual string UsrEquipAttrAssetNbr { get; set; }
        public abstract class usrEquipAttrAssetNbr : PX.Data.BQL.BqlString.Field<usrEquipAttrAssetNbr> { }
        #endregion 

        #region UsrEquipAttrEngineer
        public const string Engineer = "ENGINEER";
        public class ENGINEER_Attr : PX.Data.BQL.BqlString.Constant<ENGINEER_Attr>
        {
            public ENGINEER_Attr() : base(Engineer) { }
        }

        [PXString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Equip. Engineer", Enabled = false)]
        [PXSelector(typeof(SelectFrom<CSAttributeDetail>.Where<CSAttributeDetail.attributeID.IsEqual<ENGINEER_Attr>>.SearchFor<CSAttributeDetail.valueID>),
                    DescriptionField = typeof(CSAttributeDetail.description))]
        [PXUnboundDefault(typeof(SelectFrom<CSAnswers>.InnerJoin<FSEquipment>.On<FSEquipment.noteID.IsEqual<CSAnswers.refNoteID>
                                                                          .And<CSAnswers.attributeID.IsEqual<ENGINEER_Attr>>>
                                               .Where<FSEquipment.SMequipmentID.IsEqual<FSAppointmentDet.SMequipmentID.FromCurrent>>
                                               .SearchFor<CSAnswers.value>),
                   PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBScalar(typeof(SelectFrom<CSAnswers>.InnerJoin<FSEquipment>.On<FSEquipment.noteID.IsEqual<CSAnswers.refNoteID>
                                                                           .And<CSAnswers.attributeID.IsEqual<ENGINEER_Attr>>>
                                                .Where<FSEquipment.SMequipmentID.IsEqual<FSAppointmentDet.SMequipmentID>>.SearchFor<CSAnswers.value>))]
        [PXFormula(typeof(Default<FSAppointmentDet.SMequipmentID>))]
        public virtual string UsrEquipAttrEngineer { get; set; }
        public abstract class usrEquipAttrEngineer : PX.Data.BQL.BqlString.Field<usrEquipAttrEngineer> { }
        #endregion 

        #region UsrEquipAttrSrvTerms
        public const string SrvTerms = "SERVTERM";
        public class SERVTERM_Attr : PX.Data.BQL.BqlString.Constant<SERVTERM_Attr>
        {
            public SERVTERM_Attr() : base(SrvTerms) { }
        }

        [PXString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Equip. Service Terms", Enabled = false, IsLocalizable = true)]
        [PXSelector(typeof(SelectFrom<CSAttributeDetail>.Where<CSAttributeDetail.attributeID.IsEqual<SERVTERM_Attr>>.SearchFor<CSAttributeDetail.valueID>),
                    DescriptionField = typeof(CSAttributeDetail.description))]
        [PXUnboundDefault(typeof(SelectFrom<CSAnswers>.InnerJoin<FSEquipment>.On<FSEquipment.noteID.IsEqual<CSAnswers.refNoteID>
                                                                          .And<CSAnswers.attributeID.IsEqual<SERVTERM_Attr>>>
                                               .Where<FSEquipment.SMequipmentID.IsEqual<FSAppointmentDet.SMequipmentID.FromCurrent>>
                                               .SearchFor<CSAnswers.value>),
                   PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBScalar(typeof(SelectFrom<CSAnswers>.InnerJoin<FSEquipment>.On<FSEquipment.noteID.IsEqual<CSAnswers.refNoteID>
                                                                           .And<CSAnswers.attributeID.IsEqual<SERVTERM_Attr>>>
                                                .Where<FSEquipment.SMequipmentID.IsEqual<FSAppointmentDet.SMequipmentID>>.SearchFor<CSAnswers.value>))]
        [PXFormula(typeof(Default<FSAppointmentDet.SMequipmentID>))]
        public virtual string UsrEquipAttrSrvTerms { get; set; }
        public abstract class usrEquipAttrSrvTerms : PX.Data.BQL.BqlString.Field<usrEquipAttrSrvTerms> { }
        #endregion 

        #endregion

        #region UsrRMARequired
        [PXDBBool()]
        [PXUIField(DisplayName = "RMA Required", Enabled = false)]
        [PXDefault(typeof(IIf<Where<FSAppointmentDetExt.usrDummyRMAReq, Equal<CS.string1>>, True, False>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<FSAppointmentDetExt.usrDummyRMAReq>))]
        public virtual bool? UsrRMARequired { get; set; }
        public abstract class usrRMARequired : PX.Data.BQL.BqlBool.Field<usrRMARequired> { }
        #endregion

        #region UsrIsDOA
        [PXDBBool()]
        [PXUIField(DisplayName = "Dead On Arrival")]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? UsrIsDOA { get; set; }
        public abstract class usrIsDOA : PX.Data.BQL.BqlBool.Field<usrIsDOA> { }
        #endregion
    }
}