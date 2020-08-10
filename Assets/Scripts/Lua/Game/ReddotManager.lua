-----------------------------------------------------------
-- FileName:    ReddotManager.lua
-- 红点管理系统，红点数据在这个脚本统一调度。由编辑器自动生成，
-- 手动修改该脚本可能导致别人的再次生成给你的修改顶掉
-----------------------------------------------------------

local ReddotManager = class("ReddotManager")

function ReddotManager:ctor(...)
    
end

function ReddotManager:GetReddot_DailyMissionSingle(id)
   return GameData.GetDailyMissionSingleReddot(id)
end

function ReddotManager:GetReddot_DailyMissionAll()
   return GameData.GetMissionAwardCount(Define.MissionType.Daily)
end

function ReddotManager:GetReddot_NPCMissionSingle(id)
   return GameData.GetNPCMissionSingleReddot(id)
end

function ReddotManager:GetReddot_NPCMissionAll()
   return GameData.GetMissionAwardCount(Define.MissionType.NPC)
end

function ReddotManager:GetReddot_MainMissionIcon()
   return self.GetReddot_DailyMissionAll() + self.GetReddot_NPCMissionAll()
end


return ReddotManager