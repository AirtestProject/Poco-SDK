
// var AbstractNode = require('./sdk/AbstractNode')
var AbstractNode = window.AbstractNode


var Node = function (node, screenWidth, screenHeight) {
    AbstractNode.call(this)
    this.node = node
    this.screenWidth = screenWidth
    this.screenHeight = screenHeight
}
Node.prototype = Object.create(AbstractNode.prototype)

Node.prototype.getParent = function () {
    var parent = this.node.parent
    if (!parent) {
        return null
    }
    return new Node(parent, this.screenWidth, this.screenHeight)
}

Node.prototype.getChildren = function () {
    var children = null
    var nodeChildren = this.node.$children
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
        if (this.node.$visible) {
            var parent= this.node.parent
            while(parent){
                if(!parent.$visible){
                    return false
                }
                parent = parent.parent
            }
            return true;
        } else {
            return false;
        }
    }
    else if (attrName === 'name') {
        if(this.node.name){
            return this.node.name
        }else{
            return this.node.__class__
        }
    }
    else if (attrName === 'text') {
        return this.node.text
    }
    else if (attrName === 'type') {
        var type = this.node.__class__.toString();

        // type的小图标只会识别'.'分割的最后一段，即 eui.Label <=> Label
        // 前缀可以保留，在脚本里编写时可读性好
        // var temp = type.split('.')
        // if(temp[0] === 'egret' || temp[0]==='eui' || temp[1] === 'gui') {
        //     type = temp[1] === 'gui' ? temp[2] : temp[1]
        // }
        // if(temp[0] === 'egret' && this.node.$children !== undefined) {
        //     type = 'Layer'
        // }
        
        return type
    }
    else if (attrName === 'pos') {
        // 转换成归一化坐标系，
        // eui.* 类型的UI，以中心点为锚点，其余类型取默认pos点
        var x = 0
        var y = 0

        var point = this.node.localToGlobal();
        if (this.node.__class__.toString().startsWith("eui")) {
            var w = this.node.width
            var h = this.node.height
            x = (point.x + w / 2) / this.screenWidth 
            y = (point.y + h / 2) / this.screenHeight
        } else {
            x = point.x / this.screenWidth 
            y = point.y / this.screenHeight
        }

        return [x, y]
    }
    else if (attrName === 'size') {
        // 转换成归一化坐标系
        var width = 0;
        var height = 0;

        width = this.node.width
        height = this.node.height
        width /= this.screenWidth
        height /= this.screenHeight
        // console.log([width, height],"    ",this.node.__class__,this.node.width,this.screenWidth)
        return [width, height]
    }
    else if (attrName === 'scale') {
        return [this.node.$scaleX, this.node.$scaleY]
    }
    else if (attrName === 'anchorPoint') {
        // var anchor = cgetter(this.node, 'anchorPoint')
        // return [anchor.x, 1 - anchor.y]
        if (this.node.__class__.toString().startsWith("eui")) {
            return [0.5, 0.5]
        }
        return [this.node.$anchorOffsetX, this.node.$anchorOffsetY]
    }
    else if (attrName === 'zOrders') {
        var local = 0;
        var global = 0;
        var parent = this.node.parent;
        var child = this.node
        if (this.node.parent) {
            local = this.node.parent.getChildIndex(this.node)
        }
        while (parent) {
            global = global + parent.getChildIndex(child)
            child = parent
            parent = parent.parent
        }
        return {
            local: local,
            global: global,
        }
    }
    else if (attrName == 'enabled') {
        return this.node.enabled
    }
    else if (attrName === 'touchable') {
        return this.node.$touchEnabled
        
    }
    else if (attrName === 'rotation') {
        return this.node.$rotation;
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
        'enabled',
        'touchable',
        'rotation',
        'text',
    ])
}


try {
    module.exports = Node;
} catch (e) {
    if (window.module && window.module.exports) {
        window.module.exports = Node;
    }
}
