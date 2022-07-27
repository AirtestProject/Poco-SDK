//@ts-nocheck
import Dumper from "./CocosCreator3Dumper"
const POCO_SDK_VERSION = "1.1.0"

type retInfo = {
    id: string
    jsonrpc: string
    result: any
    error: { message: string } | null
}

export default class PocoManager {
    port: number
    poco: Dumper
    rpc_dispacher: any
    constructor(port: number) {
        this.port = port || 5003
        this.poco = new Dumper()
        this.rpc_dispacher = {
            getSDKVersion: function () {
                return POCO_SDK_VERSION
            },
            GetSDKVersion: function () {
                return POCO_SDK_VERSION
            }, // for the compatibility
            dump: this.poco.dumpHierarchy,
            Dump: this.poco.dumpHierarchy, // for the compatibility
            test: function () {
                return "test"
            },
        }
        this.init_server()
    }

    handle_request(req: any) {
        var ret: retInfo = {
            id: req.id,
            jsonrpc: req.jsonrpc,
            result: null,
            error: null,
        }
        var method = req.method
        var func = this.rpc_dispacher[method]
        if (!func) {
            ret.error = {
                message:
                    'No such rpc method "' + method + '", reqid: ' + req.id,
            }
        } else {
            var params = req.params
            try {
                var result = func.apply(this.poco, params)
                ret.result = result
            } catch (error: any) {
                ret.error = { message: error.stack }
            }
        }
        console.log(ret)
        return ret
    }

    init_server() {
        console.log("try starting wss..")
        var that = this
        try {
            if (typeof WebSocketServer == "undefined") {
                console.error("WebSocketServer is not enabled!")
                return
            }

            var s = new WebSocketServer()

            s.listen(this.port, (err: any) => {
                if (!err) console.log("server booted!")
            })

            s.onconnection = function (conn: any) {
                console.log("Network onConnection...")
                conn.ondata = function (data: any) {
                    console.log("Network onMessage...")
                    console.log(data)
                    try {
                        var req = JSON.parse(data)
                        var res = that.handle_request(req)
                        var sres = JSON.stringify(res)

                        conn.send(sres, (err: any) => {})
                    } catch (error: any) {
                        console.log(
                            "[Poco] error when handling rpc request. req=" +
                                data +
                                "\nerror message: " +
                                error.stack
                        )
                    }
                }
                conn.onclose = function () {
                    console.log("connection gone!")
                }
            }

            s.onclose = function () {
                console.log("server is closed!")
            }
        } catch (err: any) {
            console.log(err.stack + "\n" + err.message)
        }
    }
}
