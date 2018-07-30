
// var AbstractDumper = require('./sdk/AbstractDumper')

var AbstractDumper = window.AbstractDumper
var Node = window.Node

var Dumper = function (root) {
    AbstractDumper.call(this)
    this.root = root;
}

Dumper.prototype = Object.create(AbstractDumper.prototype)

Dumper.prototype.getRoot = function () {
    return new Node(this.root, this.root.width, this.root.height)
}

try {
    module.exports = Dumper;
} catch (e) {
    if (window.module && window.module.exports) {
        window.module.exports = Dumper;
    }
}
