/*
* @Author: gzliuxin
* @Date:   2017-08-04 10:06:01
* @Last Modified by:   gzliuxin
* @Last Modified time: 2018-04-03 13:48:55
*/
'use strict';
// var DefaultMatcher = require('./DefaultMatcher')
var DefaultMatcher = window.DefaultMatcher;
var ISelector = function () {
};
ISelector.prototype.select = function (cond, multiple) {
    // :rettype: list of support.poco.sdk.AbstractNode
};
var Selector = function (dumper, matcher) {
    this.dumper = dumper;
    this.matcher = matcher || new DefaultMatcher();
};
Selector.prototype.getRoot = function () {
    return this.dumper.getRoot();
};
Selector.prototype.select = function (cond, multiple) {
    return this.selectImpl(cond, multiple, this.getRoot(), 9999, true, true);
};
Selector.prototype.selectImpl = function (cond, multiple, root, maxDepth, onlyVisibleNode, includeRoot) {
    // 凡是visible为false后者parentVisible为false的都不选
    var result = [];
    if (!root) {
        return result;
    }
    var op = cond[0];
    var args = cond[1];
    if (op === '>' || op === '/') {
        var parents = [root];
        for (var index in args) {
            index = parseInt(index);
            var arg = args[index];
            var midResult = [];
            for (var j in parents) {
                var parent = parents[j];
                var _maxDepth = maxDepth;
                if (op === '/' && index !== 0) {
                    _maxDepth = 1;
                }
                var _res = this.selectImpl(arg, true, parent, _maxDepth, onlyVisibleNode, false);
                for (var k in _res) {
                    if (midResult.indexOf(_res[k]) < 0) {
                        midResult.push(_res[k]);
                    }
                }
            }
            parents = midResult;
        }
        result = parents;
    }
    else if (op === '-') {
        var query1 = args[0];
        var query2 = args[1];
        var result1 = this.selectImpl(query1, multiple, root, maxDepth, onlyVisibleNode, includeRoot);
        for (var index in result1) {
            var n = result1[index];
            var sibling_result = this.selectImpl(query2, multiple, n.getParent(), 1, onlyVisibleNode, includeRoot);
            for (var k in sibling_result) {
                if (result.indexOf(sibling_result[k]) < 0) {
                    result.push(sibling_result[k]);
                }
            }
        }
    }
    else if (op === 'index') {
        var cond = args[0];
        var i = args[1];
        result = [this.selectImpl(cond, multiple, root, maxDepth, onlyVisibleNode, includeRoot)[i]];
    }
    else {
        this._selectTraverse(cond, root, result, multiple, maxDepth, onlyVisibleNode, includeRoot);
    }
    return result;
};
Selector.prototype._selectTraverse = function (cond, node, outResult, multiple, maxDepth, onlyVisibleNode, includeRoot) {
    // 剪掉不可见节点branch
    if (onlyVisibleNode && !node.getAttr('visible')) {
        return false;
    }
    if (this.matcher.match(cond, node)) {
        // 父子/祖先后代节点选择时，默认是不包含父节点/祖先节点的
        // 在下面的children循环中则需要包含，因为每个child在_selectTraverse中就当做是root
        if (includeRoot) {
            if (outResult.indexOf(node) < 0) {
                outResult.push(node);
            }
            if (!multiple) {
                return true;
            }
        }
    }
    // 最大搜索深度耗尽并不表示遍历结束，其余child节点仍需遍历
    if (maxDepth === 0) {
        return false;
    }
    maxDepth -= 1;
    var children = node.getChildren();
    for (var i in node.getChildren()) {
        var child = children[i];
        var finished = this._selectTraverse(cond, child, outResult, multiple, maxDepth, onlyVisibleNode, true);
        if (finished) {
            return true;
        }
    }
    return false;
};
try {
    module.exports = Selector;
}
catch (e) {
    if (window.module && window.module.exports) {
        window.module.exports = Selector;
    }
}
var IDumper = function () {
};
IDumper.prototype.getRoot = function () {
    // :rettype: support.poco.sdk.AbstractNode
};
IDumper.prototype.dumpHierarchy = function () {
    // :rettype: dict or NoneType
};
var AbstractDumper = function () {
    IDumper.call(this);
};
AbstractDumper.prototype = Object.create(IDumper.prototype);
AbstractDumper.prototype.dumpHierarchy = function () {
    return this.dumpHierarchyImpl(this.getRoot());
};
AbstractDumper.prototype.dumpHierarchyImpl = function (node, onlyVisibleNode) {
    if (!node) {
        return null;
    }
    if (onlyVisibleNode === undefined) {
        onlyVisibleNode = true;
    }
    var payload = node.enumerateAttrs();
    var result = {};
    var children = [];
    var nodeChildren = node.getChildren();
    for (var i in nodeChildren) {
        var child = nodeChildren[i];
        if (!onlyVisibleNode || (payload['visible'] || child.getAttr('visible'))) {
            children.push(this.dumpHierarchyImpl(child, onlyVisibleNode));
        }
    }
    if (children.length > 0) {
        result['children'] = children;
    }
    result['name'] = payload['name'] || node.getAttr('name');
    result['payload'] = payload;
    return result;
};
try {
    module.exports = AbstractDumper;
}
catch (e) {
    if (window.module && window.module.exports) {
        window.module.exports = AbstractDumper;
    }
}
var Attributor = function () {
};
Attributor.prototype.getAttr = function (node, attrName) {
    var node_ = node;
    if (!node.__isPocoNodeWrapper__) {
        node_ = node[0];
    }
    return node_.getAttr(attrName);
};
Attributor.prototype.setAttr = function (node, attrName, attrVal) {
    var node_ = node;
    if (!node.__isPocoNodeWrapper__) {
        node_ = node[0];
    }
    node_.setAttr(attrName, attrVal);
};
try {
    module.exports = Attributor;
}
catch (e) {
    if (window.module && window.module.exports) {
        window.module.exports = Attributor;
    }
}
var IMatcher = function () {
};
IMatcher.prototype.match = function (cond, node) {
    // :rettype: bool
};
var EqualizationComparator = function () { };
EqualizationComparator.prototype.compare = function (l, r) {
    // 现在只判断字符串、数字、bool值的这种简单属性
    // 如果要判断数组或者dict等复杂结构再加
    return l === r;
};
var RegexpComparator = function () { };
RegexpComparator.prototype.compare = function (origin, pattern) {
    if (!origin || !pattern) {
        return false;
    }
    return origin.toString().match(pattern) !== null;
};
var DefaultMatcher = function () {
    IMatcher.call(this);
    this.comparators = {
        'attr=': new EqualizationComparator(),
        'attr.*=': new RegexpComparator(),
    };
};
DefaultMatcher.prototype = Object.create(IMatcher.prototype);
DefaultMatcher.prototype.match = function (cond, node) {
    var op = cond[0];
    var args = cond[1];
    // 条件匹配
    if (op === 'and') {
        for (var i in args) {
            var arg = args[i];
            if (!this.match(arg, node)) {
                return false;
            }
        }
        return true;
    }
    if (op === 'or') {
        for (var i in args) {
            var arg = args[i];
            if (this.match(arg, node)) {
                return true;
            }
        }
        return false;
    }
    // 属性匹配
    var comparator = this.comparators[op];
    if (comparator) {
        var attribute = args[0];
        var value = args[1];
        var targetValue = node.getAttr(attribute);
        return comparator.compare(targetValue, value);
    }
    return false;
};
try {
    module.exports = DefaultMatcher;
}
catch (e) {
    if (window.module && window.module.exports) {
        window.module.exports = DefaultMatcher;
    }
}
var IScreen = function () {
};
IScreen.prototype.getPortSize = function () {
    // return [width, height] in pixels of float type
};
IScreen.prototype.getScreen = function (width) {
    // return promisable
};
try {
    module.exports = IScreen;
}
catch (e) {
    if (window.module && window.module.exports) {
        window.module.exports = IScreen;
    }
}
var AbstractNode = function () {
};
AbstractNode.prototype.getParent = function () {
    return null;
};
AbstractNode.prototype.getChildren = function () {
    // :rettype: Iterable<AbstractNode>
};
AbstractNode.prototype.getAttr = function (attrName) {
    var attrs = {
        name: '<Root>',
        type: 'Root',
        visible: true,
        pos: [0.0, 0.0],
        size: [0.0, 0.0],
        scale: [1.0, 1.0],
        anchorPoint: [0.5, 0.5],
        zOrders: { 'local': 0, 'global': 0 },
    };
    return attrs[attrName];
};
AbstractNode.prototype.setAttr = function (attrName, val) {
    throw new Error('Unable to set attributes ' + attrName + ' on this node. (NotImplemented) ');
};
AbstractNode.prototype.getAvailableAttributeNames = function () {
    // enumerate all available attributes' name of this node
    // :rettype: Iterable<string>
    return [
        "name",
        "type",
        "visible",
        "pos",
        "size",
        "scale",
        "anchorPoint",
        "zOrders",
    ];
};
AbstractNode.prototype.enumerateAttrs = function () {
    // :rettype: Iterable<string, ValueType>
    var ret = {};
    var allAttrNames = this.getAvailableAttributeNames();
    for (var i in allAttrNames) {
        var attrName = allAttrNames[i];
        var attrVal = this.getAttr(attrName);
        if (attrVal !== undefined) {
            ret[attrName] = attrVal;
        }
    }
    return ret;
};
try {
    module.exports = AbstractNode;
}
catch (e) {
    if (window.module && window.module.exports) {
        window.module.exports = AbstractNode;
    }
}
// var AbstractNode = require('./sdk/AbstractNode')
var AbstractNode = window.AbstractNode;
var Node = function (node, screenWidth, screenHeight) {
    AbstractNode.call(this);
    this.node = node;
    this.screenWidth = screenWidth;
    this.screenHeight = screenHeight;
};
Node.prototype = Object.create(AbstractNode.prototype);
Node.prototype.getParent = function () {
    var parent = this.node.parent;
    if (!parent) {
        return null;
    }
    return new Node(parent, this.screenWidth, this.screenHeight);
};
Node.prototype.getChildren = function () {
    var children = null;
    var nodeChildren = this.node.$children;
    if (nodeChildren) {
        children = [];
        for (var i in nodeChildren) {
            var child = nodeChildren[i];
            children.push(new Node(child, this.screenWidth, this.screenHeight));
        }
    }
    return children;
};
Node.prototype.getAttr = function (attrName) {
    if (attrName === 'visible') {
        if (this.node.$visible) {
            var parent = this.node.parent;
            while (parent) {
                if (!parent.$visible) {
                    return false;
                }
                parent = parent.parent;
            }
            return true;
        }
        else {
            return false;
        }
    }
    else if (attrName === 'name') {
        if (this.node.name) {
            return this.node.name;
        }
        else {
            return this.node.__class__;
        }
    }
    else if (attrName === 'text') {
        return this.node.text;
    }
    else if (attrName === 'type') {
        var type = this.node.__class__.toString();
        // type的小图标只会识别'.'分割的最后一段，即 eui.Label <=> Label
        // 前缀可以保留，在脚本里编写时可读性好
        // var temp = type.split('.')
        // if(temp[0] === 'egret' || temp[0]==='eui' || temp[1] === 'gui') {
        //     type = temp[1] === 'gui' ? temp[2] : temp[1]
        // }
        // if(temp[0] === 'egret' && this.node.$children !== undefined) {
        //     type = 'Layer'
        // }
        return type;
    }
    else if (attrName === 'pos') {
        // 转换成归一化坐标系，
        // eui.* 类型的UI，以中心点为锚点，其余类型取默认pos点
        var x = 0;
        var y = 0;
        // 兼容node转换为global坐标失败的情况
        var gx = 0;
        var gy = 0;
        var point = this.node.localToGlobal();
        if (point.x === point.x && point.y === point.y) {
            gx = point.x;
            gy = point.y;
        } else {
            gx = this.node.x;
            gy = this.node.y;
        }
        if (this.node.__class__.toString().startsWith("eui")) {
            // 兼容node读不到width、height的情况
            var w = 0;
            var h = 0;
            if (this.node.width === this.node.width && this.node.height === this.node.height) {
                w = this.node.width;
                h = this.node.height;
            } else {
                w = this.node.stage.stageWidth;
                h = this.node.stage.stageHeight;
            }

            x = (gx + w / 2) / this.screenWidth;
            y = (gy + h / 2) / this.screenHeight;
        }
        else {
            x = gx / this.screenWidth;
            y = gy / this.screenHeight;
        }
        return [x, y];
    }
    else if (attrName === 'size') {
        // 转换成归一化坐标系
        var width = 0;
        var height = 0;

        // 兼容node读不到width、height的情况
        if (this.node.width === this.node.width && this.node.height === this.node.height) {
            width = this.node.width;
            height = this.node.height;
        } else {
            width = this.node.stage.stageWidth;
            height = this.node.stage.stageHeight;
        }

        width /= this.screenWidth;
        height /= this.screenHeight;
        // console.log([width, height],"    ",this.node.__class__,this.node.width,this.screenWidth)
        return [width, height];
    }
    else if (attrName === 'scale') {
        return [this.node.$scaleX, this.node.$scaleY];
    }
    else if (attrName === 'anchorPoint') {
        // var anchor = cgetter(this.node, 'anchorPoint')
        // return [anchor.x, 1 - anchor.y]
        if (this.node.__class__.toString().startsWith("eui")) {
            return [0.5, 0.5];
        }
        return [this.node.$anchorOffsetX, this.node.$anchorOffsetY];
    }
    else if (attrName === 'zOrders') {
        var local = 0;
        var global = 0;
        var parent = this.node.parent;
        var child = this.node;
        if (this.node.parent) {
            local = this.node.parent.getChildIndex(this.node);
        }
        while (parent) {
            global = global + parent.getChildIndex(child);
            child = parent;
            parent = parent.parent;
        }
        return {
            local: local,
            global: global,
        };
    }
    else if (attrName == 'enabled') {
        return this.node.enabled;
    }
    else if (attrName === 'touchable') {
        return this.node.$touchEnabled;
    }
    else if (attrName === 'rotation') {
        return this.node.$rotation;
    }
    return undefined;
};
Node.prototype.setAttr = function (attrName, val) {
    // raise UnableToSetAttributeException(attrName, self.node)
};
Node.prototype.getAvailableAttributeNames = function () {
    // enumerate all available attributes' name of this node
    // :rettype: Iterable<string>
    return AbstractNode.prototype.getAvailableAttributeNames.call(this).concat([
        'enabled',
        'touchable',
        'rotation',
        'text',
    ]);
};
try {
    module.exports = Node;
}
catch (e) {
    if (window.module && window.module.exports) {
        window.module.exports = Node;
    }
}
// var IScreen = require('./sdk/IScreen')
var IScreen = window.IScreen;
var Screen = function (root) {
    IScreen.call(this);
    this.root = root;
};
Screen.prototype = Object.create(IScreen.prototype);
Screen.prototype.getPortSize = function () {
    return [this.root.width, this.root.height];
};
Screen.prototype.getScreen = function (width) {
    var prefix = 'data:image/jpeg;base64,';
    var rt = new egret.RenderTexture();
    var size = this.getPortSize();
    rt.drawToTexture(this.root);
    var screenData = rt.toDataURL("image/jpeg", new egret.Rectangle(0, 0, size[0], size[1]));
    screenData = screenData.slice(prefix.length);
    return [screenData, 'jpeg'];
};
try {
    module.exports = Screen;
}
catch (e) {
    if (window.module && window.module.exports) {
        window.module.exports = Screen;
    }
}
// var AbstractDumper = require('./sdk/AbstractDumper')
var AbstractDumper = window.AbstractDumper;
var Node = window.Node;
var Dumper = function (root) {
    AbstractDumper.call(this);
    this.root = root;
};
Dumper.prototype = Object.create(AbstractDumper.prototype);
Dumper.prototype.getRoot = function () {
    // root节点读取的长宽消息从width\height改为stageWidth\stageHeight
    return new Node(this.root, this.root.stageWidth, this.root.stageHeight);
    // return new Node(this.root, this.root.width, this.root.height);
};
try {
    module.exports = Dumper;
}
catch (e) {
    if (window.module && window.module.exports) {
        window.module.exports = Dumper;
    }
}
// poco-sdk-egret version
var POCO_SDK_VERSION = '1.0.0';
try {
    module.exports = POCO_SDK_VERSION;
}
catch (e) {
    if (window.module && window.module.exports) {
        window.module.exports = POCO_SDK_VERSION;
    }
}
var Dumper = window.Dumper;
var Screen = window.Screen;
var POCO_SDK_VERSION = window.POCO_SDK_VERSION || '0.0.0';
var PORT = 5003;
function PocoManager(stage, port) {
    this.port = port || PORT;
    this.poco = new Dumper(stage);
    this.screen = new Screen(stage);
    // this.poco.dumpHierarchy();
    // console.log(this.poco)
    this.rpc_dispacher = {
        "GetSDKVersion": function () { return POCO_SDK_VERSION; },
        "Dump": this.poco.dumpHierarchy.bind(this.poco),
        "Screenshot": this.screen.getScreen.bind(this.screen),
        "GetScreenSize": this.screen.getPortSize.bind(this.screen),
        "test": function () { return "test"; },
    };
    console.log('registered rpc methods.', this.rpc_dispacher);
    this.init_server();
}
PocoManager.prototype.init_server = function () {
    console.log("try starting wss..");
    try {
        this.server = new WebSocket("ws://localhost:" + this.port.toString());
        this.server.onopen = function (evt) {
            console.log('Network onConnection...');
        };
        this.server.onmessage = function (evt) {
            console.log('Network onMessage...');
            var fr = new FileReader();
            fr.onloadend = function (e) {
                var text = e.srcElement.result;
                console.log(text);
                try {
                    var req = JSON.parse(text);
                    var res = this.handle_request(req);
                    var sres = JSON.stringify(res);
                    this.server.send(sres);
                }
                catch (error) {
                    console.log("[Poco] error when handling rpc request. req=" + evt.data + '\nerror message: ' + error.stack);
                }
            }.bind(this);
            fr.readAsText(evt.data);
        };
        this.server.onclose = function (evt) {
            console.log('Network onDisconnection...');
            console.log(JSON.stringify(evt));
        };
        this.server.onerror = function (evt) {
            console.log('Network onerror...');
            console.log(JSON.stringify(evt));
        };
        this.server.onopen = this.server.onopen.bind(this);
        this.server.onmessage = this.server.onmessage.bind(this);
        this.server.onclose = this.server.onclose.bind(this);
        this.server.onerror = this.server.onerror.bind(this);
    }
    catch (err) {
        console.log(err.stack + "\n" + err.message);
    }
};
PocoManager.prototype.handle_request = function (req) {
    var ret = {
        id: req.id,
        jsonrpc: req.jsonrpc,
        result: undefined,
        error: undefined,
    };
    var method = req.method;
    var func = this.rpc_dispacher[method];
    if (!func) {
        ret.error = { message: 'No such rpc method "' + method + '", reqid: ' + req.id };
    }
    else {
        var params = req.params;
        try {
            var result = func.apply(this.poco, params);
            ret.result = result;
        }
        catch (error) {
            ret.error = { message: error.stack };
        }
    }
    console.log(ret);
    return ret;
};
try {
    module.exports = PocoManager;
}
catch (e) {
    if (window.module && window.module.exports) {
        window.module.exports = PocoManager;
    }
}
