using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;

namespace PX.Objects.FS
{
    public class FSEquipmentExtension : PXCacheExtension<FSEquipment>
    {
        #region UsrEquipmentModel
        // [Phase - II] Add new Field in Equipment and Appointment
        [PXDBString(500, IsUnicode = true)]
        [PXUIField(DisplayName = "Equipment Model", Visible = false)]
        public virtual string UsrEquipmentModel { get; set; }
        public abstract class usrEquipmentModel : PX.Data.BQL.BqlString.Field<usrEquipmentModel> { }
        #endregion

        #region Unbound Fields

        #region UsrEquipAttrAssetNbr
        [PXString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Asset Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDBScalar(typeof(SelectFrom<CSAnswers>.Where<CSAnswers.refNoteID.IsEqual<FSEquipment.noteID>
                                                       .And<CSAnswers.attributeID.IsEqual<FSAppointmentDetExt.ASSETNBR_Attr>>>.SearchFor<CSAnswers.value>))]
        public virtual string UsrEquipAttrAssetNbr { get; set; }
        public abstract class usrEquipAttrAssetNbr : PX.Data.BQL.BqlString.Field<usrEquipAttrAssetNbr> { }
        #endregion 

        #endregion
    }
}
