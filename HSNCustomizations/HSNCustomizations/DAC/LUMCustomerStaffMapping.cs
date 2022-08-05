using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.FS;
using PX.Objects.CR;
using CRLocation = PX.Objects.CR.Standalone.Location;

namespace HSNCustomizations.DAC
{
    [Serializable]
    [PXCacheName("LUMCustomerStaffMapping")]
    public class LUMCustomerStaffMapping : IBqlTable
    {
        public static class FK
        {
            public class Customer : PX.Objects.AR.Customer.PK.ForeignKeyOf<LUMCustomerStaffMapping>.By<customerID> { }
            public class CustomerLocation : Location.PK.ForeignKeyOf<LUMCustomerStaffMapping>.By<customerID, locationID> { }
        }

        #region CustomerID
        [PXDBInt(IsKey = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Customer", Visibility = PXUIVisibility.SelectorVisible)]
        [PXRestrictor(typeof(Where<BAccountSelectorBase.status, IsNull,
          Or<BAccountSelectorBase.status, Equal<CustomerStatus.active>,
          Or<BAccountSelectorBase.status, Equal<CustomerStatus.prospect>,
          Or<BAccountSelectorBase.status, Equal<CustomerStatus.oneTime>>>>>),
          PX.Objects.AR.Messages.CustomerIsInStatus, typeof(BAccountSelectorBase.status))]
        [FSSelectorBusinessAccount_CU_PR_VC]
        [PXForeignReference(typeof(FK.Customer))]
        public virtual int? CustomerID { get; set; }
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
        #endregion

        #region LocationID
        [LocationActive(typeof(
           Where<Location.bAccountID, Equal<Current<LUMCustomerStaffMapping.customerID>>,
               And<MatchWithBranch<Location.cBranchID>>>),
                   DescriptionField = typeof(Location.descr), DisplayName = "Location", DirtyRead = true, IsKey = true)]
        [PXDefault(typeof(Coalesce<Search2<
           BAccountR.defLocationID,
           InnerJoin<CRLocation,
               On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>,
               And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>>,
           Where<BAccountR.bAccountID, Equal<Current<LUMCustomerStaffMapping.customerID>>,
               And<CRLocation.isActive, Equal<True>,
               And<MatchWithBranch<CRLocation.cBranchID>>>>>,
           Search<
           CRLocation.locationID,
           Where<CRLocation.bAccountID, Equal<Current<LUMCustomerStaffMapping.customerID>>,
               And<CRLocation.isActive, Equal<True>,
               And<MatchWithBranch<CRLocation.cBranchID>>>>>>), PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXForeignReference(typeof(FK.CustomerLocation))]
        public virtual int? LocationID { get; set; }
        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
        #endregion

        #region EmployeeID
        [PXDBInt(IsKey = true)]
        [PXDefault]
        [FSSelector_StaffMember_ServiceOrderProjectID]
        [PXUIField(DisplayName = "Staff Member", TabOrder = 0)]
        public virtual int? EmployeeID { get; set; }
        public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
        #endregion

        #region EquipmentTypeID
        [PXDBInt]
        [PXUIField(DisplayName = "Equipment Type")]
        [FSSelectorEquipmentType]
        public virtual int? EquipmentTypeID { get; set; }
        public abstract class equipmentTypeID : PX.Data.BQL.BqlInt.Field<equipmentTypeID> { }
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