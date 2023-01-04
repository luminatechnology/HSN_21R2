using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.SO;

namespace VFCustomizations.DAC_Extension
{
    public class SOShipmentExt : PXCacheExtension<SOShipment>
    {
        #region UsrDeliverDate
        [PXDBDateAndTime(UseTimeZone = false, PreserveTime = true, DisplayMask = "g")]
        [PXUIField(DisplayName = "Deliver Date")]
        public virtual DateTime? UsrDeliverDate { get; set; }
        public abstract class usrDeliverDate : PX.Data.BQL.BqlDateTime.Field<usrDeliverDate> { }
        #endregion

    }
}
