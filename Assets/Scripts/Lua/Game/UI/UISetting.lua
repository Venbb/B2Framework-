-----------------------------------------------------------
-- FileName:    UISetting.lua
-- Author:      Administrator
-- date:        2020-08-07 16:51:42
-- 
-----------------------------------------------------------
local UIBase = import '.UIBase'

local UISetting = class("UISetting", UIBase)

function UISetting:ctor(...)
    return self.super.ctor(self, ...)
end

function UISetting:Awake()
    self._root.ButtonClose.gameObject:GetComponent("Button").onClick:AddListener(function()
        UIMgr:Remove(self)
    end)

    self._root.Button1.gameObject:GetComponent("Button").onClick:AddListener(function()
        AudioMgr:LoadBG("Audio/Finish1.ogg")
        AudioMgr:LoadBG("Audio/Finish2.ogg")
        AudioMgr:LoadBG("Audio/Boss.ogg")
        AudioMgr:LoadBG("Audio/Story1.ogg")
    end)

    self._root.Button2.gameObject:GetComponent("Button").onClick:AddListener(function()
        AudioMgr:UnLoadBG("Audio/Finish1.ogg")
        AudioMgr:UnLoadBG("Audio/Finish2.ogg")
        AudioMgr:UnLoadBG("Audio/Boss.ogg")
        AudioMgr:UnLoadBG("Audio/Story1.ogg")
    end)
end

function UISetting:OnDestroy()
    self.super.OnDestroy(self)
end
return UISetting