/*
* @Author: gzliuxin
* @Date:   2017-08-04 10:06:01
* @Last Modified by:   gzliuxin
* @Last Modified time: 2018-04-03 13:48:55
*/

'use strict';

var Dumper = window.Dumper
var Screen = window.Screen

var POCO_SDK_VERSION = window.POCO_SDK_VERSION || '0.0.0'
var PORT = 5003

function PocoManager(stage, port) {
    this.port = port || PORT
    this.poco = new Dumper(stage);
    this.screen = new Screen(stage);
    this.rpc_dispacher = {
        "GetSDKVersion": function () { return POCO_SDK_VERSION },
        "Dump": this.poco.dumpHierarchy.bind(this.poco),
        "Screenshot": this.screen.getScreen.bind(this.screen),
        "GetScreenSize": this.screen.getPortSize.bind(this.screen),
        "test": function () { return "test" },
    }
    console.log('registered rpc methods.', this.rpc_dispacher)

    this.init_server();
}

PocoManager.prototype.init_server = function () {
    console.log("try starting wss..")
    try {
        this.server = new WebSocket("ws://localhost:" + this.port.toString())

        this.server.onopen = function (evt) {
            console.log('Network onConnection...');
        };

        this.server.onmessage = function (evt) {
            console.log('Network onMessage...');
            var text = decodeUTF8(new Uint8Array(evt.data));
            console.log(text);
            try {
                var req = JSON.parse(text);
                var res = this.handle_request(req)
                var sres = JSON.stringify(res)
                this.server.send(sres)
            } catch (error) {
                console.log("[Poco] error when handling rpc request. req=" + evt.data + '\nerror message: ' + error.stack)
            }
        };

        this.server.onclose = function (evt) {
            console.log('Network onDisconnection...');
            console.log(JSON.stringify(evt));
        };

        this.server.onerror = function (evt) {
            console.log('Network onerror...');
            console.log(JSON.stringify(evt));
        };

        this.server.onopen = this.server.onopen.bind(this)
        this.server.onmessage = this.server.onmessage.bind(this)
        this.server.onclose = this.server.onclose.bind(this)
        this.server.onerror = this.server.onerror.bind(this)
    } catch (err) {
        console.log(err.stack + "\n" + err.message);
    }
}

PocoManager.prototype.handle_request = function (req) {
    var ret = {
        id: req.id,
        jsonrpc: req.jsonrpc,
        result: undefined,
        error: undefined,
    }
    var method = req.method
    var func = this.rpc_dispacher[method]
    if (!func) {
        ret.error = { message: 'No such rpc method "' + method + '", reqid: ' + req.id }
    } else {
        var params = req.params
        try {
            var result = func.apply(this.poco, params)
            ret.result = result
        } catch (error) {
            ret.error = { message: error.stack }
        }
    }
    console.log(ret);
    return ret
}

function decodeUTF8(arr) {
    for (var i = 0; i < arr.length; i++) {
        if (arr[i] < 0) arr[i] += 256;
    }
    var res = [];
    for (i = 0; i < arr.length; i++) {
        if (arr[i] == 0) break;
        if ((arr[i] & 128) == 0) res.push(arr[i]);				//1位
        else if ((arr[i] & 64) == 0) res.push(arr[i] & 127);		//1位
        else if ((arr[i] & 32) == 0)	//2位
        {
            res.push((arr[i] & 31) << 6 + (arr[i + 1] & 63));
            i++;
        }
        else if ((arr[i] & 16) == 0)	//3位
        {
            res.push((arr[i] & 15) << 12 + (arr[i + 1] & 63) << 6 + (arr[i + 2] & 63));
            i += 2;
        }
        else if ((arr[i] & 8) == 0)	//4位
        {
            res.push((arr[i] & 7) << 18 + (arr[i + 1] & 63) << 12 + (arr[i + 2] & 63) << 6 + (arr[i + 3] & 63));
            i += 3;
        }
    }
    var str = "";
    for (i = 0; i < res.length; i++) {
        str += String.fromCharCode(res[i]);
    }
    return str;
}


try {
    module.exports = PocoManager;
} catch (e) {
    if (window.module && window.module.exports) {
        window.module.exports = PocoManager;
    }
}
