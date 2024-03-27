using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.AR
{
    public class ARInvoiceDiscountDetailExtension : PXCacheExtension<ARInvoiceDiscountDetail>
    {
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "External Discount Code")]
        [PXMergeAttributes(Method = MergeMethod.Replace)]
        public virtual string ExtDiscCode { get; set; }
    }
}
