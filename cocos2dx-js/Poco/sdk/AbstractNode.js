var AbstractNode = function () {
}

AbstractNode.prototype.getParent = function () {
    return null
}
    
AbstractNode.prototype.getChildren = function () {
    // :rettype: Iterable<AbstractNode>
}
    
AbstractNode.prototype.getAttr = function (attrName) {
    var attrs = {
        name: '<Root>',
        type: 'Root',
        visible: true,
        pos: [0.0, 0.0],
        size: [0.0, 0.0],
        scale: [1.0, 1.0],
        anchorPoint: [0.5, 0.5],
        zOrders: {'local': 0, 'global': 0},
    }

    return attrs[attrName]
}
    
AbstractNode.prototype.setAttr = function (attrName, val) {
    throw new Error('Unable to set attributes ' + attrName + ' on this node. (NotImplemented) ')
}

AbstractNode.prototype.getAvailableAttributeNames = function () {
    // enumerate all available attributes' name of this node
    // :rettype: Iterable<string>

    return [
        "name",
        "type",
        "visible",
        "pos",
        "size",
        "scale",
        "anchorPoint",
        "zOrders",
    ]
}

AbstractNode.prototype.enumerateAttrs = function () {
    // :rettype: Iterable<string, ValueType>
    var ret = {}
    var allAttrNames = this.getAvailableAttributeNames()
    for (var i in allAttrNames) {
        var attrName = allAttrNames[i]
        var attrVal = this.getAttr(attrName)
        if (attrVal !== undefined) {
            ret[attrName] = attrVal
        }
    }
    return ret
}

try {
    module.exports = AbstractNode;
} catch (e) {
    if (window.module && window.module.exports) {
        window.module.exports = AbstractNode;
    }
}