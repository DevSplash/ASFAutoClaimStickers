# ASFAutoClaimStickers
ArchiSteamFarm plugin for claiming stickers automatically.
## Config
Plugin config is located in ASF.json: 
```json
{
    //ASF global config
    ...
    "AutoClaimStickersInterval": 360
}
```
### `AutoClaimStickersInterval`
Optional parameter of type `ushort` with default value of `360`. The plugin will check and claim stickers every `AutoClaimStickersInterval` minutes and will not check when the value is `0`.
