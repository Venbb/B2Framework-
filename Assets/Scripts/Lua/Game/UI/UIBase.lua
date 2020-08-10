-----------------------------------------------------------
-- FileName:    UIBase.lua
-- Author:      Venbb
-- date:        2020-05-25 21:46:15
-- 
-- UI 基类
-----------------------------------------------------------
local UIBase = class("UIBase")

function UIBase:ctor(...)
    self._param = {...}
end

function UIBase:Awake()

end

function UIBase:OnEnable()
    if self.windowType == Define.WindowType.Base then
        GuideMgr:Check(self)
    end
end

function UIBase:Destory()
    GameObject.Destroy(self.gameObject)
end

function UIBase:OnDisable()
    
end

function UIBase:OnDestroy()
    self._param = nil
    self.transform = nil
    self.gameObject = nil
    EvtMgr:RemoveListener(self)
end

function UIBase:RefreshParam(...)
    self._param = {...}
    if self.OnRefreshParam then
        self:OnRefreshParam(self._param)
    end
end

return UIBase