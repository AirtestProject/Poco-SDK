// var cc = cc

var AbstractNode = hunter.require('support.poco.sdk.AbstractNode')

var Node = function (node, screenWidth, screenHeight) {
    AbstractNode.call(this)
    this.node = node
    this.screenWidth = screenWidth
    this.screenHeight = screenHeight
}
Node.prototype = Object.create(AbstractNode.prototype)

Node.prototype.getParent = function () {
    var parent = this.node.getParent()
    if (!parent) {
        return null
    }
    return new Node(parent, this.screenWidth, this.screenHeight)
}

Node.prototype.getChildren = function () {
    var children = null
    var nodeChildren = this.node.getChildren()
    if (nodeChildren) {
        children = []
        for (var i in nodeChildren) {
            var child = nodeChildren[i]
            children.push(new Node(child, this.screenWidth, this.screenHeight))
        }
    }
    return children
}

Node.prototype.getAttr = function (attrName) {
    if (attrName === 'visible') {
        if (this.node.isVisible) {
            var visible = this.node.isVisible()
            if (!visible) {
                return false
            }

            // if the node is visible, check its parent's visibility
            // 举一个很简单的例子：
            // 有两个layer，一个layer中有个button，这个button点下去后把layer的visible设为false，
            // 这时候这个button的visible仍然是true的，如果这时候判断这个button是否存在或可见，
            // 则需要判断他的所有父节点是否可见了
            var parent = this.node.getParent()
            while (parent) {
                var parentVisible = parent.isVisible()
                if (!parentVisible) {
                    return false
                }
                parent = parent.getParent()
            }
            return true
        } else {
            return this.node._activeInHierarchy
        }
    }
    else if (attrName === 'name') {
        return this.node.getName() || '<no-name>'
    }
    else if (attrName === 'text') {
        for (var i in this.node._components) {
            var c = this.node._components[i]
            if (c.string !== undefined) {
                return c.string
            }
        }
    }
    else if (attrName === 'type') {
        var ntype = ''
        if (this.node._components) {
            for (var i = this.node._components.length - 1; i >= 0; i--) {
                ntype = this.node._components[i].__classname__
                if (ntype.startsWith('cc')) {
                    break
                }
            }
        } else {
            ntype = this.node.__classname__
        }
        return ntype.replace(/\w+\./, '')
    }
    else if (attrName === 'pos') {
        // 转换成归一化坐标系，原点左上角
        var pos = this.node.convertToWorldSpaceAR(new cc.Vec2(0, 0))
        pos.x /= this.screenWidth
        pos.y /= this.screenHeight
        pos.y = 1 - pos.y
        return [pos.x, pos.y]
    }
    else if (attrName === 'size') {
        // 转换成归一化坐标系
        var size = new cc.Size(this.node.getContentSize())
        size.width /= this.screenWidth
        size.height /= this.screenHeight
        return [size.width, size.height]
    }
    else if (attrName === 'scale') {
        return [this.node.getScaleX(), this.node.getScaleY()]
    }
    else if (attrName === 'anchorPoint') {
        var anchor = this.node.getAnchorPoint()
        return [anchor.x, 1 - anchor.y]
    }
    else if (attrName === 'zOrders') {
        return {
            local: this.node.getLocalZOrder(), 
            global: this.node.getGlobalZOrder(),
        }
    }
    else if (attrName == 'touchable') {
        if (this.node.isTouchEnabled) {
            return this.node.isTouchEnabled()
        }
    }
    else if (attrName === 'tag') {
        return this.node.getTag()
    }
    else if (attrName === 'enabled') {
        if (this.node.isEnabled) {
            return this.node.isEnabled()
        }
    }
    else if (attrName === 'rotation') {
        return this.node.getRotation()
    }

    return undefined
}

Node.prototype.setAttr = function (attrName, val) {
    // raise UnableToSetAttributeException(attrName, self.node)
}

Node.prototype.getAvailableAttributeNames = function () {
    // enumerate all available attributes' name of this node
    // :rettype: Iterable<string>

    return AbstractNode.prototype.getAvailableAttributeNames.call(this).concat([
        'text',
        'touchable',
        'enabled',
        'tag',
        'rotation',
    ])
}

module.exports = Node;