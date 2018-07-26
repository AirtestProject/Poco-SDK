var IScreen = function () {
}

IScreen.prototype.getPortSize = function () {
    // return [width, height] in pixels of float type
}

IScreen.prototype.getScreen = function (width) {
    // return promisable
}
        
try {
    module.exports = IScreen;
} catch (e) {
    if (window.module && window.module.exports) {
        window.module.exports = IScreen;
    }
}