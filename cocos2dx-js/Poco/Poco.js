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
        "dump": this.poco.dumpHierarchy,
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
            var data = JSON.parse(evt.data);
            var method = data.method
            var params = data.params
            var func = this.rpc_dispacher[method]
            var result = func.apply(this.poco, params)
            var ret = {
                "id": data.id,
                "jsonrpc": data.jsonrpc,
                "result": result,
            }
            ret = JSON.stringify(ret)
            console.log(ret);
            this.server.send(evt.socketId, ret)
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


try {
    module.exports = PocoManager;
} catch (e) {
    if (window.module && window.module.exports) {
        window.module.exports = PocoManager;
    }
}