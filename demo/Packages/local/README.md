# MSP Unity SDK packages

MSP 依赖通过 `scripts/sync-msp-version.sh` 写入根 `manifest.json`（Unity 只允许在根 manifest 里使用 git URL，不能放在本地聚合包的 dependencies 里）。

## 升级 MSP 版本

在项目根目录（`unity/grid-light-unity`）运行：

```bash
MSP_VERSION=v4.5.0-rc.0 ./scripts/sync-msp-version.sh
# 或
./scripts/sync-msp-version.sh v4.5.0-rc.0
```

脚本会更新 `Packages/manifest.json` 和 `Packages/local/msp-version`。`MSP_VERSION` 带不带 `v` 前缀都可以。

当前 tag：见 `Packages/local/msp-version`

已启用 adapters：nova / google / facebook / liftoff / moloco

保存后在 Unity 中刷新 Package Manager（或重启 Editor），`packages-lock.json` 会自动更新。

## 测试参数

`MSPAdRequest` 使用 `TestParams` 指定测试网络：

```csharp
var request = new MSPAdRequest(placementId);
request.TestParams["test_ad"] = true;
request.TestParams["ad_network"] = "msp_nova";
```
