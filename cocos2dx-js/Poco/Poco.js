/*
* @Author: gzliuxin
* @Date:   2017-08-04 10:06:01
* @Last Modified by:   gzliuxin
* @Last Modified time: 2018-04-03 13:48:55
*/

'use strict';

// var Dumper = require('./Cocos2dxDumper')
// var POCO_SDK_VERSION = require('./POCO_SDK_VERSION')

var Dumper = window.Dumper
var POCO_SDK_VERSION = window.POCO_SDK_VERSION || '0.0.0'
var WebSocketServer = window.WebSocketServer  // from native
var PORT = 5003

function PocoManager(port) {
    this.port = port || PORT
    this.poco = new Dumper();
    this.rpc_dispacher = {
        "getSDKVersion": function() { return POCO_SDK_VERSION },
        "GetSDKVersion": function() { return POCO_SDK_VERSION },  // for the compatibility
        "dump": this.poco.dumpHierarchy,
        "Dump": this.poco.dumpHierarchy,  // for the compatibility
        "test": function() { return "test" },
    }
    this.init_server();
}

PocoManager.prototype.init_server = function() {
    console.log("try starting wss..")
    try{
        this.server = new WebSocketServer(this.port);

        this.server.onServerUp = function(evt) {
            console.log("Network onServerUp...");
            console.log(JSON.stringify(evt));
        };

        this.server.onServerDown = function(evt) {
            console.log('Network onServerDown...');  
        };

        this.server.onConnection = function(evt) {
            console.log('Network onConnection...');  
        };

        this.server.onMessage = function(evt) {
            console.log('Network onMessage...');
            console.log(evt.data);
            try {
                var req = JSON.parse(evt.data);
                var res = this.handle_request(req)
                var sres = JSON.stringify(res)
                this.server.send(evt.socketId, sres)
            } catch (error) {
                console.log("[Poco] error when handling rpc request. req=" + evt.data + '\nerror message: ' + error.stack)
            }
        };

        this.server.onDisconnection = function(evt) {
            console.log('Network onDisconnection...');  
            console.log(JSON.stringify(evt));
        };

        this.server.onError = function(evt) {
            console.log('Network onerror...');
            console.log(JSON.stringify(evt));
        };

        this.server.onServerUp = this.server.onServerUp.bind(this)
        this.server.onServerDown = this.server.onServerDown.bind(this)
        this.server.onConnection = this.server.onConnection.bind(this)
        this.server.onMessage = this.server.onMessage.bind(this)
        this.server.onDisconnection = this.server.onDisconnection.bind(this)
        this.server.onError = this.server.onError.bind(this)
    } catch(e){
        console.log(err.stack + "\n" + err.message);
    }
}

PocoManager.prototype.handle_request = function(req) {
    var ret = {
        id: req.id,
        jsonrpc: req.jsonrpc,
        result: undefined,
        error: undefined,
    }
    var method = req.method
    var func = this.rpc_dispacher[method]
    if (!func) {
        ret.error = {message: 'No such rpc method "' + method + '", reqid: ' + req.id}
    } else {
        var params = req.params
        try {
            var result = func.apply(this.poco, params)
            ret.result = result
        } catch (error) {
            ret.error = {message: error.stack}
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