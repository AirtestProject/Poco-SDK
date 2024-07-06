interface IComparator {
    compare(cond: any, node: any): boolean
}

class EqualizationComparator implements IComparator {
    compare(cond: any, node: any): boolean {
        return cond === node
    }
}

class RegexpComparator implements IComparator {
    compare(origin: any, pattern: any): boolean {
        if (!origin || !pattern) {
            return false
        }
        return origin.toString().match(pattern) !== null
    }
}

export default class DefaultMatcher {
    comparators: { [key: string]: IComparator } = {
        "attr=": new EqualizationComparator(),
        "attr.*=": new RegexpComparator(),
    }
    match(cond: any, node: any): boolean {
        var op = cond[0]
        var args = cond[1]

        // 条件匹配
        if (op === "and") {
            for (var i in args) {
                var arg = args[i]
                if (!this.match(arg, node)) {
                    return false
                }
            }
            return true
        }

        if (op === "or") {
            for (var i in args) {
                var arg = args[i]
                if (this.match(arg, node)) {
                    return true
                }
            }
            return false
        }

        // 属性匹配
        var comparator: IComparator = this.comparators[op]
        if (comparator) {
            var attribute = args[0]
            var value = args[1]
            var targetValue = node.getAttr(attribute)
            return comparator.compare(targetValue, value)
        }

        return false
    }
}
