## 游戏主要模块
### Core
游戏的核心逻辑模块，负责管理游戏状态、玩家、食物以及游戏规则。
- **Game**：游戏运行的核心协调对象，负责组织游戏更新、玩家管理、食物生成、碰撞检测以及游戏状态处理。
- **Player**：管理玩家相关状态，并关联对应的 Snake。
- **Snake**：负责蛇的移动、转向以及技能状态管理。


### Rendering
游戏渲染模块，负责将游戏视图数据转换为终端画面。接口契约为`IWorldView`

渲染模块通过 `IWorldView` (包含`IPlayerView` ) 获取所需数据(不直接依赖具体的 Game、Player 对象)

本地游戏与 Guest 接收到的游戏数据可以共用同一套渲染逻辑。

内部按照绘制职责进一步划分：
- **Renderer**：渲染模块的统一入口，组织各个绘制模块的调用顺序。
- BoardRenderer：负责游戏世界的绘制，包括地图、食物和蛇。
- HudRenderer：负责游戏信息的绘制，包括玩家信息、分数和技能条。
- OverlayRenderer：负责覆盖界面的绘制，包括主菜单、暂停界面和结算界面。
- CanvasPainter：提供基础绘制方法，将字符和颜色写入 FrameBuffer。
- ConsolePresenter：负责比较前后两帧的画面差异，并通过 ANSI 转义序列将变化输出到终端。
- FrameBuffer：负责存储当前帧的字符与颜色数据。

*（ps：比较多，不建议细看，总之就是一个比较独立的模块）*


### Network
网络通信模块，负责 Host 与 Guest 之间的数据传输及相关处理。

**通信模块：**
- **MqttTransport**：基于 MQTTnet 封装的基础通信对象，提供连接、订阅、发布和消息接收功能。
- **Protocol**：使用 Protocol Buffers 定义网络通信的数据结构，例如 PlayerInput 和 WorldSnapshot。

**Gamerunner模块：**
- **WorldSnapshotFactory**：负责将本地游戏状态一键转换为 WorldSnapshot 数据对象,为发布提供元数据
- **WorldSnapshotBroadcaster**：负责管理 Host 端游戏状态的定时广播任务
- **RemoteInputProcessor**：负责解析远程玩家的输入信息，并将其转换为对应的游戏操作。

**GuestRunner模块：**
- **GuestInputBroadcaster**：负责 Guest 端的键盘输入信息广播


## Application
应用运行模块，负责组织各个模块的协作，控制游戏运行流程。

### Gamerunner
本地游戏/Host（若多人联机）的运行器，负责组织输入处理、游戏推进、画面渲染以及网络通信四个职责。
(适配本地单人/双人/多人联机等多模式)

- **游戏模块**：
	- 持续调用 `Game.UpdateGameIfdue()`，推进游戏模拟。
	- **处理本地键盘输入**，例如蛇的转向、加速、游戏暂停与退出。
- **渲染模块**：
	- 按照渲染频率调用 `Renderer.RenderFrame()`，更新终端画面。
- **通信模块**（Host 模式）：
	- 接收并处理 Guest 发送的远程输入信息（Topic：`snake/input`）。
	- 管理游戏状态的广播任务，将 WorldSnapshot 发布至网络（Topic：`snake/world`）。

### Guestrunner
轻量级的 Guest 客户端运行器，不负责本地游戏模拟与状态推进，主要负责协调键盘输入、网络通信和画面渲染。*（其实就是mini版的GameRunner）*

- **输入处理**
	- 将玩家操作封装为 `PlayerInput` 数据对象，并交由 `GuestInputBroadcaster` 处理。
- **通信模块**：
	- 接收：
		- 接收 Host 发布的 `WorldSnapshot`（Topic：`snake/world`）
		- 转换为 `GuestWorldView`，为Renderer提供数据
	- 发布：
		- 通过 `GuestInputBroadcaster` 管理输入队列与异步发布任务。
		- 将玩家输入发布至 MQTT（Topic：`snake/input`）
- **渲染模块**：
	- 使用 `GuestWorldView`（满足接口契约）提供的数据完成画面绘制。（调用方法和GameRunner一样）

---
### 其他模块
#### View
提供游戏视图的数据接口，作为 Core、Application 与 Rendering 之间的抽象层。

- **IWorldView**：定义游戏世界的视图接口，约定地图尺寸、玩家、食物和游戏状态等数据。（Renderer的接口契约）
- **IPlayerView**：定义玩家视图接口，约定玩家身份、分数、身体坐标和技能状态等数据。  
- **GuestWorldView / GuestPlayerView**：将 Guest 接收到的网络数据转换为对应的视图对象，实现上述接口，使 Guest 端能够复用 Rendering 模块。  

其中，Game 和 Player 分别提供本地游戏的视图接口实现。

#### Input
提供键盘输入`ConsoleKey`的扩展方法。

- **ConsoleKeyExtensions**：通过扩展方法将方向键和 WASD 按键映射为游戏中的 Direction。  (同时适配了本地和多人联机)


#### Config
负责游戏运行参数的配置。

- **GameConfig**：游戏配置的数据模型。  
- **configClassic.json**：定义地图尺寸、游戏模拟步长、移动速度、技能冷却、食物生成等参数。

#### Skin
负责玩家外观配置。所以每一条蛇的蛇头与蛇身的颜色都不一样(虽然现在只有两套)


