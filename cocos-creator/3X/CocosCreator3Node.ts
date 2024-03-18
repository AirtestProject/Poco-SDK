import { Node, UITransformComponent } from "cc"
import AbstractNode from "./sdk/AbstractNode"

export default class CCCNode implements AbstractNode {
  node: Node
  static screenWidth: number = 0
  static screenHeight: number = 0
  constructor(node: Node) {
    this.node = node
  }

  getParent() {
    let parent = this.node.parent
    if (!parent) {
      return null
    }
    return new CCCNode(parent)
  }

  getChildren() {
    let children: CCCNode[] = []
    let nodeChildren = this.node.children
    for (const i of nodeChildren) {
      children.push(new CCCNode(i))
    }
    return children
  }

  getAttr(attrName: string) {
    if (attrName === "visible") {
      return this.node.activeInHierarchy
    } else if (attrName === "name") {
      return this.node.name || "<no-name>"
    } else if (attrName === "text") {
      for (const component of this.node.components) {
        if ("string" in component) {
          return (component as any).string
        }
      }
      return ""
    } else if (attrName === "type") {
      const componentSize = this.node.components.length
      let ntype: string = ""
      //一般第一个是UI组件，pass
      switch (componentSize) {
        case 0:
          return ""
        case 1:
          ntype = this.node.components[0].name
          break
        default:
          ntype = this.node.components[1].name
          break
      }
      return ntype.replace(/\w+\./, "")
    } else if (attrName === "pos") {
      // 转换成归一化坐标系，原点左上角
      let pos = this.node.worldPosition
      return [pos.x / CCCNode.screenWidth, 1 - pos.y / CCCNode.screenHeight]
    } else if (attrName === "size") {
      // 转换成归一化坐标系
      let size = this.node.getComponent(UITransformComponent)?.contentSize
      if (!size) return [0, 0]
      return [
        size.width / CCCNode.screenWidth,
        size.height / CCCNode.screenHeight,
      ]
    } else if (attrName === "scale") {
      return [this.node.scale.x, this.node.scale.y]
    } else if (attrName === "anchorPoint") {
      let ui = this.node.getComponent(UITransformComponent)
      if (!ui) return [0, 0]
      return [ui.anchorX, ui.anchorY]
    } else if (attrName == "touchable") {
      return true
    } else if (attrName === "tag") {
      return ""
    } else if (attrName === "enabled") {
      return true
    } else if (attrName === "rotation") {
      return this.node.rotation
    }

    return undefined
  }

  getAvailableAttributeNames() {
    return [
      "name",
      "type",
      "visible",
      "pos",
      "size",
      "scale",
      "anchorPoint",
      "text",
      "enabled",
      "rotation",
    ]
  }

  setAttr() {}

  enumerateAttrs() {
    var ret: any = {}
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
}
