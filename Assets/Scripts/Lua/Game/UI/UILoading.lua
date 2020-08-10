-----------------------------------------------------------
-- FileName:    UILoading.lua
-- Author:      Administrator
-- date:        2020-07-06 14:25:42
-- 
-----------------------------------------------------------
local UIBase = import '.UIBase'

local UILoading = class("UILoading", UIBase)

function UILoading:ctor(...)
    print("UILoading:ctor....")
    local param = {...}
    self._currentWeight = 0
    self._totalWeight = param[1] and param[1] or 0
    self.bg = param[2]
    self._callback = param[3]
    return self.super.ctor(self, ...)
end

function UILoading:Awake()
    self.slider = self.transform:Find("Slider")
    if self.bg then
        ResMgr:LoadAssetAsync(self.bg, typeof(UnityEngine.Sprite), function(sp)
            print("sp: ", sp)
            self.transform:Find("Background"):GetComponent("Image").sprite = sp
        end)
    end
end

function UILoading:Start()

end

function UILoading:SetTotalWeight(totalWeight)
    self._totalWeight = totalWeight
end

function UILoading:SetCurrentWeight(currentWeight)
    self._currentWeight = currentWeight
    self:SetPersent(self._currentWeight / self._totalWeight)
end

function UILoading:SetPersent(persent)
    if self.slider then
        self.slider.gameObject:GetComponent("Slider").value = persent
    end
    if persent >= 1 then
        self._callback(self.transform)
    end
end

function UILoading:OnDestroy()
    self.super.OnDestroy(self)
    -- self.submit.onClick:RemoveAllListeners()
end
return UILoading