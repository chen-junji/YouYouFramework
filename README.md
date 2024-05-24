YouYou框架QQ交流群：185187673 <br>

YouYou框架使用教程地址：https://www.bilibili.com/video/BV1Fu411s7MV/ <br>

技术支持QQ(收费):2925856889 <br>

 **简介** 
------------
- 这是一款适合开发商业级游戏的U3D代码框架，分别参考了多款主流代码框架 <br>
- 框架主要对游戏开发过程中常用模块进行了封装，很大程度地规范开发过程、加快开发速度并保证产品质量 <br>
- 上手难度相对比较简单，模块之间的依赖比较少， 方便自行删除或扩展模块 <br>
- 支持HybridCLR的代码热更新，AssetBundle资源管理 <br>
- 支持UniTask异步编程，告别委托回调式地狱写法 <br>

 **原作者: 饭饭**<br>
 **框架开源迭代: 五杀时间到了** 

在最新的 YouYou Framework 中, 包含以下内置模块. 

 **YouYouEditor(编辑器扩展界面)**
------------------------------------
{
- 资源打包配置(AssetBundle) : <br>
1.资源包版本号<br>
2.资源包加密<br>
3.设置文件夹内容打整包或散包, 如Excel,Lua脚本可以打成整包，角色、场景、特效、UI可以打成散包<br>
4.打包到【对应版本号/平台】文件夹内，生成"依赖关系文件"和"版本信息文件"，加载时自动读取

- 宏配置(Macro) : <br>
1.选择资源加载模式，如AssetBundle加载,AssetDatabase加载<br>
2.选择需要过滤的DebugLog信息

- 全局配置(Config) : <br>
1.Http失败重试次数<br>
2.AssetBunldle池释放间隔 等等

- 对象池分析(Pool Analyze) - 方便查看项目里各个池中的资源的资源计数和剩余释放时间.<br>
1.类对象池 <br>
2.AssetBundle池 <br>
3.Asset池

}

**YouYouMain(框架初始启动模块)**
---------------------------------------
{
- 调试器 (Reporter) - 基于Reporter实现，可在手机真机运行时，调出调试器窗口，便于查看运行时日志、调试信息等

- 下载 (Download) - 提供下载文件的功能，支持断点续传，并可指定允许几个下载器进行同时下载。更新资源时会主动调用此模块。

- 检查更新 (CheckVersion) - 实现了资源检查更新和下载相关逻辑，并提供了默认的检查更新界面

- 代码热更新 (Hotfix) - 提供了基于HybridCLR的代码热更新, 可以热更新YouYouFramework文件夹和YouYouScript文件夹内的所有代码

}

 **YouYouFramework（框架运行功能模块）**
---------------------------------------
{
- 事件 (Event) - 观察者模式思想, YouYouFramework 中的很多模块在完成操作后会抛出内置事件,用户也可以定义自己的游戏逻辑事件。

- 定时器 (Time) - 提供了定时功能，支持多种定时需求, 如复活倒计时, 技能CD等, 用于管理游戏运行时的各种定时器。

- 有限状态机 (FSM) - 提供创建、使用和销毁有限状态机的功能。方便管理多个状态机.

- 流程 (Procedure) - 是贯穿游戏运行时整个生命周期的有限状态机。通过流程，将不同的游戏状态进行解耦将是一个非常好的习惯。对于网络游戏，你可能需要如检查资源流程、更新资源流程、检查服务器列表流程、选择服务器流程、登录服务器流程、创建角色流程等流程，而对于单机游戏，你可能需要在游戏选择菜单流程和游戏实际玩法流程之间做切换。如果想增加流程，只要派生自 ProcedureBase 类并实现自己的流程类即可使用。

- 数据表 (DataTable) - 将游戏数据以表格（Excel）的形式进行配置后，可以使用此模块读取数据表。数据表的格式是支持自定义的。

- 模型 (Model) - 基于MVC思想, 管理运行时的所有Model, 以便统一管理与释放运行时数据

- Web 请求 (Http) - 提供使用短连接的功能，可以用 Get 或者 Post 方法向服务器发送请求并获取响应数据，可指定允许几个 Web 请求器进行同时请求。

- 本地化 (Localization) - 提供本地化功能，也就是我们平时所说的多语言。支持Text和Image的本地化

- 对象池 (Pool) - 提供对象缓存池的功能，避免频繁地创建和销毁各种游戏对象，提高游戏性能。目前支持类对象池, 变量池, GameObject对象池.

- 场景 (Scene) - 提供场景管理的功能，可以同时加载多个场景，也可以随时卸载任何一个场景，从而很容易地实现场景的分部加载。

- 资源 (Loader) - 支持同步加载和UniTask异步加载, 使用了一套完整的加载资源体系。像数据表、Shader、场景、界面、角色等任何资源，都可使用Loader进行加载。 提供了默认的内存管理策略: 基于主Asset和主AB包和依赖AB包做引用计数, 并使用AssetBundle.Unload(true)释放内存。

- 界面 (UI) - 提供管理界面和界面组的功能，如 激活界面、改变界面层级 界面反切(回到上一级)等。界面使用结束后可以不立刻销毁(UI池)，从而等待下一次重新使用。

- 声音 (Audio) - 提供管理声音和声音组的功能，支持全局音量调节, 音效音量单独配表

- 输入系统 (Input) - 提供了跨平台的Input封装, 支持PC(键盘鼠标)和手机(触屏)实时切换, 并实现输入方和监听方的代码解耦合。

- 本地数据存档 (PlayerPrefs) - 基于Unity官方PlayerPrefs的本地数据存档, 支持存Object对象，拥有更好的性能

- 任务 (Task) - 提供了Action委托的分组管理, 支持"顺序链式调用"和"并行调用", 在一定程度上解决了委托套娃的现象 

}

为了让框架尽量轻量化，现将一些非必须的功能模块拆分到了别的仓库，同时在这里推荐一些第三方的框架，可与YouYouFramework组合使用，各位可自行接入：

 **可选工具包** 
------------------------------------
{
- 行为树框架：https://gitee.com/chen_junji/BehaviourTree
- 技能编辑器：https://gitee.com/chen_junji/SkillEditor
- Camera控制（跟随 旋转 缩放）: https://gitee.com/chen_junji/CameraFollow
- 摇杆: https://gitee.com/chen_junji/UIScroller
- ECS框架: https://gitee.com/chen_junji/EntitasExample
- 红点系统：https://gitee.com/chen_junji/ReddotTree
- 新手引导系统：https://gitee.com/chen_junji/Guide
- UI无限滚动列表: https://gitee.com/chen_junji/UIScroller
- 后端框架：https://gitee.com/leng_yue/GameDesigner?_from=gitee_search <br>
- Playable自定义动画系统：https://gitee.com/chen_junji/playable-animator

} 
