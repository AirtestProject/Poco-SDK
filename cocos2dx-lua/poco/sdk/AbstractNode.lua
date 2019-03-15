local AbstractNode = {}
AbstractNode.__index = AbstractNode
AbstractNode.__isPocoNodeWrapper__ = true

function AbstractNode:getParent()
    --
    -- :rettype: AbstractNode or nil
    
    return nil
end
    
function AbstractNode:getChildren()
    -- 
    -- :rettype: Iterable<AbstractNode>
end

function AbstractNode:getAvailableAttributeNames()
    -- get available attributes of this node
    -- :rettype: Iterable<string>

    return {
        "name",
        "type",
        "visible",
        "pos",
        "size",
        "scale",
        "anchorPoint",
        "zOrders",
    }
end

function AbstractNode:getAttr(attrName)
    -- 
    -- :rettype: <any>

    local attrs = {
        name = '<Root>',
        type = 'Root',
        visible = true,
        pos = {0.0, 0.0},
        size = {0.0, 0.0},
        scale = {1.0, 1.0},
        anchorPoint = {0.5, 0.5},
        zOrders = {['local'] = 0, ['global'] = 0},
    }
    return attrs[attrName]
end

function AbstractNode:setAttr(attrName, val)
    --
    -- :retval: true if success else false
    
    assert(false, string.format('unable to set attributes "%s" on this node', attrName))
    return false
end
    
function AbstractNode:enumerateAttrs()
    --
    -- :rettype: Iterable<string, ValueType>

    local attrs = {}
    for _, attrName in ipairs(self:getAvailableAttributeNames()) do
        local attrVal = self:getAttr(attrName)
        if attrVal ~= nil then
            attrs[attrName] = attrVal
        end
    end
    return attrs
end

return AbstractNode