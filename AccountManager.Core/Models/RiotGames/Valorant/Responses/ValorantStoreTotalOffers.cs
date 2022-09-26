using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AccountManager.Core.Models.RiotGames.Valorant.Responses
{
    public sealed class Cost
    {
        [JsonPropertyName("85ad13f7-3d1b-5128-9eb2-7cd8ee0b5741")]
        public int _85ad13f73d1b51289eb27cd8ee0b5741 { get; set; }
    }

    public sealed class Offer
    {
        [JsonPropertyName("OfferID")]
        public string OfferID { get; set; } = string.Empty;

        [JsonPropertyName("IsDirectPurchase")]
        public bool IsDirectPurchase { get; set; } = false;

        [JsonPropertyName("StartDate")]
        public string StartDate { get; set; } = string.Empty;

        [JsonPropertyName("Cost")]
        public Cost Cost { get; set; } = new();

        [JsonPropertyName("Rewards")]
        public List<Reward> Rewards { get; set; } = new();
    }

    public sealed class Reward
    {
        [JsonPropertyName("ItemTypeID")]
        public string ItemTypeID { get; set; } = string.Empty;

        [JsonPropertyName("ItemID")]
        public string ItemID { get; set; } = string.Empty;

        [JsonPropertyName("Quantity")]
        public int Quantity { get; set; } = 0;
    }

    public sealed class ValorantStoreTotalOffers
    {
        [JsonPropertyName("Offers")]
        public List<Offer> Offers { get; set; } = new();

        [JsonPropertyName("UpgradeCurrencyOffers")]
        public List<UpgradeCurrencyOffer> UpgradeCurrencyOffers { get; set; } = new();
    }

    public sealed class UpgradeCurrencyOffer
    {
        [JsonPropertyName("OfferID")]
        public string OfferID { get; set; } = string.Empty;

        [JsonPropertyName("StorefrontItemID")]
        public string StorefrontItemID { get; set; } = string.Empty;
    }


}
