# EDM4U（External Dependency Manager）

本目录为 Google External Dependency Manager，用于解析 Android Maven / iOS CocoaPods 依赖。

## 菜单位置

Unity 顶部菜单栏（不是 Package Manager 窗口内）：

**Assets → External Dependency Manager → Android Resolver → Force Resolve**

## 首次导入

1. 重新打开工程或 **Assets → Reimport All**
2. 等待 Console 编译完成
3. 若弹出 Version Handler 提示，选择 **Enable** / **Update**

## 说明

MSP 的 Android 依赖已写入 `Assets/Plugins/Android/mainTemplate.gradle`，不执行 Force Resolve 也可尝试直接打 Android 包。
