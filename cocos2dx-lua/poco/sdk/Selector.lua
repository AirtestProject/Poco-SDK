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


local function _table_contains(t, v)
    for _, tv in ipairs(t) do
        if tv == v then
            return true
        end
    end
    return false
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
                _res = self:selectImpl(arg, true, parent, _maxDepth, onlyVisibleNode, false)
                for _, r in ipairs(_res) do
                    if not _table_contains(midResult, r) then
                        table.insert(midResult, r)
                    end
                end
            end
            parents = midResult
        end
        result = parents
    elseif op == '-' then
        -- 兄弟节点选择
        local query1, query2 = unpack(args)
        local result1 = self:selectImpl(query1, multiple, root, maxDepth, onlyVisibleNode, includeRoot)
        for _, n in ipairs(result1) do
            sibling_result = self:selectImpl(query2, multiple, n:getParent(), 1, onlyVisibleNode, includeRoot)
            for _, r in ipairs(sibling_result) do
                if not _table_contains(result, r) then
                    table.insert(result, r)
                end
            end
        end
    elseif op == 'index' then
        local cond, i = unpack(args)
        result = {self:selectImpl(cond, multiple, root, maxDepth, onlyVisibleNode, includeRoot)[i + 1]}
    elseif op == '^' then
        -- parent
        -- only select parent of the first matched UI element
        local query1, _ = unpack(args)
        local result1 = self:selectImpl(query1, false, root, maxDepth, onlyVisibleNode, includeRoot)
        if #result1 > 0 then
            local parent_node = result1[1]:getParent()
            if parent_node ~= nil then
                result = {parent_node}
            end
        end
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
            if not _table_contains(outResult, node) then
                table.insert(outResult, node)
            end
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