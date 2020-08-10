-----------------------------------------------------------
-- FileName:    UISpaceCraft.lua
-- Author:      Administrator
-- date:        2020-05-22 16:51:42
-- 
-----------------------------------------------------------
local UIBase = import '.UIBase'

local UISpaceCraft = class("UISpaceCraft", UIBase)

function UISpaceCraft:ctor(...)
    print("UISpaceCraft:ctor....")
    return self.super.ctor(self, ...)
end

function UISpaceCraft:Awake()
    AudioMgr:PlayBackgroundMusic("Audio/Boss.ogg")
    self._root.Lose.Button.gameObject:GetComponent("Button").onClick:AddListener(function()
        SceneMgr:LoadSceneAsync("Main", nil, function()
                    
        end)
    end)
    self._root.Win.Button.gameObject:GetComponent("Button").onClick:AddListener(function()
        SceneMgr:LoadSceneAsync("Main", nil, function()
                    
        end)
    end)
end

function UISpaceCraft:Start()
    EvtMgr:AddListener(self, "craftblood", function(caller, value)
        if self.win then
            return
        end
        self.transform:Find("Slider").gameObject:GetComponent("Slider").maxValue = value.maxblood
        self.transform:Find("Slider").gameObject:GetComponent("Slider").value = value.nowblood
        if value.nowblood == 0 then
            self._root.Lose.gameObject:SetActive(true)
            AudioMgr:PlayBackgroundMusicGroup({"Audio/Finish1.ogg","Audio/Finish2.ogg"}, 2)
        end
    end)
    EvtMgr:AddListener(self, "win", function(caller, value)
        self.win = true
        self._root.Win.gameObject:SetActive(true)
        AudioMgr:PlayBackgroundMusicGroup({"Audio/Finish1.ogg","Audio/Finish2.ogg"}, 2)
    end)
end

function UISpaceCraft:OnDestroy()
    self.super.OnDestroy(self)
    -- self.submit.onClick:RemoveAllListeners()
end

return UISpaceCraft