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
	public int? NextClaimTime { get; init; }
	[JsonPropertyName("reward_item")]
	public RewardItem? RewardItem { get; init; }
}
internal sealed record ClaimItemData {
	[JsonPropertyName("response"), JsonRequired]
	public ClaimItemResponse? Response { get; init; }
}
internal sealed record ClaimItemResponse {
	[JsonPropertyName("communityitemid")]
	public long? CommunityItemId { get; init; }
	[JsonPropertyName("next_claim_time")]
	public int? NextClaimTime { get; init; }
	[JsonPropertyName("reward_item")]
	public RewardItem? RewardItem { get; init; }
}
#pragma warning disable IDE1006
internal sealed record RewardItem {
	public int? appid { get; init; }
	public int? defid { get; init; }
	public int? type { get; init; }
	public int? community_item_class { get; init; }
	public int? community_item_type { get; init; }
	public string? point_cost { get; init; }
	public int? timestamp_created { get; init; }
	public int? timestamp_updated { get; init; }
	public int? timestamp_available { get; init; }
	public int? timestamp_available_end { get; init; }
	public string? quantity { get; init; }
	public string? internal_description { get; init; }
	public bool? active { get; init; }
	public CommunityItemData? community_item_data { get; init; }
	public int? usable_duration { get; init; }
	public int? bundle_discount { get; init; }
}
internal sealed record CommunityItemData {
	public string? item_name { get; init; }
	public string? item_title { get; init; }
	public string? item_description { get; init; }
	public string? item_image_small { get; init; }
	public string? item_image_large { get; init; }
	public bool? animated { get; init; }
}
#pragma warning restore CA1812
