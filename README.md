## 游戏主要模块
### Core
游戏的核心逻辑模块，负责管理游戏状态、玩家、食物以及游戏规则，不依赖其他模块独立
- [Game](./Snake.Core/Core/Game.cs)：游戏运行的核心协调对象，负责组织游戏更新、玩家管理、食物生成、碰撞检测以及游戏状态处理。
- [Player](./Snake.Core/Core/Player.cs)：管理玩家相关状态，并关联对应的 Snake。
- [Snake](./Snake.Core/Core/Snake.cs)：负责蛇的移动、转向以及技能状态管理。


## Application
应用运行模块，负责组织各个模块的协作，控制游戏运行流程。

### [Gamerunner](Snake.App/Application/GameRunner.cs)
本地游戏/Host的运行器，负责组织输入处理、游戏推进、画面渲染以及网络通信四个职责。
(适配本地单人/双人/多人联机等多模式)

- **游戏模块**：
	- 持续调用 `Game.UpdateGameIfdue()`，推进游戏模拟。
	- **订阅并处理本地键盘输入事件**，例如蛇的转向、加速、游戏暂停与退出。
- **渲染模块**：
	- 使用 `Renderer.RenderFrame(WorldFrame frame)`，通过订阅`WorldFrame`，更新终端画面。
- **通信模块**（Host 模式）：
	- 接收并处理 Guest 发送的远程输入信息（Topic：`snake/input`）。
	- 管理游戏状态的广播任务，将 WorldSnapshot 发布至网络（Topic：`snake/world`）。

### [GuestRunner](Snake.App/Application/GuestRunner.cs)
轻量级的 Guest 客户端运行器，主要负责协调键盘输入、网络通信和画面渲染。

- **输入处理**
	- 将玩家操作封装为 `PlayerInput` 数据对象，并交由 `GuestInputBroadcaster` 处理。
- **通信模块**：
	- 接收：
		- 接收 Host 发布的 `WorldSnapshot`（Topic：`snake/world`）
		- 通过 `WorldSnapshotFactory.ToWorldFrame()` 转换为快照，为 Renderer 提供数据。
	- 发布：
		- 通过 `GuestInputBroadcaster` 管理输入队列与异步发布任务。
		- 将玩家输入发布至 MQTT（Topic：`snake/input`）
- **渲染模块**：
	- 使用 `WorldFrame` 完成画面绘制，与 GameRunner 共用同一个渲染入口。



### Rendering
渲染模块通过 `WorldFrame` 获取渲染终端画面所需的所有数据。

 `WorldFrame`作为游戏核心作为游戏核心与渲染器的数据传输对，具体定义见 [WorldFrame.cs](./Snake.Core/Views/WorldFrame.cs)

内部按照绘制职责进一步划分：
- [Renderer](Snake.Rendering/Rendering/Renderer.cs)：渲染模块的统一入口，向外提供 `public void RenderFrame(WorldFrame frame)`接口
- BoardRenderer：负责游戏世界的绘制，包括地图、食物和蛇。
- HudRenderer：负责游戏信息的绘制，包括玩家信息、分数和技能条。
- OverlayRenderer：负责覆盖界面的绘制，包括主菜单、暂停界面和结算界面。
- CanvasPainter：提供基础绘制方法，将字符和颜色写入 FrameBuffer。
- ConsolePresenter：负责比较前后两帧的画面差异，并通过 ANSI 转义序列将变化输出到终端。
- FrameBuffer：负责存储当前帧的字符与颜色数据。


## Network
网络通信模块，负责 Host 与 Guest 之间的数据传输及相关处理。

**通信模块：**
- **MqttTransport**：基于 MQTTnet 封装的基础通信对象，提供连接、订阅、发布和消息接收功能。
- **Protocol**：使用 Protocol Buffers 定义网络通信的数据结构，包含 [PlayerInput](Snake.Network/Network/Protocol/player_input.proto) 和 [WorldSnapshot](Snake.Network/Network/Protocol/world_snapshot.proto)。

**[Gamerunner](Snake.App/Application/GameRunner.cs) 模块：**
  - [WorldFrameFactory](Snake.Core/Views/WorldFrameFactory.cs)：负责将游戏数据`WorldFrame`转换为 `WorldSnapshot`，作为**Mqtt的数据传输对象**
- [WorldSnapshotBroadcaster](Snake.Network/Network/WorldSnapshotBroadcaster.cs)：负责管理 Host 端游戏数据的广播任务
- [RemoteInputProcessor](Snake.Network/Network/RemoteInputProcessor.cs)：负责处理远程玩家的输入信息，并调用`Game.cs`提供的操作接口

**[GuestRunner](Snake.App/Application/GuestRunner.cs) 模块：**
- [GuestInputBroadcaster](Snake.Network/Network/GuestInputBroadcaster.cs)：负责 Guest 端的键盘输入信息广播


## Godot
Godot游戏引擎模块，正在施工中。

将游戏运行在godot中，使用2D界面交互与3D游戏画面渲染，同时兼容与终端联机


