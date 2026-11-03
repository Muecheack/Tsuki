# 美术资源与授权状态

本项目**代码**以 [MIT](LICENSE) 授权。但**图片资源不随代码授权**——它们多数来自网络，
没有可再分发的授权，正在逐批替换。这份文件记录现状，避免有人误以为它们也是 MIT。

## 一句话结论

> 代码可以随便用；**图片请自己换**。

---

## 资源清单

### 🔴 待替换：来源不明（从网络收集，无授权）

| 路径 | 用途 | 说明 |
|---|---|---|
| `cs/cs/cs/Resource/Avatar/*.jpg` `.png`（18 张） | 后端头像库，AI 给每个角色挑一张 | 文件名形如 `thumb.400_0.jpg`、`OIP-C.jpg`、`20200229054606_anihu.jpg`，是典型的图站/图库快照命名，可判定为网络采集 |
| `Face/bot-face/src/renderer/src/assets/neko.png` | 主界面背景 | 网络收集 |
| `Face/bot-face/src/renderer/src/assets/neko_mid.jpg` | 备用背景 | 网络收集 |
| `Face/bot-face/src/renderer/src/assets/muecheack1.jpg` | 头像 | 网络收集 |
| `Face/bot-face/src/renderer/src/assets/avator/neko_head.png` | 默认头像（官方账号、卡片、注册页） | 网络收集 |
| `Face/bot-face/src/renderer/src/assets/avator/twohiregird.jpg` | 头像 | 网络收集 |
| `Face/bot-face/src/renderer/src/assets/avator/xiduoqiandai.jpg` | 头像 | 网络收集 |

> 这些图片的著作权属于各自的作者。本项目**没有**、也无法代表作者授予任何使用权。
> 保留它们仅作为开发阶段的占位内容。

### 🟡 待确认：项目图标

| 路径 | 用途 |
|---|---|
| `Face/bot-face/build/tsuki_ico.ico` | exe 与安装器图标 |
| `Face/bot-face/build/tsuki_ic_pn.png` | 图标源图 |
| `Face/bot-face/resources/tsuki_ic_pn.png` | Linux 窗口图标 |

如果是自己绘制或由 AI 工具生成，请在各平台条款允许的范围内确认可再分发；
如果是网络收集，同样需要替换。

### 🟢 安全：第三方开源资源

| 路径 | 来源 | 许可 |
|---|---|---|
| `Face/bot-face/src/renderer/src/assets/electron.svg` | electron-vite 模板 | MIT |
| `Face/bot-face/src/renderer/src/assets/wavy-lines.svg` | electron-vite 模板 | MIT |
| `Face/bot-face/build/icon.icns` | Electron 默认图标（待替换为项目图标） | MIT |
| `imgs/*.png`（3 张） | 本项目界面截图（截图里出现的第三方图仍受上表约束） | 项目自有 |

代码依赖的第三方库（Vue / Pinia / vfluent3 / Flask / openai SDK 等）各自遵循其原许可，
其中 `@creatorsn/vfluent3` 为 MIT。

---

## 替换计划

头像系统的核心机制是"**文字描述**"：AI 依据 `AvatarUrl.Description`（如"灰发蓝瞳可爱猫娘少女"）
选择头像，图片只负责展示。所以图片可以整体替换，不影响功能。

| 步骤 | 做法 |
|---|---|
| 1 | 头像库换成 CC0 / 自行生成的一组图，保持**文件名与 `AvatarUrl.Url` 一一对应**（改文件名会导致老用户数据里的路径失效） |
| 2 | 界面背景与默认头像换成程序生成的占位（CSS 渐变 / SVG），或 CC0 素材 |
| 3 | 图标换成自己绘制或 AI 生成并确认可再分发的版本 |

推荐的合法素材来源：

- **DiceBear**（<https://www.dicebear.com>）— 程序化头像生成，多数风格为 CC0/MIT，可自托管
- **OpenGameArt**（<https://opengameart.org>）— 按 CC0 / CC-BY 筛选
- **Kenney**（<https://kenney.nl>）— 游戏与 UI 素材，CC0

---

## 历史版本

> ⚠️ 这些图片已经存在于 Git 历史的早期提交中（截至 `7cef673`）。
> 仅从当前工作区删除**不会**让它们从历史中消失，GitHub 上仍可检出。
> 如需彻底清除，必须重写历史（`git filter-repo`）或重建仓库，并重新打包发布。

---

## 侵权处理

如果你是某张图片的作者或权利人，认为本项目未经许可使用了你的作品，
请联系 **1704156528@qq.com**（或在本仓库开 Issue），我会在确认后**立即移除**。
