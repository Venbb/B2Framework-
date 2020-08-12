-----------------------------------------------------------
-- FileName:    ResourcesManager.lua
-- Author:      Administrator
-- date:        2020-06-12 14:09:06
-- 
-- 封装AssetsMgr,不建议直接访问The.AssetsMgr
-----------------------------------------------------------
-- Assets加载管理器
local Assets = UnityB2Framework.Assets

local ResourcesManager = class("ResourcesManager")

function ResourcesManager:ctor(...)
    -- TODO:do something on ctor
end

---
-- 同步方式加载资源
-- @param path 资源路径，如"Localization/lc_cn_ERROR.json"
-- @return res_type 资源类型，如typeof(GameObject)
-- @return 返回对应类型资源
---
function ResourcesManager:LoadAsset(path, res_type)
    local request = Assets.LoadAsset(path, res_type)
    local asset = request.asset
    request:Release();
    return asset
    -- Assets.UnloadAsset(request);
end

---
-- 异步方式加载资源
-- @param path 资源路径，如"Localization/lc_cn_ERROR.json"
-- @param res_type 资源类型，如typeof(GameObject)
-- @param callback 回调方法
---
function ResourcesManager:LoadAssetAsync(path, res_type, callback)
    assert(callback ~= nil and type(callback) == "function", "Need to provide a function as callback")
    local request = Assets.LoadAssetAsync(path, res_type)
    request.completed = function(re)
        local asset = re.asset
        if (callback) then callback(asset) end
        re:Release()
    end
end

---
-- 卸载未使用的资源
---
function ResourcesManager:UnloadUnusedAssets()
    Assets.UnloadUnusedAssets()
end
return ResourcesManager