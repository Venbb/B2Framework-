# B2Framework

## 目录结构

![B2Framework](Docs/image-20200507173919966.png)

* Arts 本地资源目录，存放<font color=#ff0000>不热更</font>的所有资源，目录结构与AssetBundles 目录基本一致，一般由美术、策划、程序人员参与管理
  * Animations 动画文件，按照功能在该目录创建子目录自行管理
  * Fonts 游戏中使用的字体
  * Materials 所有的材质
  * Models 模型文件，如Spine动画
  * Prefabs 预制文件
  * Shaders 所有用到的Shader文件
  * Textures 美术输出的所有图片资源
* AssetBundles 需要热更的资源
  * Animations 需要热更的动画文件
  * Audio 动态加载的声音
  * Fonts 动态加载的字体
  * Materials 需要动态加载的材质球
  * Prefabs 动态加载的预制
    * Effects 特效的预制
    * Models 模型的预制，如Spine的预制
    * UI 界面的预制
  * Scripts 需要热更的脚本，如Lua，目录结构跟存放C#的Scripts 基本一致
    * Game 游戏逻辑
      * Battle 战斗逻辑
      * ConstDefine 一些常量定义
      * UI 界面相关逻辑
    * Utils 公用的工具脚本
  * Shaders 所有需要动态加载的Shader文件
  * Textures 需要动态加载的图片，如立绘等
* Scenes 存放所有场景，不要随意添加场景
  * Launch 游戏初始化，接口初始化
  * Loading 检测版本更新，下载资源
  * Login 登录
  * Main 游戏主场景
  * Battle 战斗场景
* Scripts 存放C#脚本，无法热更
  * Editor 所有对Unity编辑器扩展的脚本放这里
  * Game 游戏逻辑代码
    * Battle 战斗逻辑
    * ConstDefine 一些常量定义
    * UI 界面相关逻辑
  * Utils 公用的工具类放这里
* 其它都为第三方扩展库文件，或Unity引擎原生目录
* <font color=#ff0000>严禁横向增加目录，只能扩展子目录</font>

## 参考框架

https://github.com/egametang/ET

https://github.com/cocowolf/loxodon-framework

https://github.com/xasset/xasset

https://github.com/smilehao/xlua-framework

https://github.com/mr-kelly/KSFramework

lua性能调试：https://github.com/leinlin/Miku-LuaProfiler

https://github.com/liuhaopen/UnityMMO

https://github.com/yimengfan/BDFramework.Core

## 搜索GitHub

xxx stars:>500

解决方案搜索：UGUI、xLua、Unity
