using PX.Data;
using PX.Data.BQL.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VFCustomizations.DAC;

namespace VFCustomizations.Graph
{
    public class LUMVFPreferenceMaint : PXGraph<LUMVFPreferenceMaint>
    {
        public PXSave<LUMVerifonePreference> Save;
        public PXCancel<LUMVerifonePreference> Cancel;
        public SelectFrom<LUMVerifonePreference>.View VFPreference;
    }
}
