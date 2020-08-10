-----------------------------------------------------------
-- FileName:    SceneBase.lua
-- Author:      zhbd
-- date:        2020-05-25 21:46:15
-- 
-- 场景 基类
-----------------------------------------------------------
local SceneBase = class("SceneBase")

function SceneBase:ctor(name, param)
    self.name = name
    dump(param)
end

function SceneBase:onSceneLoad()
    print("SceneBase:onSceneLoad: ", self.name)
end

function SceneBase:onSceneUnload()
    print("SceneBase:onSceneUnload: ", self.name)
end

return SceneBase