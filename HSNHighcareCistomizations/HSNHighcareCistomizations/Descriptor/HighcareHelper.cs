﻿using HSNHighcareCistomizations.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.FS;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSNHighcareCistomizations.Descriptor
{
    public class HighcareHelper
    {
        public INItemClass GetItemclass(int? inventoryID)
        {
            return SelectFrom<INItemClass>
                   .InnerJoin<InventoryItem>.On<INItemClass.itemClassID.IsEqual<InventoryItem.itemClassID>>
                   .Where<InventoryItem.inventoryID.IsEqual<P.AsInt>>
                   .View.Select(new PXGraph(), inventoryID).RowCast<INItemClass>().FirstOrDefault();
        }

        public FSEquipment GetEquipmentInfo(int? equipmentID)
        {
            return FSEquipment.PK.Find(new PXGraph(), equipmentID);
        }

        public IEnumerable<LUMEquipmentPINCode> GetEquipmentPINCodeList(int? baccountID, int? equipmentID)
        {
           return SelectFrom<LUMCustomerPINCode>
                           .InnerJoin<FSEquipment>.On<LUMCustomerPINCode.bAccountID.IsEqual<FSEquipment.ownerID>>
                           .InnerJoin<LUMEquipmentPINCode>.On<FSEquipment.SMequipmentID.IsEqual<LUMEquipmentPINCode.sMEquipmentID>>
                           .Where<LUMCustomerPINCode.bAccountID.IsEqual<P.AsInt>
                             .And<FSEquipment.SMequipmentID.IsEqual<P.AsInt>>>
                 .OrderBy<Asc<LUMCustomerPINCode.startDate>>
                 .View.Select(new PXGraph(), baccountID, equipmentID).RowCast<LUMEquipmentPINCode>();
        }
    }

    public class HighcareClassAttr : PX.Data.BQL.BqlString.Constant<HighcareClassAttr>
    {
        public HighcareClassAttr() : base("HIGHCARE") { }
    }

    [Serializable]
    public class ServiceScopeFilter : IBqlTable
    {
        #region CPriceClassID
        [PXString(10, IsUnicode = true, InputMask = "")]
        [PXSelector(typeof(PX.Objects.AR.ARPriceClass.priceClassID))]
        [PXUIField(DisplayName = "Price Class ID")]
        public virtual string CPriceClassID { get; set; }
        public abstract class cPriceClassID : PX.Data.BQL.BqlString.Field<cPriceClassID> { }
        #endregion
    }
}
