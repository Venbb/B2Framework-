-----------------------------------------------------------
-- FileName:    App.lua
-- Author:      Administrator
-- date:        2020-05-26 17:20:38
-- 
-- 全局变量或方法
-- 1、将CS端设置映射到Lua
-- 2、或者在Lua侧的全局定义
-- 3、强烈建议在这里扩展应用级别的变量和方法
-----------------------------------------------------------
local App = class("App")

-- 初始化，将CS端的游戏设置同步到Lua，此后使用这个借口访问
function App:ctor(...)
    -- 当前平台
    self.platform = The.platform
    -- 当前语言
    self.lanCode = Loc.lanCode
end

-- example: App:IsPlatform("WebGL")
function App:IsPlatform(platform)
    return self.platform == platform
end

return App