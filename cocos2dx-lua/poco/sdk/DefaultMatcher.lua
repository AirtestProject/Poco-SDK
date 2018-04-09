local EqualizationComparator = {}

function EqualizationComparator:compare(l, r)
    return l == r
end


local RegexpComparator = {}

function RegexpComparator:compare(origin, pattern)
    if origin == nil or pattern == nil then
        return false
    end
    return string.match(origin, pattern) ~= nil
end


local DefaultMatcher = {}
DefaultMatcher.__index = DefaultMatcher

DefaultMatcher.comparators = {
    ['attr='] = EqualizationComparator,
    ['attr.*='] = RegexpComparator,
}


function DefaultMatcher:match(cond, node)
    local op, args = unpack(cond)

    -- 条件匹配
    if op == 'or' then
        for _, arg in ipairs(args) do
            if self:match(arg, node) then
                return true
            end
        end
        return false
    end

    if op == 'and' then
        for _, arg in ipairs(args) do
            if not self:match(arg, node) then
                return false
            end
        end
        return true
    end

    -- 属性匹配
    comparator = self.comparators[op]
    if comparator ~= nil then
        local attribute, value = unpack(args)
        local targetValue = node:getAttr(attribute)
        return comparator:compare(targetValue, value) 
    end

    return false
end

return DefaultMatcher