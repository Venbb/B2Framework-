-----------------------------------------------------------
-- FileName:    CommonFunc.lua
-- Author:      zhbd
-- date:        2020-07-10 13:51:06
-- 
-----------------------------------------------------------

local CommonFunc = {}

--不会写peer，就这么弄了。。。
function CommonFunc:GenerateTree(transform)
    local tree = nil
    if transform then
        tree = {}
        tree["gameObject"] = transform.gameObject
        tree["transform"] = transform
        if transform.childCount > 0 then
            for i = 0, transform.childCount - 1 do
                local tr = transform:GetChild(i).transform
                tree[tr.gameObject.name] = CommonFunc:GenerateTree(tr)
            end
        end
    end
    return tree
end

return CommonFunc