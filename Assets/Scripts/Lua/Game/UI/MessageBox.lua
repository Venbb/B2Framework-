-----------------------------------------------------------
-- FileName:    MessageBox.lua
-- Author:      Administrator
-- date:        2020-05-22 16:51:42
-- 
-----------------------------------------------------------
local UIBase = import '.UIBase'

local MessageBox = class("MessageBox", UIBase)

function MessageBox:ctor(...)
    return self.super.ctor(self, ...)
end

function MessageBox:Awake()
    MessageBox.super.Awake(self);
    local info = self._param[1]
    
    self._root.Background.Info.Image.gameObject:SetActive(info.icon)
    self._root.Background.Info.Text.gameObject:SetActive(info.text)
    self._root.Background.choices.ButtonLeft.gameObject:SetActive(info.funcLeft or info.showLeftButton)
    self._root.Background.choices.ButtonRight.gameObject:SetActive(info.funcRight or info.showRightButton)
    
    --TODO:根据Id读取，现在先这么写
    if info.icon then
        ResMgr:LoadAssetAsync(info.icon, typeof(UnityEngine.Sprite), function(sp)
            self._root.Background.Info.Image.transform:GetComponent("Image").sprite = sp
        end)
    end

    if info.text then
        self._root.Background.Info.Text.transform:GetComponent("Text").text = info.text
    end
    if info.funcLeft then
        self.funcLeft = info.funcLeft
    end
    if info.funcRight then
        self.funcRight = info.funcRight
    end

    self._root.Background.choices.ButtonLeft.gameObject:GetComponent("Button").onClick:AddListener(function()
        if self.funcLeft then
            self.funcLeft()
        end
        UIMgr:Remove(self)
    end)
    self._root.Background.choices.ButtonRight.gameObject:GetComponent("Button").onClick:AddListener(function()
        if self.funcRight then
            self.funcRight()
        end
        UIMgr:Remove(self)
    end)
end

function MessageBox:Start()
    
end

function MessageBox:Update()

end

function MessageBox:OnDestroy()

end

return MessageBox