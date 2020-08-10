# 游戏工具说明



## 一、Protobuf

* 本项目选用的是lua-protobuf：https://github.com/starwing/lua-protobuf

* lua-protobuf的中文说明文档：https://zhuanlan.zhihu.com/p/26014103

* 在XLua环境集成lua-protobuf：https://github.com/chexiongsheng/build_xlua_with_libs

* 加载proto文件的方式：

  1、assert(pb.loadfile "login.pb")

  2、直接加载源文件字符串：assert(pb.load(protoStr))

  3、或者是用lua-protobuf的protoc.lua加载：assert(protoc:load (protoStr))

* 为了支持加载.pb的方式，我们需要使用protoc来将proto文件编译成pb文件

  要去官网下载对应平台的protoc可执行文件：https://github.com/protocolbuffers/protobuf/releases

  或者下载源码来自己编译protoc：https://github.com/protocolbuffers/protobuf

  具体编译过程还是有点麻烦，自行百度相关文档，我们这里直接使用release版本的protoc

## 二、配置表

* 提供将Excel配置表转换成Lua Table的功能