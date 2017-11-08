// var cc = cc

var AbstractDumper = hunter.require('support.poco.sdk.AbstractDumper')
var Node = hunter.require('support.poco.cocos2dx.Node')

var Dumper = function () {
    AbstractDumper.call(this)
}
Dumper.prototype = Object.create(AbstractDumper.prototype)

Dumper.prototype.getRoot = function () {
    var winSize = cc.director.getWinSize()
    return new Node(cc.director.getScene(), winSize.width, winSize.height)
}

module.exports = Dumper;