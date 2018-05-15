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

local cc = _G.cc or require('cc')
local AbstractNode = import('.sdk.AbstractNode')

local Node = {}
Node.__index = Node
setmetatable(Node, AbstractNode)

function Node:new(node, screenWidth, screenHeight)
    local n = {}
    setmetatable(n, Node)
    n.node = node
    n.screenWidth = screenWidth
    n.screenHeight = screenHeight
    return n
end


function Node:getParent()
    local parent = self.node:getParent()
    if parent == nil then
        return nil
    end
    return self:new(parent, self.screenWidth, self.screenHeight)
end

function Node:getChildren()
    local children = {}
    for _, child in ipairs(self.node:getChildren()) do
        table.insert(children, self:new(child, self.screenWidth, self.screenHeight))
    end
    return children
end

function Node:getAvailableAttributeNames()
    local ret = {
        'text',
        'touchable',
        'enabled',
        'tag',
        'desc',
        'rotation',
        'rotation3D',
        'skew',
    }
    for _, name in ipairs(AbstractNode.getAvailableAttributeNames(self)) do
        table.insert(ret, name)
    end
    return ret
end

function Node:getAttr(attrName)
    if attrName == 'visible' then
        local visible = self.node:isVisible()
        if not visible then
            return false
        end

        -- if the node is visible, check its parent's visibility
        local parent = self.node:getParent()
        while parent do
            local parentVisible = parent:isVisible()
            if not parentVisible then
                return false
            end
            parent = parent:getParent()
        end
        return true

    elseif attrName == 'name' then
        local name = self.node:getName()
        if name == '' then
            name = self.node:getDescription()
        end
        return name

    elseif attrName == 'text' then
        -- auto strip
        if self.node.getString then
            return self.node:getString():match("^%s*(.-)%s*$")    -- for Label
        elseif self.node.getStringValue then
            return self.node:getStringValue():match("^%s*(.-)%s*$")  -- for TextField
        elseif self.node.getTitleText then
            return self.node:getTitleText():match("^%s*(.-)%s*$")  -- for Button
        end
        return nil

    elseif attrName == 'type' then
        local nodeType = tolua.type(self.node)
        nodeType = nodeType:gsub("^ccui%.", '')
        nodeType = nodeType:gsub("^cc%.", '')
        return nodeType

    elseif attrName == 'pos' then
        -- 转换成归一化坐标系，原点左上角
        local pos = self.node:convertToWorldSpaceAR(cc.p(0, 0))
        pos.x = pos.x / self.screenWidth
        pos.y = pos.y / self.screenHeight
        pos.y = 1 - pos.y
        return {pos.x, pos.y}

    elseif attrName == 'size' then
        -- 转换成归一化坐标系
        local size = self.node:getContentSize()
        -- 有些版本的engine对于某类特殊节点会没有这个值，所以要判断
        if size ~= nil then
            size.width = size.width / self.screenWidth
            size.height = size.height / self.screenHeight
            return {size.width, size.height}
        end

    elseif attrName == 'scale' then
        return {self.node:getScaleX(), self.node:getScaleY()}

    elseif attrName == 'anchorPoint' then
        local anchor = self.node:getAnchorPoint()
        anchor.y = 1 - anchor.y
        return {anchor.x, anchor.y}

    elseif attrName == 'zOrders' then
        local zOrders = {
            global = self.node:getGlobalZOrder(),
            ['local'] = self.node:getLocalZOrder(),
        }
        return zOrders

    elseif attrName == 'touchable' then
        if self.node.isTouchEnabled then
            return self.node:isTouchEnabled()
        end
        return nil

    elseif attrName == 'tag' then
        return self.node:getTag()

    elseif attrName == 'enabled' then
        if self.node.isEnabled then
            return self.node:isEnabled()
        end
        return nil

    elseif attrName == 'desc' then
        return self.node:getDescription()

    elseif attrName == 'rotation' then
        local rotationX, rotationY
        if self.node.getRotationSkewX ~= nil and self.node.getRotationSkewY ~= nil then
            rotationX, rotationY = self.node:getRotationSkewX(), self.node:getRotationSkewY()
        end
        return rotationX or rotationY

    elseif attrName == 'rotation3D' then
        local rotationX, rotationY
        if self.node.getRotationSkewX ~= nil and self.node.getRotationSkewY ~= nil then
            rotationX, rotationY = self.node:getRotationSkewX(), self.node:getRotationSkewY()
        end
        if rotationX == rotationY and self.node.getRotation3D then
            return self.node:getRotation3D()
        end
        return nil

    elseif attrName == 'skew' then
        if self.node.getSkewX and self.node.getSkewY then
            return {self.node:getSkewX(), self.node:getSkewY()}
        end
        return nil

    end

    return AbstractNode.getAttr(self, attrName)
end

function Node:setAttr(attrName, val)
    if attrName == 'text' then
        if self.node.setString then
            self.node:setString(val)
            return true
        elseif self.node.setText then
            self.node:setText(val)
            return true
        end
    end
    return AbstractNode.setAttr(self, attrName, val)
end


return Node