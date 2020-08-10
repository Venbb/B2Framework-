-----------------------------------------------------------
-- FileName:    UIWaiting.lua
-- Author:      Administrator
-- date:        2020-07-06 14:25:42
-- 
-----------------------------------------------------------
local UIBase = import '.UIBase'

local UIWaiting = class("UIWaiting", UIBase)

function UIWaiting:ctor(...)
    self.CallCount = 0
    self.waitingInfo = {}
    self.timeOut = 30
    self.waitIdx = nil
    return self.super.ctor(self, ...)
end

function UIWaiting:Awake()

end

function UIWaiting:Start()
    self.waitIdx = Timer:SetTimeout(self.timeOut, function()
        print("等待超时，详情如下： ")
        dump(self.waitingInfo)
        for k, _ in pairs(self.waitingInfo) do
            for i = 1, #self.waitingInfo[k].timeOutCallback do
                self.waitingInfo[k].timeOutCallback[i]()
            end
        end 
        UIMgr:Remove(self)
    end)
end

function UIWaiting:Retain(key, timeOutCallback)
    self.CallCount = self.CallCount + 1
    if key then
        if not self.waitingInfo[key] then
            self.waitingInfo[key] = {}
            self.waitingInfo[key].Count = 0
            self.waitingInfo[key].timeOutCallback = {}
        end
        self.waitingInfo[key].Count = self.waitingInfo[key].Count + 1
        if timeOutCallback then
            table.insert(self.waitingInfo[key].timeOutCallback, timeOutCallback)
        end
    end
end

function UIWaiting:Release(key)
    self.CallCount = self.CallCount - 1
    print("self.CallCount: ", self.CallCount)
    if self.CallCount <= 0 then
        UIMgr:Remove(self)
    end
    if key then
        if self.waitingInfo[key] then
            self.waitingInfo[key].Count = self.waitingInfo[key].Count - 1
            if self.waitingInfo[key].Count == 0 then
                self.waitingInfo[key] = nil
            end
        end
    end
end

function UIWaiting:OnDestroy()
    self.super.OnDestroy(self)
    if self.waitIdx then
        Timer:Remove(self.waitIdx)
    end
end

return UIWaiting