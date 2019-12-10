var Attributor = function () {

}

Attributor.prototype.getAttr = function (node, attrName) {
    var node_ = node
    if (!node.__isPocoNodeWrapper__) {
        node_ = node[0]
    }
    return node_.getAttr(attrName)
}
    
Attributor.prototype.setAttr = function (node, attrName, attrVal) {
    var node_ = node
    if (!node.__isPocoNodeWrapper__) {
        node_ = node[0]
    }
    node_.setAttr(attrName, attrVal)
}

try {
    module.exports = Attributor;
} catch (e) {
    if (window.module && window.module.exports) {
        window.module.exports = Attributor;
    }
}