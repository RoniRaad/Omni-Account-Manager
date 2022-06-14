using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AccountManager.Core.Models.RiotGames.Valorant.Responses
{
    public class Cost
    {
        [JsonPropertyName("85ad13f7-3d1b-5128-9eb2-7cd8ee0b5741")]
        public int _85ad13f73d1b51289eb27cd8ee0b5741 { get; set; }
    }

    public class Offer
    {
        [JsonPropertyName("OfferID")]
        public string OfferID { get; set; }

        [JsonPropertyName("IsDirectPurchase")]
        public bool IsDirectPurchase { get; set; }

        [JsonPropertyName("StartDate")]
        public string StartDate { get; set; }

        [JsonPropertyName("Cost")]
        public Cost Cost { get; set; }

        [JsonPropertyName("Rewards")]
        public List<Reward> Rewards { get; set; }
    }

    public class Reward
    {
        [JsonPropertyName("ItemTypeID")]
        public string ItemTypeID { get; set; }

        [JsonPropertyName("ItemID")]
        public string ItemID { get; set; }

        [JsonPropertyName("Quantity")]
        public int Quantity { get; set; }
    }

    public class ValorantStoreTotalOffers
    {
        [JsonPropertyName("Offers")]
        public List<Offer> Offers { get; set; }

        [JsonPropertyName("UpgradeCurrencyOffers")]
        public List<UpgradeCurrencyOffer> UpgradeCurrencyOffers { get; set; }
    }

    public class UpgradeCurrencyOffer
    {
        [JsonPropertyName("OfferID")]
        public string OfferID { get; set; }

        [JsonPropertyName("StorefrontItemID")]
        public string StorefrontItemID { get; set; }
    }


}
