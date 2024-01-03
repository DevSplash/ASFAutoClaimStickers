using Newtonsoft.Json;

namespace AutoClaimStickers;
#pragma warning disable CA1812
internal sealed record CanClaimItemData {
	[JsonProperty("response", Required = Required.Always)]
	public CanClaimItemResponse? Response { get; set; }
}
internal sealed record CanClaimItemResponse {
	[JsonProperty("can_claim")]
	public bool? CanClaim { get; set; }
	[JsonProperty("next_claim_time")]
	public int? NextClaimTime { get; set; }
	[JsonProperty("reward_item")]
	public RewardItem? RewardItem { get; set; }
}
internal sealed record ClaimItemData {
	[JsonProperty("response", Required = Required.Always)]
	public ClaimItemResponse? Response { get; set; }
}
internal sealed record ClaimItemResponse {
	[JsonProperty("communityitemid")]
	public long? CommunityItemId { get; set; }
	[JsonProperty("next_claim_time")]
	public int? NextClaimTime { get; set; }
	[JsonProperty("reward_item")]
	public RewardItem? RewardItem { get; set; }
}
#pragma warning disable IDE1006
internal sealed record RewardItem {
	public int? appid { get; set; }
	public int? defid { get; set; }
	public int? type { get; set; }
	public int? community_item_class { get; set; }
	public int? community_item_type { get; set; }
	public string? point_cost { get; set; }
	public int? timestamp_created { get; set; }
	public int? timestamp_updated { get; set; }
	public int? timestamp_available { get; set; }
	public int? timestamp_available_end { get; set; }
	public string? quantity { get; set; }
	public string? internal_description { get; set; }
	public bool? active { get; set; }
	public CommunityItemData? community_item_data { get; set; }
	public int? usable_duration { get; set; }
	public int? bundle_discount { get; set; }
}
internal sealed record CommunityItemData {
	public string? item_name { get; set; }
	public string? item_title { get; set; }
	public string? item_description { get; set; }
	public string? item_image_small { get; set; }
	public string? item_image_large { get; set; }
	public bool? animated { get; set; }
}
#pragma warning restore IDE1006
#pragma warning restore CA1812
