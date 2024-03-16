using System.Text.Json.Serialization;

namespace AutoClaimStickers;

#pragma warning disable CA1812
internal sealed record CanClaimItemData {
	[JsonPropertyName("response"), JsonRequired]
	public CanClaimItemResponse? Response { get; init; }
}
internal sealed record CanClaimItemResponse {
	[JsonPropertyName("can_claim")]
	public bool? CanClaim { get; init; }
	[JsonPropertyName("next_claim_time")]
	public uint? NextClaimTime { get; init; }
	[JsonPropertyName("reward_item")]
	public RewardItem? RewardItem { get; init; }
}
internal sealed record ClaimItemData {
	[JsonPropertyName("response"), JsonRequired]
	public ClaimItemResponse? Response { get; init; }
}
internal sealed record ClaimItemResponse {
	[JsonPropertyName("communityitemid")]
	public string? CommunityItemId { get; init; }
	[JsonPropertyName("next_claim_time")]
	public uint? NextClaimTime { get; init; }
	[JsonPropertyName("reward_item")]
	public RewardItem? RewardItem { get; init; }
}
internal sealed record RewardItem {
	[JsonPropertyName("appid")]
	public int? AppId { get; init; }
	[JsonPropertyName("defid")]
	public int? DefId { get; init; }
	[JsonPropertyName("type")]
	public int? Type { get; init; }
	[JsonPropertyName("community_item_class")]
	public int? CommunityItemClass { get; init; }
	[JsonPropertyName("community_item_type")]
	public int? CommunityItemType { get; init; }
	[JsonPropertyName("point_cost")]
	public string? PointCost { get; init; }
	[JsonPropertyName("timestamp_created")]
	public uint? TimestampCreated { get; init; }
	[JsonPropertyName("timestamp_updated")]
	public uint? TimestampUpdated { get; init; }
	[JsonPropertyName("timestamp_available")]
	public uint? TimestampAvailable { get; init; }
	[JsonPropertyName("timestamp_available_end")]
	public uint? TimestampAvailableEnd { get; init; }
	[JsonPropertyName("quantity")]
	public string? Quantity { get; init; }
	[JsonPropertyName("internal_description")]
	public string? InternalDescription { get; init; }
	[JsonPropertyName("active")]
	public bool? Active { get; init; }
	[JsonPropertyName("community_item_data")]
	public CommunityItem? CommunityItemData { get; init; }
	[JsonPropertyName("usable_duration")]
	public int? UsableDuration { get; init; }
	[JsonPropertyName("bundle_discount")]
	public int? BundleDiscount { get; init; }
}
internal sealed record CommunityItem {
	[JsonPropertyName("item_name")]
	public string? ItemName { get; init; }
	[JsonPropertyName("item_title")]
	public string? ItemTitle { get; init; }
	[JsonPropertyName("item_description")]
	public string? ItemDescription { get; init; }
	[JsonPropertyName("item_image_small")]
	public string? ItemImageSmall { get; init; }
	[JsonPropertyName("item_image_large")]
	public string? ItemImageLarge { get; init; }
	[JsonPropertyName("animated")]
	public bool? Animated { get; init; }
}
#pragma warning restore CA1812
