-----------------------------------------------------------
-- FileName:    StoryManager.lua
-- Author:      zhbd
-- date:        2020-07-29 11:09:25
-- 
-----------------------------------------------------------
local StoryConfig = require("Game.Data.Config.StoryConfig")
local StoryManager = class("StoryManager")

function StoryManager:ctor(...)
    
end

function StoryManager:Check(id, callback)
    if id <= UserData.storyId then
        local config = StoryConfig[id]
        if config.needLevel <= UserData.level then
            UIMgr:Show("UIStory", config, callback)
            return
        end
    end
    if callback then
        callback()
    end
end

return StoryManager