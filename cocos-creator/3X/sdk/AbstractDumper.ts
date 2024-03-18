import type AbstractNode from "./AbstractNode"
type dumpInfo = {
    name: string
    payload: any
    children: dumpInfo[]
}
export default class AbstractDumper {
    getRoot(): AbstractNode {
        throw new Error("not impl")
    }

    dumpHierarchy(
        node: AbstractNode|boolean,
        onlyVisibleNode: boolean=true
    ): dumpInfo | null {
        if (!node) {
            return null
        }
        if (node===true){
            node=this.getRoot()
        }

        var payload = node.enumerateAttrs()
        payload['zOrders']={'local':0,'global':0}
        var result: dumpInfo = {
            name: payload["name"] || node.getAttr("name"),
            payload,
            children: [],
        }
        var nodeChildren = node.getChildren()
        for (var i in nodeChildren) {
            var child = nodeChildren[i]
            if (
                !onlyVisibleNode ||
                payload["visible"] ||
                child.getAttr("visible")
            ) {
                result.children.push(
                    this.dumpHierarchy(child, onlyVisibleNode)!
                )
            }
        }

        return result
    }
}
