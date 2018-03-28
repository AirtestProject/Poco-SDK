/*
* @Author: gzliuxin
* @Date:   2017-08-04 10:06:01
* @Last Modified by:   gzliuxin
* @Last Modified time: 2017-08-11 13:48:55
*/

'use strict';

var ws = WebSocket || window.WebSocket || window.MozWebSocket;
const SERVER_ADDRESS = "ws://localhost:5001";
const SERVER_ADDRESS_EXTRA = "ws://localhost:5002";
var Dumper = require('./Cocos2dxDumper');


class Rpc{

	constructor(addr){
		this.addr = addr;
		// this.connected = false;
        this.dumper = new Dumper();
        this.rpc_dispacher = {
            "dump": this.dumper.dumpHierarchy,
        }
        this.init_server();
	}

    init_server(){
        console.log("try start wss..")
        try{
            this.server = new WebSocketServer(5003);

            this.server.onServerUp = (evt) => {
                console.log("Network onServerUp...", evt);
            };

            this.server.onServerDown = (evt) => {
                console.log('Network onServerDown...');  
            };

            this.server.onConnection = (evt) => {
                console.log('Network onConnection...');  
            };

            this.server.onMessage = (evt) => {
                console.log('Network onMessage...', evt.data);
                var data = JSON.parse(evt.data);
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
            };

            this.server.onDisconnection = (evt) => {
                console.log('Network onDisconnection...');  
            };

            this.server.onError = (evt) => {
                console.log('Network onerror...');
            };
        } catch(e){
            console.log(e);
        }
    }

    init_connection(){
		// if (!this.connected){
		// 	console.log("try connecting ws server..")
		// 	this.connect()	
		// }
		// setTimeout(this.init_connection.bind(this), 5000);
	}

    connect(){
        // init socket client
        var s = new ws(this.addr)
        s.onopen = (evt) => {
            console.log('Network onopen...');
            this.connected = true;
            // s.send("Hello, I am cocosjs");
        };

        s.onmessage = (evt) => {  
            console.log('Network onmessage...'); //, evt);
            var data = JSON.parse(evt.data);
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
        };
          
        s.onerror = (evt) => {  
            console.log('Network onerror...');  
        };
          
        s.onclose = (evt) => {  
            console.log('Network onclose...');  
            this.connected = false;

            s = null;
        };
    }

}

var r = new Rpc(SERVER_ADDRESS);
// var r2 = new Rpc(SERVER_ADDRESS_EXTRA);
module.exports = r;
