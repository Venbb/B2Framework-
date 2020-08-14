-----------------------------------------------------------
-- FileName:    UITestDlg1.lua
-- Author:      Administrator
-- date:        2020-05-22 16:51:42
-- 
-----------------------------------------------------------
local UIBase = import '.UIBase'

local UITestDlg1 = class("UITestDlg1", UIBase)

function UITestDlg1:ctor(...)
    print("UITestDlg1:ctor....")
    return self.super.ctor(self, ...)
end

function UITestDlg1:Awake()
    print("UITestDlg1:Awake.....", self._param)
    dump(self._root)

    UITestDlg1.super.Awake(self);
    print(self.Awake)
    print(self.gameObject)
    EvtMgr:AddListener(self, "lalala111", self.test1)
    EvtMgr:AddListener(self, "lalala111", self.test2)
    EvtMgr:AddListener(self, "lalala111", self.test3)
    EvtMgr:AddListener(self, "ggg111", self.test4)
    EvtMgr:LoveMeBabby()
    AudioMgr:PlayBackgroundMusic("Audio/MainScene.ogg")
    local iii = 0

    self._root.Image.Button.gameObject:GetComponent("Button").onClick:AddListener(function()
        UIMgr:Remove("UITestDlg1")
    end)

    self._root.UITestDlg2.gameObject:GetComponent("Button").onClick:AddListener(function()
        UIMgr:Show("UITestDlg2")
    end)

    self._root.ButtonLoadingLayer.gameObject:GetComponent("Button").onClick:AddListener(function()
        self.upd = true
        self.currentWeight = 0
        self.UILoading = UIMgr:Show("UILoading", 100, "Textures/biaoge.png", function(tr)
            self.upd = false
            ResMgr:LoadAssetAsync("Textures/test/f2.png", typeof(UnityEngine.Sprite), function(sp)
                print("sp: ", sp)
                tr.transform:Find("Slider/EndImage"):GetComponent("Image").sprite = sp
            end)
        end)
    end)

    self._root.Button1.gameObject:GetComponent("Button").onClick:AddListener(function()
        iii = iii + 1
        UIMgr:Show("UINoticeTip", iii)
    end)
    
    self._root.Button2.gameObject:GetComponent("Button").onClick:AddListener(function()
        UIMgr:Show("MessageBox", {
            icon = "Textures/test/biaoge.png",
            text = "彪哥彪哥，你真了不得！五行大山压不住你，蹦出个彪行者！",
            funcLeft = function()
                print("OKOKOK")
            end,
            showRightButton = true
        })
    end)

    self._root.Button3.gameObject:GetComponent("Button").onClick:AddListener(function()
        iii = iii + 1
        UIMgr:Show("UIWorldMessage", "世界消息啦啦啦，你和姐姐采棉花，姐姐采了：" .. iii .. "个（为什么我要说姐姐呢）")
    end)

    self._root.Button4.gameObject:GetComponent("Button").onClick:AddListener(function()
        local uiWaiting = UIMgr:Show("UIWaiting1")
        uiWaiting:Retain("aaa", function()
            print("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")
        end)
        UIMgr:Show("UIWaiting1"):Retain("bbb", function()
            print("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb")
        end)
        UIMgr:Show("UIWaiting1"):Retain("ccc", function()
            print("cccccccccccccccccccccccccccccccc")
        end)
        UIMgr:Show("UIWaiting1"):Retain("ddd", function()
            print("dddddddddddddddddddddddddddddddd")
        end)
        Timer:SetTimeout(1, function()
            uiWaiting:Release("aaa")
        end)
        Timer:SetTimeout(2, function()
            uiWaiting:Release("bbb")
        end)
        Timer:SetTimeout(3, function()
            uiWaiting:Release("ccc")
        end)
        Timer:SetTimeout(4, function()
            uiWaiting:Release("ddd")
        end)
    end)

    self._root.Button5.gameObject:GetComponent("Button").onClick:AddListener(function()
        local uiWaiting = UIMgr:Show("UIWaiting2")
        Timer:SetTimeout(2, function()
            uiWaiting:Release()
        end)
    end)

    self._root.Button6.gameObject:GetComponent("Button").onClick:AddListener(function()
        UserData.guideId = 1
        GuideMgr:Check(self)
    end)

    self._root.Button7.gameObject:GetComponent("Button").onClick:AddListener(function()
        UIMgr:Show("UITestDlg3")
    end)

    self._root.Button8.gameObject:GetComponent("Button").onClick:AddListener(function()
        local str = self._root.InputField.gameObject:GetComponent("InputField").text
        print("和谐之前： " , str)
        print("和谐之之后： " , Game.sensitiveWordsFilter:Check(str))
        UIMgr:Show("UINoticeTip", "请看LOG，界面没地方显示了。")
    end)

    self._root.Button9.gameObject:GetComponent("Button").onClick:AddListener(function()
        UIMgr:Show("UISetting")
    end)
end

function UITestDlg1:Update()
    if self.upd then
        self.currentWeight = self.currentWeight + 1
        self.UILoading:SetCurrentWeight(self.currentWeight)
    end
end

function UITestDlg1:test1()
    local loopSr = self.transform:Find("TabScrollView/TabScrollView/Viewport/Content/VerticalScroll_Grid").gameObject:GetComponent("LoopVerticalScrollRect")
    print("loopSr: ", loopSr.name)
    loopSr.prefabSource.prefabName = "ScrollCell"
    loopSr.totalCount = 160
    loopSr:RefillCells(0, false, function(go, index)
        print("-----", go.name, ",  index: ", index)
    end);
    print("--------111---test1")
end

function UITestDlg1:test2()
    print("--------111---test2")
end

function UITestDlg1:test3()
    print("--------111---test3")
end

function UITestDlg1:test4()
    print("--------111---test4")
    EvtMgr:RemoveListener(self, "lalala111", self.test2)
    EvtMgr:RemoveListener(self, "ggg111")
    EvtMgr:LoveMeBabby()
end

function UITestDlg1:Start()
    print("UITestDlg1:start.....")
    -- printf("vector:%s", self.vector:ToString())
    -- printf("color:%s", self.color:ToString())
    -- self.submit.onClick:AddListener(function()
    --     print("submit")
    --     printf("username:%s", self.username.text)
    -- end)
    EvtMgr:Notify("lalala111")
    EvtMgr:Notify("ggg111")
end

function UITestDlg1:OnDestroy()
    self.super.OnDestroy(self)
    -- self.submit.onClick:RemoveAllListeners()
end
return UITestDlg1