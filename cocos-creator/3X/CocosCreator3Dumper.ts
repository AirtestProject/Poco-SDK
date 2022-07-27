import AbstractDumper from "./sdk/AbstractDumper"
import CCNode from "./CocosCreator3Node"
import { director, Node, UITransform } from "cc"

export default class Dumper extends AbstractDumper {
    getRoot() {
        const scene = director
            .getScene()!
            .getChildByName("Canvas")! as unknown as Node
        const ui = scene.getComponent(UITransform)!
        CCNode.screenHeight = ui.height
        CCNode.screenWidth = ui.width
        return new CCNode(scene)
    }
}
