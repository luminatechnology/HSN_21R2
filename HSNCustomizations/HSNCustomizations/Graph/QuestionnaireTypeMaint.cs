using HSNCustomizations.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSNCustomizations.Graph
{
    // [PhaseII -  Appointment Questionnaire]
    public class QuestionnaireTypeMaint : PXGraph<QuestionnaireTypeMaint>
    {
        public PXSave<LUMQuestionnaireType> Save;
        public PXCancel<LUMQuestionnaireType> Cancel;
        public PXInsert<LUMQuestionnaireType> Insert;
        public PXFirst<LUMQuestionnaireType> First;
        public PXLast<LUMQuestionnaireType> Last;
        public PXDelete<LUMQuestionnaireType> Delete;
        public SelectFrom<LUMQuestionnaireType>.View Document;
        public CSAttributeGroupList<LUMQuestionnaireType.questionnaireType, LUMApptQuestionnaire> Mapping;
    }
}
