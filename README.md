## 游戏主要模块
### Core
- 主要是以Game.cs为中心的游戏运行核心，组织玩家、食物生成以及游戏逻辑处理。

### Rendering
- 渲染模块。可以根据约定的接口数据协议绘制出游戏画面。
- 只依赖数据运行，不依赖其他主要对象，所以可以同时供给GameRunner和GuestRunner使用

### Network
- 负责网络通信的基础功能（MqttTransport）以及数据传输的通信协议管理（Proto）
- 负责Host-Gamerunner的网络处理模块：
	- 网络端的操作信息接收处理
	- 管理WorldSnapshot的信息发布任务

## 运行器 Application
### Gamerunner
主要拥有三个模块：游戏推进、渲染画面与网络通信（若进行多人联机），组织游戏运行的客户端的各个模块
- **游戏模块**：
	- 持续调用game.UpdateGameIfdue()
	- 处理本地键盘事件（比如snake的转向加速和game的暂停退出）
- **渲染模块**：
	- 按照帧率上限去调用renderer.RenderFrame(game)
- **通信模块**：
	- 接收并处理远程的输入事件(Topic：snake/input)
	- 管理网络发布的任务（对应上面的NetWork-第二个模块）

### Guestrunner
一个轻量的Guest游戏运行器，不负责游戏运行，主要组织Guest客户端的网络通信和画面渲染
- 通信模块：
	- 接收Host端传来的WorldSnapshot(Topic: snake/world)
- 渲染模块：
	- 处理转换数据对象，调用renderer.RenderFrame(GuestWorldView)

---
### 一些其他的地方
#### Views
主要是**Rendering**与**Application** 模块之间的协议处理，Application根据 这个协议将对应的数据提供给Renderer
- IWorldView：约定接口协议
- GuestWorldView 与 GuestPlayerView：为GuestRunner提供接口的数据适配器

#### Input
一个为GameRunner的键盘输入模块提供的Extension扩展

#### Config
主要负责游戏配置部分。
- *json*：游戏配置的数据
- GameConfig：游戏配置的数据协议

