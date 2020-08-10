-----------------------------------------------------------
-- FileName:    UITestDlg2.lua
-- Author:      Administrator
-- date:        2020-05-22 16:51:42
-- 
-----------------------------------------------------------
local UIBase = import '.UIBase'

local UITestDlg2 = class("UITestDlg2", UIBase)

function UITestDlg2:ctor(...)
    print("UITestDlg2:ctor....")
    return self.super.ctor(self, ...)
end

function UITestDlg2:Awake()
    print("UITestDlg2:Awake.....")
    UITestDlg2.super.Awake(self);
    print(self.Awake)
    print(self.gameObject)
    EvtMgr:AddListener(self, "lalala222", self.test1)
    EvtMgr:AddListener(self, "lalala222", self.test2)
    EvtMgr:AddListener(self, "lalala222", self.test3)
    EvtMgr:AddListener(self, "ggg222", self.test4)
    EvtMgr:LoveMeBabby()

    self.transform:Find("ButtonClose").gameObject:GetComponent("Button").onClick:AddListener(function()
        UIMgr:Remove("UITestDlg2")
    end)

    self.transform:Find("BtnToBattleScene").gameObject:GetComponent("Button").onClick:AddListener(function()
        self.upd = true
        self.currentWeight = 0
        self.UILoading = UIMgr:Show("UILoading", 200, "Textures/test/biaogegogogo.png", function(tr)
            self.upd = false
            SceneMgr:LoadSceneAsync("BattleScene", {p1 = "bbbb"}, function()
                    
            end)
        end)
    end)

    self.transform:Find("Button1").gameObject:GetComponent("Button").onClick:AddListener(function()
        UIMgr:Show("UITestDlg1")
    end)

    self.transform:Find("Button2").gameObject:GetComponent("Button").onClick:AddListener(function()
        UIMgr:Show("UITestDlg3")
    end)
    
    self.transform:Find("Button3").gameObject:GetComponent("Button").onClick:AddListener(function()
        StoryMgr:Check(1, function()
            self.upd = true
            self.currentWeight = 0
            self.UILoading = UIMgr:Show("UILoading", 200, "Textures/test/biaogegogogo.png", function(tr)
                self.upd = false
                SceneMgr:LoadSceneAsync("BattleScene", {p1 = "bbbb"}, function()
                        
                end)
            end)
        end)
    end)
end

function UITestDlg2:test1()
    print("--------222---test1")
end

function UITestDlg2:test2()
    print("--------222---test2")
end

function UITestDlg2:test3()
    print("--------222---test3")
end

function UITestDlg2:test4()
    print("--------222---test4")
    EvtMgr:RemoveListener(self, "lalala222", self.test2)
    EvtMgr:RemoveListener(self, "ggg222")
end

function UITestDlg2:Start()
    print("UITestDlg2:start.....")
    -- printf("vector:%s", self.vector:ToString())
    -- printf("color:%s", self.color:ToString())
    -- self.submit.onClick:AddListener(function()
    --     print("submit")
    --     printf("username:%s", self.username.text)
    -- end)
    EvtMgr:Notify("lalala222")
    EvtMgr:Notify("ggg222")
    EvtMgr:LoveMeBabby()
end

function UITestDlg2:Update()
    if self.upd then
        self.currentWeight = self.currentWeight + 1
        self.UILoading:SetCurrentWeight(self.currentWeight)
    end
end

function UITestDlg2:OnDestroy()
    self.super.OnDestroy(self)
    -- self.submit.onClick:RemoveAllListeners()
end
return UITestDlg2