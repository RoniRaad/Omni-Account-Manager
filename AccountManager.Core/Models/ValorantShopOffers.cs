using System.Text.Json.Serialization;

namespace AccountManager.Core.Models
{
    public class Bundle
    {
        [JsonPropertyName("ID")]
        public string ID { get; set; } = string.Empty;

        [JsonPropertyName("DataAssetID")]
        public string DataAssetID { get; set; } = string.Empty;

        [JsonPropertyName("CurrencyID")]
        public string CurrencyID { get; set; } = string.Empty;

        [JsonPropertyName("Items")]
        public List<ItemWrapper> Items { get; set; } = new();

        [JsonPropertyName("DurationRemainingInSeconds")]
        public int DurationRemainingInSeconds { get; set; } = 0;

        [JsonPropertyName("WholesaleOnly")]
        public bool WholesaleOnly { get; set; } = false;
    }

    public class Bundle2
    {
        [JsonPropertyName("ID")]
        public string ID { get; set; } = string.Empty;

        [JsonPropertyName("DataAssetID")]
        public string DataAssetID { get; set; } = string.Empty;

        [JsonPropertyName("CurrencyID")]
        public string CurrencyID { get; set; } = string.Empty;

        [JsonPropertyName("Items")]
        public List<ItemWrapper> Items { get; set; } = new();

        [JsonPropertyName("DurationRemainingInSeconds")]
        public int DurationRemainingInSeconds { get; set; } = 0;

        [JsonPropertyName("WholesaleOnly")]
        public bool WholesaleOnly { get; set; } = false;
    }

    public class FeaturedBundle
    {
        [JsonPropertyName("Bundle")]
        public Bundle? Bundle { get; set; }

        [JsonPropertyName("Bundles")]
        public List<Bundle> Bundles { get; set; } = new();

        [JsonPropertyName("BundleRemainingDurationInSeconds")]
        public int BundleRemainingDurationInSeconds { get; set; }
    }

    public class ItemWrapper
    {
        [JsonPropertyName("Item")]
        public ItemWrapper? Item { get; set; }

        [JsonPropertyName("BasePrice")]
        public int BasePrice { get; set; } = 0;

        [JsonPropertyName("CurrencyID")]
        public string CurrencyID { get; set; } = string.Empty;

        [JsonPropertyName("DiscountPercent")]
        public double DiscountPercent { get; set; } = 0;

        [JsonPropertyName("DiscountedPrice")]
        public double DiscountedPrice { get; set; } = 0;

        [JsonPropertyName("IsPromoItem")]
        public bool IsPromoItem { get; set; } = false;
    }

    public class Item2
    {
        [JsonPropertyName("ItemTypeID")]
        public string ItemTypeID { get; set; } = string.Empty;

        [JsonPropertyName("ItemID")]
        public string ItemID { get; set; } = string.Empty;

        [JsonPropertyName("Amount")]
        public int Amount { get; set; } = 0;
    }

    public class ValorantShopOffers
    {
        [JsonPropertyName("FeaturedBundle")]
        public FeaturedBundle FeaturedBundle { get; set; } = new();

        [JsonPropertyName("SkinsPanelLayout")]
        public SkinsPanelLayout SkinsPanelLayout { get; set; } = new();
    }

    public class SkinsPanelLayout
    {
        [JsonPropertyName("SingleItemOffers")]
        public List<string> SingleItemOffers { get; set; } = new();

        [JsonPropertyName("SingleItemOffersRemainingDurationInSeconds")]
        public int SingleItemOffersRemainingDurationInSeconds { get; set; }
    }
}