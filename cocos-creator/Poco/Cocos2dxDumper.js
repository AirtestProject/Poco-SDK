

var AbstractDumper = require('./sdk/AbstractDumper')
var Node = require('./Cocos2dxNode')


var Dumper = function () {
    AbstractDumper.call(this)
}
Dumper.prototype = Object.create(AbstractDumper.prototype)

Dumper.prototype.getRoot = function () {
    var winSize = cc.director.getWinSize()
    var scene = null
    if (cc.director.getScene) {
        scene = cc.director.getScene()
    } else {
        scene = cc.director.getRunningScene()
    }
    return new Node(scene, winSize.width, winSize.height)
}

try {
    module.exports = Dumper;
} catch (e) {
    if (window.module && window.module.exports) {
        window.module.exports = Dumper;
    }
}
