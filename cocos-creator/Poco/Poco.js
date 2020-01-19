

'use strict';

var Dumper = require('./Cocos2dxDumper')
var POCO_SDK_VERSION = require('./POCO_SDK_VERSION')


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

PocoManager.prototype.init_server = function() {
    console.log("try starting wss..")
    var that = this
    try{

        if(typeof WebSocketServer == "undefined") {
            console.error("WebSocketServer is not enabled!");
            return;
        }        

        var s = new WebSocketServer();

        s.listen(this.port, (err) => {
            if(!err) console.log("server booted!");
         });
 

        s.onconnection = function(conn) {
            console.log('Network onConnection...');  
            conn.ondata = function(data) {
                console.log('Network onMessage...');
                console.log(data);
                try {
                    var req = JSON.parse(data);
                    var res = that.handle_request(req);
                    var sres = JSON.stringify(res);

                    conn.send(sres, (err)=>{});

                } catch (error) {
                    console.log("[Poco] error when handling rpc request. req=" + data + '\nerror message: ' + error.stack);
                }
            }
            conn.onclose = function() { console.log("connection gone!");} ;
        };
        
        s.onclose = function() {
          console.log("server is closed!")
        }

    } catch(err){
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