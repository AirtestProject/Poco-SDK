local IDumper = {}
IDumper.__index = IDumper

function IDumper:getRoot() 
    -- :rettype: support.poco.sdk.AbstractNode
end

function IDumper:dumpHierarchy() 
    -- :rettype: dict or NoneType
end

local AbstractDumper = {}
AbstractDumper.__index = AbstractDumper
setmetatable(AbstractDumper, IDumper)


function AbstractDumper:dumpHierarchy() 
    return self:dumpHierarchyImpl(self:getRoot())
end


function AbstractDumper:dumpHierarchyImpl(node, onlyVisibleNode)
    if node == nil then
        return nil
    end
    if onlyVisibleNode == nil then
        onlyVisibleNode = true
    end

    local payload = node:enumerateAttrs()
    local result = {}
    local children = {}
    for _, child in ipairs(node:getChildren()) do
        if not onlyVisibleNode or (payload['visible'] or child:getAttr('visible')) then
            table.insert(children, self:dumpHierarchyImpl(child, onlyVisibleNode))
        end
    end
    if #children > 0 then
        result['children'] = children
    end
    
    result['name'] = payload['name'] or node:getAttr('name')
    result['payload'] = payload
    
    return result
end

return AbstractDumper