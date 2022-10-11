using HSNCustomizations.DAC;
using HSNCustomizations.Descriptor;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using PX.Objects.FS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSNCustomizations.Graph
{
    // [PhaseII -  Appointment Questionnaire]
    public class LUMApptQuestionnaireResultMaint : PXGraph<LUMApptQuestionnaireResultMaint, LUMApptQuestionnaire>
    {
        public SelectFrom<LUMApptQuestionnaire>
                .LeftJoin<Contact>.On<LUMApptQuestionnaire.contactID.IsEqual<Contact.contactID>>
                //.Where<LUMApptQuestionnaire.apptRefNbr.IsEqual<LUMApptQuestionnaire.apptRefNbr.AsOptional>>
                .View Document;

        public SelectFrom<FSAppointmentEmployee>
               .Where<FSAppointmentEmployee.refNbr.IsEqual<LUMApptQuestionnaire.apptRefNbr.FromCurrent>>
               .View StaffList;

        public SelectFrom<LUMApptQuensionnaireContactHistory>
               .Where<LUMApptQuensionnaireContactHistory.uniqueID.IsEqual<LUMApptQuestionnaire.uniqueID.FromCurrent>>
               .OrderBy<Asc<LUMApptQuensionnaireContactHistory.contactDate>>
               .View ContactHistory;

        [PXViewName(PX.Objects.CR.Messages.Answers)]
        [PXCopyPasteHiddenView]
        public CRAttributeList<LUMApptQuestionnaire> Answers;

        public IEnumerable document()
        {
            return SelectFrom<LUMApptQuestionnaire>
                   .Where<LUMApptQuestionnaire.uniqueID.IsEqual<P.AsString>>
                   .View.Select(this, (this.Document.Current?.UniqueID ?? PXView.Searches[0]));
        }

        #region Events

        public virtual void _(Events.FieldUpdated<LUMApptQuestionnaire.apptRefNbr> e)
        {
            if (e.Row != null)
            {
                this.Document.Cache.Clear();
                this.Document.Cache.ClearQueryCache();
                this.Document.Cache.SetValueExt<LUMApptQuestionnaire.uniqueID>((LUMApptQuestionnaire)e.Row, e.NewValue);
                this.Document.View.RequestRefresh();
            }
        }

        public virtual void _(Events.RowSelected<LUMApptQuestionnaire> e)
        {
            var row = e.Row;
            if (row != null)
            {
                if (!row.CustomerID.HasValue)
                {
                    object newCustomerID;
                    e.Cache.RaiseFieldDefaulting<LUMApptQuestionnaire.customerID>(row, out newCustomerID);
                    row.CustomerID = (int?)newCustomerID;
                }

                if (!row.ExecutionDate.HasValue)
                {
                    object newExecutionDate;
                    e.Cache.RaiseFieldDefaulting<LUMApptQuestionnaire.executionDate>(row, out newExecutionDate);
                    row.ExecutionDate = (DateTime?)newExecutionDate;
                }

                if (!row.ContactID.HasValue)
                {
                    object newContactID;
                    e.Cache.RaiseFieldDefaulting<LUMApptQuestionnaire.contactID>(row, out newContactID);
                    row.ContactID = (int?)newContactID;
                }

                if (!row.BranchID.HasValue)
                {
                    object newBranchID;
                    e.Cache.RaiseFieldDefaulting<LUMApptQuestionnaire.branchID>(row, out newBranchID);
                    row.BranchID = (int?)newBranchID;

                }

                if (string.IsNullOrEmpty(row.QuestionnaireType))
                {
                    var newQuestionnaireType = SelectFrom<LUMQuestionnaireType>.View.Select(this).TopFirst?.QuestionnaireType;
                    row.QuestionnaireType = newQuestionnaireType;
                }

                if (string.IsNullOrEmpty(row.DocDesc))
                {
                    object newDocDesc;
                    e.Cache.RaiseFieldDefaulting<LUMApptQuestionnaire.docDesc>(row, out newDocDesc);
                    row.DocDesc = (string)newDocDesc;
                }

                if (string.IsNullOrEmpty(row.ApptRefNbr) && !string.IsNullOrEmpty(row.UniqueID))
                    row.ApptRefNbr = row.UniqueID;

            }
        }

        public virtual void _(Events.FieldDefaulting<LUMApptQuensionnaireContactHistory.lineNbr> e)
        {
            var currentList = this.ContactHistory.Select().RowCast<LUMApptQuensionnaireContactHistory>();
            var maxLineNbr = currentList.Count() == 0 ? 0 : currentList.Max(x => x?.LineNbr ?? 0);
            e.NewValue = maxLineNbr + 1;
        }

        #endregion

    }
}
