-----------------------------------------------------------
-- FileName:    Print.lua
-- Author:      Venbb
-- date:        2020-05-24 16:46:39
-- 
-- 重写print，对接C#的Log
-- 打印内容支持自定义颜色（依赖C#的Log），
-- 如print(string.format("<color=#ff0000>%s</color>", "hello world"))
-----------------------------------------------------------
---
local log = B2Framework.Log

local sysp = print

--[[--
    格式化打印

    -- debug.getinfo(p) 这里的参数表示调用的栈，1表示当前方法所在的栈，2表示调用此方法的栈，根据调用层级递增
    -- 因为内部封装了一次，所以这里默认值是3

    @param [mixed ...] 更多参数
]]
local function doprint(func, ...)
    func = func or sysp
    local getinfo = debug.getinfo(3)
    local str = string.format("[%s:%d][f:%d][t:%.2f]: ", getinfo.short_src, getinfo.currentline, Time.frameCount, os.clock())
    for i = 1, select('#', ...) do  -->获取参数总数
        local arg = select(i, ...); -->读取参数
        str = str .. tostring(arg) .. "  "
    end
    func(str)
end
--[[--
    重写print，打印日志信息

    @param [mixed ...] 更多参数
]]
function print(...)
    doprint(log.Debug, ...)
end

--[[--
    打印错误信息

    @param [mixed ...] 更多参数
]]
function print_e(...)
    doprint(log.Error, ...)
end

--[[--
    打印警告信息

    @param [mixed ...] 更多参数
]]
function print_w(...)
    doprint(log.Warning, ...)
end

--[[--
    输出格式化字符串

    ~~~ lua
    printf("The value = %d", 100)
    ~~~

    @param string fmt 输出格式
    @param [mixed ...] 更多参数
]]
function printf(fmt, ...)
    -- print(string.format(tostring(fmt), ...))
    doprint(log.Debug, string.format(tostring(fmt), ...))
end

--[[    打印table的工具函数

    ~~~ lua
    -- string test
    local strTest = "hello world!"
    dump(strTest)
    -- - "<var>" = "hello world!"
    
    -- table test
    local t = {a = {one = 111, two = 222}, b = "666", c = {{id = 1, num = 123}, {id = 2, num = 456}}}
    dump(t, "table print->")
    "table print->" = {
    -     "a" = {
    -         "one" = 111
    -         "two" = 222
    -     }
    -     "b" = "666"
    -     "c" = {
    -         1 = {
    -             "id"  = 1
    -             "num" = 123
    -         }
    -         2 = {
    -             "id"  = 2
    -             "num" = 456
    -         }
    -     }
    - }
    ~~~

    @params value 需要打印的内容
    @params desciption 描述
    @params nesting 打印内容的嵌套级数，默认3级
]]
function dump(value, description, nesting)
    if type(nesting) ~= "number" then nesting = 3 end

    local lookupTable = {}
    local result = {}

    -- local traceback = string.split(debug.traceback("", 2), "\n")
    -- print("dump from: " .. string.trim(traceback[3]))
    local function dump_value_(v)
        if type(v) == "string" then
            v = "\"" .. v .. "\""
        end
        return tostring(v)
    end

    local function dump_(value, description, indent, nest, keylen)
        description = description or "<var>"
        local spc = ""
        if type(keylen) == "number" then
            spc = string.rep(" ", keylen - string.len(dump_value_(description)))
        end
        if type(value) ~= "table" then
            result[#result + 1] = string.format("%s%s%s = %s", indent, dump_value_(description), spc, dump_value_(value))
        elseif lookupTable[tostring(value)] then
            result[#result + 1] = string.format("%s%s%s = *REF*", indent, dump_value_(description), spc)
        else
            lookupTable[tostring(value)] = true
            if nest > nesting then
                result[#result + 1] = string.format("%s%s = *MAX NESTING*", indent, dump_value_(description))
            else
                result[#result + 1] = string.format("%s%s = {", indent, dump_value_(description))
                local indent2 = indent .. "    "
                local keys = {}
                local keylen = 0
                local values = {}
                for k, v in pairs(value) do
                    keys[#keys + 1] = k
                    local vk = dump_value_(k)
                    local vkl = string.len(vk)
                    if vkl > keylen then keylen = vkl end
                    values[k] = v
                end
                table.sort(keys, function(a, b)
                    if type(a) == "number" and type(b) == "number" then
                        return a < b
                    else
                        return tostring(a) < tostring(b)
                    end
                end)
                for i, k in ipairs(keys) do
                    dump_(values[k], k, indent2, nest + 1, keylen)
                end
                result[#result + 1] = string.format("%s}", indent)
            end
        end
    end
    -- dump_(value, description, "- ", 1)
    dump_(value, description, "", 1)

    local str = ''
    for i, line in ipairs(result) do
        -- print(line)
        str = str .. '\n' .. line
    end
    doprint(log.Debug, str)
end