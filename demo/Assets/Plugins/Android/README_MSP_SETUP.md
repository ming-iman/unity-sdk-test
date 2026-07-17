# MSP Android 依赖说明

## External Dependency Manager（EDM4U）

已内置在 `Assets/ExternalDependencyManager/`（无需再从 Git URL 安装）。

重新打开 Unity 后，顶部菜单应出现：

**Assets → External Dependency Manager → Android Resolver → Force Resolve**

若仍看不到该菜单：

1. 等待 Unity 完成脚本编译（右下角进度条结束）
2. 查看 Console 是否有 `ExternalDependencyManager` 相关报错
3. 尝试 **Assets → Reimport All**

## 可以跳过 Force Resolve 吗？

可以。`Assets/Plugins/Android/mainTemplate.gradle` 已包含 MSP 所需 Maven 依赖：

- `ai.themsp:msp-core:4.5.0`
- `ai.themsp:prebid-adapter:4.5.0`
- `ai.themsp:nova-adapter:4.5.0`
- `ai.themsp:google-adapter:4.5.0`
- `ai.themsp:facebook-adapter:4.5.0`
- `ai.themsp:liftoff-adapter:4.5.0`
- `ai.themsp:moloco-adapter:4.5.0`

若未改 MSP 包版本，可直接 **File → Build Settings → Android → Build**，不必先 Force Resolve。

升级 MSP UPM 包后，建议再执行一次 Force Resolve，让 `mainTemplate.gradle` 与包内 `Dependencies.xml` 同步。

## 其他检查

- **Player Settings → Android → Package Name**：`com.particlemedia.msp`
- **Custom Gradle Settings Template** 已启用（`settingsTemplate.gradle`）
- 构建前确认网络可访问 Maven Central / Google Maven
