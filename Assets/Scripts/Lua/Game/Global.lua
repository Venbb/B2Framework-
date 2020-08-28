-----------------------------------------------------------
-- FileName:    Global.lua
-- Author:      Administrator
-- date:        2020-05-26 17:27:31
-- 
-- 全局定义
-- 1、游戏相关的所有全局访问对象的初始化
-- 2、强烈建议将所有全局表初始化工作放这里，以便统一管理
-- 3、此脚本是由GameLogic.lua唯一调用
-----------------------------------------------------------
local require = require
--[[--
    加载json 解析库

    local t = json.decode('{"a":123}')
    print(t.a)
    t.a = 456
    local s = json.encode(t)
    print('json', s)      
]]
Json            = require 'rapidjson'
-- 加载Protobuf解析库
Protobuf        = require 'Comm.Protobuf'
-- 协程
CSCoroutine     = require 'Comm.CSCoroutine'
-- 定时器
Timer           = require('Comm.Timer').new()

--彪哥无敌，彪哥万岁，彪哥天王盖地虎，彪哥宝塔镇河妖，彪哥记得玩我的营救女儿国游戏哈
CommonFunc = require("Game.CommonFunc")
--红点管理器
Define          = require('Game.Define')
-- 事件Key
EventKey        = require('Game.EventKey')
--红点管理器
UserData          = require('Game.Data.UserData')
-- 初始化全局访问库
App             = require('Game.App').new()
-- 资源管理
ResMgr          = require('Game.ResourcesManager').new()
-- 事件管理
EvtMgr          = require('Game.EventManager').new()
-- 场景管理
SceneMgr        = require('Game.SceneManager').new()
-- 数据管理
DataMgr         = require('Game.DataManager').new()
-- 网络管理
NetMgr          = require('Game.NetManager').new()
-- 音效管理
AudioMgr        = require('Game.AudioManager').new()
-- UI管理
UIMgr           = require('Game.UI.UIManager').new()
-- 红点系统
ReddotMgr       = require('Game.ReddotManager').new()
-- 新手引导系统
GuideMgr        = require('Game.GuideManager').new()
-- 剧情故事系统
StoryMgr        = require('Game.StoryManager').new()