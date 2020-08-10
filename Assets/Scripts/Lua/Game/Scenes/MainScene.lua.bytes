-----------------------------------------------------------
-- FileName:    MainScene.lua
-- Author:      Venbb
-- date:        2020-05-25 21:46:15
-- 
-- UI 基类
-----------------------------------------------------------
local SceneBase = import '.SceneBase'
local MainScene = class("MainScene", SceneBase)

function MainScene:ctor(param)
    self.super.ctor(self, "MainScene", param)
    UIMgr:Clear(true)
end

function MainScene:onSceneLoad()
    self.super.onSceneLoad(self)
    UIMgr:Show("UITestDlg1", 1)
end

function MainScene:onSceneUnload()
    self.super.onSceneUnload(self)
end

return MainScene