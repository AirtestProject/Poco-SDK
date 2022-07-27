import AbstractDumper from "./sdk/AbstractDumper"
import CCNode from "./CocosCreator3Node"
import { director, Node, UITransform, View } from "cc"

export default class Dumper extends AbstractDumper {
    getRoot() {
        const scene = director
            .getScene()! as unknown as Node
        CCNode.screenHeight = screen.height
        CCNode.screenWidth = screen.width
        return new CCNode(scene)
    }
}
