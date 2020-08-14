-----------------------------------------------------------
-- FileName:    CSCoroutine.lua
-- Author:      Administrator
-- date:        2020-06-19 15:36:14
-- 
-- CS协程结合Lua的协程实现
-- 1、依赖挂在LuaManager上的LuaCoroutine.cs
-- 2、在调用此脚本后，同时会在LuaManager上挂载LuaCoroutine.cs
-----------------------------------------------------------
--[[    https://www.jianshu.com/p/e4b543f3ff17

    coroutine.create(f):
    创建一个主体函数为 f 的新协程。 f 必须是一个 Lua 的函数。 
    返回这个新协程，它是一个类型为 "thread" 的对象。不会启动该协程。

    coroutine.resume(co, [, val1, ...]):
    开始或继续协程co的运行。当第一次执行一个协程时，他会从主函数处开始运行。val1,...这些值会以参数形式传入主体函数。 
    如果该协程被挂起，resume 会重新启动它； val1, ... 这些参数会作为挂起点的返回值。
    如果协程运行起来没有错误， resume 返回 true 加上传给 yield 的所有值 （当协程挂起）， 或是主体函数的所有返回值（当协程中止）。
    coroutine.resume 是在保护模式中运行,如果有任何错误发生, Lua 是不会显示任何错误， 而是 返回 false 加错误消息。同时,这个协程的状态会变成dead。

    coroutine.yield(...):
    挂起正在调用的协程的执行。 传递给 yield 的参数都会转为 resume 的额外返回值。

    coroutine.status(co):
    以字符串形式返回协程 co 的状态：

    当协程正在运行（它就是调用 status 的那个） ，返回 "running"；
    如果协程调用 yield 挂起或是还没有开始运行，返回 "suspended"；
    如果协程是活动的，都并不在运行（即它正在延续其它协程），返回 "normal"；
    如果协程运行完主体函数或因错误停止，返回 "dead"。

    coroutine.running():
    返回当前的协程,如是实在主线程,则返回nil

    use case:
    local a = CSCoroutine.start(function()
        print('coroutine a started')

        coroutine.yield(CSCoroutine.start(function() 
            print('coroutine b stated inside cotoutine a')
            coroutine.yield(CS.UnityEngine.WaitForSeconds(1))
            print('i am coroutine b')
        end))
        print('coroutine b finish')

        while true do
            coroutine.yield(CS.UnityEngine.WaitForSeconds(1))
            print('i am coroutine a')
        end
    end)

    CSCoroutine.start(function()
        print('stop coroutine a after 5 seconds')
        coroutine.yield(CS.UnityEngine.WaitForSeconds(5))
        CSCoroutine.stop(a)
        print('coroutine a stoped')

        require 'xlua.util' .print_func_ref_by_csharp()
    end)
]]
local util = require 'xlua.util'

local behaviour = Game.LuaMgr.luaCoroutine

local CSCoroutine = {}

CSCoroutine.start = function(...)
    return behaviour:StartCoroutine(util.cs_generator(...))
end

CSCoroutine.stop = function(co)
    behaviour:StopCoroutine(co)
end
return CSCoroutine