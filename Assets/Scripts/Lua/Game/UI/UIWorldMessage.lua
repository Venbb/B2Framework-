-----------------------------------------------------------
-- FileName:    UIWorldMessage.lua
-- Author:      Administrator
-- date:        2020-05-22 16:51:42
-- 
-----------------------------------------------------------
local UIBase = import '.UIBase'
local UIWorldMessage = class("UIWorldMessage", UIBase)

function UIWorldMessage:ctor(...)
    self.MessageList = {}
    self.nowMsgPage = 1
    self.canPlayNext = true
    self.isPlaying1 = false
    self.isPlaying2 = false
    return self.super.ctor(self, ...)
end

function UIWorldMessage:OnEnable()
    self.super.OnEnable(self)
    --无法播放下一个，说明这个是隐藏再启动，那么继续播放就好
    -- if self.canPlayNext == false then
    --     if self.nowMsgPage == 1 then
    --         self._root.Background.Text1.gameObject:GetComponent("Animation"):Play()
    --     elseif self.nowMsgPage == 2 then
    --         self._root.Background.Text2.gameObject:GetComponent("Animation"):Play()
    --     end
    -- end
    if self.isPlaying1 then
        self._root.Background.Text1.gameObject:GetComponent("AnimationHelper"):Resume()
    end
    if self.isPlaying2 then
        self._root.Background.Text2.gameObject:GetComponent("AnimationHelper"):Resume()
    end
end

function UIWorldMessage:Awake()
    UIWorldMessage.super.Awake(self);
    self._root.Background.Text1.gameObject:GetComponent("AnimationHelper"):AddListener("MoveOutCenter", function()
        self.canPlayNext = true
        self.nowMsgPage = 2
        self:CheckAndPlay(true)
    end)
    self._root.Background.Text2.gameObject:GetComponent("AnimationHelper"):AddListener("MoveOutCenter", function()
        self.canPlayNext = true
        self.nowMsgPage = 1
        self:CheckAndPlay(true)
    end)
    self._root.Background.Text1.gameObject:GetComponent("AnimationHelper"):AddListener("MoveToEnd", function()
        self.isPlaying1 = false
        if #self.MessageList == 0 and self._root.Background.Text2.gameObject:GetComponent("Animation").isPlaying == false then
            UIMgr:Remove(self)
        end
    end)
    self._root.Background.Text2.gameObject:GetComponent("AnimationHelper"):AddListener("MoveToEnd", function()
        self.isPlaying2 = false
        if #self.MessageList == 0 and self._root.Background.Text1.gameObject:GetComponent("Animation").isPlaying == false then
            UIMgr:Remove(self)
        end
    end)
end

function UIWorldMessage:Start()
    
end

function UIWorldMessage:OnRefreshParam(param)
    if param[1] then
        table.insert(self.MessageList, param[1])
        self:CheckAndPlay(false)
    end
end

function UIWorldMessage:CheckAndPlay(calledByAnimation)
    if #self.MessageList == 0 then
        self.MessageList = {}
        return
    end

    if self.canPlayNext then
        if self.nowMsgPage == 1 then
            self._root.Background.Text1.gameObject:GetComponent("Text").text = self.MessageList[1]
            self._root.Background.Text1.gameObject:GetComponent("Animation"):Stop()
            self._root.Background.Text1.gameObject:GetComponent("Animation"):Play()
            self.isPlaying1 = true
        elseif  self.nowMsgPage == 2 then
            self._root.Background.Text2.gameObject:GetComponent("Text").text = self.MessageList[1]
            self._root.Background.Text2.gameObject:GetComponent("Animation"):Stop()
            self._root.Background.Text2.gameObject:GetComponent("Animation"):Play()
            self.isPlaying2 = true
        end
        self.canPlayNext = false
        table.remove(self.MessageList, 1)
    end
end

function UIWorldMessage:OnDisable()
    self.super.OnDisable(self)
end

function UIWorldMessage:OnDestroy()
    self.super.OnDestroy(self)
end

return UIWorldMessage