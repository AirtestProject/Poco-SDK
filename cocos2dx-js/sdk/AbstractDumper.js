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

AbstractDumper.prototype.dumpHierarchyImpl = function (node) {
    if (!node) {
        return null
    }

    var payload = node.enumerateAttrs()
    var result = {}
    var children = []
    var nodeChildren = node.getChildren()
    for (var i in nodeChildren) {
        var child = nodeChildren[i]
        if (payload['visible'] || child.getAttr('visible')) {
            children.push(this.dumpHierarchyImpl(child))
        }
    }
    if (children.length > 0) {
        result['children'] = children
    }

    result['name'] = payload['name'] || node.getAttr('name')
    result['payload'] = payload

    return result
}

module.exports = AbstractDumper;