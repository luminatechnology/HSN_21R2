using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.CR;

namespace PX.Objects.AR
{
    public class CustomerLocationMaintExt : PXGraphExtension<CustomerLocationMaint>
    {

        #region Events
        public virtual void _(Events.FieldDefaulting<Location.cTaxCalcMode> e, PXFieldDefaulting baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);
            var row = e.Row as Location;
            if (row.BAccountID.HasValue)
            {
                var baccountInfo = BAccount.PK.Find(Base, row.BAccountID);
                var locationInfo = Location.PK.Find(Base, baccountInfo.BAccountID, baccountInfo.DefLocationID);
                e.NewValue = locationInfo?.CTaxCalcMode;
            }
        }

        #endregion

    }
}
