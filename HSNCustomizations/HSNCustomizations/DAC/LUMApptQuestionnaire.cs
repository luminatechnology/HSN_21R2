using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR;
using PX.Objects.FS;
using PX.Objects.GL;
using PX.Objects.CR;
using HSNCustomizations.DAC;
using PX.Data.BQL.Fluent;

namespace HSNCustomizations
{
    // [PhaseII -  Appointment Questionnaire]
    [Serializable]
    [PXCacheName("LUMApptQuestionnaire")]
    public class LUMApptQuestionnaire : IBqlTable
    {

        #region SrvOrdType

        //[PXDBString(4, IsKey = true, IsFixed = true, InputMask = ">AAAA")]
        //[PXDefault(typeof(Coalesce<
        //    Search<FSxUserPreferences.dfltSrvOrdType,
        //    Where<
        //        PX.SM.UserPreferences.userID.IsEqual<AccessInfo.userID.FromCurrent>>>,
        //    Search<FSSetup.dfltSrvOrdType>>))]
        //[PXUIField(DisplayName = "Service Order Type", Visibility = PXUIVisibility.SelectorVisible)]
        //[FSSelectorSrvOrdTypeNOTQuote]
        //[PXRestrictor(typeof(Where<FSSrvOrdType.active, Equal<True>>), null)]
        //[PX.Data.EP.PXFieldDescription]
        //public virtual string SrvOrdType { get; set; }
        //public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        #endregion

        #region ApptRefNbr
        [PXDBString(20, IsKey = true, IsUnicode = true, InputMask = "CCCCCCCCCCCCCCCCCCCC")]
        [PXDefault("<NEW>", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Appointment Nbr.", Visibility = PXUIVisibility.SelectorVisible, Visible = true, Enabled = true)]
        [PXSelector(typeof(
            Search2<FSAppointment.refNbr,
            LeftJoin<FSServiceOrder,
                On<FSServiceOrder.sOID, Equal<FSAppointment.sOID>>,
            LeftJoin<Customer,
                On<Customer.bAccountID, Equal<FSServiceOrder.customerID>>,
            LeftJoin<Location,
                On<Location.bAccountID, Equal<FSServiceOrder.customerID>,
                    And<Location.locationID, Equal<FSServiceOrder.locationID>>>>>>,
                Where<
                    Customer.bAccountID, IsNull,
                    Or<Match<Customer, Current<AccessInfo.userName>>>>,
            OrderBy<
                Desc<FSAppointment.refNbr>>>),
                    new Type[] {
                                typeof(FSAppointment.refNbr),
                                typeof(Customer.acctCD),
                                typeof(Customer.acctName),
                                typeof(Location.locationCD),
                                typeof(FSAppointment.docDesc),
                                typeof(FSAppointment.status),
                                typeof(FSAppointment.scheduledDateTimeBegin)
                    })]
        [PX.Data.EP.PXFieldDescription]
        public virtual string ApptRefNbr { get; set; }
        public abstract class apptRefNbr : PX.Data.BQL.BqlString.Field<apptRefNbr> { }
        #endregion

        #region QuestionnaireType
        [PXString(100, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Questionnaire Type", Enabled = false)]
        [PXDefault(typeof(SelectFrom<FSSrvOrdType>
                          .InnerJoin<FSAppointment>.On<FSSrvOrdType.srvOrdType.IsEqual<FSAppointment.srvOrdType>>
                          .Where<FSAppointment.refNbr.IsEqual<LUMApptQuestionnaire.apptRefNbr.FromCurrent>>
                          .SearchFor<FSSrvOrdTypeExt.usrQuestionnaireType>)
            , PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXSelector(
            typeof(Search<LUMQuestionnaireType.questionnaireType>),
            typeof(LUMQuestionnaireType.description))]
        public virtual string QuestionnaireType { get; set; }
        public abstract class questionnaireType : PX.Data.BQL.BqlString.Field<questionnaireType> { }
        #endregion

        #region BranchID

        [PXInt]
        [PXUIField(DisplayName = "Branch", Enabled = false, TabOrder = 0)]
        [PXDefault(typeof(AccessInfo.branchID), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Search<Branch.branchID>), SubstituteKey = typeof(Branch.branchCD), DescriptionField = typeof(Branch.acctName))]
        public virtual Int32? BranchID { get; set; }
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        #endregion

        #region CustomerID
        [PXInt]
        [PXDefault(typeof(Search<FSAppointment.customerID, Where<FSAppointment.refNbr, Equal<Current<LUMApptQuestionnaire.apptRefNbr>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Customer", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
        [PXRestrictor(typeof(Where<Customer.status, IsNull,
                Or<Customer.status, Equal<CustomerStatus.active>,
                Or<Customer.status, Equal<CustomerStatus.oneTime>>>>),
                PX.Objects.AR.Messages.CustomerIsInStatus, typeof(Customer.status))]
        [FSSelectorBAccountCustomerOrCombined]
        public virtual int? CustomerID { get; set; }
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

        #endregion

        #region ExecutionDate

        [PXDate]
        [PXDefault(typeof(Search<FSAppointment.executionDate, Where<FSAppointment.refNbr, Equal<Current<LUMApptQuestionnaire.apptRefNbr>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Actual Start Date", Enabled = false)]
        public virtual DateTime? ExecutionDate { get; set; }
        public abstract class executionDate : PX.Data.BQL.BqlDateTime.Field<executionDate> { }
        #endregion

        #region ContactID

        [PXInt]
        [PXDefault(
            typeof(SelectFrom<Contact>
                   .Where<Contact.bAccountID.IsEqual<LUMApptQuestionnaire.customerID.FromCurrent>>
                   .SearchFor<Contact.contactID>)
            , PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Contact", Enabled = false)]
        [FSSelectorContact(typeof(LUMApptQuestionnaire.customerID))]
        public virtual int? ContactID { get; set; }
        public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
        #endregion

        #region Attributes
        [CRAttributesField(typeof(LUMApptQuestionnaire.questionnaireType), typeof(LUMApptQuestionnaire.noteid))]
        public string[] Attributes { get; set; }
        #endregion

        #region Noteid
        [PXNote()]
        public virtual Guid? Noteid { get; set; }
        public abstract class noteid : PX.Data.BQL.BqlGuid.Field<noteid> { }
        #endregion

        #region CreatedByID
        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID { get; set; }
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        #endregion

        #region CreatedByScreenID
        [PXDBCreatedByScreenID()]
        public virtual string CreatedByScreenID { get; set; }
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
        #endregion

        #region CreatedDateTime
        [PXDBCreatedDateTime()]
        public virtual DateTime? CreatedDateTime { get; set; }
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
        #endregion

        #region LastModifiedByID
        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID { get; set; }
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
        #endregion

        #region LastModifiedByScreenID
        [PXDBLastModifiedByScreenID()]
        public virtual string LastModifiedByScreenID { get; set; }
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
        #endregion

        #region LastModifiedDateTime
        [PXDBLastModifiedDateTime()]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
        #endregion

        #region Tstamp
        [PXDBTimestamp()]
        [PXUIField(DisplayName = "Tstamp")]
        public virtual byte[] Tstamp { get; set; }
        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
        #endregion
    }
}