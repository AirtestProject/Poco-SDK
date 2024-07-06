import AbstractDumper from "./sdk/AbstractDumper"
import CCNode from "./CocosCreator3Node"
import { director, Node, View } from "cc"

export default class Dumper extends AbstractDumper {
  getRoot() {
    const scene = director.getScene()! as unknown as Node
    const size = View.instance.getVisibleSize()
    CCNode.screenHeight = size.height
    CCNode.screenWidth = size.width
    return new CCNode(scene)
  }
}
