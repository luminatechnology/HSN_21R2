using HSNCustomizations.DAC;
using HSNCustomizations.Descriptor;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.IN;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.SO
{
    public class SOOrderEntryExt_HSN : PXGraphExtension<SOOrderEntry>
    {
        public SelectFrom<Contact>
                .InnerJoin<BAccount>.On<Contact.bAccountID.IsEqual<BAccount.bAccountID>>
                .Where<Contact.bAccountID.IsEqual<SOOrder.customerID.FromCurrent>>.View StandardContactSelector;

        #region Delegate Actions
        public delegate IEnumerable CreateShipmentIssueDelegate(PXAdapter adapter, [PXDate] DateTime? shipDate, [PXInt] int? siteID);
        [PXOverride]
        public IEnumerable CreateShipmentIssue(PXAdapter adapter, [PXDate] DateTime? shipDate, [PXInt] int? siteID, CreateShipmentIssueDelegate baseMethod)
        {
            ///<remarks>
            /// The system should verify that the Details tab contains a minimum of one inventory item with its type (InventoryItem.ItemType) marked as "Service" 
            /// and its amount is greater than 0 at the time the 'Create Shipment' action is executed. 
            ///</remarks>
            List<SOLine> lines = Base.Transactions.Select().RowCast<SOLine>().Where(w => InventoryItem.PK.Find(Base, w.InventoryID)?.ItemType == INItemTypes.ServiceItem).ToList();

            if (Base.soordertype.Current?.GetExtension<SOOrderTypeExt>().UsrRequireAtLeastOneSrvItemInSO == true &&
                lines.Exists(f => f.CuryLineAmt > 0m) == false)
            {
                throw new PXSetPropertyException<SOLine.inventoryID>(HSNMessages.SrvItemWithNoAmt);
            }

            return baseMethod(adapter, shipDate, siteID);
        }
        #endregion

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
            var isCASHSALE = false;
            if ((setup?.EnablePromptMessageForCashSale ?? false))
            {
                var customerInfo = Customer.PK.Find(Base, (int?)e.NewValue);
                var attrCASHSALE = CSAnswers.PK.Find(Base, customerInfo.NoteID, "CASHSALE");
                if (attrCASHSALE?.Value == "1")
                    isCASHSALE = true;
                // 沒有設定Attbiute 則需判斷是不是Defualt = true
                else
                {
                    var attrGroup = CSAttributeGroup.PK.Find(Base, "CASHSALE", customerInfo.CustomerClassID, "PX.Objects.AR.Customer");
                    if (attrGroup?.DefaultValue == "True")
                        isCASHSALE = true;
                }
                if (isCASHSALE && Base.Document.Current?.OrderType != "CS")
                    Base.Document.Ask(PXMessages.LocalizeFormatNoPrefix("This is a Cash Sale Customer, please alter the Order Type to “CS” accordingly."), MessageButtons.OK);
            }
        }
        #endregion
    }
}
