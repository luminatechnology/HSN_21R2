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
using PX.Objects.CR;
using HSNCustomizations.Descriptor;

namespace PX.Objects.SO
{
    public class SOOrderEntryExt_HSN : PXGraphExtension<SOOrderEntry>
    {
        public SelectFrom<Contact>
                .InnerJoin<BAccount>.On<Contact.bAccountID.IsEqual<BAccount.bAccountID>>
                .Where<Contact.bAccountID.IsEqual<SOOrder.customerID.FromCurrent>>.View StandardContactSelector;

        #region Events

        [PXMergeAttributes(Method = MergeMethod.Replace)]
        [PXUIEnabled(typeof(Where<SOOrder.customerID, IsNotNull>))]
        [ContactRaw(typeof(SOOrder.customerID), null, null, null, new[]
        {
             typeof(Contact.displayName),
             typeof(ContactExtensions.usrLocationID),
             typeof(Contact.salutation),
             typeof(Contact.fullName),
             typeof(BAccount.acctCD),
             typeof(Contact.eMail),
             typeof(Contact.phone1),
             typeof(Contact.contactType)
            }, new string[]
        {
            "Contact",
                        "Location",
                        "Job Title",
                        "Account Name",
                        "Business Account",
                        "Email",
                        "Phone GG",
                        "Type"
            }, WithContactDefaultingByBAccount = true)]
        public virtual void _(Events.CacheAttached<SOOrder.contactID> e) { }

        public virtual void _(Events.FieldUpdated<SOOrder.customerID> e, PXFieldUpdated baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);
            var setup = SelectFrom<LUMHSNSetup>.View.Select(Base).TopFirst;
            if ((setup?.EnablePromptMessageForCashSale ?? false))
            {
                var customerInfo = Customer.PK.Find(Base, (int?)e.NewValue);
                var attrCASHSALE = CSAnswers.PK.Find(Base, customerInfo.NoteID, "CASHSALE");
                if (attrCASHSALE?.Value == "1" && Base.Document.Current?.OrderType != "CS")
                    Base.Document.Ask(PXMessages.LocalizeFormatNoPrefix("This is a Cash Sale Customer, please alter the Order Type to “CS” accordingly."), MessageButtons.OK);
            }
        }

        #endregion
    }
}
