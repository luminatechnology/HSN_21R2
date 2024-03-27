using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSNHighcareCistomizations.Entity
{
    /// <summary>
    /// Login WasaTeam request 
    /// </summary>
    public class WasatemAPILoginEntity
    {
        public string email { get; set; }

        public string password { get; set; }
    }

    /// <summary>
    /// Login WasaTeam API Response
    /// </summary>
    public class WasateamAPITokenEntity
    {
        public string access_token { get; set; }

        public DateTime? expires_at { get; set; }
    }

    #region WasaTeam Coupon Entity
    /// <summary> Wasateam Coupon Entity root</summary>
    public class WasateamCouponEntity
    {
        public Data data { get; set; }
    }

    public class Data
    {
        public string id { get; set; }
        public DateTime created_at { get; set; }
        public object redeemed_at { get; set; }
        public DateTime redeem_start_at { get; set; }
        public DateTime redeem_expired_at { get; set; }
        public string status { get; set; }
        public ShopCampaign shop_campaign { get; set; }
    }

    public class ShopCampaign
    {
        public string id { get; set; }
        public string name { get; set; }
        public object condition { get; set; }
        public int is_active { get; set; }
        public object cover_image { get; set; }
        public int full_amount { get; set; }
        public object discount_percent { get; set; }
        public int discount_amount { get; set; }
        public string coupon_exchange_code { get; set; }
        public object coupon_exchange_start_at { get; set; }
        public object coupon_exchange_expired_at { get; set; }
        public object coupon_exchange_limit { get; set; }
        public string condition_description { get; set; }
        public string description { get; set; }
        public string precautions { get; set; }
        public string instructions { get; set; }
        public object revoke_at { get; set; }
        public string custom_no { get; set; }
    } 
    #endregion


}
