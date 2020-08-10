-----------------------------------------------------------
-- FileName:    UINoticeTip.lua
-- Author:      Administrator
-- date:        2020-05-22 16:51:42
-- 
-----------------------------------------------------------
local UIBase = import '.UIBase'

local UINoticeTip = class("UINoticeTip", UIBase)

function UINoticeTip:ctor(...)
    print("UINoticeTip:ctor....")
    self.idx = -1
    return self.super.ctor(self, ...)
end

function UINoticeTip:Awake()
    
end

function UINoticeTip:Update()

end

function UINoticeTip:OnRefreshParam(param)
    self._root.Tip.Text.gameObject:GetComponent("Text").text = param[1]
    self._root.Tip.gameObject:GetComponent("Animation"):Stop()
    self._root.Tip.gameObject:GetComponent("Animation"):Play()
    if self.idx ~= -1 then
        Timer:Remove(self.idx)
    end
    self.idx = Timer:SetTimeout(2, function()
        UIMgr:Remove(self)
    end)

end

function UINoticeTip:OnDestroy()
    self.super.OnDestroy(self)
    -- self.submit.onClick:RemoveAllListeners()
end
return UINoticeTip