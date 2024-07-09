using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Poco;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Reflection;
using TcpServer;
using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Collections;

public class PocoManager : MonoBehaviour
{
    public event Action<string> MessageReceived;

    public const int versionCode = 6;
    public int port = 5001;
    private bool mRunning;
    public AsyncTcpServer server = null;
    public PocoListenersBase pocoListenersBase;
    private RPCParser rpc = null;
    private SimpleProtocolFilter prot = null;
    private UnityDumper dumper = new UnityDumper();
    private ConcurrentDictionary<string, TcpClientState> inbox = new ConcurrentDictionary<string, TcpClientState>();
    protected ConcurrentDictionary<string, ClientDumpHelp> clientDumpHelpDic = new ConcurrentDictionary<string, ClientDumpHelp>();
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
        rpc.addRpcMethod("SendMessage", SendMessage);

        rpc.addRpcMethod("GetSDKVersion", GetSDKVersion);

        if (pocoListenersBase != null)
        {
            PocoListenerUtils.SubscribePocoListeners(rpc, pocoListenersBase);
        }

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

    //废弃
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
    private object SendMessage(List<object> param)
    {
        if (MessageReceived == null)
        {
            return false;
        }

        var textVal = param[0] as string;

        MessageReceived.Invoke(textVal);

        return true;
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
                string response = HandleMessage(msg, client);
                if (string.IsNullOrEmpty(response)) return;
                ProcessAndSendResponse(client, response, sw, t0);
            });
        }
        vr_support.PeekCommand();
        foreach (ClientDumpHelp cdh in clientDumpHelpDic.Values)
        {
            if (cdh.requestDump && cdh.dumpCanSendFlag)
            {
                cdh.requestDump = false;
                Thread thread = new Thread(new ParameterizedThreadStart(HandleDumpMessage));
                thread.Start(cdh);
            }
        }
    }

    public string HandleMessage(string json, TcpClientState client)
    {
        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json, rpc.settings);
        if (data.TryGetValue("method", out var methodObj) == false)
        {
            Debug.Log("ignore message without method");
            return null;
        }
        var method = methodObj.ToString();
        var idAction = data.TryGetValue("id", out var id) ? id : null;
        List<object> param = null;
        if (data.TryGetValue("params", out var value))
        {
            param = ((JArray)value).ToObject<List<object>>();
        }
        try
        {
            object result = null;
            switch (method)
            {
                case "Invoke":
                    result = PocoListenerUtils.HandleInvocation(rpc.Listeners, data);
                    break;
                case "Dump":
                    ClientDumpHelp cdh = new ClientDumpHelp(client, true, param, idAction, true, false);
                    clientDumpHelpDic.TryAdd(cdh.tcpClientKey, cdh);
                    IEnumerator enumerator = DumpHierarchyCoroutine(cdh);
                    StartCoroutine(enumerator);
                    break;
                default:
                    result = rpc.RPCHandler[method](param);
                    break;
            }
            if (result == null) return string.Empty;
            return rpc.formatResponse(idAction, result);
        }
        catch (Exception exception)
        {
            Debug.LogError(exception);
            return rpc.formatResponseError(idAction, null, exception);
        }
    }

    protected void HandleDumpMessage(object _cdh)
    {
        ClientDumpHelp cdh = (ClientDumpHelp)_cdh;
        var sw = new Stopwatch();
        sw.Start();
        long t0 = sw.ElapsedMilliseconds;
        string response = "";
        try
        {
            response = rpc.formatResponse(cdh.dumpIdAction, cdh.rootResult);
        }
        catch (Exception exception)
        {
            Debug.LogError(exception);
            response = rpc.formatResponseError(cdh.dumpIdAction, null, exception);
        }
        ProcessAndSendResponse(cdh.tcs, response, sw, t0);
        ClientDumpHelp tcdh;
        clientDumpHelpDic.TryRemove(cdh.tcpClientKey, out tcdh);
        tcdh = null;
    }

    protected IEnumerator DumpHierarchyCoroutine(ClientDumpHelp cdh)
    {
        var param = cdh.dumpParams;
        var rootResult = cdh.rootResult;
        var rootNode = dumper.getRoot();
        var onlyVisibleNode = true;
        rootResult.Clear();
        if (param.Count > 0)
        {
            onlyVisibleNode = (bool)param[0];
        }
        if (rootNode == null)
        {
            yield break;
        }
        Stack<(AbstractNode node, Dictionary<string, object> result)> stack = new Stack<(AbstractNode, Dictionary<string, object>)>();
        stack.Push((rootNode, rootResult));
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        while (stack.Count > 0)
        {
            var (currentNode, currentResult) = stack.Pop();
            Dictionary<string, object> payload = currentNode.enumerateAttrs();
            string name = (string)currentNode.getAttr("name");
            currentResult.Add("name", name);
            currentResult.Add("payload", payload);
            List<object> children = new List<object>();
            foreach (AbstractNode child in currentNode.getChildren())
            {
                if (stopwatch.ElapsedMilliseconds > 16f)
                {
                    stopwatch.Restart();
                    yield return null;
                }
                if (!onlyVisibleNode || (bool)child.getAttr("visible"))
                {
                    Dictionary<string, object> childResult = new Dictionary<string, object>();
                    children.Add(childResult);
                    stack.Push((child, childResult));
                }
            }
            if (children.Count > 0)
            {
                currentResult.Add("children", children);
            }
        }
        cdh.dumpRunningFlag = false;
        cdh.dumpCanSendFlag = true;
    }

    protected void ProcessAndSendResponse(TcpClientState client, string response, Stopwatch sw, long t0)
    {
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

    public Dictionary<string, RpcMethod> RPCHandler = new Dictionary<string, RpcMethod>();
    public Dictionary<string, (object instance, MethodInfo method)> Listeners = new Dictionary<string, (object, MethodInfo)>();

    public JsonSerializerSettings settings = new JsonSerializerSettings()
    {
        StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
    };

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

    public void addListener(object instance, string name, MethodInfo method)
    {
        Listeners[name] = (instance, method);
    }
}




public class ClientDumpHelp
{
    public bool requestDump;
    public bool dumpRunningFlag;
    public bool dumpCanSendFlag;
    public object dumpIdAction;
    public List<object> dumpParams;
    public string tcpClientKey;
    public Dictionary<string, object> rootResult = new Dictionary<string, object>();
    public TcpClientState tcs;

    public ClientDumpHelp(TcpClientState _tcs, bool _requestDump, List<object> param, object idAction, bool _dumpRunningFlag, bool _dumpCanSendFlag)
    {
        tcs = _tcs;
        requestDump = _requestDump;
        dumpParams = param;
        tcpClientKey = tcs.TcpClient.Client.RemoteEndPoint.ToString();
        dumpIdAction = idAction;
        dumpRunningFlag = _dumpRunningFlag;
        dumpCanSendFlag = _dumpCanSendFlag;
    }
}

