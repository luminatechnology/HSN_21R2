using PX.Data;
using PX.Objects.FS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSNCustomizations.DAC
{
    public class StaffProviderRec : IBqlTable
    {
        #region BAccountID
        [PXDBInt]
        [PXUIField(DisplayName = "BAccountID", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual int? BAccountID { get; set; }
        public abstract class bAccountID : PX.Data.IBqlField { }

        #endregion

        #region AcctCD
        [PXDBString(128, InputMask = "", IsUnicode = true)]
        [PXUIField(DisplayName = "AcctCD", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string AcctCD { get; set; }
        public abstract class acctCD : PX.Data.IBqlField { }

        #endregion

        #region AcctName
        [PXDBString(128, InputMask = "", IsUnicode = true)]
        [PXUIField(DisplayName = "AcctName", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string AcctName { get; set; }
        public abstract class acctName : PX.Data.IBqlField { }
        #endregion

        #region Type
        [EmployeeType.List()]
        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual string Type { get; set; }
        public abstract class type : PX.Data.IBqlField { }
        #endregion

        #region Status
        [PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string Status { get; set; }
        public abstract class status : PX.Data.IBqlField { }
        #endregion

        #region PositionID
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Position", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string PositionID { get; set; }
        public abstract class positionID : PX.Data.IBqlField { }
        #endregion

    }
}
