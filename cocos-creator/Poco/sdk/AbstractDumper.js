var IDumper = function () {

}

IDumper.prototype.getRoot = function () {
    // :rettype: support.poco.sdk.AbstractNode
}

IDumper.prototype.dumpHierarchy = function () {
    // :rettype: dict or NoneType
}

var AbstractDumper = function () {
    IDumper.call(this)
}
AbstractDumper.prototype = Object.create(IDumper.prototype)

AbstractDumper.prototype.dumpHierarchy = function () {
    return this.dumpHierarchyImpl(this.getRoot())
}

AbstractDumper.prototype.dumpHierarchyImpl = function (node, onlyVisibleNode) {
    if (!node) {
        return null
    }
    if (onlyVisibleNode === undefined) {
        onlyVisibleNode = true
    }

    var payload = node.enumerateAttrs()
    var result = {}
    var children = []
    var nodeChildren = node.getChildren()
    for (var i in nodeChildren) {
        var child = nodeChildren[i]
        if (!onlyVisibleNode || (payload['visible'] || child.getAttr('visible'))) {
            children.push(this.dumpHierarchyImpl(child, onlyVisibleNode))
        }
    }
    if (children.length > 0) {
        result['children'] = children
    }

    result['name'] = payload['name'] || node.getAttr('name')
    result['payload'] = payload

    return result
}

try {
    module.exports = AbstractDumper;
} catch (e) {
    if (window.module && window.module.exports) {
        window.module.exports = AbstractDumper;
    }
}