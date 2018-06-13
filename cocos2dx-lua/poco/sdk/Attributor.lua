local Attributor = {}
Attributor.__index = Attributor

function Attributor:getAttr(node, attrName)
    local node_ = node
    if node.__isPocoNodeWrapper__ == nil then
        node_ = node[1]
    end
    return node_:getAttr(attrName)
end

function Attributor:setAttr(node, attrName, attrVal)
    local node_ = node
    if node.__isPocoNodeWrapper__ == nil then
        node_ = node[1]
    end
    node_:setAttr(attrName, attrVal)
end

return Attributor