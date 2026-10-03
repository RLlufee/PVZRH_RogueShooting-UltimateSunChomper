<div align="center">

# PVZRH_RogueShooting-UltimateSunChomper

**终焉级圣樱战神的「诸神进化」附属插件**

让圣樱战神进入诸神进化模式，获得专属词条、星辉强化，以及每行坚果滚动攻击。

<p>
  <a href="https://github.com/RLlufee/PVZRH_RogueShooting-UltimateSunChomper/stargazers"><img src="https://img.shields.io/github/stars/RLlufee/PVZRH_RogueShooting-UltimateSunChomper?style=flat-square&logo=github&label=Stars" alt="GitHub Stars"></a>
  <a href="https://github.com/RLlufee/PVZRH_RogueShooting-UltimateSunChomper/commits/main"><img src="https://img.shields.io/github/last-commit/RLlufee/PVZRH_RogueShooting-UltimateSunChomper?style=flat-square&logo=git&label=Last%20commit" alt="Last commit"></a>
  <img src="https://img.shields.io/badge/.NET-6.0-512BD4?style=flat-square&logo=dotnet&logoColor=white" alt=".NET 6">
  <img src="https://img.shields.io/badge/BepInEx-IL2CPP-6D4AFF?style=flat-square" alt="BepInEx IL2CPP">
  <a href="LICENSE"><img src="https://img.shields.io/badge/License-Apache--2.0-2ea44f?style=flat-square&logo=apache" alt="Apache 2.0 License"></a>
</p>

</div>

## 项目简介

这是一个独立的 BepInEx IL2CPP 附属 DLL，基于 `ShootingMODGeneralLib` 为终焉级圣樱战神接入诸神进化路线。插件保留圣樱本体的攻击、吞食、点击和坚果机制，只在诸神进化模式下叠加本项目的强化效果。

> [!IMPORTANT]
> 本项目依赖圣樱本体插件、`ShootingMODGeneralLib` 和 BepInEx IL2CPP。请先确认这些前置组件能正常加载，再安装本插件。

## 功能一览

| 模块 | 效果 |
| --- | --- |
| 诸神进化路线 | 为圣樱注册专属进化路线，支持输出 / 防御定位 |
| 质变：终焉意志·圣樱 | 索敌范围 **+2 格**，诸神进化模式下攻击倍率提升 |
| 质变：大胃袋 | 吞食消化时间降为 **5 秒**，回复效果提高为 **3 倍** |
| 超进化：星辉 | 吞食消化固定为 5 秒；成功吞食时在每行左侧生成巨型圣樱坚果 |
| 点击联动 | 点击星辉圣樱时，同样触发每行坚果生成；本体点击效果完整保留 |

## 效果说明

### 专属词条

- **终焉意志·圣樱**：扩大索敌范围，并提高诸神进化模式下的攻击表现。
- **大胃袋**：把吞食消化时间缩短至 5 秒，同时将回复效果提高到原来的 3 倍。

两个词条由附属 DLL 通过 `ShootingMODGeneralLib` 注册，只作用于诸神进化模式，不覆盖普通模式下本体的同名词条。

### 星辉强化

获得星辉后，圣樱会额外获得以下效果：

1. 吞食消化时间固定为 5 秒。
2. 每次成功吞食时，在草坪每一行的最左侧生成一颗巨型圣樱坚果。
3. 坚果沿对应行向右滚动并攻击僵尸。
4. 点击星辉圣樱时，同样触发每行坚果生成。

### 点击效果

圣樱本体原有的点击行为继续生效：消耗阳光、回复本体生命，并生成本体原有的巨型圣樱坚果。在诸神进化模式且获得星辉后，额外追加每行坚果滚动攻击。

## 安装

1. 安装并确认 **BepInEx IL2CPP** 可以正常启动游戏。
2. 安装圣樱本体插件 `UltimateSunChomper.BepInEx.dll`。
3. 安装前置库 `ShootingMODGeneralLib.dll`。
4. 下载或构建 `RogueShooting-UltimateSunChomper.dll`，放入游戏目录的 `BepInEx/plugins/`。
5. 启动游戏，在 BepInEx 日志中确认出现“诸神进化模式词条注册完成”。

### 从源码构建

```bash
dotnet build -c Release
```

项目目标框架为 `.NET 6.0`。当前 `.csproj` 使用本机游戏目录中的 DLL 引用，首次构建时请按自己的 BepInEx、圣樱本体和 `ShootingMODGeneralLib` 路径调整引用位置。

## 依赖与兼容性

| 依赖 | 用途 |
| --- | --- |
| BepInEx IL2CPP | 插件加载与运行环境 |
| `UltimateSunChomper.BepInEx.dll` | 圣樱本体组件与原生玩法 |
| `ShootingMODGeneralLib.dll` | 诸神进化路线、词条和星辉接口 |
| Harmony | 对本体组件进行运行时补丁 |

- 作为独立附属 DLL 工作，不需要修改圣樱本体源码。
- 普通模式下的圣樱能力和本体词条不受本项目影响。
- 若游戏版本、圣樱本体或前置库 API 发生变化，可能需要重新编译适配。
- 本项目当前按作者本地的《植物大战僵尸融合版》4.0 环境开发，其他版本请自行验证。

## 开发提示

插件入口位于 `RogueShooting_UltimateSunChomper/Core.cs`，Harmony 补丁位于 `RogueShooting_UltimateSunChomper/UltimateSunChomperPatch.cs`。运行时日志带有 `[圣樱诸神进化附属]` 前缀，遇到兼容性问题时可以先从 BepInEx 日志定位。

## 许可证

本项目以 [Apache License 2.0](LICENSE) 发布。圣樱本体、游戏本体及其他前置库的版权和许可归其各自作者所有。

## 致谢

- 感谢恒小暝提供 `ShootingMODGeneralLib` 与诸神进化框架。
- 感谢圣樱本体插件作者提供可扩展的组件接口。
