var Attributor = function () {

}

Attributor.prototype.getAttr = function (node, attrName) {
    if (!node.__isPocoNodeWrapper__) {
        node = node[0]
    }
    return node.getAttr(attrName)
}
    
Attributor.prototype.setAttr = function (node, attrName, attrVal) {
    if (!node.__isPocoNodeWrapper__) {
        node = node[0]
    }
    node.setAttr(attrName, attrVal)
}

try {
    module.exports = Attributor;
} catch (e) {
    if (window.module && window.module.exports) {
        window.module.exports = Attributor;
    }
}