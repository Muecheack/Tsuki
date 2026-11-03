# 美术资源说明

本项目**代码**以 [MIT](LICENSE) 授权；**图片资源不随代码授权**。

## 现状与声明

仓库与安装包中的头像、背景等图片，是开发阶段从网络收集的素材，**未取得原作者授权**。

- 本项目是**个人学习项目**，不提供商业服务，不通过本项目获利
- 这些图片仅作为界面演示使用，作者不主张对它们的任何权利
- 任何人 fork / 二次开发 / 再分发本项目时，**请自行替换这些图片**，不要沿用
- 如权利人认为存在侵权，请联系 **1704156528@qq.com** 或在本仓库开 Issue，
  确认后会在收到通知后**立即移除**相关资源（包括 Release 中的安装包）

以上声明说明使用意图，但**不能替代授权**。如果本项目将来涉及商用或正式分发，
这些图片必须先替换掉。

---

## 资源清单

### 网络收集（无授权，仅演示用）

| 路径 | 用途 |
|---|---|
| `cs/cs/cs/Resource/Avatar/*.jpg` `.png`（18 张） | 后端头像库，AI 给每个角色挑一张 |
| `Face/bot-face/src/renderer/src/assets/neko.png` | 主界面背景 |
| `Face/bot-face/src/renderer/src/assets/neko_mid.jpg` | 备用背景 |
| `Face/bot-face/src/renderer/src/assets/muecheack1.jpg` | 头像 |
| `Face/bot-face/src/renderer/src/assets/avator/neko_head.png` | 默认头像（官方账号、卡片、注册页） |
| `Face/bot-face/src/renderer/src/assets/avator/twohiregird.jpg` | 头像 |
| `Face/bot-face/src/renderer/src/assets/avator/xiduoqiandai.jpg` | 头像 |
| `Face/bot-face/build/tsuki_ico.ico`、`tsuki_ic_pn.png`、`resources/tsuki_ic_pn.png` | 应用图标 |

### 第三方开源资源（可自由使用）

| 路径 | 来源 | 许可 |
|---|---|---|
| `Face/bot-face/src/renderer/src/assets/electron.svg` | electron-vite 模板 | MIT |
| `Face/bot-face/src/renderer/src/assets/wavy-lines.svg` | electron-vite 模板 | MIT |
| `Face/bot-face/build/icon.icns` | Electron 默认图标 | MIT |
| `imgs/*.png`（3 张） | 本项目界面截图（截图里出现的第三方图仍受上表约束） | 项目自有 |

### 代码依赖

Vue / Pinia / Vue Router / vue-i18n / axios / vue3-emoji / Flask / openai SDK 等各自遵循原许可；
`@creatorsn/vfluent3` 为 MIT。

---

## 关于 Git 历史

这些图片从早期提交起就存在于 Git 历史中。**仅删除当前工作区的文件不会让它们从历史消失**，
通过 `git log` 仍可检出。彻底清除需要重写历史（`git filter-repo`）或重建仓库。

作者已知悉该状态，当前选择保留（见上方声明）。

---

## 如果将来要替换

头像系统的核心是"**文字描述**"：AI 依据 `AvatarUrl.Description`（如"灰发蓝瞳可爱猫娘少女"）
选择头像，图片只负责展示。所以图片可以整体替换，不影响功能。

替换时注意：**保持文件名与 `AvatarUrl.Url` 一一对应**，否则老用户数据库里的路径会失效。
建议先把数据库里的 `AvatarUrl` 表导出对一遍。

可用的合法素材来源：

- **DiceBear**（<https://www.dicebear.com>）— 程序化头像生成，多数风格 CC0/MIT，可自托管
- **OpenGameArt**（<https://opengameart.org>）— 按 CC0 / CC-BY 筛选
- **Kenney**（<https://kenney.nl>）— UI 与游戏素材，CC0
