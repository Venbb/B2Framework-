-----------------------------------------------------------
-- FileName:    UIGuide.lua
-- Author:      Administrator
-- date:        2020-07-21 14:25:42
-- 
-----------------------------------------------------------
local UIBase = import '.UIBase'

local UIGuide = class("UIGuide", UIBase)

function UIGuide:ctor(...)
    self.onCicleClick = nil
    self.isShowClick = true
    self.isShowBlackBg = true
    return self.super.ctor(self, ...)
end

function UIGuide:Awake()
    self._root.Button.gameObject:GetComponent("Button").onClick:AddListener(function()
        if self.onCicleClick then
            self.onCicleClick()
        end
    end)
end

function UIGuide:Start()
    
end

function UIGuide:UpdateState(guidingLayer, guideInfo)
    if not self._root then
        return
    end
    if self.isShowClick then
        if not self._root.SeeArea.gameObject.activeInHierarchy then
            self._root.SeeArea.gameObject:SetActive(true)
        end
        if not self._root.Button.gameObject.activeInHierarchy then
            self._root.Button.gameObject:SetActive(true)
        end
    else
        if self._root.SeeArea.gameObject.activeInHierarchy then
            self._root.SeeArea.gameObject:SetActive(false)
        end
        if self._root.Button.gameObject.activeInHierarchy then
            self._root.Button.gameObject:SetActive(false)
        end
    end
    if self.isShowBlackBg then
        self._root.Background.gameObject:GetComponent("Image").color = Color(0, 0, 0, 0.5)
    else
        self._root.Background.gameObject:GetComponent("Image").color = Color(0, 0, 0, 0)
    end

    local ui = guidingLayer._root.transform:Find(guideInfo.UI)
    local uiCircle = self._root.SeeArea.transform
    local uiButton = self._root.Button.transform

    local uiRect = guidingLayer._root.transform:Find(guideInfo.UI):GetComponent("RectTransform")
    local uiCircleRect = self._root.SeeArea.transform:GetComponent("RectTransform")
    local uiButtonRect = self._root.Button.transform:GetComponent("RectTransform")

    local p = uiRect.position
    local scale = math.max(uiRect.rect.width, uiRect.rect.height) * uiRect.localScale.x / uiCircleRect.rect.width
    uiCircle.position = ui.position
    uiButton.position = ui.position
    uiCircleRect.localScale = UnityEngine.Vector3(scale, scale, scale)
    uiButtonRect.localScale = UnityEngine.Vector3(scale, scale, scale)
end

function UIGuide:BindClick(onClick)
    self.onCicleClick = onClick
end

function UIGuide:HideBlackBg()
    self.isShowBlackBg = false
    if self._root then
        self._root.Background.gameObject:GetComponent("Image").color = Color(0, 0, 0, 0)
    end
end

function UIGuide:ShowBlackBg()
    self.isShowBlackBg = true
    if self._root then
        self._root.Background.gameObject:GetComponent("Image").color = Color(0, 0, 0, 0.5)
    end
end

function UIGuide:HideClick()
    self.isShowClick = false
    if self._root then
        if self._root.SeeArea.gameObject.activeInHierarchy then
            self._root.SeeArea.gameObject:SetActive(false)
        end
        if self._root.Button.gameObject.activeInHierarchy then
            self._root.Button.gameObject:SetActive(false)
        end
    end
end

function UIGuide:ShowClick()
    self.isShowClick = true
    if self._root then
        if not self._root.SeeArea.gameObject.activeInHierarchy then
            self._root.SeeArea.gameObject:SetActive(true)
        end
        if not self._root.Button.gameObject.activeInHierarchy then
            self._root.Button.gameObject:SetActive(true)
        end
    end
end

function UIGuide:OnDestroy()
    self.super.OnDestroy(self)
end

return UIGuide