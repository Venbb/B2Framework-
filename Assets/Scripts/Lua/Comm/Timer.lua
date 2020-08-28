-----------------------------------------------------------
-- FileName:    Timer.lua
-- Author:      Administrator
-- date:        2020-06-24 15:03:06
-- 
-- 定时器
-- 1、使用协程实现的定时器功能
-- 2、使用完成后一定要记得调用Remove方法回收定时器
-- 3、在适当的时机调用Clear方法来释放协程引用
-----------------------------------------------------------
local Timer = class("Timer")

function Timer:ctor(...)
    self.handlers = {}
    self.unique = 0
end

--- 延迟执行，执行完成后会自动回收
--- @param integer sec 延迟执行时间
--- @param function callback 回调函数
--- @return number
function Timer:SetTimeout(sec, callback)
    assert(sec > 0 and callback ~= nil and type(callback) == "function", "SetInterval param error")
    local idx = self.unique + 1
    self.unique = idx
    local handler = CSCoroutine.start(function()
        coroutine.yield(UnityEngine.WaitForSeconds(sec))
        callback()
        self:Remove(idx)
    end)
    self.handlers[idx] = handler
    return idx
end

--- 间隔执行
--- @param integer sec 间隔时间
--- @param function callback 回调函数
--- @return number
function Timer:SetInterval(sec, callback)
    assert(sec > 0 and callback ~= nil and type(callback) == "function", "SetInterval param error")
    local idx = self.unique + 1
    self.unique = idx
    local handler = CSCoroutine.start(function()
        while true do
            coroutine.yield(UnityEngine.WaitForSeconds(sec))
            callback()
        end
    end)
    self.handlers[idx] = handler
    return idx
end

--- 移除某个定时器
--- @param integer unique 计时器索引
function Timer:Remove(unique)
    local handler = self.handlers[unique]
    if handler then
        self.handlers[unique] = nil
        CSCoroutine.stop(handler)
    end
end

--- 清除所有
function Timer:Clear()
    table.walk(self.handlers, function(v, k)
        CSCoroutine.stop(v)
    end)
    self.unique = 0
    self.handlers = {}
end
return Timer