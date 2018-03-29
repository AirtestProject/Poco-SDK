/*
* @Author: gzliuxin
* @Date:   2017-08-04 10:06:01
* @Last Modified by:   gzliuxin
* @Last Modified time: 2017-08-11 13:48:55
*/

'use strict';

var PORT = 5003
var Dumper = require('./Cocos2dxDumper')


class PocoManager{

    constructor(port){
        this.port = port | PORT
        this.poco = new Dumper();
        this.rpc_dispacher = {
            "dump": this.poco.dumpHierarchy,
        }
        this.init_server();
    }

    init_server(){
        console.log("try start wss..")
        try{
            this.server = new WebSocketServer(this.port);

            this.server.onServerUp = (evt) => {
                console.log("Network onServerUp...", evt);
                console.log(JSON.stringify(evt));
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
                var result = func.apply(this.poco, params)
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
}


module.exports = PocoManager
