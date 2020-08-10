-----------------------------------------------------------
-- FileName:    EventManager.lua
-- Author:      zhbd
-- date:        2020-06-17 11:09:25
-- 
-----------------------------------------------------------

local EventManager = class("EventManager")

function EventManager:ctor(...)
    self._events_ByReceiver = {}
    self._events_ByKey = {}
end

local _defaultcall = nil

function EventManager:AddListener(receiver, key, callback)
    assert(key ~= nil and receiver ~= nil and callback ~= nil and type(callback) == "function")

    if self._events_ByReceiver[receiver] == nil then
        self._events_ByReceiver[receiver] = {}
    end
    if not self._events_ByKey[key] then
        self._events_ByKey[key] = {}
    end

    local events = self._events_ByReceiver[receiver]
    if not events[key] then
        events[key] = {}
    else
        for i = 1, #events[key] do
            if events[key][i].callback == callback then
                --这里存在的话，self._events_ByKey[key]里面肯定存在了,所以都不必执行了
                return false
            end
        end
    end

    local tbl = {receiver = receiver, callback = callback}
    table.insert(events[key], tbl)
    table.insert(self._events_ByKey[key], tbl)

    return true
end

function EventManager:RemoveListener(receiver, key, callback)
    assert(receiver ~= nil)

    if key == nil then
        self:RemoveListenerByReceiver(receiver)
        return
    end

    local events = self._events_ByReceiver[receiver]

    if events then
        if events[key] then
            if callback then
                for i = 1, #events[key] do
                    if events[key][i].callback == callback then
                        table.remove(events[key], i)
                        break
                    end
                end
            else
                events[key] = nil
            end
        end
    end

    if self._events_ByKey[key] then
        if callback then
            for i = 1, #self._events_ByKey[key] do
                if self._events_ByKey[key][i].callback == callback then
                    table.remove(self._events_ByKey[key], i)
                    break
                end
            end
            if #self._events_ByKey[key] == 0 then
                self._events_ByKey[key] = nil
            end
        else
            self._events_ByKey[key] = nil
        end
    end
end

function EventManager:RemoveListenerByReceiver(receiver)
    for k, _ in pairs(self._events_ByKey) do
        local i = 1
        local events = self._events_ByKey[k]
        while i <= #events do
            if events[i].receiver == receiver then
                table.remove(events,i)
            else
                i = i + 1
            end
        end
        if #events == 0 then
            self._events_ByKey[k] = nil
        end
    end
    self._events_ByReceiver[receiver] = nil
end

function EventManager:Notify(key, ...)
    assert(key ~= nil)
    local callTable = {}
    if self._events_ByKey[key] then
        for _, v in ipairs(self._events_ByKey[key]) do
            table.insert(callTable, v)
        end
    end
    
    for k, v in pairs(callTable) do
        v.callback(v.receiver, ...)
    end
end

function EventManager:ClearAllListener()
    self._events_ByReceiver = {}
    self._events_ByKey = {}
end

--测试看Log用
function EventManager:LoveMeBabby()
    print("===============FuckMeBabby==============")
    dump(self._events_ByReceiver)
    dump(self._events_ByKey)
end

return EventManager