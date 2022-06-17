using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.FS
{
    public class FSManufacturerModelExtension : PXCacheExtension<FSManufacturerModel>
    {
        [PXDBString(20, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCC")]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        public virtual string ManufacturerModelCD { get; set; }
    }
}
