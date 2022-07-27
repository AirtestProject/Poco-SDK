export namespace Attributor{
    function getAttr(node, attrName) {
        let node_ = node
        if (!node.__isPocoNodeWrapper__) {
            node_ = node[0]
        }
        return node_.getAttr(attrName)
    }
    function setAttr (node, attrName, attrVal) {
        let node_ = node
        if (!node.__isPocoNodeWrapper__) {
            node_ = node[0]
        }
        node_.setAttr(attrName, attrVal)
    }
}