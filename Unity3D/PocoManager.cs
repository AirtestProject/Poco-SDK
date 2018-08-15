using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Poco;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using TcpServer;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class PocoManager : MonoBehaviour
{
    public const int versionCode = 3;
    float i = 0f;
    public int port = 5001;
    private bool mRunning;
    public AsyncTcpServer server = null;
    private RPCParser rpc = null;
    private SimpleProtocolFilter prot = null;
    private UnityDumper dumper = new UnityDumper();
    private ConcurrentDictionary<string, TcpClientState> inbox = new ConcurrentDictionary<string, TcpClientState>();
    private static Queue<Action> commands = new Queue<Action>();


    [Flags]
    public enum MouseEventFlags
    {
        LeftDown = 0x00000002,
        LeftUp = 0x00000004
    }

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
        rpc.addRpcMethod("isVRSupported", isVRSupported);
        rpc.addRpcMethod("hasMovementFinished", IsQueueEmpty);
        rpc.addRpcMethod("RotateObject", RotateObject);
        rpc.addRpcMethod("ObjectLookAt", ObjectLookAt);
        rpc.addRpcMethod("Screenshot", Screenshot);
        rpc.addRpcMethod("GetScreenSize", GetScreenSize);
        rpc.addRpcMethod("Dump", Dump);
        rpc.addRpcMethod("GetDebugProfilingData", GetDebugProfilingData);
        rpc.addRpcMethod("SetText", SetText);
        rpc.addRpcMethod("GetSDKVersion", GetSDKVersion);

        mRunning = true;

        server = new AsyncTcpServer(port);
        server.Encoding = Encoding.UTF8;
        server.ClientConnected +=
            new EventHandler<TcpClientConnectedEventArgs>(server_ClientConnected);
        server.ClientDisconnected +=
            new EventHandler<TcpClientDisconnectedEventArgs>(server_ClientDisconnected);
        server.DatagramReceived +=
            new EventHandler<TcpDatagramReceivedEventArgs<byte[]>>(server_Received);
        server.Start();
        Debug.Log("Tcp server started");
        commands.Clear();
    }

    [RPC]
    public static object isVRSupported(List<object> param)
    {
        return UnityEngine.XR.XRSettings.loadedDeviceName.Equals("CARDBOARD");
    }

    [RPC]
    public static object IsQueueEmpty(List<object> param)
    {
        Debug.Log("Checking queue");
        if (commands != null && commands.Count > 0)
        {
            return null;
        }
        else
        {
            Thread.Sleep(1000); // we wait a bit and check again just in case we run in between calls
            if (commands != null && commands.Count > 0)
            {
                return null;
            }
        }

        return commands.Count;
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
    static object RotateObject(List<object> param)
    {
        var xRotation = Convert.ToSingle(param[0]);
        var yRotation = Convert.ToSingle(param[1]);
        var zRotation = Convert.ToSingle(param[2]);
        float speed = 0f;
        if (param.Count > 5)
            speed = Convert.ToSingle(param[5]);
        Vector3 mousePosition = new Vector3(xRotation, yRotation, zRotation);
        foreach (GameObject cameraContainer in GameObject.FindObjectsOfType<GameObject>())
        {
            if (cameraContainer.name.Equals(param[3]))
            {
                foreach (GameObject cameraFollower in GameObject.FindObjectsOfType<GameObject>())
                {
                    if (cameraFollower.name.Equals(param[4]))
                    {
                        lock (commands)
                        {
                            commands.Enqueue(() => recoverOffset(cameraFollower, cameraContainer, speed));
                        }

                        lock (commands)
                        {
                            var currentRotation = cameraContainer.transform.rotation;
                            commands.Enqueue(() => rotate(cameraContainer, currentRotation, mousePosition, speed));
                        }
                        return true;
                    }
                }

                return true;
            }
        }
        return false;
    }

    static private void rotate(GameObject go, Quaternion originalRotation, Vector3 mousePosition, float speed)
    {
        Debug.Log("rotating");
        if (!UnityNode.RotateObject(originalRotation, mousePosition, go, speed))
        {
            lock (commands)
            {
                commands.Dequeue();
            }
        }
    }


    [RPC]
    static object ObjectLookAt(List<object> param)
    {
        float speed = 0f;
        if (param.Count > 3)
            speed = Convert.ToSingle(param[3]);
        foreach (GameObject toLookAt in GameObject.FindObjectsOfType<GameObject>()) // hacer un loop y tener los objetos a null antes
        {
            if (toLookAt.name.Equals(param[0]))
            {
                foreach (GameObject cameraContainer in GameObject.FindObjectsOfType<GameObject>())
                {
                    if (cameraContainer.name.Equals(param[1]))
                    {
                        foreach (GameObject cameraFollower in GameObject.FindObjectsOfType<GameObject>())
                        {
                            if (cameraFollower.name.Equals(param[2]))
                            {
                                lock (commands)
                                {
                                    commands.Enqueue(() => recoverOffset(cameraFollower, cameraContainer, speed));
                                }

                                lock (commands)
                                {
                                    commands.Enqueue(() => objectLookAt(cameraContainer, toLookAt, speed));
                                }

                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    static private void recoverOffset(GameObject subcontainter, GameObject cameraContainer, float speed)
    {
        Debug.Log("recovering " + cameraContainer.name);
        if (!UnityNode.ObjectRecoverOffset(subcontainter, cameraContainer, speed))
        {
            lock (commands)
            {
                commands.Dequeue();
            }
        }
    }

    static private void objectLookAt(GameObject go, GameObject toLookAt, float speed)
    {
        Debug.Log("looking at " + toLookAt.name);
        Debug.Log("from " + go.name);
        if (!UnityNode.ObjectLookAtObject(toLookAt, go, speed))
        {
            lock (commands)
            {
                commands.Dequeue();
            }
        }
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
        server.Stop();
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
        //     Camera.main.transform.rotation = rotation;

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

        if (null != commands && commands.Count > 0)
        {
            Debug.Log("command executed " + commands.Count);
            commands.Peek()();
        }
    }

    void OnApplicationQuit()
    { // stop listening thread
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
