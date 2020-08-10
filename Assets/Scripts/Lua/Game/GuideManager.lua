-----------------------------------------------------------
-- FileName:    GuideManager.lua
-- Author:      zhbd
-- date:        2020-07-20 11:09:25
-- 
-----------------------------------------------------------
local GuideConfig = require("Game.Data.Config.GuideConfig")
local GuideManager = class("GuideManager")

function GuideManager:ctor(...)
    self._isGuiding = false
    self._guideInfo = nil
    self._guidingLayer = nil
    self._guidMask = nil
    self.coroutine = nil
end

function GuideManager:Check(layer)
    self._guidingLayer = layer
    if self._isGuiding then
        return
    end
    local guideInfo = GuideConfig[UserData.guideId]
    if guideInfo == nil then
        self:EndGuide()
        return
    end
    if (guideInfo.layerName ~= self._guidingLayer.name) or (guideInfo.needLevel > UserData.level) then
        self:EndGuide()
        return
    end
    self._guideInfo = guideInfo
    self._isGuiding = true
    self:StartGuide()
end

function GuideManager:NextGuide()
    UserData.guideId = self._guideInfo.nextId
    local guideInfo = GuideConfig[UserData.guideId]
    if guideInfo == nil then
        self:EndGuide()
        return
    end
    if guideInfo.needLevel > UserData.level then
        self:EndGuide()
        return
    end
    self._guideInfo = guideInfo
    self._isGuiding = true
    if self._guideInfo.delay > 0 then
        if self._guidMask then
            self._guidMask:HideBlackBg()
            self._guidMask:HideClick()
            Timer:SetTimeout(self._guideInfo.delay, function()
                self._guidMask:ShowBlackBg()
                self._guidMask:ShowClick()
            end)
        end
    else
        self._guidMask:ShowClick()
    end
end

function GuideManager:StartGuide()
    if not self._guidMask then
        self._guidMask = UIMgr:Show("UIGuide")
    end

    self._guidMask:BindClick(function()
        local UI = self._guidingLayer._root.transform:Find(self._guideInfo.UI)
        UI.gameObject:GetComponent("Button").onClick:Invoke()
        if self._guideInfo.callback then
            if self._guideInfo.callback() then
                return
            end
        end
        if self._guideInfo.nextId then
            self:NextGuide()
        else
            self:EndGuide()
        end
    end)
    --不断调整遮罩的光环的位置
    if not self.coroutine then
        self.coroutine = CSCoroutine.start(function()
            while self._isGuiding do
                if self._guidingLayer and self._guidMask and self._guidingLayer.name == self._guideInfo.layerName then
                    self._guidMask:UpdateState(self._guidingLayer, self._guideInfo)
                end
                coroutine.yield(CS.UnityEngine.WaitForSeconds(0.2))
            end
        end)
        
        if self._guideInfo.delay > 0 then
            if self._guidMask then
                self._guidMask:HideBlackBg()
                self._guidMask:HideClick()
                Timer:SetTimeout(self._guideInfo.delay, function()
                    self._guidMask:ShowBlackBg()
                    self._guidMask:ShowClick()
                end)
            end
        else
            self._guidMask:ShowClick()
        end
    end
end

function GuideManager:EndGuide()
    if self.coroutine then
        CSCoroutine.stop(self.coroutine)
        self.coroutine = nil
    end
    if self._guidMask then
        self._guidMask._root.SeeArea.gameObject:SetActive(false)
        self._guidMask._root.Button.gameObject:SetActive(true)
        UIMgr:Remove("UIGuide")
        self._guidMask = nil
    end
    self._isGuiding = false
    self._guideInfo = nil
    self._guidingLayer = nil
end

return GuideManager