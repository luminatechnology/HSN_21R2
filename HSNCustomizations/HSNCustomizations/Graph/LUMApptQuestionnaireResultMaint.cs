using HSNCustomizations.DAC;
using HSNCustomizations.Descriptor;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using PX.Objects.FS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSNCustomizations.Graph
{
    public class LUMApptQuestionnaireResultMaint : PXGraph<LUMApptQuestionnaireResultMaint, LUMApptQuestionnaire>
    {
        public SelectFrom<LUMApptQuestionnaire>
                .LeftJoin<Contact>.On<LUMApptQuestionnaire.contactID.IsEqual<Contact.contactID>>
                //.Where<LUMApptQuestionnaire.questionnaireType.IsEqual<LUMApptQuestionnaire.questionnaireType.FromCurrent>>
                .View Document;

        public SelectFrom<FSAppointmentEmployee>
               .Where<FSAppointmentEmployee.refNbr.IsEqual<LUMApptQuestionnaire.apptRefNbr.FromCurrent>>
               .View StaffList;

        [PXViewName(PX.Objects.CR.Messages.Answers)]
        [PXCopyPasteHiddenView]
        public CRAttributeList<LUMApptQuestionnaire> Answers;

        #region Events

        public virtual void _(Events.RowSelected<LUMApptQuestionnaire> e)
        {
            var row = e.Row;
            if (row != null)
            {
                if (string.IsNullOrEmpty(row.QuestionnaireType))
                {
                    object newQuestionnaireType;
                    e.Cache.RaiseFieldDefaulting<LUMApptQuestionnaire.questionnaireType>(row, out newQuestionnaireType);
                    row.QuestionnaireType = (string)newQuestionnaireType;
                }

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

            }
        }

        #endregion

    }
}
