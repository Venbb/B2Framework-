-----------------------------------------------------------
-- FileName:    Main.lua
-- Author:      Administrator
-- date:        2020-05-21 10:36:16
-- 
-- 从这里开始Lua的不归之路，这个模块执行完之后就可以开心的用Lua开发啦！
-- 0、这个脚本由LuaManger.cs文件在初始化时调用，主要来初始化Lua的环境
-- 1、初始化全局表，这里相当于初始化Lua的运行环境
-- 2、初始化全局表之后可以提高访问速度
-- 3、Use caching 提前缓存频繁访问的接口是个好习惯，比如在执行循环前使用local
-----------------------------------------------------------

-- 加载CS接口
require 'Comm.Unity3D'

-- 为了提升性能，这个使用了tolua的扩展
Mathf           = require 'Comm.UnityEngine.Mathf'
Vector2         = require "Comm.UnityEngine.Vector2"
Vector3         = require "Comm.UnityEngine.Vector3"
Vector4         = require "Comm.UnityEngine.Vector4"
Quaternion      = require "Comm.UnityEngine.Quaternion"
Color           = require "Comm.UnityEngine.Color"
Ray             = require "Comm.UnityEngine.Ray"
Bounds          = require "Comm.UnityEngine.Bounds"
RaycastHit      = require "Comm.UnityEngine.RaycastHit"
Touch           = require "Comm.UnityEngine.Touch"
LayerMask       = require "Comm.UnityEngine.LayerMask"
Plane           = require "Comm.UnityEngine.Plane"
Time            = require "Comm.UnityEngine.Time"

-- 加载公共库，拆成多个文件是不是有IO消耗?
require 'Comm.Utils'

-- 对接日志系统，这里对系统的print进行了重写
require 'Comm.Print'

--  Test
-- local t = os.clock()
-- local temp
-- for i = 1, 200000 do
--     -- 测试调用CS接口
--     -- 这里使用AssetsMgr.dataPath比The.AssetsMgr.dataPath快多了哈
--     -- temp = AssetsMgr.dataPath
--     -- 测试Vector3
--     -- local v = Vector3.New(i, i, i)
--     -- local x, y, z = v.x, v.y, v.z
-- end
-- print(os.clock() - t)
-- print(temp)