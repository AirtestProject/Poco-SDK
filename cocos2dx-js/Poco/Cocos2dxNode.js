// var cc = cc

// var AbstractNode = require('./sdk/AbstractNode')
var AbstractNode = window.AbstractNode

var Vec2 = cc.Vec2 || cc.math.Vec2 || cc.math.Vec3

var cgetter = function(node, property) {
    var getterFunc = 'get' + property[0].toUpperCase() + property.slice(1)
    if (node[getterFunc]) {
        return node[getterFunc].call(node)
    } else if (node[property]) {
        return node[property]
    } else {
        return node['_' + property]  // 尝试访问私有属性
    }
}

var csetter = function(node, property, val) {
    var setterFunc = 'set' + property[0].toUpperCase() + property.slice(1)
    if (node[setterFunc]) {
        node[setterFunc].call(node, val)
    } else {
        node[property] = val
    }
}

var Node = function (node, screenWidth, screenHeight) {
    AbstractNode.call(this)
    this.node = node
    this.screenWidth = screenWidth
    this.screenHeight = screenHeight
}
Node.prototype = Object.create(AbstractNode.prototype)

Node.prototype.getParent = function () {
    var parent = cgetter(this.node, 'parent')
    if (!parent) {
        return null
    }
    return new Node(parent, this.screenWidth, this.screenHeight)
}

Node.prototype.getChildren = function () {
    var children = null
    var nodeChildren = cgetter(this.node, 'children')
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
        if (this.node.isVisible || this.node.visible) {
            var visible = cgetter(this.node, 'visible')
            if (!visible) {
                return false
            }

            // if the node is visible, check its parent's visibility
            // 举一个很简单的例子：
            // 有两个layer，一个layer中有个button，这个button点下去后把layer的visible设为false，
            // 这时候这个button的visible仍然是true的，如果这时候判断这个button是否存在或可见，
            // 则需要判断他的所有父节点是否可见了
            var parent = cgetter(this.node, 'parent')
            while (parent) {
                var parentVisible = parent.isVisible()
                if (!parentVisible) {
                    return false
                }
                parent = cgetter(parent, 'parent')
            }
            return true
        } else {
            return this.node._activeInHierarchy
        }
    }
    else if (attrName === 'name') {
        return cgetter(this.node, 'name') || '<no-name>'
    }
    else if (attrName === 'text') {
        for (var i in this.node._components) {
            var c = this.node._components[i]
            if (c.string !== undefined) {
                return c.string
            }
        }
        
        return cgetter(this.node, 'string')
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
        }
        if (!ntype) {
            ntype = this.node.__classname__ || this.node._className
        }
        if (!ntype) {
            if (this.node.constructor) {
                ntype = this.node.constructor.name
            }
        }
        if (!ntype) {
            ntype = 'Object'
        }
        return ntype.replace(/\w+\./, '')
    }
    else if (attrName === 'pos') {
        // 转换成归一化坐标系，原点左上角
        var pos = this.node.convertToWorldSpaceAR(new Vec2(0, 0))
        pos.x /= this.screenWidth
        pos.y /= this.screenHeight
        pos.y = 1 - pos.y
        return [pos.x, pos.y]
    }
    else if (attrName === 'size') {
        // 转换成归一化坐标系
        var size = null
        if (this.node.getContentSize || this.node.contentSize) {
            size = cgetter(this.node, 'contentSize')
        } else {
            size = new cc.Size(this.node.width, this.node.height)
        }
        size.width /= this.screenWidth
        size.height /= this.screenHeight
        return [size.width, size.height]
    }
    else if (attrName === 'scale') {
        return [cgetter(this.node, 'scaleX'), cgetter(this.node, 'scaleY')]
    }
    else if (attrName === 'anchorPoint') {
        var anchor = cgetter(this.node, 'anchorPoint')
        return [anchor.x, 1 - anchor.y]
    }
    else if (attrName === 'zOrders') {
        return {
            local: cgetter(this.node, 'localZOrder'), 
            global: cgetter(this.node, 'globalZOrder'),
        }
    }
    else if (attrName == 'touchable') {
        if (this.node.isTouchEnabled) {
            return this.node.isTouchEnabled()
        }
    }
    else if (attrName === 'tag') {
        return cgetter(this.node, 'tag')
    }
    else if (attrName === 'enabled') {
        if (this.node.isEnabled) {
            return this.node.isEnabled()
        }
    }
    else if (attrName === 'rotation') {
        return cgetter(this.node, 'rotation')
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


try {
    module.exports = Node;
} catch (e) {
    if (window.module && window.module.exports) {
        window.module.exports = Node;
    }
}
