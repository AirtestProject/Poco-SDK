-- import
function import(moduleName, currentModuleName)
    local currentModuleNameParts
    local moduleFullName = moduleName
    local offset = 1

    while true do
        if string.byte(moduleName, offset) ~= 46 then
            moduleFullName = string.sub(moduleName, offset)
            if currentModuleNameParts and #currentModuleNameParts > 0 then
                moduleFullName = table.concat(currentModuleNameParts, ".") .. "." .. moduleFullName
            end
            break
        end
        offset = offset + 1

        if not currentModuleNameParts then
            if not currentModuleName then
                local n, v = debug.getlocal(3, 1)
                currentModuleName = v
            end
            currentModuleNameParts = string.split(currentModuleName, ".")
        end
        table.remove(currentModuleNameParts, #currentModuleNameParts)
    end

    return require(moduleFullName)
end

local DefaultMatcher = import('.DefaultMatcher')

local function _table_append(t1, t2)
    for _, v in ipairs(t2) do
        table.insert(t1, v)
    end
    return t1
end


local Selector = {}
Selector.__index = Selector

function Selector:new(dumper, matcher)
    local s = {}
    setmetatable(s, Selector)
    s.dumper = dumper
    s.matcher = matcher or DefaultMatcher
    return s
end

function Selector:getRoot()
    return self.dumper:getRoot()
end

function Selector:select(cond, multiple)
    if multiple == nil then
        multiple = false
    end
    return self:selectImpl(cond, multiple, self:getRoot(), 9999, true, true)
end

function Selector:selectImpl(cond, multiple, root, maxDepth, onlyVisibleNode, includeRoot)
    -- 凡是visible为False后者parentVisible为false的都不选
    local result = {}
    if root == nil then
        return result
    end

    local op, args = unpack(cond)
    if op == '>' or op == '/' then
        -- 相对选择
        local parents = {root}
        for index, arg in ipairs(args) do
            local midResult = {}
            for _, parent in ipairs(parents) do
                local _maxDepth = maxDepth
                if op == '/' and index ~= 1 then
                    _maxDepth = 1
                end
                -- 按路径进行遍历一定要multiple为true才不会漏掉
                _table_append(midResult, self:selectImpl(arg, true, parent, _maxDepth, onlyVisibleNode, false))
            end
            parents = midResult
        end
        result = parents
    elseif op == '-' then
        -- 兄弟节点选择
        local query1, query2 = unpack(args)
        local result1 = self:selectImpl(query1, multiple, root, maxDepth, onlyVisibleNode, includeRoot)
        for _, n in ipairs(result1) do
            _table_append(result, self:selectImpl(query2, multiple, n:getParent(), 1, onlyVisibleNode, includeRoot))
        end
    elseif op == 'index' then
        local cond, i = unpack(args)
        result = {self:selectImpl(cond, multiple, root, maxDepth, onlyVisibleNode, includeRoot)[i + 1]}
    else
        self:_selectTraverse(cond, root, result, multiple, maxDepth, onlyVisibleNode, includeRoot)
    end

    return result
end

function Selector:_selectTraverse(cond, node, outResult, multiple, maxDepth, onlyVisibleNode, includeRoot)
    -- 剪掉不可见节点branch
    if onlyVisibleNode and not node:getAttr('visible') then
        return false
    end

    if self.matcher:match(cond, node) then
        if includeRoot then
            table.insert(outResult, node)
            if not multiple then
                return true
            end
        end
    end

    -- 最大搜索深度耗尽并不表示遍历结束，其余child节点仍需遍历
    if maxDepth == 0 then
        return false
    end
    maxDepth = maxDepth - 1

    for _, child in ipairs(node:getChildren()) do
        local finished = self:_selectTraverse(cond, child, outResult, multiple, maxDepth, onlyVisibleNode, true)
        if finished then
            return true
        end
    end

    return false
end


return Selector