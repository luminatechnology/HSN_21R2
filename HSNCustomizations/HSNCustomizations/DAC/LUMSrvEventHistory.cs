﻿using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.FS;

namespace HSNCustomizations.DAC
{
    [Serializable]
    [PXCacheName("Service Order Event History")]
    public class LUMSrvEventHistory : IBqlTable
    {
        #region IdentityID
        [PXDBIdentity(IsKey = true)]
        public virtual int? IdentityID { get; set; }
        public abstract class identityID : PX.Data.BQL.BqlInt.Field<identityID> { }
        #endregion

        #region SrvOrdType
        [PXDBString(4, IsFixed = true, InputMask = ">AAAA")]
        [PXUIField(DisplayName = "Srv Ord Type")]
        [PXDBDefault(typeof(FSSrvOrdType.srvOrdType))]
        public virtual string SrvOrdType { get; set; }
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }
        #endregion

        #region SORefNbr
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "SO Ref Nbr.")]
        //[FSSelectorSORefNbr_Appointment]
        public virtual string SORefNbr { get; set; }
        public abstract class sORefNbr : PX.Data.BQL.BqlString.Field<sORefNbr> { }
        #endregion

        #region ApptRefNbr
        [PXDBString(20, IsUnicode = true, InputMask = "CCCCCCCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Ref Nbr.")]
        //[PXSelector(typeof(Search2<FSAppointment.refNbr, LeftJoin<FSServiceOrder, On<FSServiceOrder.sOID, Equal<FSAppointment.sOID>>,
        //                                                 LeftJoin<Customer, On<Customer.bAccountID, Equal<FSServiceOrder.customerID>>,
        //                                                 LeftJoin<Location, On<Location.bAccountID, Equal<FSServiceOrder.customerID>,
        //                                                                       And<Location.locationID, Equal<FSServiceOrder.locationID>>>>>>,
        //                                                 Where2<Where<FSAppointment.srvOrdType, Equal<Optional<FSAppointment.srvOrdType>>>,
        //                                                              And<Where<Customer.bAccountID, IsNull,
        //                                                                        Or<Match<Customer, Current<AccessInfo.userName>>>>>>,
        //                                                 OrderBy<Desc<FSAppointment.refNbr>>>),
        //                                                              new Type[] {
        //                                                                        typeof(FSAppointment.refNbr),
        //                                                                        typeof(Customer.acctCD),
        //                                                                        typeof(Customer.acctName),
        //                                                                        typeof(Location.locationCD),
        //                                                                        typeof(FSAppointment.docDesc),
        //                                                                        typeof(FSAppointment.status),
        //                                                                        typeof(FSAppointment.scheduledDateTimeBegin)
        //                                                              })]
        public virtual string ApptRefNbr { get; set; }
        public abstract class apptRefNbr : PX.Data.BQL.BqlString.Field<apptRefNbr> { }
        #endregion

        #region ApptStatus
        [PXDBString(1, IsFixed = true, InputMask = "")]
        [PXUIField(DisplayName = "Status",Visible = false)]
        [FSAppointment.status.Values.List]
        public virtual string ApptStatus { get; set; }
        public abstract class apptStatus : PX.Data.BQL.BqlString.Field<apptStatus> { }
        #endregion

        #region FromStage
        [PXDBString(30)]
        [PXUIField(DisplayName = "From Stage")]
        //[FSSelectorWorkflowStage(typeof(LUMAppEventHistory.srvOrdType))]
        public virtual string FromStage { get; set; }
        public abstract class fromStage : PX.Data.BQL.BqlString.Field<fromStage> { }
        #endregion

        #region ToStage
        [PXDBString(30)]
        [PXUIField(DisplayName = "To Stage")]
        //[FSSelectorWorkflowStage(typeof(LUMAppEventHistory.srvOrdType))]
        public virtual string ToStage { get; set; }
        public abstract class toStage : PX.Data.BQL.BqlString.Field<toStage> { }
        #endregion

        #region WFRule
        [PXDBString(12, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Rule")]
        public virtual string WFRule { get; set; }
        public abstract class wFRule : PX.Data.BQL.BqlString.Field<wFRule> { }
        #endregion

        #region Descr
        [PXDBString(256, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Description")]
        public virtual string Descr { get; set; }
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
        #endregion

        #region CreatedByID
        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID { get; set; }
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        #endregion

        #region CreatedDateTime
        [PXDBCreatedDateTime()]
        public virtual DateTime? CreatedDateTime { get; set; }
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
        #endregion
    }
}