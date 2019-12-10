var IMatcher = function () {
}

IMatcher.prototype.match = function (cond, node) {
    // :rettype: bool
}


var EqualizationComparator = function () {}
EqualizationComparator.prototype.compare = function (l, r) {
    // 现在只判断字符串、数字、bool值的这种简单属性
    // 如果要判断数组或者dict等复杂结构再加
    return l === r
}


var RegexpComparator = function () {}
RegexpComparator.prototype.compare = function (origin, pattern) {
    if (!origin || !pattern) {
        return false
    }
    return origin.toString().match(pattern) !== null
}


var DefaultMatcher = function () {
    IMatcher.call(this)
    this.comparators = {
        'attr=': new EqualizationComparator(),
        'attr.*=': new RegexpComparator(),
    }
}
DefaultMatcher.prototype = Object.create(IMatcher.prototype)


DefaultMatcher.prototype.match = function (cond, node) {
    var op = cond[0]
    var args = cond[1]

    // 条件匹配
    if (op === 'and') {
        for (var i in args) {
            var arg = args[i]
            if (!this.match(arg, node)) {
                return false
            }
        }
        return true
    }

    if (op === 'or') {
        for (var i in args) {
            var arg = args[i]
            if (this.match(arg, node)) {
                return true
            }
        }
        return false
    }

    // 属性匹配
    var comparator = this.comparators[op]
    if (comparator) {
        var attribute = args[0]
        var value = args[1]
        var targetValue = node.getAttr(attribute)
        return comparator.compare(targetValue, value) 
    }

    return false
}

try {
    module.exports = DefaultMatcher;
} catch (e) {
    if (window.module && window.module.exports) {
        window.module.exports = DefaultMatcher;
    }
}
