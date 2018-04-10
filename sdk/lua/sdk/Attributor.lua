local Attributor = {}
Attributor.__index = Attributor

function Attributor:getAttr(node, attrName)
    if node.__isPocoNodeWrapper__ == nil then
        node = node[1]
    end
    return node:getAttr(attrName)
end

function Attributor:setAttr(node, attrName, attrVal)
    if node.__isPocoNodeWrapper__ == nil then
        node = node[1]
    end
    node:setAttr(attrName, attrVal)
end

return Attributor