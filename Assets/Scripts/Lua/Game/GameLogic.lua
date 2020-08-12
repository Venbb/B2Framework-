-----------------------------------------------------------
-- FileName:    GameLogic.lua
-- Author:      Administrator
-- date:        2020-05-26 17:32:35
-- 
-- 游戏主逻辑入口
-- 1、这里是由LuaManager.cs在启动游戏并开始真正侧游戏逻辑时调用
-- 2、这里是Lua端游戏真正开始的唯一接口
-- 3、主要工作包括：初始化全局Table、游戏主逻辑更新，如Update等统一调度
-----------------------------------------------------------
-- 加载全局定义
require 'Game.Global'

local GameLogic = {}

local function Start()
    UIMgr:Show("UILogin", 1, function()
        print("=======================OK===================")
    end)
end

-- function GameLogic:Update()

-- end

-- function GameLogic:LateUpdate()

-- end

-- function GameLogic:FixedUpdate()

-- end

function GameLogic:Notify(key, value)
    EvtMgr:Notify(key, Json.decode(value))
end

Start();
return GameLogic

