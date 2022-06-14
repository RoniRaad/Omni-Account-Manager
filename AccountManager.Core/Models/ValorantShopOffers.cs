using System.Text.Json.Serialization;

public class Bundle
{
    [JsonPropertyName("ID")]
    public string ID { get; set; }

    [JsonPropertyName("DataAssetID")]
    public string DataAssetID { get; set; }

    [JsonPropertyName("CurrencyID")]
    public string CurrencyID { get; set; }

    [JsonPropertyName("Items")]
    public List<ItemWrapper> Items { get; set; }

    [JsonPropertyName("DurationRemainingInSeconds")]
    public int DurationRemainingInSeconds { get; set; }

    [JsonPropertyName("WholesaleOnly")]
    public bool WholesaleOnly { get; set; }
}

public class Bundle2
{
    [JsonPropertyName("ID")]
    public string ID { get; set; }

    [JsonPropertyName("DataAssetID")]
    public string DataAssetID { get; set; }

    [JsonPropertyName("CurrencyID")]
    public string CurrencyID { get; set; }

    [JsonPropertyName("Items")]
    public List<ItemWrapper> Items { get; set; }

    [JsonPropertyName("DurationRemainingInSeconds")]
    public int DurationRemainingInSeconds { get; set; }

    [JsonPropertyName("WholesaleOnly")]
    public bool WholesaleOnly { get; set; }
}

public class FeaturedBundle
{
    [JsonPropertyName("Bundle")]
    public Bundle Bundle { get; set; }

    [JsonPropertyName("Bundles")]
    public List<Bundle> Bundles { get; set; }

    [JsonPropertyName("BundleRemainingDurationInSeconds")]
    public int BundleRemainingDurationInSeconds { get; set; }
}

public class ItemWrapper
{
    [JsonPropertyName("Item")]
    public ItemWrapper Item { get; set; }

    [JsonPropertyName("BasePrice")]
    public int BasePrice { get; set; }

    [JsonPropertyName("CurrencyID")]
    public string CurrencyID { get; set; }

    [JsonPropertyName("DiscountPercent")]
    public int DiscountPercent { get; set; }

    [JsonPropertyName("DiscountedPrice")]
    public int DiscountedPrice { get; set; }

    [JsonPropertyName("IsPromoItem")]
    public bool IsPromoItem { get; set; }
}

public class Item2
{
    [JsonPropertyName("ItemTypeID")]
    public string ItemTypeID { get; set; }

    [JsonPropertyName("ItemID")]
    public string ItemID { get; set; }

    [JsonPropertyName("Amount")]
    public int Amount { get; set; }
}

public class ValorantShopOffers
{
    [JsonPropertyName("FeaturedBundle")]
    public FeaturedBundle FeaturedBundle { get; set; }

    [JsonPropertyName("SkinsPanelLayout")]
    public SkinsPanelLayout SkinsPanelLayout { get; set; }
}

public class SkinsPanelLayout
{
    [JsonPropertyName("SingleItemOffers")]
    public List<string> SingleItemOffers { get; set; }

    [JsonPropertyName("SingleItemOffersRemainingDurationInSeconds")]
    public int SingleItemOffersRemainingDurationInSeconds { get; set; }
}

