using HSNCustomizations.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.SO
{
    public class SOOrderEntryExt_HSN : PXGraphExtension<SOOrderEntry>
    {

        #region Events

        public virtual void _(Events.FieldUpdated<SOOrder.customerID> e, PXFieldUpdated baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);
            var setup = SelectFrom<LUMHSNSetup>.View.Select(Base).TopFirst;
            if ((setup?.EnablePromptMessageForCashSale ?? false))
            {
                var customerInfo = Customer.PK.Find(Base, (int?)e.NewValue);
                var attrCASHSALE = CSAnswers.PK.Find(Base, customerInfo.NoteID, "CASHSALE");
                if (attrCASHSALE?.Value == "1")
                    Base.Document.Ask(PXMessages.LocalizeFormatNoPrefix("This is a Cash Sale Customer, please alter the Order Type to “CS” accordingly."), MessageButtons.OK);
            }
        }

        #endregion
    }
}
