-----------------------------------------------------------
-- FileName:    NetManager.lua
-- Author:      Administrator
-- date:        2020-06-23 14:19:28
--
-- 网络管理器
-- 1、依赖NetManager.cs单例，调用此脚本后就会生成NetManager单例
-- 2、支持WebSocket，且对WebSocketClient和NetSocketClient接口做了统一的封装
-----------------------------------------------------------
local NetManager = class("NetManager")

function NetManager:ctor(...)
    -- local obj = UnityEngine.GameObject.Find("NetManager")
    -- if IsNull(obj) then
    --     obj = UnityEngine.GameObject('NetManager')
    -- end
    -- UnityEngine.Object.DontDestroyOnLoad(obj)
    -- if App:IsPlatform("WebGL") then
    --     self.client = obj:AddComponent(typeof(B2Framework.Net.WebSocketClient))
    -- else
    --     self.client = obj:AddComponent(typeof(B2Framework.Net.NetSocketClient))
    -- end
    local manager = Game.NetMgr:Initialize()
    self.manager = manager
end
-- 发起连接
function NetManager:Connect()
    local host = "127.0.0.1"
    local port = 8964
    local timeout = 30
    self.manager:Connect(
        host,
        port,
        timeout,
        handler(self, self.OnConnect),
        handler(self, self.OnReceive),
        handler(self, self.OnFailed)
    )
end
-- 连接状态返回
function NetManager:OnConnect(status, msg)
end
-- 接受消息
function NetManager:OnReceive(cmd, data)
end
-- 发生错误
function NetManager:OnFailed()
end
-- 发送消息
function NetManager:Send(cmd, data)
    return self.manager:SendCmd(cmd, data)
end
-- 重连
function NetManager:Reconnect()
    self.manager:Reconnect()
end
-- 断开连接
function NetManager:Disconnect()
    self.manager:Disconnect()
end
-- 是否已经连接
function NetManager:IsConnect()
    return self.manager:IsConnect()
end
-- 关闭socket
function NetManager:Close()
    self.manager:Close()
end
-- Dispose
function NetManager:Dispose()
    self:Close()
    self.manager = nil
end

return NetManager
