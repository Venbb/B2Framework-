-----------------------------------------------------------
-- FileName:    UIManager.lua
-- Author:      Administrator
-- date:        2020-05-26 15:32:44
-- 
-----------------------------------------------------------
local UIConfig = require('Game.UI.UIConfig')

local UIManager = class("UIManager")
function UIManager:ctor(...)
    self.UIRoot = {
        --一般UI界面、弹窗UI等
        [Define.WindowType.Base] = GameObject.Find("UIRoot/UICanvas/Base"),
        --顶层UI：比如类似那种世界消息、全局通告、弹幕、战力增加提示等，在操作层的上层
        [Define.WindowType.Top] = GameObject.Find("UIRoot/UICanvas/Top"),
        --超级弹出：这个不是常规弹窗，这个是那种比如断线重连这种弹窗。当发生显示该弹窗事件，将阻止下面所有UI发生的事情
        [Define.WindowType.PopUp] = GameObject.Find("UIRoot/UICanvas/PopUp"),
    }
    self.UIShow = {}
    self.UIStack = {
        [Define.WindowType.Base] = {},
        [Define.WindowType.Top] = {},
        [Define.WindowType.PopUp] = {}
    }
    for k, v in pairs(self.UIRoot) do
        print(k)
    end
    self.isLoading = false
end

function UIManager:LoadUI(path, loadCallback)
    ResMgr:LoadAssetAsync(path, typeof(GameObject), function(prefab)
        local go = GameObject.Instantiate(prefab)
        loadCallback(go)
    end)
end

--[[]
    1、正在由弹窗创建中，则无反应，继续等待就好。
    2、将要显示的UI正在显示，则该UI提升到最顶层。
    3、除了以上情况，则创建并显示该UI
]]
function UIManager:Show(name, ...)
    if self.isLoading then
        if self.UIShow[name] then
            return self.UIShow[name], false
        end
        return nil, false
    end
    local param = {...}
    if self.UIShow[name] then
        self:RefreshActiveState(name, false, param)
        return self.UIShow[name], true
    end
    
    self.isLoading = true

    local layer = require(UIConfig[name].luaScriptPath).new(unpack(param))
    layer.name = name
    layer.windowType = UIConfig[name].windowType
    self.UIShow[name] = layer

    self:LoadUI(UIConfig[name].prefab, function(go)
        local UIRoot = self.UIRoot[UIConfig[name].windowType]
        go.transform:SetParent(UIRoot.transform)
        go:SetActive(false)

        local LuaBehaviour = go:AddComponent(typeof(B2Framework.LuaBehaviour))
        LuaBehaviour.metatable = layer
        layer.transform = go.transform
        layer.gameObject = go
        --只在新建的时候生成一下，此后里面代码动态添加的全都不算进来
        layer._root = CommonFunc:GenerateTree(go.transform)

        self.isLoading = false
        self.UIShow[name].isLoading = false
        
        self:RefreshActiveState(name, true, param)
        self:FixLayerSizeAndPosition(go)
    end)

    return layer, true
end

function UIManager:Remove(layer, forceDestroy)
    local name = ""
    if type(layer) == "table" then
        name = layer.name
    else
        name = layer
    end
    if self.UIShow[name] == nil then
        return
    end

    local windowType = UIConfig[name].windowType
    for i = 1, #self.UIStack[windowType] do
        if self.UIStack[windowType][i] == self.UIShow[name] then
            table.remove(self.UIStack[windowType], i)
            break
        end
    end
    if UIConfig[name].cacheWhenRemove and (not forceDestroy) then
        self.UIShow[name].gameObject:SetActive(false)
        self.UIShow[name].transform:SetAsFirstSibling()
        table.insert(self.UIStack[windowType], 1, self.UIShow[name])
    else
        self.UIShow[name]:Destory()
        self.UIShow[name] = nil
    end
    if #self.UIStack[windowType] > 0 then
        local uplayer = self.UIStack[windowType][#self.UIStack[windowType]]
        self:RefreshActiveState(uplayer.name, false, uplayer._param)
    end
end

function UIManager:Clear(forceDestroy)
    for k, _ in pairs(self.UIShow) do
        self:Remove(k, forceDestroy)
    end
end

function UIManager:RefreshActiveState(name, isNewLayer, param)
    local windowType = UIConfig[name].windowType
    local layer = self.UIShow[name]
    local needInsert = true

    --如果不是新的Layer的话，#self.UIStack[windowType]肯定大于0的
    if not isNewLayer then
        local tblIdx = #self.UIStack[windowType]
        for i = 1, #self.UIStack[windowType] do
            if self.UIStack[windowType][i] == layer then
                tblIdx = i
                break
            end
        end
        if tblIdx ~= #self.UIStack[windowType] then
            table.remove(self.UIStack[windowType], tblIdx)
        else
            needInsert = false
        end
        layer.transform:SetAsLastSibling()
    end

    if needInsert then
        table.insert(self.UIStack[windowType], layer)
    end

    if UIConfig[name].hideOtherLayer then
        for i = #self.UIStack[windowType] - 1, 1, -1 do
            self.UIStack[windowType][i].gameObject:SetActive(false)
        end
    else
        for i = #self.UIStack[windowType] - 1, 1, -1 do
            self.UIStack[windowType][i].gameObject:SetActive(true)
            if UIConfig[self.UIStack[windowType][i].name].hideOtherLayer then
                break
            end
        end
    end
    layer.gameObject:SetActive(true)
    --是新的Layer或者这个Layer在下层被新建提了上来,所以检查下默认显示动画
    if isNewLayer or needInsert then
        self:CheckShowAnimation(layer.gameObject)
    end
    layer:RefreshParam(unpack(param))
end

function UIManager:CheckShowAnimation(go)
    local animation = go:GetComponent("Animation")
    if animation and (not animation:Equals(nil)) then
        animation:Play()
    end
end

function UIManager:FixLayerSizeAndPosition(go)
    local rect = go:GetComponent("RectTransform")
    rect.localScale = Vector3(1, 1, 1)
    rect.localPosition = Vector3(0, 0, 0)
    rect.offsetMin = Vector2(0, 0)
    rect.offsetMax = Vector2(0, 0)
end

return UIManager