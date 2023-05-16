using HSNHighcareCistomizations.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSNHighcareCistomizations.Graph
{
    public class HighcarePreferenceMaint : PXGraph<HighcarePreferenceMaint>
    {
        public PXSave<LUMHighcarePreference> Save;
        public PXCancel<LUMHighcarePreference> Cancel;

        public SelectFrom<LUMHighcarePreference>.View HighcarePreference;
    }
}
