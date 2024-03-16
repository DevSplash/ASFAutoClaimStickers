using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Plugins.Interfaces;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Steam.Integration;
using ArchiSteamFarm.Web.Responses;

namespace AutoClaimStickers;

[Export(typeof(IPlugin))]
internal sealed class AutoClaimStickers : IPlugin, IASF, IDisposable {
	public string Name => nameof(AutoClaimStickers);
	public Version Version => typeof(AutoClaimStickers).Assembly.GetName().Version ?? throw new InvalidOperationException(nameof(Version));
	private static Uri SteamApiURL => new("https://api.steampowered.com");
	private static Uri RefererURL => new(ArchiWebHandler.SteamStoreURL, "/category/casual");
	private ushort Interval = 360; // 6 * 60
	private ImmutableHashSet<string> Blacklist = [];
	private Timer? AutoClaimTimer;
	private static readonly SemaphoreSlim AutoClaimSemaphore = new(1, 1);
	private static readonly SemaphoreSlim BotSemaphore = new(3, 3);
	private static readonly JsonSerializerOptions SerializerOptions = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

	public Task OnLoaded() {
		AutoClaimTimer = new(OnAutoClaimTimer);
		return Task.CompletedTask;
	}
	public Task OnASFInit(IReadOnlyDictionary<string, JsonElement>? additionalConfigProperties = null) {
		if (additionalConfigProperties == null) {
			return Task.CompletedTask;
		}
		if (AutoClaimTimer == null) {
			throw new InvalidOperationException(nameof(AutoClaimTimer));
		}
		foreach ((string configProperty, JsonElement configValue) in additionalConfigProperties) {
			switch (configProperty) {
				case $"{nameof(AutoClaimStickers)}{nameof(Interval)}" when configValue.ValueKind == JsonValueKind.Number:
					if (configValue.TryGetUInt16(out ushort iterval)) {
						lock (AutoClaimSemaphore) {
							Interval = iterval;
						}
					}
					break;
				case $"{nameof(AutoClaimStickers)}{nameof(Blacklist)}" when configValue.ValueKind == JsonValueKind.Array:
					ImmutableHashSet<string>? blackList = null;
					try {
						blackList = configValue.EnumerateArray().Select(item => item.GetString() ?? string.Empty).Where(item => !string.IsNullOrWhiteSpace(item)).ToImmutableHashSet();
						lock (Blacklist) {
							Blacklist = blackList;
						}
					} catch (Exception) {
						ASF.ArchiLogger.LogGenericWarning($"Invalid config property: {configProperty}");
					}
					break;
				default:
					break;
			}
		}
		lock (AutoClaimSemaphore) {
			_ = Interval != 0
				? AutoClaimTimer.Change(TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(Interval))
				: AutoClaimTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
		}
		return Task.CompletedTask;
	}
	private async Task AutoClaim() {
		if (!await AutoClaimSemaphore.WaitAsync(0).ConfigureAwait(false)) {
			return;
		}
		try {
			HashSet<Bot>? bots = Bot.GetBots("ASF");
			if (bots == null) {
				return;
			}
			List<Task> tasks = [];
			foreach (Bot bot in bots) {
				if (bot.IsConnectedAndLoggedOn) {
					lock (Blacklist) {
						if (Blacklist.Any(item => item.Equals(bot.BotName, StringComparison.OrdinalIgnoreCase))) {
							continue;
						}
					}
					tasks.Add(Task.Run(async () => {
						await BotSemaphore.WaitAsync().ConfigureAwait(false);
						try {
							await ClaimItemTask(bot).ConfigureAwait(false);
						} finally {
							_ = BotSemaphore.Release();
						}
					}));
				}
			}
			await Task.WhenAll([.. tasks]).ConfigureAwait(false);
		} finally {
			_ = AutoClaimSemaphore.Release();
		}
	}
	private static async Task ClaimItemTask(Bot bot) {
		string? token = bot.AccessToken;
		if (string.IsNullOrWhiteSpace(token)) {
			ASF.ArchiLogger.LogGenericWarning($"[{bot.BotName}] Missing token.");
			return;
		}
		if (!await CanClaimItem(bot, token).ConfigureAwait(false)) {
			ASF.ArchiLogger.LogGenericInfo($"[{bot.BotName}] No reward to claim.");
			return;
		}
		(bool success, ClaimItemResponse? response) = await ClaimItem(bot, token).ConfigureAwait(false);
		if (success) {
			CommunityItem? rewardItemData = response!.RewardItem?.CommunityItemData;
			ASF.ArchiLogger.LogGenericInfo($"[{bot.BotName}] Claim success! ItemId: {response.CommunityItemId}{(rewardItemData == null ? "" : $"({rewardItemData.ItemName})")}");
		} else {
			ASF.ArchiLogger.LogGenericWarning($"[{bot.BotName}] Claim failed! Response: {JsonSerializer.Serialize(response, SerializerOptions)}");
		}
	}
	private static async Task<bool> CanClaimItem(Bot bot, string token) {
		Uri uri = new(SteamApiURL, $"/ISaleItemRewardsService/CanClaimItem/v1?access_token={token}");
		ObjectResponse<CanClaimItemData>? response = await bot.ArchiWebHandler.UrlGetToJsonObjectWithSession<CanClaimItemData>(uri, referer: RefererURL).ConfigureAwait(false);
		return response != null && response.StatusCode.IsSuccessCode() && (response.Content?.Response?.CanClaim ?? false);
	}
	private static async Task<(bool success, ClaimItemResponse? response)> ClaimItem(Bot bot, string token) {
		Uri uri = new(SteamApiURL, $"/ISaleItemRewardsService/ClaimItem/v1?access_token={token}");
		ObjectResponse<ClaimItemData>? response = await bot.ArchiWebHandler.UrlPostToJsonObjectWithSession<ClaimItemData>(uri, data: null, referer: RefererURL, session: ArchiWebHandler.ESession.None).ConfigureAwait(false);
		if (response == null || !response.StatusCode.IsSuccessCode()) {
			return (false, response?.Content?.Response);
		}
		ClaimItemData? result = response.Content;
		return (!string.IsNullOrWhiteSpace(result?.Response?.CommunityItemId), result?.Response);
	}
	private async void OnAutoClaimTimer(object? state = null) => await AutoClaim().ConfigureAwait(false);
	public void Dispose() => AutoClaimTimer?.Dispose();
}
