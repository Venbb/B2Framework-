-----------------------------------------------------------
-- FileName:    UIStory.lua
-- Author:      Administrator
-- date:        2020-07-29 16:51:42
-- 
-----------------------------------------------------------
local UIBase = import '.UIBase'

local UIStory = class("UIStory", UIBase)

function UIStory:ctor(...)
    self.config = nil
    self.stepUI = {}
    self.stepChatBg = {}
    self.characters = {}
    self.BGMs = {}
    self.SpeekAudios = {}

    self.nowLeftCharacter = ""
    self.nowRightCharacter = ""
    self.nowBg = ""
    self.step = 0
    self.animationStep = 1
    self.langStep = 1
    self.totalLoadCount = 0;
    self.textIsTyping = true
    return self.super.ctor(self, ...)
end

function UIStory:Awake()
    self.config = self._param[1]
    self.callback = self._param[2]
    self:LoadRes()
    self._root.BackGround.gameObject:GetComponent("Button").onClick:AddListener(function()
        self:OnClick();
    end)

    self._root.ButtonSkip.gameObject:GetComponent("Button").onClick:AddListener(function()
        self:Finish();
    end)

    self._root.Image.Text.gameObject:GetComponent("TypewriterEffect"):BindEndCallback(function()
        self.textIsTyping = false
    end)
end

function UIStory:LoadRes()
    for i = 1, #self.config.step do
        --加载附属的UI
        if not self.stepUI[self.config.step[i].UI] then
            self.totalLoadCount = self.totalLoadCount + 1
            self.stepUI[self.config.step[i].UI] = ""
        end
        --如果有对话，加载对话图片背景，对话人物
        if self.config.step[i].chatBg and (not self.stepChatBg[self.config.step[i].chatBg]) then
            self.totalLoadCount = self.totalLoadCount + 1
            self.stepChatBg[self.config.step[i].chatBg] = ""
        end
        if self.config.step[i].characterLeft and (not self.characters[self.config.step[i].characterLeft]) then
            self.totalLoadCount = self.totalLoadCount + 1
            self.characters[self.config.step[i].characterLeft] = ""
        end
        if self.config.step[i].characterRight and (not self.characters[self.config.step[i].characterRight]) then
            self.totalLoadCount = self.totalLoadCount + 1
            self.characters[self.config.step[i].characterRight] = ""
        end
        if self.config.step[i].BGM and (not self.BGMs[self.config.step[i].BGM]) then
            self.totalLoadCount = self.totalLoadCount + 1
            self.BGMs[self.config.step[i].BGM] = ""
        end
        if self.config.step[i].SpeekAudio then
            for j = 1, #self.config.step[i].SpeekAudio do
                if not self.SpeekAudios[self.config.step[i].SpeekAudio[j]] then
                    self.totalLoadCount = self.totalLoadCount + 1
                    self.SpeekAudios[self.config.step[i].SpeekAudio[j]] = ""
                end
            end
        end
    end
    --这样分开写，有可能那个异步方法是同步的返回。这样会计算总数不断地同步+1-1(目前是这样的，可能后续C#那边加载会修改)。。。。
    for k, v in pairs(self.stepUI) do
        ResMgr:LoadAssetAsync(k, typeof(GameObject), function(prefab)
            local go = GameObject.Instantiate(prefab)
            go.transform:SetParent(self._root.transform)
            go.transform:SetAsFirstSibling()
            go:SetActive(false)
            self.stepUI[k] = go.name
            local rect = go:GetComponent("RectTransform")
            rect.localScale = Vector3(1, 1, 1)
            rect.localPosition = Vector3(0, 0, 0)
            rect.offsetMin = Vector2(0, 0)
            rect.offsetMax = Vector2(0, 0)
            self:OneResLoadFinish()
        end)
    end
    for k, v in pairs(self.stepChatBg) do
        ResMgr:LoadAssetAsync(k, typeof(UnityEngine.Sprite), function(sp)
            self.stepChatBg[k] = sp
            self:OneResLoadFinish()
        end)
    end
    for k, v in pairs(self.characters) do
        ResMgr:LoadAssetAsync(k, typeof(UnityEngine.Sprite), function(sp)
            self.characters[k] = sp
            self:OneResLoadFinish()
        end)
    end
    for k, v in pairs(self.stepChatBg) do
        ResMgr:LoadAssetAsync(k, typeof(UnityEngine.Sprite), function(sp)
            self.stepChatBg[k] = sp
            self:OneResLoadFinish()
        end)
    end
    for k, v in pairs(self.BGMs) do
        AudioMgr:LoadBG(k, function()
            self:OneResLoadFinish()
        end)
    end
    
    for k, v in pairs(self.SpeekAudios) do
        AudioMgr:LoadSE(k, function()
            self:OneResLoadFinish()
        end)
    end
end

function UIStory:OneResLoadFinish()
    self.totalLoadCount = self.totalLoadCount - 1
    if self.totalLoadCount == 0 then
        self._root = CommonFunc:GenerateTree(self._root.transform)
        self:AddAnimatoinListener()
        self:NextStep()
    end
end

function UIStory:AddAnimatoinListener()
    for k, v in pairs(self.stepUI) do
        self._root[v].gameObject:GetComponent("AnimationHelper"):AddListener("Finish", function()
            local animations = self.config.step[self.step].animations
            self.animationStep = self.animationStep + 1
            if self.animationStep <= #animations then
                local uiName = self.stepUI[self.config.step[self.step].UI]
                self._root[uiName].gameObject:GetComponent("Animation"):Stop()
                self._root[uiName].gameObject:GetComponent("Animation"):Play(animations[self.animationStep])
            end
        end)
    end
end

function UIStory:NextStep()
    self.step = self.step + 1
    if self.step > #self.config.step then
        self:Finish()
        return
    end
    self.animationStep = 1
    self.langStep = 1

    local UI = self.config.step[self.step].UI
    
    for k, v in pairs(self.stepUI) do
        if k == UI then
            self._root[v].gameObject:SetActive(true)
        else
            self._root[v].gameObject:SetActive(false)
        end
    end
    if self.config.step[self.step].animations then
        self:playNextByAnimation()
    elseif self.config.step[self.step].characterLeft or self.config.step[self.step].characterRight then
        self:playNextByDialog()
    else
        self:Finish()
    end
    if self.config.step[self.step].BGM then
        AudioMgr:PlayBackgroundMusic(self.config.step[self.step].BGM)
    end
end

function UIStory:playNextByAnimation()
    local lang = self.config.step[self.step].lang
    local animations = self.config.step[self.step].animations

    if self.langStep <= #lang then
        self.textIsTyping = true
        if self.config.step[self.step].SpeekAudio then
            AudioMgr:playSound(self.config.step[self.step].SpeekAudio[self.langStep], true)
        end
        self._root.Image.Text.gameObject:GetComponent("TypewriterEffect"):SetText(lang[self.langStep])
        if self.animationStep <= #animations then
            local uiName = self.stepUI[self.config.step[self.step].UI]
            self._root[uiName].gameObject:GetComponent("Animation"):Stop()
            self._root[uiName].gameObject:GetComponent("Animation"):Play(animations[self.animationStep])
        end
    else
        self:NextStep()
    end
end

function UIStory:playNextByDialog()
    self.textIsTyping = true
    local name = self.config.step[self.step].characterName or ""
    local lang = self.config.step[self.step].lang
    local uiName = self.stepUI[self.config.step[self.step].UI]

    if self.config.step[self.step].SpeekAudio then
        AudioMgr:playSound(self.config.step[self.step].SpeekAudio[self.langStep], true)
    end
    self._root.Image.Text.gameObject:GetComponent("TypewriterEffect"):SetText(name .. ":\n\n" .. lang[self.langStep])

    if self.config.step[self.step].chatBg and self.nowBg ~= self.config.step[self.step].chatBg then
        self.nowBg = self.config.step[self.step].chatBg
        self._root[uiName].bg.transform:GetComponent("Image").sprite = self.stepChatBg[self.config.step[self.step].chatBg]
    end

    if self.config.step[self.step].characterLeft then
        if self.nowLeftCharacter ~= self.config.step[self.step].characterLeft then
            self.nowLeftCharacter = self.config.step[self.step].characterLeft
            self._root[uiName].Left.transform:GetComponent("Image").sprite = self.characters[self.config.step[self.step].characterLeft]
        end
    end
    if self.config.step[self.step].characterRight then
        if self.nowRightCharacter ~= self.config.step[self.step].characterRight then
            self.nowRightCharacter = self.config.step[self.step].characterLeft
            self._root[uiName].Right.transform:GetComponent("Image").sprite = self.characters[self.config.step[self.step].characterRight]
        end
    end

    if self.config.step[self.step].LRSpeeking == "L" then
        self._root[uiName].Left.gameObject:GetComponent("RectTransform").localScale = Vector3(1, 1, 1)
        self._root[uiName].Left.gameObject:GetComponent("Image").color = Color(1, 1, 1, 1)
        if self.config.step[self.step].otherCharacterHideType then
            if self.config.step[self.step].otherCharacterHideType == 0 then
                local uiName = self.stepUI[self.config.step[self.step].UI]
                self._root[uiName].Right.gameObject:GetComponent("Image").color = Color(1, 1, 1, 0)
            elseif self.config.step[self.step].otherCharacterHideType == 1 then
                local uiName = self.stepUI[self.config.step[self.step].UI]
                self._root[uiName].Right.gameObject:GetComponent("RectTransform").localScale = Vector3(0.7, 0.7, 0.7)
                self._root[uiName].Right.gameObject:GetComponent("Image").color = Color(0.5, 0.5, 0.5, 1)
            end
        end
    else
        self._root[uiName].Right.gameObject:GetComponent("RectTransform").localScale = Vector3(1, 1, 1)
        self._root[uiName].Right.gameObject:GetComponent("Image").color = Color(1, 1, 1, 1)
        if self.config.step[self.step].otherCharacterHideType then
            if self.config.step[self.step].otherCharacterHideType == 0 then
                local uiName = self.stepUI[self.config.step[self.step].UI]
                self._root[uiName].Left.gameObject:GetComponent("Image").color = Color(1, 1, 1, 0)
            elseif self.config.step[self.step].otherCharacterHideType == 1 then
                local uiName = self.stepUI[self.config.step[self.step].UI]
                self._root[uiName].Left.gameObject:GetComponent("RectTransform").localScale = Vector3(0.7, 0.7, 0.7)
                self._root[uiName].Left.gameObject:GetComponent("Image").color = Color(0.5, 0.5, 0.5, 1)
            end
        end
    end 
end

function UIStory:OnClick()
    if self.step > #self.config.step then
        self:Finish()
        return
    end
    if self.textIsTyping then
        self._root.Image.Text.gameObject:GetComponent("TypewriterEffect"):Finish()
    else
        local lang = self.config.step[self.step].lang
        self.langStep = self.langStep + 1
        if self.langStep <= #lang then
            self.textIsTyping = true
            if self.config.step[self.step].SpeekAudio then
                AudioMgr:playSound(self.config.step[self.step].SpeekAudio[self.langStep], true)
            end
            self._root.Image.Text.gameObject:GetComponent("TypewriterEffect"):SetText(lang[self.langStep])
            if self.config.step[self.step].langToAnimation and self.animationStep < self.config.step[self.step].langToAnimation[self.langStep] then
                local animations = self.config.step[self.step].animations
                self.animationStep = self.config.step[self.step].langToAnimation[self.langStep]
                local uiName = self.stepUI[self.config.step[self.step].UI]
                self._root[uiName].gameObject:GetComponent("Animation"):Stop()
                self._root[uiName].gameObject:GetComponent("Animation"):Play(animations[self.animationStep])
            end
        else
            self:NextStep()
        end
    end
end

function UIStory:Finish()
    UIMgr:Remove(self)
    AudioMgr:StopSound()
    if self.callback then
        self.callback()
    end
end

function UIStory:OnDestroy()
    self.super.OnDestroy(self)
end
return UIStory