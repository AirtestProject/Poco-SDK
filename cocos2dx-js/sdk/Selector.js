var DefaultMatcher = hunter.require('support.poco.sdk.DefaultMatcher')

var ISelector = function () {
}

ISelector.prototype.select = function (cond, multiple) {
    // :rettype: list of support.poco.sdk.AbstractNode
}


var Selector = function (dumper, matcher) {
    this.dumper = dumper
    this.matcher = matcher || new DefaultMatcher()
}

Selector.prototype.getRoot = function () {
    return this.dumper.getRoot()
}

Selector.prototype.select = function (cond, multiple) {
    return this.selectImpl(cond, multiple, this.getRoot(), 9999, true, true)
}

Selector.prototype.selectImpl = function (cond, multiple, root, maxDepth, onlyVisibleNode, includeRoot) {
    // 凡是visible为false后者parentVisible为false的都不选
    var result = []
    if (!root) {
        return result
    }

    var op = cond[0]
    var args = cond[1]
    
    if (op === '>' || op === '/') {
        var parents = [root]
        for (var index in args) {
            index = parseInt(index)
            var arg = args[index]
            var midResult = []
            for (var j in parents) {
                var parent = parents[j]
                var _maxDepth = maxDepth;
                if (op === '/' && index !== 0) {
                    _maxDepth = 1
                }
                midResult = midResult.concat(this.selectImpl(arg, true, parent, _maxDepth, onlyVisibleNode, false))
            }
            parents = midResult
        }
        result = parents
    }
    else if (op === '-') {
        var query1 = args[0]
        var query2 = args[1]
        var result1 = this.selectImpl(query1, multiple, root, maxDepth, onlyVisibleNode, includeRoot)
        for (var index in result1) {
            var n = result1[index]
            result.concat(this.selectImpl(query2, multiple, n.getParent(), 1, onlyVisibleNode, includeRoot))
        }
    }
    else if (op === 'index') {
        var cond = args[0]
        var i = args[1]
        result = [this.selectImpl(cond, multiple, root, maxDepth, onlyVisibleNode, includeRoot)[i]]
    }
    else {
        this._selectTraverse(cond, root, result, multiple, maxDepth, onlyVisibleNode, includeRoot)
    }
    return result
}

Selector.prototype._selectTraverse = function (cond, node, outResult, multiple, maxDepth, onlyVisibleNode, includeRoot) {
    // 剪掉不可见节点branch
    if (onlyVisibleNode && !node.getAttr('visible')) {
        return false
    }

    if (this.matcher.match(cond, node)) {
        // 父子/祖先后代节点选择时，默认是不包含父节点/祖先节点的
        // 在下面的children循环中则需要包含，因为每个child在_selectTraverse中就当做是root
        if (includeRoot) {
            outResult.push(node)
            if (!multiple) {
                return true
            }
        }
    }

    // 最大搜索深度耗尽并不表示遍历结束，其余child节点仍需遍历
    if (maxDepth === 0) {
        return false
    }
    maxDepth -= 1

    var children = node.getChildren()
    for (var i in node.getChildren()) {
        var child = children[i]
        var finished = this._selectTraverse(cond, child, outResult, multiple, maxDepth, onlyVisibleNode, true)
        if (finished) {
            return true
        }
    }

    return false
}

module.exports = Selector;