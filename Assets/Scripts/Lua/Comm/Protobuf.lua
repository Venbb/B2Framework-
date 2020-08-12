-----------------------------------------------------------
-- FileName:    Protobuf.lua
-- Author:      Administrator
-- date:        2020-06-08 16:10:59
-- 
-- 这里封装lua-protobuf
-- 参考地址：
-- https://github.com/chexiongsheng/build_xlua_with_libs
-- https://github.com/starwing/lua-protobuf
-- https://www.cnblogs.com/xiaohutu/p/12168781.html
-----------------------------------------------------------
local Protobuf = {}
--[[    
    
    protobuf 解析库

    local bytes = assert(pb.encode('Person', data))
    print(pb.tohex(bytes))
    local data2 = assert(pb.decode('Person', bytes))

    通过assert(pb.loadfile "login.pb")加载pb文件
]]
local pb = require 'pb'

--[[    
    
    用于解析proto文本文件到pb

    assert(protoc:load (proto))也可以通过assert(pb.loadfile "login.pb")直接加载编译过的pb文件

    这个文件来自https://github.com/starwing/lua-protobuf
]]
local protoc = require 'Comm.protoc'

-- local proto=[[
--     message Phone {
--         optional string name        = 1;
--         optional int64  phonenumber = 2;
--     }
--     message Person {
--         optional string name     = 1;
--         optional int32  age      = 2;
--         optional string address  = 3;
--         repeated Phone  contacts = 4;
--     } ]]
-- assert(protoc:load (proto))
-- local data = {
--     name = 'ilse',
--     age = 18,
--     contacts = {
--         { name = 'alice', phonenumber = 12312341234 },
--         { name = 'bob', phonenumber = 45645674567 }
--     }
-- }
-- local bytes = assert(pb.encode('Person', data))
-- print(pb.tohex(bytes))
-- local data2 = assert(pb.decode('Person', bytes))
-- print(data2.name)
-- print(data2.age)
-- print(data2.address)
-- print(data2.contacts[1].name)
-- print(data2.contacts[1].phonenumber)
-- print(data2.contacts[2].name)
-- print(data2.contacts[2].phonenumber)
-- 加载pb文件
local function load(proto)
    assert(pb.loadfile(proto))
    -- assert(protoc:load (proto))
end

-- 序列化成proto文件
local function encode(name, data)
    return pb.encode(name, data)
end

-- 反序列化成table数据
local function decode(name, bytes)
    return pb.decode(name, bytes)
end

Protobuf.load = load
Protobuf.encode = encode
Protobuf.decode = decode

return Protobuf