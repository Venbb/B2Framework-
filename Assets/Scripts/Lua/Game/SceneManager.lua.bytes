-----------------------------------------------------------
-- FileName:    SceneManager.lua
-- Author:      zhbd
-- date:        2020-06-29 13:51:06
--
-----------------------------------------------------------

local SceneConfig = require("Game.Scenes.SceneConfig")
local SceneManager = class("SceneManager")

local mgr = The.SceneMgr

function SceneManager:ctor(...)
    -- TODO:do something on ctor
    self.currentLuaScene = nil
    self.lastLuaScene = nil
    self.loadCallback = nil
    self.loading = false
end

function SceneManager:sceneLoaded(scene, loadSceneMode)
    self.loading = false
    self.currentLuaScene:onSceneLoad()
    EvtMgr:Notify(EventKey.SceneLoaded)
    if self.loadCallback then
        self.loadCallback()
        self.loadCallback = nil
    end
end

function SceneManager:sceneUnloaded(scene)
    EvtMgr:Notify(EventKey.SceneUnLoaded)

    if self.lastLuaScene then
        self.lastLuaScene:onSceneUnload()
        self.lastLuaScene = nil
    end

    The.LuaMgr.luaEnv:FullGc()
    UnityEngine.Resources.UnloadUnusedAssets()
    CS.System.GC.Collect()
end

function SceneManager:LoadSceneAsync(name, param, callBack)
    if self.loading then
        return
    end
    self.loading = true
    if not SceneConfig[name] then
        print("场景不存在或者没有写到SceneConfig里面进行配置......")
        return
    end

    local luaScene = require(SceneConfig[name].luaScriptPath).new(param)

    mgr:LoadSceneAsync(SceneConfig[name].name)
    mgr.OnCompleted = function(name)
        self.lastLuaScene = self.currentLuaScene
        self.currentLuaScene = luaScene
        self.loadCallback = callBack

        self:sceneLoaded()
    end
end

return SceneManager
