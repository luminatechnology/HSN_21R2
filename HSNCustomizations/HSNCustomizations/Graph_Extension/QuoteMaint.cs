using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using HSNCustomizations.DAC;
using HSNCustomizations.Descriptor;
using PX.Objects.CR.Extensions.CRCreateInvoice;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Common;
using PX.Objects.CS;
using PX.Objects.Common.Discount;
using PX.Objects.TX;

namespace PX.Objects.CR
{
    public class QuoteMaint_Extension : PXGraphExtension<QuoteMaintExt, QuoteMaint>
    {
        public const string QuoteMYRptID = "LM604500";
        public const string QuoteMY2RptID = "LM604501";

        #region Selects
        public SelectFrom<LUMHSNSetup>.View HSNSetupView;
        public SelectFrom<LUMOpprTermCond>.Where<LUMOpprTermCond.quoteID.IsEqual<CRQuote.quoteID.FromCurrent>>.OrderBy<LUMOpprTermCond.sortOrder.Asc>.View TermsConditions;
        #endregion

        #region Override Methods
        public override void Initialize()
        {
            base.Initialize();

            this.reportFolder.AddMenuAction(printQuoteMY, nameof(Base.PrintQuote), true);
            this.reportFolder.AddMenuAction(printQuoteMY2, nameof(PrintQuoteMY), true);
        }
        #endregion

        #region Delegate Methods
        public delegate void PersistDelegate();
        [PXOverride]
        public void Persist(PersistDelegate baseMethod)
        {
            var quote = Base.QuoteCurrent.Current;

            if (quote != null && HSNSetupView.Select().TopFirst?.EnableOpportunityEnhance == true)
            {
                if (quote.ExpirationDate != null)// && Base.CurrentOpportunity.Select().TopFirst?.GetExtension<CROpportunityExt>().UsrValidityDate == null)
                {
                    PXUpdate<Set<CROpportunityExt.usrValidityDate, Required<CRQuote.expirationDate>>,
                             CROpportunity,
                             Where<CROpportunity.opportunityID, Equal<Required<CRQuote.opportunityID>>,
                                   And<CROpportunity.defQuoteID, Equal<Required<CRQuote.quoteID>>>>>.Update(Base, quote.ExpirationDate, quote.OpportunityID, quote.QuoteID);
                }

                if (TermsConditions.Select().Count <= 0)
                {
                    CopyRecordFromOpportunity();
                }
            }

            baseMethod();
        }
        #endregion

        #region Actions
        public PXAction<CRQuote> reportFolder;
        [PXButton(MenuAutoOpen = true)]
        [PXUIField(DisplayName = "Reports")]
        protected virtual IEnumerable ReportFolder(PXAdapter adapter) => adapter.Get();

        public PXAction<CRQuote> printQuoteMY;
        [PXButton()]
        [PXUIField(DisplayName = "Print Quote-MY", MapEnableRights = PXCacheRights.Select)]
        protected virtual void PrintQuoteMY()
        {
            if (Base.Quote.Current != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    [nameof(CRQuote.OpportunityID)] = Base.Quote.Current.OpportunityID,
                    [nameof(CRQuote.QuoteNbr)] = Base.Quote.Current.QuoteNbr
                };

                throw new PXReportRequiredException(parameters, QuoteMYRptID, QuoteMYRptID) { Mode = PXBaseRedirectException.WindowMode.New };
            }
        }

        public PXAction<CRQuote> printQuoteMY2;
        [PXButton()]
        [PXUIField(DisplayName = "Print Quote-MY 2", MapEnableRights = PXCacheRights.Select)]
        protected virtual void PrintQuoteMY2()
        {
            if (Base.Quote.Current != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    [nameof(CRQuote.OpportunityID)] = Base.Quote.Current.OpportunityID,
                    [nameof(CRQuote.QuoteNbr)] = Base.Quote.Current.QuoteNbr
                };

                throw new PXReportRequiredException(parameters, QuoteMY2RptID, QuoteMY2RptID) { Mode = PXBaseRedirectException.WindowMode.New };
            }
        }



        #endregion

        #region Event Handlers
        protected void _(Events.RowSelected<CRQuote> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            var row = e.Row;

            if (row == null) { return; }

            TermsConditions.AllowSelect = HSNSetupView.Select().TopFirst?.EnableOpportunityEnhance ?? false;

            Base.printQuote.SetEnabled((row?.Status == CRQuoteStatusAttribute.Approved || row?.Status == CRQuoteStatusAttribute.Sent || row?.Status == CRQuoteStatusAttribute.Draft) && !string.IsNullOrEmpty(row?.OpportunityID));

            printQuoteMY.SetEnabled(Base.printQuote.GetEnabled());
            printQuoteMY2.SetEnabled(Base.printQuote.GetEnabled());

            printQuoteMY.SetVisible(Base.Shipping_Address?.Current?.CountryID == "MY");
            printQuoteMY2.SetVisible(Base.Shipping_Address?.Current?.CountryID == "MY");
        }

        protected void _(Events.FieldVerifying<LUMOpprTermCond.sortOrder> e)
        {
            ///<remarks> Add the following verification to avoid that this field is not numbered in order or maintained incorrectly.</remarks>
            if (TermsConditions.Select().RowCast<LUMOpprTermCond>().Where(x => x.SortOrder == (int)e.NewValue).Count() > 0)
            {
                throw new PXSetPropertyException<LUMOpprTermCond.sortOrder>(HSNMessages.DuplicSortOrder);
            }
            if (SelectFrom<LUMOpprTermCond>.Where<LUMOpprTermCond.quoteID.IsEqual<@P.AsGuid>>.AggregateTo<Max<LUMOpprTermCond.sortOrder>>.View.Select(Base, Base.QuoteCurrent.Current?.QuoteID)?.TopFirst.SortOrder > (int)e.NewValue)
            {
                throw new PXSetPropertyException<LUMOpprTermCond.sortOrder>(HSNMessages.SortOrdMustGreater);
            }
        }
        #endregion

        #region Methods
        public virtual void CopyRecordFromOpportunity()
        {
            foreach (LUMOpprTermCond row in SelectFrom<LUMOpprTermCond>.Where<LUMOpprTermCond.opportunityID.IsEqual<@P.AsString>>.View.Select(Base, Base.Quote.Current.OpportunityID))
            {
                LUMOpprTermCond termCond = new LUMOpprTermCond()
                {
                    IsActive = row.IsActive,
                    SortOrder = row.SortOrder,
                    Title = row.Title,
                    Definition = row.Definition
                };

                termCond.OpportunityID = null;

                TermsConditions.Insert(termCond);
            }
        }
        #endregion
    }
    /// <summary> [All-Phase2] ½Æ»sSales Qutoes ¤¤ªº Convert to Invoice -> Converyt to DebitMemo </summary>
    public class LUMCRCreateInvoiceExt : CRCreateInvoice<QuoteMaint.Discount, QuoteMaint, CRQuote>
    {
        #region Initialization

        public static bool IsActive() => IsExtensionActive();

        #endregion

        #region Events

        public virtual void _(Events.RowSelected<CRQuote> e)
        {
            CRQuote row = e.Row as CRQuote;
            if (row == null)
                return;

            bool hasProducts = Base.Products.SelectSingle() != null;

            var products = Base.Products.View.SelectMultiBound(new object[] { row }).RowCast<CROpportunityProducts>();

            bool allProductsHasNoInventoryID = products.Any(_ => _.InventoryID == null) && !products.Any(_ => _.InventoryID != null);

            CreateDebitMemo
                .SetEnabled(hasProducts
                    && !allProductsHasNoInventoryID
                    && e.Row.BAccountID != null);
        }

        #endregion

        public PXAction<CRQuote> CreateDebitMemo;

        [PXUIField(DisplayName = "Convert to Debit memo", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
        [PXButton(Category = "Record Creation", DisplayOnMainToolbar = false)]
        public virtual IEnumerable createDebitMemo(PXAdapter adapter)
        {
            Document masterEntity = base.DocumentView.Current;

            Customer customer = PXSelect<
                    Customer,
                Where<
                    Customer.bAccountID, Equal<Current<Document.bAccountID>>>>
                .SelectSingleBound(Base, new object[] { DocumentView.Current });
            if (customer == null)
            {
                throw new PXException(Messages.ProspectNotCustomer);
            }

            var quouteID = this.DocumentView.GetValueExt<Document.quoteID>(masterEntity);
            var nonStockItems = PXSelectJoin<CROpportunityProducts,
                InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<CROpportunityProducts.inventoryID>>>,
                Where<InventoryItem.stkItem, Equal<False>,
                    And<CROpportunityProducts.quoteID, Equal<Required<Document.quoteID>>>>>.
                SelectMultiBound(Base, new object[] { DocumentView.Current }, quouteID);
            if (nonStockItems.Any_() == false)
            {
                throw new PXException(Messages.InvoiceHasOnlyNonStockLines);
            }

            if (masterEntity.BAccountID != null)
            {
                BAccount baccount = PXSelectJoin<
                        BAccount,
                        LeftJoin<Contact,
                            On<Contact.contactID, Equal<BAccount.defContactID>>>,
                        Where<
                            BAccount.bAccountID, Equal<Current<Document.bAccountID>>>>
                        .SelectSingleBound(Base, new object[] { DocumentView.Current });
                if (baccount.Type == BAccountType.VendorType || baccount.Type == BAccountType.ProspectType)
                {
                    WebDialogResult result = DocumentView.View.Ask(masterEntity, Messages.AskConfirmation, Messages.InvoiceRequiredConvertBusinessAccountToCustomerAccount, MessageButtons.YesNo, MessageIcon.Question);
                    if (result == WebDialogResult.Yes)
                    {
                        PXLongOperation.StartOperation(this, () => ConvertToCustomerAccount(baccount, masterEntity));
                    }

                    return adapter.Get();
                }
            }

            if (customer != null)
            {
                if (CreateInvoicesParams.View.Answer == WebDialogResult.None)
                {
                    CreateInvoicesParams.Cache.Clear();
                    CreateInvoicesParams.Cache.Insert();
                }

                if (this.CreateInvoicesParams.AskExtFullyValid((graph, viewName) => { }, DialogAnswerType.Positive))
                {
                    Base.Actions.PressSave();

                    var graph = PXGraph.CreateInstance<QuoteMaint>();

                    PXLongOperation.StartOperation(Base, delegate ()
                    {
                        this.DoCreateDebitMemo();
                    });
                }
            }

            return adapter.Get();
        }

        public override CRQuote GetQuoteForWorkflowProcessing()
            => Base.QuoteCurrent.Current;

        protected virtual void DoCreateDebitMemo()
        {
            CreateInvoicesFilter filter = this.CreateInvoicesParams.Current;
            Document masterEntity = this.DocumentView.Current;

            if (filter == null || masterEntity == null)
                return;

            bool recalcAny =
                filter.RecalculatePrices == true
                || filter.RecalculateDiscounts == true
                || filter.OverrideManualDiscounts == true
                || filter.OverrideManualDocGroupDiscounts == true
                || filter.OverrideManualPrices == true;

            ARInvoiceEntry docgraph = PXGraph.CreateInstance<ARInvoiceEntry>();

            Customer customer = (Customer)PXSelect<Customer, Where<Customer.bAccountID, Equal<Current<Document.bAccountID>>>>.Select(Base);
            docgraph.customer.Current = customer;

            ARInvoice invoice = new ARInvoice();
            invoice.DocType = ARDocType.DebitMemo;
            invoice.CuryID = masterEntity.CuryID;
            invoice.CuryInfoID = masterEntity.CuryInfoID;
            invoice.DocDate = masterEntity.CloseDate;
            invoice.Hold = true;
            invoice.BranchID = masterEntity.BranchID;
            invoice.CustomerID = masterEntity.BAccountID;
            invoice = PXCache<ARInvoice>.CreateCopy(docgraph.Document.Insert(invoice));

            invoice.TermsID = masterEntity.TermsID ?? customer.TermsID;
            invoice.InvoiceNbr = masterEntity.OpportunityID;
            invoice.DocDesc = masterEntity.Subject;
            invoice.CustomerLocationID = masterEntity.LocationID ?? customer.DefLocationID;

            #region Shipping Details

            CRShippingContact _crShippingContact = Base.Caches[typeof(CRShippingContact)].Current as CRShippingContact;
            ARShippingContact _shippingContact = docgraph.Shipping_Contact.Select();
            if (_shippingContact != null && _crShippingContact != null)
            {
                _crShippingContact.RevisionID = _crShippingContact.RevisionID ?? _shippingContact.RevisionID;
                if (_shippingContact.RevisionID != _crShippingContact.RevisionID)
                {
                    _crShippingContact.IsDefaultContact = false;
                }
                _crShippingContact.BAccountContactID = _crShippingContact.BAccountContactID ?? _shippingContact.CustomerContactID;
                ContactAttribute.CopyRecord<ARInvoice.shipContactID>(docgraph.Document.Cache, invoice, _crShippingContact, true);
            }

            CRShippingAddress _crShippingAddress = Base.Caches[typeof(CRShippingAddress)].Current as CRShippingAddress;
            ARShippingAddress _shippingAddress = docgraph.Shipping_Address.Select();
            if (_shippingAddress != null && _crShippingAddress != null)
            {
                _crShippingAddress.RevisionID = _crShippingAddress.RevisionID ?? _shippingAddress.RevisionID;
                if (_shippingAddress.RevisionID != _crShippingAddress.RevisionID)
                {
                    _crShippingAddress.IsDefaultAddress = false;
                }
                _crShippingAddress.BAccountAddressID = _crShippingAddress.BAccountAddressID ?? _shippingAddress.CustomerAddressID;
                AddressAttribute.CopyRecord<ARInvoice.shipAddressID>(docgraph.Document.Cache, invoice, _crShippingAddress, true);
            }

            #endregion

            #region Billing Details

            CRBillingContact _crBillingContact = Base.Caches[typeof(CRBillingContact)].Current as CRBillingContact;
            ARContact _arContactContact = docgraph.Billing_Contact.Select();
            if (_arContactContact != null && _crBillingContact != null)
            {
                _crBillingContact.RevisionID = _crBillingContact.RevisionID ?? _arContactContact.RevisionID;
                if (_arContactContact.RevisionID != _crBillingContact.RevisionID)
                {
                    _crBillingContact.OverrideContact = true;
                }
                _crBillingContact.BAccountContactID = _crBillingContact.BAccountContactID ?? _arContactContact.BAccountContactID;
                ContactAttribute.CopyRecord<ARInvoice.billContactID>(docgraph.Document.Cache, invoice, _crBillingContact, true);
            }

            CRBillingAddress _crBillingAddress = Base.Caches[typeof(CRBillingAddress)].Current as CRBillingAddress;
            ARAddress _arAddressAddress = docgraph.Billing_Address.Select();
            if (_arAddressAddress != null && _crBillingAddress != null)
            {
                _crBillingAddress.RevisionID = _crBillingAddress.RevisionID ?? _arAddressAddress.RevisionID;
                if (_arAddressAddress.RevisionID != _crBillingAddress.RevisionID)
                {
                    _crBillingAddress.OverrideAddress = true;
                }
                _crBillingAddress.BAccountAddressID = _crBillingAddress.BAccountAddressID ?? _arAddressAddress.BAccountAddressID;
                AddressAttribute.CopyRecord<ARInvoice.billAddressID>(docgraph.Document.Cache, invoice, _crBillingAddress, true);
            }

            #endregion

            if (masterEntity.ManualTotalEntry == true)
                recalcAny = false;

            #region Tax Info

            if (masterEntity.TaxZoneID != null)
            {
                invoice.TaxZoneID = masterEntity.TaxZoneID;
                if (!recalcAny && masterEntity.ManualTotalEntry != true)
                    TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(docgraph.Transactions.Cache, null, TaxCalc.ManualCalc);
            }
            invoice.TaxCalcMode = masterEntity.TaxCalcMode;
            invoice.ExternalTaxExemptionNumber = masterEntity.ExternalTaxExemptionNumber;
            invoice.AvalaraCustomerUsageType = masterEntity.AvalaraCustomerUsageType;

            invoice.ProjectID = masterEntity.ProjectID;
            invoice = docgraph.Document.Update(invoice);

            #endregion

            #region Relation: Opportunity -> Invoice

            var opportunity = (CROpportunity)PXSelectReadonly<CROpportunity, Where<CROpportunity.opportunityID, Equal<Current<Document.opportunityID>>>>.Select(Base);
            var relation = docgraph.RelationsLink.Insert();
            relation.RefNoteID = invoice.NoteID;
            relation.RefEntityType = invoice.GetType().FullName;
            relation.Role = CRRoleTypeList.Source;
            relation.TargetType = CRTargetEntityType.CROpportunity;
            relation.TargetNoteID = opportunity.NoteID;
            relation.DocNoteID = opportunity.NoteID;
            relation.EntityID = opportunity.BAccountID;
            relation.ContactID = opportunity.ContactID;
            docgraph.RelationsLink.Update(relation);

            #endregion

            #region Relation: Primary/Current Quote (Source) -> Invoice

            var quote = (CRQuote)PXSelectReadonly<CRQuote, Where<CRQuote.quoteID, Equal<Current<Document.quoteID>>>>.Select(Base);
            if (quote != null)
            {
                var invoiceQuoteRelation = docgraph.RelationsLink.Insert();
                invoiceQuoteRelation.RefNoteID = invoice.NoteID;
                invoiceQuoteRelation.RefEntityType = invoice.GetType().FullName;
                invoiceQuoteRelation.Role = CRRoleTypeList.Source;
                invoiceQuoteRelation.TargetType = CRTargetEntityType.CRQuote;
                invoiceQuoteRelation.TargetNoteID = quote.NoteID;
                invoiceQuoteRelation.DocNoteID = quote.NoteID;
                invoiceQuoteRelation.EntityID = quote.BAccountID;
                invoiceQuoteRelation.ContactID = quote.ContactID;
                docgraph.RelationsLink.Update(invoiceQuoteRelation);
            }

            #endregion

            if (masterEntity.ManualTotalEntry == true)
            {
                ARTran tran = new ARTran();
                tran.Qty = 1;
                tran.CuryUnitPrice = masterEntity.CuryAmount;
                tran = docgraph.Transactions.Insert(tran);
                if (tran != null)
                {
                    tran.CuryDiscAmt = masterEntity.CuryDiscTot;

                    using (new PXLocaleScope(customer.LocaleName))
                    {
                        tran.TranDesc = PXMessages.LocalizeNoPrefix(Messages.ManualAmount);
                    }
                }
                tran = docgraph.Transactions.Update(tran);
            }
            else
            {
                foreach (PXResult<CROpportunityProducts, InventoryItem> res in PXSelectJoin<CROpportunityProducts,
                    LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<CROpportunityProducts.inventoryID>>>,
                    Where<CROpportunityProducts.quoteID, Equal<Current<Document.quoteID>>,
                    And<InventoryItem.stkItem, Equal<False>>>>
                .Select(Base))
                {
                    CROpportunityProducts product = (CROpportunityProducts)res;
                    InventoryItem item = (InventoryItem)res;

                    ARTran tran = new ARTran();
                    tran = docgraph.Transactions.Insert(tran);
                    if (tran != null)
                    {
                        tran.InventoryID = product.InventoryID;
                        using (new PXLocaleScope(customer.LocaleName))
                        {
                            tran.TranDesc = PXDBLocalizableStringAttribute.GetTranslation(Base.Caches[typeof(CROpportunityProducts)],
                                                          product, typeof(CROpportunityProducts.descr).Name, Base.Culture.Name);
                        }

                        tran.Qty = product.Quantity;
                        tran.UOM = product.UOM;
                        tran.CuryUnitPrice = product.CuryUnitPrice;
                        tran.IsFree = product.IsFree;
                        tran.SortOrder = product.SortOrder;

                        tran.CuryTranAmt = product.CuryAmount;
                        tran.TaxCategoryID = product.TaxCategoryID;
                        tran.ProjectID = product.ProjectID;
                        tran.TaskID = product.TaskID;
                        tran.CostCodeID = product.CostCodeID;

                        if (filter.RecalculatePrices != true)
                        {
                            tran.ManualPrice = true;
                        }
                        else
                        {
                            if (filter.OverrideManualPrices != true)
                                tran.ManualPrice = product.ManualPrice;
                            else
                                tran.ManualPrice = false;
                        }

                        if (filter.RecalculateDiscounts != true)
                        {
                            tran.ManualDisc = true;
                        }
                        else
                        {
                            if (filter.OverrideManualDiscounts != true)
                                tran.ManualDisc = product.ManualDisc;
                            else
                                tran.ManualDisc = false;
                        }

                        tran.CuryDiscAmt = product.CuryDiscAmt;
                        tran.DiscAmt = product.DiscAmt;
                        tran.DiscPct = product.DiscPct;

                        if (item.Commisionable.HasValue)
                        {
                            tran.Commissionable = item.Commisionable;
                        }
                    }

                    tran = docgraph.Transactions.Update(tran);

                    PXNoteAttribute.CopyNoteAndFiles(Base.Caches[typeof(CROpportunityProducts)], product, docgraph.Transactions.Cache, tran, Base.Caches[typeof(CRSetup)].Current as PXNoteAttribute.IPXCopySettings);
                }
            }
            PXNoteAttribute.CopyNoteAndFiles(Base.Caches[typeof(CRQuote)], masterEntity.Base, docgraph.Document.Cache, invoice, Base.Caches[typeof(CRSetup)].Current as PXNoteAttribute.IPXCopySettings);

            //Skip all customer dicounts
            if (filter.RecalculateDiscounts != true && filter.OverrideManualDiscounts != true)
            {
                var discounts = new Dictionary<string, ARInvoiceDiscountDetail>();
                foreach (ARInvoiceDiscountDetail discountDetail in docgraph.ARDiscountDetails.Select())
                {
                    docgraph.ARDiscountDetails.SetValueExt<ARInvoiceDiscountDetail.skipDiscount>(discountDetail, true);
                    string key = discountDetail.Type + ':' + discountDetail.DiscountID + ':' + discountDetail.DiscountSequenceID;
                    discounts.Add(key, discountDetail);
                }

                foreach (PX.Objects.Extensions.Discount.Discount discount in Base1.Discounts.Select())
                {
                    CROpportunityDiscountDetail discountDetail = discount.Base as CROpportunityDiscountDetail;
                    ARInvoiceDiscountDetail detail;
                    string key = discountDetail.Type + ':' + discountDetail.DiscountID + ':' + discountDetail.DiscountSequenceID;
                    if (discounts.TryGetValue(key, out detail))
                    {
                        docgraph.ARDiscountDetails.SetValueExt<ARInvoiceDiscountDetail.skipDiscount>(detail, false);
                        if (discountDetail.IsManual == true && discountDetail.Type == DiscountType.Document)
                        {
                            docgraph.ARDiscountDetails.SetValueExt<ARInvoiceDiscountDetail.extDiscCode>(detail, discountDetail.ExtDiscCode);
                            docgraph.ARDiscountDetails.SetValueExt<ARInvoiceDiscountDetail.description>(detail, discountDetail.Description);
                            docgraph.ARDiscountDetails.SetValueExt<ARInvoiceDiscountDetail.isManual>(detail, discountDetail.IsManual);
                            docgraph.ARDiscountDetails.SetValueExt<ARInvoiceDiscountDetail.curyDiscountAmt>(detail, discountDetail.CuryDiscountAmt);
                        }
                    }
                    else
                    {
                        detail = (ARInvoiceDiscountDetail)docgraph.ARDiscountDetails.Cache.CreateInstance();
                        detail.Type = discountDetail.Type;
                        detail.DiscountID = discountDetail.DiscountID;
                        detail.DiscountSequenceID = discountDetail.DiscountSequenceID;
                        detail.ExtDiscCode = discountDetail.ExtDiscCode;
                        detail.Description = discountDetail.Description;
                        detail = (ARInvoiceDiscountDetail)docgraph.ARDiscountDetails.Cache.Insert(detail);
                        if (discountDetail.IsManual == true && (discountDetail.Type == DiscountType.Document || discountDetail.Type == DiscountType.ExternalDocument))
                        {
                            detail.CuryDiscountAmt = discountDetail.CuryDiscountAmt;
                            detail.IsManual = discountDetail.IsManual;
                            docgraph.ARDiscountDetails.Cache.Update(detail);
                        }
                    }
                }
                ARInvoice old_row = PXCache<ARInvoice>.CreateCopy(docgraph.Document.Current);
                docgraph.Document.Cache.SetValueExt<ARInvoice.curyDiscTot>(docgraph.Document.Current, DiscountEngineProvider.GetEngineFor<ARTran, ARInvoiceDiscountDetail>().GetTotalGroupAndDocumentDiscount(docgraph.ARDiscountDetails));
                docgraph.Document.Cache.RaiseRowUpdated(docgraph.Document.Current, old_row);
                invoice = docgraph.Document.Update(invoice);
            }

            if (masterEntity.TaxZoneID != null && !recalcAny)
            {
                foreach (CRTaxTran tax in PXSelect<CRTaxTran,
                    Where<CRTaxTran.quoteID, Equal<Current<Document.quoteID>>>>.Select(Base))
                {
                    if (masterEntity.TaxZoneID == null)
                    {
                        Base.Caches[typeof(Document)].RaiseExceptionHandling<Document.taxZoneID>(
                            masterEntity, null,
                                new PXSetPropertyException<Document.taxZoneID>(ErrorMessages.FieldIsEmpty,
                                    $"[{nameof(Document.taxZoneID)}]"));
                    }

                    ARTaxTran new_artax = new ARTaxTran();
                    new_artax.TaxID = tax.TaxID;

                    new_artax = docgraph.Taxes.Insert(new_artax);

                    if (new_artax != null)
                    {
                        new_artax = PXCache<ARTaxTran>.CreateCopy(new_artax);
                        new_artax.TaxRate = tax.TaxRate;
                        new_artax.TaxBucketID = 0;
                        new_artax.CuryTaxableAmt = tax.CuryTaxableAmt;
                        new_artax.CuryTaxAmt = tax.CuryTaxAmt;
                        new_artax = docgraph.Taxes.Update(new_artax);
                    }
                }
            }

            if (recalcAny)
            {
                docgraph.recalcdiscountsfilter.Current.OverrideManualPrices = filter.OverrideManualPrices == true;
                docgraph.recalcdiscountsfilter.Current.RecalcDiscounts = filter.RecalculateDiscounts == true;
                docgraph.recalcdiscountsfilter.Current.RecalcUnitPrices = filter.RecalculatePrices == true;
                docgraph.recalcdiscountsfilter.Current.OverrideManualDiscounts = filter.OverrideManualDiscounts == true;
                docgraph.recalcdiscountsfilter.Current.OverrideManualDocGroupDiscounts = filter.OverrideManualDocGroupDiscounts == true;

                docgraph.Actions[nameof(ARInvoiceEntry.RecalculateDiscountsAction)].Press();
            }

            invoice.CuryOrigDocAmt = invoice.CuryDocBal;
            invoice.Hold = true;
            UDFHelper.CopyAttributes(Base.Caches[typeof(CRQuote)], masterEntity.Base, docgraph.Document.Cache, docgraph.Document.Cache.Current, invoice.DocType);
            docgraph.Document.Update(invoice);

            docgraph.customer.Current.CreditRule = customer.CreditRule;

            if (GetQuoteForWorkflowProcessing() is CRQuote workflowQuote)
            {
                docgraph.Caches[typeof(CRQuote)].Hold(workflowQuote);
            }

            if (!Base.IsContractBasedAPI)
                throw new PXRedirectRequiredException(docgraph, "");

            docgraph.Save.Press();
        }
    }
}