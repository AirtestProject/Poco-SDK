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
            var fr = new FileReader();
            fr.onloadend = function (e) {
                var text = e.srcElement.result;
                console.log(text);

                try {
                    var req = JSON.parse(text);
                    var res = this.handle_request(req)
                    var sres = JSON.stringify(res)
                    this.server.send(sres)
                } catch (error) {
                    console.log("[Poco] error when handling rpc request. req=" + evt.data + '\nerror message: ' + error.stack)
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


try {
    module.exports = PocoManager;
} catch (e) {
    if (window.module && window.module.exports) {
        window.module.exports = PocoManager;
    }
}