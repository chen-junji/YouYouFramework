YouYou框架QQ交流群：185187673 <br>

YouYou框架使用教程地址：https://www.bilibili.com/video/BV1Fu411s7MV/ <br>

技术支持QQ(收费):2925856889 <br>


本框架不包含联机通讯和后端框架, 如果需要做联机游戏 可额外接入后端框架 推荐使用GameDesigner框架 本人使用此框架上线过多个游戏 几乎可以完美适配 不需要改任何代码 <br>
后端框架地址：https://gitee.com/leng_yue/GameDesigner?_from=gitee_search <br>

 **简介** 
------------
- 这是一款适合开发商业游戏的U3D代码框架，分别参考了多款主流代码框架 <br>
- 框架主要对游戏开发过程中常用模块进行了封装，很大程度地规范开发过程、加快开发速度并保证产品质量 <br>
- 追求轻量化，上手难度相对比较简单，模块之间的依赖比较少， 方便自行删除或扩展模块 <br>
- 基于YooAsset的资源管理, 支持HybridCLR的代码热更新 <br>
- 支持UniTask异步编程，告别委托回调式地狱写法 <br>
- 在PC 安卓 IOS 微信小游戏等平台均有上线项目 没有框架层面的兼容问题 <br>

 **原作者: 饭饭**<br>
 **框架开源迭代: 五杀时间到了** 

在最新的 YouYou Framework 中, 包含以下内置模块. 

 **YouYouEditor(框架非运行时 编辑器扩展模块)**
------------------------------------
{
- 宏配置(Macro) : <br>
1.选择需要过滤的DebugLog信息

- 全局配置(Config) : <br>
1.Http失败重试次数<br>
2.AssetBunldle池释放间隔 等等

- 对象池分析(Pool Analyze) - 方便查看项目里各个池中的资源的资源计数和剩余释放时间.<br>
1.类对象池 <br>

}

**YouYouMain(框架运行时 启动模块 仅10多个脚本)**
---------------------------------------
{
- 调试器 (Reporter) - 基于Reporter实现，可在手机真机运行时，调出调试器窗口，便于查看运行时日志、调试信息等

- 检查更新 (CheckVersion) - 实现了资源检查更新和下载相关逻辑，并提供了默认的检查更新界面

- 代码热更新 (Hotfix) - 提供了基于HybridCLR的代码热更新, 几乎热更了框架和业务系统的所有代码

}

 **YouYouFramework（框架运行时 核心模块 仅60多个脚本）**
---------------------------------------
{
- 声音 (Audio) - 提供管理声音和声音组的功能，支持全局音量调节, 音效音量单独配表

- 数据表 (DataTable) - 将游戏数据以表格（Excel）的形式进行配置后，可以使用此模块读取数据表。数据表的格式是支持自定义的。

- 有限状态机 (FSM) - 提供创建、使用和销毁有限状态机的功能。方便管理多个状态机. 适合系统开发, 可以直接切状态, 相对简单. 

- Web 请求 (Http) - 提供使用短连接的功能，可以用 Get 或者 Post 方法向服务器发送请求并获取响应数据，可指定允许几个 Web 请求器进行同时请求。

- 资源 (Loader) - 支持同步加载和UniTask异步加载, 使用了一套完整的加载资源体系。像数据表、Shader、场景、界面、角色等任何资源，都可使用Loader进行加载, 并自动管理资源引用计数。

- 数据模型 (Model) - 基于MVC思想, 管理运行时的所有Model, 以便统一管理与释放运行时数据

- 本地数据存档 (PlayerPrefs) - 基于Unity官方PlayerPrefs的本地数据存档, 支持存Object对象，拥有更好的性能

- 对象池 (Pool) - 提供对象缓存池的功能，避免频繁地创建和销毁各种游戏对象，提高游戏性能。目前支持类对象池, 变量池, GameObject对象池.

- 流程 (Procedure) - 是贯穿游戏运行时整个生命周期的有限状态机。通过流程，将不同的游戏状态进行解耦将是一个非常好的习惯。对于网络游戏，你可能需要如检查资源流程、更新资源流程、检查服务器列表流程、选择服务器流程、登录服务器流程、创建角色流程等流程，而对于单机游戏，你可能需要在游戏选择菜单流程和游戏实际玩法流程之间做切换。如果想增加流程，只要派生自 ProcedureBase 类并实现自己的流程类即可使用。

- 场景 (Scene) - 提供场景管理的功能，可以同时加载多个场景，也可以随时卸载任何一个场景，从而很容易地实现场景的分部加载。

- 任务 (Task) - 提供了Action委托的分组管理, 支持"顺序链式调用", 在一定程度上解决了委托套娃的现象

- 界面 (UI) - 提供管理界面和界面组的功能，如 激活界面、改变界面层级 界面反切(返回上一个界面)等。界面使用结束后可以不立刻销毁(UI池)，从而等待下一次重新使用。

}

 **YouYouPackge（框架默认提供的工具包, 不需要可删除）**
---------------------------------------
{
- 输入系统 (Input) - 提供了跨平台的Input封装, 支持PC(键盘鼠标)和手机(触屏)实时切换, 并实现输入方和监听方的代码解耦合。

- GamePlay状态机(GamePlayFsm) - 适合开发GamePlay战斗的状态机, 在预设状态机时设定"状态连线" "状态切换条件"等等(参考Unity官方的动画状态机 但完全剥离动画系统 这样可以与Playable组合使用)

}


 **可选扩展工具包(为了让框架尽量轻量化, 需要各位自行下载接入)** 
------------------------------------
{
- 技能编辑器：https://gitee.com/chen_junji/SkillEditor
- Camera跟随 旋转 缩放: https://gitee.com/chen_junji/CameraFollow
- 摇杆: https://gitee.com/chen_junji/Joystick
- 新手引导系统：https://gitee.com/chen_junji/Guide
- UI无限滚动列表: https://gitee.com/chen_junji/UIScroller
- Playable自定义动画系统：https://gitee.com/chen_junji/playable-animator
- ECS框架(Youwant提供): https://gitee.com/chen_junji/EntitasExample
- 行为树框架(猫仙人提供)：https://gitee.com/chen_junji/BehaviourTree
- 红点系统(猫仙人提供)：https://gitee.com/chen_junji/ReddotTree
- 组件自动绑定工具(猫仙人提供)：https://gitee.com/chen_junji/component-auto-bind-tool

} 
