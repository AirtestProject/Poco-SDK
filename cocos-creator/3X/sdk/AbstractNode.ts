export default interface AbstractNode {
    getParent(): AbstractNode | null
    getChildren(): AbstractNode[]
    getAttr(attrName: string): any
    setAttr(): void
    getAvailableAttributeNames(): string[]
    enumerateAttrs(): { [key: string]: any }
}
