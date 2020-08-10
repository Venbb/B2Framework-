-----------------------------------------------------------
-- FileName:    UILogin.lua
-- Author:      Administrator
-- date:        2020-05-22 16:51:42
-- 
-----------------------------------------------------------
local UIBase = import '.UIBase'

local UILogin = class("UILogin", UIBase)

function UILogin:ctor(...)
    print("UILogin:ctor....")
    return self.super.ctor(self, ...)
end

function UILogin:Awake()
    print("UILogin:Awake.....")
    UILogin.super.Awake(self);
    print(self.Awake)
    print(self.gameObject)
    EvtMgr:AddListener(self, EventKey.Test1, self.test1)
    EvtMgr:AddListener(self, EventKey.Test1, self.test2)
    EvtMgr:AddListener(self, EventKey.Test1, self.test3)
    EvtMgr:AddListener(self, EventKey.Test2, self.test4)
    EvtMgr:LoveMeBabby()
    
    AudioMgr:PlayBackgroundMusic("Audio/Login.ogg")
    self.transform:Find("Button").gameObject:GetComponent("Button").onClick:AddListener(function()
        SceneMgr:LoadSceneAsync("Main", nil, function()
                    
        end)
    end)
end

function UILogin:test1()
    print("test1")
end

function UILogin:test2()
    print("test2")
end

function UILogin:test3()
    print("test3")
end

function UILogin:test4()
    print("test4")
    EvtMgr:RemoveListener(self, EventKey.Test1, self.test2)
    EvtMgr:RemoveListener(self, EventKey.Test2)
    EvtMgr:LoveMeBabby()
end

function UILogin:Start()
    print("UILogin:start.....")
    -- printf("vector:%s", self.vector:ToString())
    -- printf("color:%s", self.color:ToString())
    -- self.submit.onClick:AddListener(function()
    --     print("submit")
    --     printf("username:%s", self.username.text)
    -- end)
    EvtMgr:Notify(EventKey.Test1)
    EvtMgr:Notify(EventKey.Test2)
end

function UILogin:OnDestroy()
    self.super.OnDestroy(self)
    -- self.submit.onClick:RemoveAllListeners()
end
return UILogin