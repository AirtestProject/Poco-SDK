/*
* @Author: gzliuxin
* @Date:   2017-08-04 10:06:01
* @Last Modified by:   gzliuxin
* @Last Modified time: 2017-08-11 13:48:55
*/

'use strict';

var ws = WebSocket || window.WebSocket || window.MozWebSocket
var SERVER_ADDRESS = "ws://localhost:5001"
var SERVER_ADDRESS_EXTRA = "ws://localhost:5002"
var PORT = 5003
var Dumper = require('./Cocos2dxDumper')

var PocoManager = function(port) {
    Object.call(this)
    this.port = port || PORT
    this.addr = SERVER_ADDRESS  // 弃用变量
    // this.connected = false
    this.dumper = new Dumper()
    this.rpc_dispacher = {
        "dump": this.dumper.dumpHierarchy,
    }
    this.init_server()
}
PocoManager.prototype = Object.create(PocoManager.prototype)

PocoManager.prototype.init_server = function() {
    console.log("try starting wss..")
    try{
        this.server = new WebSocketServer(this.port)

        this.server.onServerUp = function(evt) {
            console.log("Network onServerUp...", evt)
        }
        this.server.onServerDown = function(evt) {
            console.log('Network onServerDown...')
        }
        this.server.onConnection = function(evt) {
            console.log('Network onConnection...')
        }
        this.server.onMessage = function(evt) {
            console.log('Network onMessage...', evt.data)
            var data = JSON.parse(evt.data)
            var method = data.method
            var params = data.params
            var func = this.rpc_dispacher[method]
            var result = func.apply(this.dumper, params)
            var ret = {
                "id": data.id,
                "jsonrpc": data.jsonrpc,
                "result": result,
            }
            console.log(ret)
            ret = JSON.stringify(ret)
            console.log(ret)
            this.server.send(evt.socketId, ret)
        }
        this.server.onDisconnection = function(evt) {
            console.log('Network onDisconnection...', evt)  
        }
        this.server.onError = function(evt) {
            console.log('Network onerror...', evt)
        }

        this.server.onServerUp = this.server.onServerUp.bind(this)
        this.server.onServerDown = this.server.onServerDown.bind(this)
        this.server.onConnection = this.server.onConnection.bind(this)
        this.server.onMessage = this.server.onMessage.bind(this)
        this.server.onDisconnection = this.server.onDisconnection.bind(this)
        this.server.onError = this.server.onError.bind(this)
    } catch(e) {
        console.log(e)
    }
}

PocoManager.prototype.connect = function() {
    // init socket client
    var s = new ws(this.addr)
    s.onopen = function(evt) {
        console.log('Network onopen...')
        this.connected = true
        // s.send("Hello, I am cocosjs")
    }
    s.onmessage = function(evt) {  
        console.log('Network onmessage...')
        var data = JSON.parse(evt.data)
        var method = data.method
        var params = data.params
        var func = this.rpc_dispacher[method]
        var result = func.apply(this.dumper, params)
        var ret = {
            "id": data.id,
            "jsonrpc": data.jsonrpc,
            "result": result,
        }
        console.log(ret)
        ret = JSON.stringify(ret)
        s.send(ret)
    }
    s.onerror = function(evt) {  
        console.log('Network onerror...')
    }
    s.onclose = function(evt) {  
        console.log('Network onclose...')
        this.connected = false
        s = null
    }

    s.onopen = s.onopen.bind(this)
    s.onmessage = s.onmessage.bind(this)
    s.onerror = s.onerror.bind(this)
    s.onclose = s.onclose.bind(this)
}

module.exports = PocoManager
