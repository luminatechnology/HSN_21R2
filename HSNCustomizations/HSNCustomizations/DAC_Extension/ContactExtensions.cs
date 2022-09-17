using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.CR
{
    public class ContactExtensions : PXCacheExtension<Contact>
    {
        // [Phase II] - Enable Customer Contacts to be Selected by Customer Locations
        #region UsrLocationID
        [PXDBInt]
        [PXUIField(DisplayName = "Location")]
        [PXSelector(typeof(Search<Location.locationID,
                           Where<Location.bAccountID, Equal<Current<Contact.bAccountID>>>>),
                    typeof(Location.locationCD),
                    typeof(Location.descr),
                    SubstituteKey = typeof(Location.locationCD),
                    DescriptionField = typeof(Location.descr))]
        public virtual int? UsrLocationID { get; set; }
        public abstract class usrLocationID : PX.Data.BQL.BqlInt.Field<usrLocationID> { }
        #endregion
    }
}
