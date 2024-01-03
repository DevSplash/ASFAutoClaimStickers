using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Plugins.Interfaces;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Steam.Integration;
using ArchiSteamFarm.Web.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutoClaimStickers;

[Export(typeof(IPlugin))]
internal sealed class AutoClaimStickers : IPlugin, IASF {
	public string Name => nameof(AutoClaimStickers);
	public Version Version => typeof(AutoClaimStickers).Assembly.GetName().Version ?? throw new InvalidOperationException(nameof(Version));
	private static Uri SteamApiURL => new("https://api.steampowered.com");
	private static Uri RefererURL => new(ArchiWebHandler.SteamStoreURL, "/category/casual");
	private ushort Interval = 360; // 6 * 60
	private static readonly Timer AutoClaimTimer = new(OnAutoClaimTimer);
	private static readonly SemaphoreSlim AutoClaimSemaphore = new(1, 1);
	private static readonly SemaphoreSlim BotSemaphore = new(3, 3);
	public Task OnLoaded() => Task.CompletedTask;

	public Task OnASFInit(IReadOnlyDictionary<string, JToken>? additionalConfigProperties = null) {
		if (additionalConfigProperties == null) {
			return Task.CompletedTask;
		}
		foreach ((string configProperty, JToken configValue) in additionalConfigProperties) {
			switch (configProperty) {
				case $"{nameof(AutoClaimStickers)}{nameof(Interval)}" when configValue.Type == JTokenType.Integer:
					Interval = configValue.Value<ushort>();
					break;
				default:
					break;
			}
		}
		if (Interval != 0) {
			lock (AutoClaimSemaphore) {
				_ = AutoClaimTimer.Change(TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(Interval));
			}
		} else {
			lock (AutoClaimSemaphore) {
				_ = AutoClaimTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
			}
		}
		return Task.CompletedTask;
	}
	private static async Task AutoClaim() {
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
		(bool success, string? token) = await bot.ArchiWebHandler.CachedAccessToken.GetValue().ConfigureAwait(false);
		if (!success || string.IsNullOrWhiteSpace(token)) {
			ASF.ArchiLogger.LogGenericWarning($"[{bot.BotName}] Missing token.");
			return;
		}
		if (!await CanClaimItem(bot, token).ConfigureAwait(false)) {
			ASF.ArchiLogger.LogGenericInfo($"[{bot.BotName}] No reward to claim.");
			return;
		}
		(success, ClaimItemResponse? response) = await ClaimItem(bot, token).ConfigureAwait(false);
		if (success) {
			CommunityItemData? rewardItemData = response!.RewardItem?.community_item_data;
			ASF.ArchiLogger.LogGenericInfo($"[{bot.BotName}] Claim success! ItemId: {response.CommunityItemId}{(rewardItemData == null ? "" : $"({rewardItemData.item_name})")}");
		} else {
			ASF.ArchiLogger.LogGenericWarning($"[{bot.BotName}] Claim failed! Response: {JsonConvert.SerializeObject(response, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })}");
		}
	}
	private static async Task<bool> CanClaimItem(Bot bot, string token) {
		Uri uri = new(SteamApiURL, $"/ISaleItemRewardsService/CanClaimItem/v1?access_token={token}");
		ObjectResponse<CanClaimItemData>? response = await bot.ArchiWebHandler.UrlGetToJsonObjectWithSession<CanClaimItemData>(uri, referer: RefererURL).ConfigureAwait(false);
		return response != null && response.StatusCode.IsSuccessCode() && (response.Content?.Response?.CanClaim ?? false);
	}
	private static async Task<(bool success, ClaimItemResponse? response)> ClaimItem(Bot bot, string token) {
		Uri uri = new(SteamApiURL, $"/ISaleItemRewardsService/ClaimItem/v1?access_token={token}");
		Dictionary<string, string> data = new(0, StringComparer.Ordinal);
		ObjectResponse<ClaimItemData>? response = await bot.ArchiWebHandler.UrlPostToJsonObjectWithSession<ClaimItemData>(uri, data: data, referer: RefererURL, session: ArchiWebHandler.ESession.None).ConfigureAwait(false);
		if (response == null || !response.StatusCode.IsSuccessCode()) {
			return (false, response?.Content?.Response);
		}
		ClaimItemData? result = response.Content;
		return (result?.Response?.CommunityItemId > 0, result?.Response);
	}
	private static async void OnAutoClaimTimer(object? state = null) => await AutoClaim().ConfigureAwait(false);
}
