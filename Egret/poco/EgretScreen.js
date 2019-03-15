
// var IScreen = require('./sdk/IScreen')
var IScreen = window.IScreen

var Screen = function (root) {
    IScreen.call(this)
    this.root = root
}

Screen.prototype = Object.create(IScreen.prototype)

Screen.prototype.getPortSize = function () { 
    return [this.root.width, this.root.height]
}

Screen.prototype.getScreen = function (width) { 
    var prefix = 'data:image/jpeg;base64,'
    var rt = new egret.RenderTexture()
    var size = this.getPortSize()
    rt.drawToTexture(this.root)
    var screenData = rt.toDataURL("image/jpeg", new egret.Rectangle(0, 0, size[0], size[1]))
    screenData = screenData.slice(prefix.length)
    return [screenData, 'jpeg']
}

try {
    module.exports = Screen;
} catch (e) {
    if (window.module && window.module.exports) {
        window.module.exports = Screen;
    }
}
