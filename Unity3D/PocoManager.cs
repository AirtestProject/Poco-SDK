using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Poco;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using TcpServer;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class PocoManager : MonoBehaviour
{
    public const int versionCode = 6;
    public int port = 5001;
    private bool mRunning;
    public AsyncTcpServer server = null;
    private RPCParser rpc = null;
    private SimpleProtocolFilter prot = null;
    private UnityDumper dumper = new UnityDumper();
    private ConcurrentDictionary<string, TcpClientState> inbox = new ConcurrentDictionary<string, TcpClientState>();
    private VRSupport vr_support = new VRSupport();
    private Dictionary<string, long> debugProfilingData = new Dictionary<string, long>() {
        { "dump", 0 },
        { "screenshot", 0 },
        { "handleRpcRequest", 0 },
        { "packRpcResponse", 0 },
        { "sendRpcResponse", 0 },
    };

    class RPC : Attribute
    {
    }

    void Awake()
    {
        Application.runInBackground = true;
        DontDestroyOnLoad(this);
        prot = new SimpleProtocolFilter();
        rpc = new RPCParser();
        rpc.addRpcMethod("isVRSupported", vr_support.isVRSupported);
        rpc.addRpcMethod("hasMovementFinished", vr_support.IsQueueEmpty);
        rpc.addRpcMethod("RotateObject", vr_support.RotateObject);
        rpc.addRpcMethod("ObjectLookAt", vr_support.ObjectLookAt);
        rpc.addRpcMethod("Screenshot", Screenshot);
        rpc.addRpcMethod("GetScreenSize", GetScreenSize);
        rpc.addRpcMethod("Dump", Dump);
        rpc.addRpcMethod("GetDebugProfilingData", GetDebugProfilingData);
        rpc.addRpcMethod("SetText", SetText);

        rpc.addRpcMethod("GetSDKVersion", GetSDKVersion);

        mRunning = true;

        for (int i = 0; i < 5; i++)
        {
            this.server = new AsyncTcpServer(port + i);
            this.server.Encoding = Encoding.UTF8;
            this.server.ClientConnected +=
                new EventHandler<TcpClientConnectedEventArgs>(server_ClientConnected);
            this.server.ClientDisconnected +=
                new EventHandler<TcpClientDisconnectedEventArgs>(server_ClientDisconnected);
            this.server.DatagramReceived +=
                new EventHandler<TcpDatagramReceivedEventArgs<byte[]>>(server_Received);
            try
            {
                this.server.Start();
                Debug.Log(string.Format("Tcp server started and listening at {0}", server.Port));
                break;
            }
            catch (SocketException e)
            {
                Debug.Log(string.Format("Tcp server bind to port {0} Failed!", server.Port));
                Debug.Log("--- Failed Trace Begin ---");
                Debug.LogError(e);
                Debug.Log("--- Failed Trace End ---");
                // try next available port
                this.server = null;
            }
        }
        if (this.server == null)
        {
            Debug.LogError(string.Format("Unable to find an unused port from {0} to {1}", port, port + 5));
        }
        vr_support.ClearCommands();
    }

    static void server_ClientConnected(object sender, TcpClientConnectedEventArgs e)
    {
        Debug.Log(string.Format("TCP client {0} has connected.",
            e.TcpClient.Client.RemoteEndPoint.ToString()));
    }

    static void server_ClientDisconnected(object sender, TcpClientDisconnectedEventArgs e)
    {
        Debug.Log(string.Format("TCP client {0} has disconnected.",
           e.TcpClient.Client.RemoteEndPoint.ToString()));
    }

    private void server_Received(object sender, TcpDatagramReceivedEventArgs<byte[]> e)
    {
        Debug.Log(string.Format("Client : {0} --> {1}",
            e.Client.TcpClient.Client.RemoteEndPoint.ToString(), e.Datagram.Length));
        TcpClientState internalClient = e.Client;
        string tcpClientKey = internalClient.TcpClient.Client.RemoteEndPoint.ToString();
        inbox.AddOrUpdate(tcpClientKey, internalClient, (n, o) =>
        {
            return internalClient;
        });
    }

    [RPC]
    private object Dump(List<object> param)
    {
        var onlyVisibleNode = true;
        if (param.Count > 0)
        {
            onlyVisibleNode = (bool)param[0];
        }
        var sw = new Stopwatch();
        sw.Start();
        var h = dumper.dumpHierarchy(onlyVisibleNode);
        debugProfilingData["dump"] = sw.ElapsedMilliseconds;

        return h;
    }

    [RPC]
    private object Screenshot(List<object> param)
    {
        var sw = new Stopwatch();
        sw.Start();

        var tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        tex.Apply(false);
        byte[] fileBytes = tex.EncodeToJPG(80);
        var b64img = Convert.ToBase64String(fileBytes);
        debugProfilingData["screenshot"] = sw.ElapsedMilliseconds;
        return new object[] { b64img, "jpg" };
    }

    [RPC]
    private object GetScreenSize(List<object> param)
    {
        return new float[] { Screen.width, Screen.height };
    }

    public void stopListening()
    {
        mRunning = false;
        server?.Stop();
    }

    [RPC]
    private object GetDebugProfilingData(List<object> param)
    {
        return debugProfilingData;
    }

    [RPC]
    private object SetText(List<object> param)
    {
        var instanceId = Convert.ToInt32(param[0]);
        var textVal = param[1] as string;
        foreach (var go in GameObject.FindObjectsOfType<GameObject>())
        {
            if (go.GetInstanceID() == instanceId)
            {
                return UnityNode.SetText(go, textVal);
            }
        }
        return false;
    }

    [RPC]
    private object GetSDKVersion(List<object> param)
    {
        return versionCode;
    }

    void Update()
    {
        foreach (TcpClientState client in inbox.Values)
        {
            List<string> msgs = client.Prot.swap_msgs();
            msgs.ForEach(delegate (string msg)
            {
                var sw = new Stopwatch();
                sw.Start();
                var t0 = sw.ElapsedMilliseconds;
                string response = rpc.HandleMessage(msg);
                var t1 = sw.ElapsedMilliseconds;
                byte[] bytes = prot.pack(response);
                var t2 = sw.ElapsedMilliseconds;
                server.Send(client.TcpClient, bytes);
                var t3 = sw.ElapsedMilliseconds;
                debugProfilingData["handleRpcRequest"] = t1 - t0;
                debugProfilingData["packRpcResponse"] = t2 - t1;
                TcpClientState internalClientToBeThrowAway;
                string tcpClientKey = client.TcpClient.Client.RemoteEndPoint.ToString();
                inbox.TryRemove(tcpClientKey, out internalClientToBeThrowAway);
            });
        }

        vr_support.PeekCommand();
    }

    void OnApplicationQuit()
    {
        // stop listening thread
        stopListening();
    }

    void OnDestroy()
    {
        // stop listening thread
        stopListening();
    }

}


public class RPCParser
{
    public delegate object RpcMethod(List<object> param);

    protected Dictionary<string, RpcMethod> RPCHandler = new Dictionary<string, RpcMethod>();
    private JsonSerializerSettings settings = new JsonSerializerSettings()
    {
        StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
    };

    public string HandleMessage(string json)
    {
        Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json, settings);
        if (data.ContainsKey("method"))
        {
            string method = data["method"].ToString();
            List<object> param = null;
            if (data.ContainsKey("params"))
            {
                param = ((JArray)(data["params"])).ToObject<List<object>>();
            }

            object idAction = null;
            if (data.ContainsKey("id"))
            {
                // if it have id, it is a request
                idAction = data["id"];
            }

            string response = null;
            object result = null;
            try
            {
                result = RPCHandler[method](param);
            }
            catch (Exception e)
            {
                // return error response
                Debug.Log(e);
                response = formatResponseError(idAction, null, e);
                return response;
            }

            // return result response
            response = formatResponse(idAction, result);
            return response;

        }
        else
        {
            // do not handle response
            Debug.Log("ignore message without method");
            return null;
        }
    }

    // Call a method in the server
    public string formatRequest(string method, object idAction, List<object> param = null)
    {
        Dictionary<string, object> data = new Dictionary<string, object>();
        data["jsonrpc"] = "2.0";
        data["method"] = method;
        if (param != null)
        {
            data["params"] = JsonConvert.SerializeObject(param, settings);
        }
        // if idAction is null, it is a notification
        if (idAction != null)
        {
            data["id"] = idAction;
        }
        return JsonConvert.SerializeObject(data, settings);
    }

    // Send a response from a request the server made to this client
    public string formatResponse(object idAction, object result)
    {
        Dictionary<string, object> rpc = new Dictionary<string, object>();
        rpc["jsonrpc"] = "2.0";
        rpc["id"] = idAction;
        rpc["result"] = result;
        return JsonConvert.SerializeObject(rpc, settings);
    }

    // Send a error to the server from a request it made to this client
    public string formatResponseError(object idAction, IDictionary<string, object> data, Exception e)
    {
        Dictionary<string, object> rpc = new Dictionary<string, object>();
        rpc["jsonrpc"] = "2.0";
        rpc["id"] = idAction;

        Dictionary<string, object> errorDefinition = new Dictionary<string, object>();
        errorDefinition["code"] = 1;
        errorDefinition["message"] = e.ToString();

        if (data != null)
        {
            errorDefinition["data"] = data;
        }

        rpc["error"] = errorDefinition;
        return JsonConvert.SerializeObject(rpc, settings);
    }

    public void addRpcMethod(string name, RpcMethod method)
    {
        RPCHandler[name] = method;
    }
}
