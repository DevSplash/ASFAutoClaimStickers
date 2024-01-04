# ASFAutoClaimStickers
ArchiSteamFarm plugin for claiming stickers automatically.
## Config
Plugin config is located in ASF.json: 
```json
{
    //ASF global config
    ...
    "AutoClaimStickersInterval": 360,
    "AutoClaimStickersBlacklist": [
        "bot1",
        "bot2",
        ...
    ]
}
```
### `AutoClaimStickersInterval`
Optional parameter of type `ushort` with default value of `360`. The plugin will check and claim stickers every `AutoClaimStickersInterval` minutes and will not check when the value is `0`.
### `AutoClaimStickersBlacklist`
Optional parameter of type `ImmutableHashSet<string>` with default value of `[]`. The plugin will always ignore blacklisted bots.
