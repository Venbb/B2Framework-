-----------------------------------------------------------
-- FileName:    BattleScene.lua
-- Author:      Venbb
-- date:        2020-05-25 21:46:15
-- 
-- UI 基类
-----------------------------------------------------------
local SceneBase = import '.SceneBase'
local BattleScene = class("BattleScene", SceneBase)

function BattleScene:ctor(param)
    self.super.ctor(self, "BattleScene", param)
    UIMgr:Clear(true)
end

function BattleScene:onSceneLoad()
    self.super.onSceneLoad(self)
    UIMgr:Show("UISpaceCraft")
end

function BattleScene:onSceneUnload()
    self.super.onSceneUnload(self)
end

return BattleScene