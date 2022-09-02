using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
