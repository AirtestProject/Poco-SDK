
// var AbstractDumper = require('./sdk/AbstractDumper')

var AbstractDumper = window.AbstractDumper
var Node = window.Node

var Dumper = function (root) {
    AbstractDumper.call(this)
    this.root = root;
}

Dumper.prototype = Object.create(AbstractDumper.prototype)

Dumper.prototype.getRoot = function () { 
    var x=document.getElementsByClassName("egret-player")
    var canvas=x[0].childNodes[1]
    // console.log(x[0],canvas)
    console.log(x[0].clientWidth, x[0].clientHeight,canvas.clientWidth,canvas.clientHeight,this.root.width,this.root.height)
    return new Node(this.root, x[0].clientWidth, x[0].clientHeight,canvas.clientWidth,canvas.clientHeight,this.root.width,this.root.height)
}

try {
    module.exports = Dumper;
} catch (e) {
    if (window.module && window.module.exports) {
        window.module.exports = Dumper;
    }
}
