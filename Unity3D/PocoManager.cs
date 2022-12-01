using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Poco;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Reflection;
using Game.SDKs.PocoSDK;
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
    // private UnityDumper dumper = new UnityDumper();
    private CustomDumper cDumper = new CustomDumper();
    private TcpServer.ConcurrentDictionary<string, TcpClientState> inbox = new TcpServer.ConcurrentDictionary<string, TcpClientState>();
    // private VRSupport vr_support = new VRSupport();
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

    // private int pid;

    // private object GetPid(List<object> param)
    // {
    //     if (param == null)
    //     {
    //         throw new ArgumentException("GetPid method must has argv");
    //     }
    //
    //     return pid;
    // }
    
    // public void OnGotPid(int pid)
    // {
    //     this.pid = pid;
    //     // Debug.LogError(string.Format("pid is: {0}", pid));
    // }

    public void RegisteCustomHandler<T>(string dumpName, Func<UnityNode, object> act, string hiearchyName = "")
        where T : Component
    {
        cDumper.RegisteCustomHandler<T>( dumpName, act, hiearchyName);
    }

    private object IsLoginDone(List<object> param)
    {
        if (param == null)
        {
            throw new ArgumentException("IsLoginDone method must has argv");
        }

        // todo 已脱敏
        return new object();
    }

    public int androidProcessId;
    public AndroidJavaObject info;
    
    void Awake()
    {
        // sampleGame.Game.OnGotPlayerId += OnGotPid;

        Utf8Json.Resolvers.CompositeResolver.RegisterAndSetAsDefault(
            
        //    UnityResolver.Instance,
             Utf8Json.Resolvers.BuiltinResolver.Instance,
        Utf8Json.Resolvers.PocoResolver.Instance
            );

        // 获取安卓对象
#if (UNITY_ANDROID && !UNITY_EDITOR) || ANDROID_CODE_VIEW
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject curApplication = currentActivity.Call<AndroidJavaObject>("getApplication");
        AndroidJavaObject am = curApplication.Call<AndroidJavaObject>("getSystemService", "activity");
        AndroidJavaObject pro = new AndroidJavaClass("android.os.Process");
        androidProcessId = pro.CallStatic<int>("myPid");
        var infos = am.Call<AndroidJavaObject[]>("getProcessMemoryInfo", new int[] { androidProcessId });
        info = infos[0];
#endif

        Thread serializeThread = new Thread(new ThreadStart(HandleResult));
        serializeThread.IsBackground = true;
        serializeThread.Start();
        
        tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        Application.runInBackground = true;
        DontDestroyOnLoad(this);
        prot = new SimpleProtocolFilter();
        rpc = new RPCParser();
        // rpc.addRpcMethod("isVRSupported", vr_support.isVRSupported);
        // rpc.addRpcMethod("hasMovementFinished", vr_support.IsQueueEmpty);
        // rpc.addRpcMethod("RotateObject", vr_support.RotateObject);
        // rpc.addRpcMethod("ObjectLookAt", vr_support.ObjectLookAt);
        
        // 截图方法已通过协程实现
        // rpc.addRpcMethod("Screenshot", Screenshot);
        rpc.addRpcMethod("GetScreenSize", GetScreenSize);
        rpc.addRpcMethod("Dump", Dump);
        rpc.addRpcMethod("GetDebugProfilingData", GetDebugProfilingData);
        rpc.addRpcMethod("SetText", SetText);

        rpc.addRpcMethod("GetSDKVersion", GetSDKVersion);
        
        //GM相关
        rpc.addRpcMethod( "gm", DoGM );
        
        // uwa gpm相关
        rpc.addRpcMethod("InitGPM", InitGPM );
        rpc.addRpcMethod("StartGPM", StartGPM );
        rpc.addRpcMethod("StopGPM", StopGPM );
        
        // 自研性能数据收集器控制方法
        rpc.addRpcMethod("InitPerfDataManager", InitPerfDataManager);
        rpc.addRpcMethod("StopPerfDataManager", StopPerfDataManager);
        rpc.addRpcMethod("TickDataFromPdm", TickDataFromPdm);
        rpc.addRpcMethod("GetStatus",GetStatus);
        
        // 操作系统相关
        rpc.addRpcMethod("GetAndroidProcessId", GetAndroidProcessId);
        
        //登录登出相关
        // rpc.addRpcMethod("GetPid", GetPid);
        rpc.addRpcMethod("IsLoginDone", IsLoginDone);
        rpc.addRpcMethod( "login", DoLogin );
        rpc.addRpcMethod("logout", DoLogout);
        
        //uwa gotonline相关
        rpc.addRpcMethod("InitGotOnline", InitGotOnline);
        rpc.addRpcMethod("StartGotOnline", StartGotOnline);
        rpc.addRpcMethod("StopGotOnline", StopGotOnline);
        rpc.addRpcMethod("UploadGotOnline", UploadGotOnline);
        rpc.addRpcMethod("IsGotOnlineRunning", IsGotOnlineRunning);
        rpc.addRpcMethod("IsGotOnlineHasRunned", IsGotOnlineHasRunned);

        //GOEGame.TimerMod.SetTimeout( () =>
        // {
        //     List<object> test = new List<object>();
        //     test.Add( "yong3001" );
        //     test.Add( 1 );
        //     test.Add( 2 );
        //     DoLogin( test );
        // }, 10 );

        //GOEGame.TimerMod.SetTimeout( () =>
        // {
        //     List<object> test = new List<object>();
        //     test.Add( "item/add" );
        //     test.Add( "{\"itemId\":\"20041\",\"cnt\":\"555\"}" );
        //     DoGM( test );
        // }, 20 );

        //GOEGame.TimerMod.SetTimeout( () =>
        // {
        //     List<object> test = new List<object>();
        //     test.Add( "Exit" ); //ReturnToLogin/Exit
        //     DoLogout( test );
        // }, 30 );

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
        // vr_support.ClearCommands();
    }

    public bool isGotOnlineRunning;
    public bool isGotOnlineHasRunned;

    private object IsGotOnlineHasRunned(List<object> param)
    {
        return isGotOnlineHasRunned;
    }
    
    private object IsGotOnlineRunning(List<object> param)
    {
        return isGotOnlineRunning;
    }

    private string uwaStat = "idle";
    
    private object InitGotOnline(List<object> param)
    {
        // UWAEngine.StaticInit(true);

        // uwaStat = "inited";
        return true;
    }

    public void OnGUI()
    {
        GUI.Label(new Rect(10, 60, 280, 30), $"uwa状态: {uwaStat}");
    }

    private object StartGotOnline(List<object> param)
    {
        if (param.Count != 1)
        {
            return false;
        }

        var p = Convert.ToInt32(param[0]);
        // UWAEngine.Mode mode = (UWAEngine.Mode)p;
        //
        // UWAEngine.Start(mode);
        // UWAEngine.SetUIActive(false);
        isGotOnlineRunning = true;
        isGotOnlineHasRunned = true;
        uwaStat = "started";
        return true;
    }
    
    private object StopGotOnline(List<object> param)
    {
        if (param == null)
        {
            return false;
        }
        // UWAEngine.Stop();
        isGotOnlineRunning = false;
        uwaStat = "stopped";
        return true;
    }
    
    private object UploadGotOnline(List<object> param)
    {
        if (param.Count != 4)
        {
            return false;
        }

        string userName = param[0].ToString();
        string passwd = param[1].ToString();
        int projectId = Convert.ToInt32(param[2]);
        int timeOut = Convert.ToInt32(param[3]);

        // UWAEngine.Upload(UploadGotOnlineCallBack, userName, passwd, projectId, timeOut);
        uwaStat = "uploaded";
        return isUploadDone;
    }

    private bool isUploadDone;
    
    private void UploadGotOnlineCallBack(bool isSuccess)
    {
        isUploadDone = isSuccess;
    }
    
    public PerfDataManager pdm;

    private object GetAndroidProcessId(List<object> param)
    {
        return androidProcessId;
    }
    
    private object GetStatus(List<object> param)
    {
        if (pdm == null)
        {
            return false;
        }
        return pdm.isRunning;
    }
    
    private object InitPerfDataManager(List<object> param)
    {
        if (param != null)
        {
            GameObject ur = GameObject.Find("UI Root");
            pdm = ur.AddComponent<PerfDataManager>();
            pdm.androidProcessId = androidProcessId;
            pdm.info = info;
            pdm.isStart = true;
            return true;
        }
        return false;
    }

    private object StopPerfDataManager(List<object> param)
    {
        if (param != null)
        {
            pdm.isStart = false;
            return true;
        }
        return false;
    }
    
    private Dictionary<string, object> dataDict = new Dictionary<string, object>();

    private object TickDataFromPdm(List<object> param)
    {
        return "TickDataFromPdm";
    }

    private void HandlePdmData()
    {
        if (pdm == null)
        {
            throw new Exception("pdm 是 null，运行tick方法之前需要初始化pdm");
        }

        dataDict["frameRateCur"] = pdm.frameRateCur;
        dataDict["uss"] = pdm.uss;
        dataDict["jankSmall"] = pdm.jankSmall;
        dataDict["jankBig"] = pdm.jankBig;
        dataDict["allocateMemoryForGraphicsDriver"] = pdm.allocateMemoryForGraphicsDriver;
        dataDict["monoHeapSize"] = pdm.monoHeapSize;
        dataDict["monoUsedSize"] = pdm.monoUsedSize;
        dataDict["tempAllocateSize"] = pdm.tempAllocateSize;
        dataDict["totalAllocateMemory"] = pdm.totalAllocateMemory;
        dataDict["totalReservedMemory"] = pdm.totalReservedMemory;
        dataDict["totalUnusedReservedMemory"] = pdm.totalUnusedReservedMemory;
        dataDict["pss"] = pdm.pss;
        dataDict["currentTime"] = pdm.currentTime;
        dataDict["jankSmallTime"] = pdm.jankSmallTime;
        dataDict["jankBigTime"] = pdm.jankBigTime;

    }

    private FieldInfo testStateInfo;
    private object gpmCtl;
    private bool isGPMStarted = false;
    private bool isGPMStarting = false;

    private object InitGPM(System.Collections.Generic.List<object> param )
    {
        if (param != null)
        {
            BindingFlags flag = BindingFlags.NonPublic | BindingFlags.Instance;
            Assembly dll = Assembly.Load("UWAWrapper_Android");
            Type gpmCtlCls = dll.GetType("UwaGpmStarter");

            testStateInfo = gpmCtlCls.GetField("_testState", flag);

            GameObject go = new GameObject();

            go.name = "GPMCTL";
            gpmCtl = go.AddComponent(gpmCtlCls);
            // go.SetActive(false);
            return true;
        }
        return false;
    }

    private object StartGPM(System.Collections.Generic.List<object> param)
    {
        if (param != null)
        {
            if (isGPMStarted)
            {
                throw new Exception("GPM已调用过StartGPM，不能二次启动");
            }
            if (gpmCtl == null || testStateInfo == null)
            {
                throw new System.NullReferenceException("GPM初始化失败，在调用StartGPM前需要调用InitGPM");
            }
            testStateInfo.SetValue(gpmCtl, 1);
            isGPMStarted = true;
            isGPMStarting = true;
            return true;
        }

        return false;
    }
    
    private object StopGPM(System.Collections.Generic.List<object> param)
    {
        if (param != null)
        {
            if (gpmCtl == null || testStateInfo == null)
            {
                throw new System.NullReferenceException("GPM初始化失败，在调用StopGPM前需要调用InitGPM");
            }

            if (!isGPMStarting)
            {
                throw new Exception("GPM未调用过StartGPM，要调用StopGPM，需要先调用StartGPM");
            }
            testStateInfo.SetValue(gpmCtl, 4);
            isGPMStarting = false;
            return true;
        }

        return false;
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

    private static Stopwatch swDump = new Stopwatch();
    [RPC]
    private string Dump(System.Collections.Generic.List<object> param)
    {

        var begin = DateTime.Now.Ticks;
        swDump.Reset();
        swDump.Start();
        
        //Dump数据
		cDumper.Dump();
        //根据Dump结果 生成ResultDict
        cDumper.GenDumpResult();

        cDumper.Clear();

        debugProfilingData["dump"] = swDump.ElapsedMilliseconds;

        var end = DateTime.Now.Ticks;
        
        if (pdm != null)
        {
            pdm.dumpTime += end - begin;
        }

        return "Dump";
    }

    private static Stopwatch swScreenShot = new Stopwatch();
    private Texture2D tex;
    private object[] texReturnValue = new object[2];
    [RPC]
    private IEnumerator Screenshot(TcpClientState client, object idAction)
    {
        yield return new WaitForEndOfFrame();
        object[] snapShotResult = SnapShotAfterRender();
        server.Send(client.TcpClient, prot.pack(rpc.formatResponse(idAction, snapShotResult)));
        TryRemoveClient(client);
    }

    private object[] SnapShotAfterRender()
    {
        tex.Resize( Screen.width, Screen.height );
        tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        tex.Apply(false);
        byte[] fileBytes = tex.EncodeToJPG(10);
        var b64img = Convert.ToBase64String(fileBytes);

        // debugProfilingData["screenshot"] = swScreenShot.ElapsedMilliseconds;
        texReturnValue[0] = b64img;
        texReturnValue[1] = "jpg";
        return texReturnValue;
    }

    private float[] screenSize = new float[2];
    [RPC]
    private object GetScreenSize(System.Collections.Generic.List<object> param)
    {
        screenSize[0] = Screen.width;
        screenSize[1] = Screen.height;
        return screenSize;
    }

    public void stopListening()
    {
        mRunning = false;
        server?.Stop();
    }

    [RPC]
    private object GetDebugProfilingData(System.Collections.Generic.List<object> param)
    {
        return debugProfilingData;
    }

    [RPC]
    private object SetText(System.Collections.Generic.List<object> param)
    {
        var instanceId = Convert.ToInt32(param[0]);
        var textVal = param[1] as string;
        // GameObject[] gos = GameObject.FindObjectsOfType<GameObject>();
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
    private object GetSDKVersion(System.Collections.Generic.List<object> param)
    {
        return versionCode;
    }
    
    [RPC]
    private object DoLogin(System.Collections.Generic.List<object> param)
    {
        if(param != null)
        {
            // todo 已脱敏 此处调用游戏业务登录逻辑
            return true;
        }
        return false;
    }

    [RPC]
    private object DoGM(System.Collections.Generic.List<object> param )
    {

        if ( param != null )
        {
            // todo 已脱敏 此处调用游戏业务GM发送逻辑
            return CallBackKey;
        }
        return false;
    }

    [RPC]
    private object DoLogout(System.Collections.Generic.List<object> param )
    {
        if ( param != null )
        {
            // todo 已脱敏 此处调用游戏业务登出逻辑
            return true;
        }
        return false;
    }

    public static string CallBackKey = "CallBackAsync";
    public static Action<string> CallBack = null;
    private TcpClientState tcs;

    private object ida;
    // private static Stopwatch sw = new Stopwatch();
    // public static List<DelayTaskObj> delayTaskObjs = new List<DelayTaskObj>();

    // JsonSerializerSettings settings = new JsonSerializerSettings()
    // {
    //     StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
    // };

    public ConcurrentQueue<ResultStruct> sQueue = new ConcurrentQueue<ResultStruct>();
    private ResultStruct resultStruct;
    
    // todo: 序列化result 使用memorystream
    private void HandleResult()
    {
        while (true)
        {
            if (!sQueue.TryDequeue(out resultStruct))
            {
                Thread.Sleep(1);
                continue;
            }

            try
            {
                resultStruct.callBack(resultStruct.client, resultStruct.idAction, resultStruct.result);

                TryRemoveClient(resultStruct.client);
                Thread.Sleep(1);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
             
             
             //sQueue.Enqueue(resultStruct); 调试用
        }
    }
    
    string methodName;
    List<object> param;
    public object idAction;
    public object result;

    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //
        //     InitPerfDataManager(new List<object>());
        //
        // }
        // return;
        // if (Input.GetKeyDown(KeyCode.S))
        // {
        //     Debug.LogError("s pressed");
        //     StartGotOnline(new List<object>() {"1"});
        //
        // }
        // if (Input.GetKeyDown(KeyCode.E))
        // {
        //     Debug.LogError("e pressed");
        //     UWAEngine.Stop();
        //
        // }
        //
        // return;
        // if (Input.GetKey(KeyCode.Space))
        //  {
        //      result = rpc.HandleMethod("Dump", null);
        //      Send2Queue(null, idAction, result, HandleDumpResult);
        //      
        //      // result = rpc.HandleMethod("Dump", null);
        //      // Send2Queue(null, idAction, result, HandleDumpResult);
        //      
        //  }
        //  return;
         
         
         foreach (TcpClientState client in inbox.Values) 
         {
            // TcpClientState internalClientToBeThrowAway;
            // string tcpClientKey = client.TcpClient.Client.RemoteEndPoint.ToString();
            if (client.TcpClient.Client.Connected == false)
            {
                continue;
            }
            
            List<string> msgs = client.Prot.swap_msgs(); 
            
            for (int i=0;i<msgs.Count;i++)
            {
                string msg = msgs[i];
                
                Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(msg, rpc.settings);
                if (!data.ContainsKey("method"))
                {
                    // 忽略不带method的消息
                    Debug.Log("ignore message without method");
                    continue;
                }
                
                if (!data.ContainsKey("params"))
                {
                    Debug.Log("ignore message without params");
                    continue;
                }
                
                if (!data.ContainsKey("id"))
                {
                    Debug.Log("ignore message without id");
                    continue;
                }
                
                methodName = data["method"].ToString();
                param = ((JArray)(data["params"])).ToObject<List<object>>();
                idAction = data["id"];
                
                if (methodName.Equals("Screenshot"))
                {
                    StartCoroutine(Screenshot(client, idAction));
                    continue;
                }
                
                result = rpc.HandleMethod(methodName, param);

                if (result is Exception)
                {    
                    var e = result as Exception;
                    server.Send(client.TcpClient, prot.pack(rpc.formatResponseError(idAction, null, e)));
                    TryRemoveClient(client);
                    continue;
                }
                
                if (methodName.Equals("Dump"))
                {
                    Send2Queue(client, idAction, result, HandleDumpResult);
                    continue;
                }
                if (methodName.Equals("TickDataFromPdm"))
                {
                    Send2Queue(client, idAction, result, HandleTickData);
                    continue;
                }
                if (methodName.Equals("gm"))
                {
                    HandleGm(client, idAction, result);
                    // Send2Queue(client, idAction, result, HandleGm);
                    continue;
                }

                tcs = null;
                ida = null;
                CallBack = null;
                server.Send(client.TcpClient, prot.pack(rpc.formatResponse(idAction, result)));

                TryRemoveClient(client);
            } 
         }
         
    }

    private TcpClientState TryRemoveClient(TcpClientState client)
    {
        TcpClientState internalClientToBeThrowAway;
        string tcpClientKey = client.TcpClient.Client.RemoteEndPoint.ToString();
        inbox.TryRemove(tcpClientKey, out internalClientToBeThrowAway);
        return internalClientToBeThrowAway;
    }
    
    public delegate void SubThreadCallBack(TcpClientState client, object idAction, object result);
    
    private void Send2Queue(TcpClientState client, object idAction, object result, SubThreadCallBack callBack)
    {
        ResultStruct struct_;
        struct_.client = client;
        struct_.idAction = idAction;
        struct_.result = result;
        struct_.callBack = callBack;
        sQueue.Enqueue(struct_);
    }

    /// <summary>
    /// 回收各容器
    /// </summary>
    /// <param name="taskObj"></param>
    private void RecyleObjects(Dictionary<string, object> taskObj)
    {
        PayloadDictionaryPool.inst.Release( taskObj["payload"] as  Dictionary<string, object>);
        var list =  taskObj["children"] as System.Collections.Generic.List<object>;
        if (list != null)
        {
            foreach (var item in list)
            {
                var childDict = item as Dictionary<string, object>;
                RecyleObjects(childDict);
            }
            
            ListRelesePool.inst.Release(list);
        }
        
        ResultDictionaryPool.inst.Release(taskObj);
    }

    // bool SetText( GameObject go, string textVal )
    // {
    //     if ( go != null )
    //     {
    //         var inputField = go.GetComponent<UnityEngine.UI.InputField>();
    //         if ( inputField != null )
    //         {
    //             inputField.text = textVal;
    //             return true;
    //         }
    //     }
    //     return false;
    // }

    
    //改成单线程
private void HandleDumpResult(TcpClientState client, object idAction, object result)
    {
        Dictionary<string, object> taskObj;
        cDumper.rQueue.TryDequeue(out taskObj);
        
        server.Send(client.TcpClient, prot.pack(rpc.formatResponse(idAction, taskObj)));
        //Dictionary序列化之后, 回收容器
        RecyleObjects(taskObj);

        // var v = rpc.formatResponse(idAction, taskObj);
        // var a = Encoding.UTF8.GetString(v);
        // Debug.Log(a);
        //
        // cDumper.rQueue.Enqueue(taskObj);
    }

    private void HandleTickData(TcpClientState client, object idAction, object result)
    {
        HandlePdmData();
        server.Send(client.TcpClient, prot.pack(rpc.formatResponse(idAction, dataDict)));
    }

    private void HandleGm(TcpClientState client, object idAction, object result)
    {
        tcs = client;
        ida = idAction;
        CallBack = DoCallBack;
        
    }
    
    void DoCallBack( string res )
    {
        server.Send(tcs.TcpClient, prot.pack(rpc.formatResponse(ida, res)));
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

public struct DelayTaskObj
{
    public TcpClientState client;
    public object idAction;
}

public class RPCParser
{
    public delegate object RpcMethod(System.Collections.Generic.List<object> param);

    protected Dictionary<string, RpcMethod> RPCHandler = new Dictionary<string, RpcMethod>();
    public JsonSerializerSettings settings = new JsonSerializerSettings()
    {
        StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
    };

    private object result;
    
    // 该方法必须在主线程内调用，否则result会出现线程安全问题
    public object HandleMethod(string methodName, List<object> param)
    {
        try
        {
            result = RPCHandler[methodName](param);
        }
        catch (Exception e)
        {
            // return error response
            Debug.Log(e);
            return e;
        }

        return result;
    }

    // Call a method in the server
    public string formatRequest(string method, object idAction, System.Collections.Generic.List<object> param = null)
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

    Dictionary<string, object> rpc = new Dictionary<string, object>();

    // Send a response from a request the server made to this client
    public byte[] formatResponse(object idAction, object result)
    {
        lock (this)
        {
            rpc.Clear();
            rpc["jsonrpc"] = "2.0";
            rpc["id"] = idAction;
            rpc["result"] = result;
        
            return Utf8Json.JsonSerializer.Serialize(rpc);   
        }
    }

    // Send a error to the server from a request it made to this client
    public byte[] formatResponseError(object idAction, IDictionary<string, object> data, Exception e)
    {
        // Dictionary<string, object> rpc = new Dictionary<string, object>();
        lock (this)
        {
            rpc.Clear();
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
            return Utf8Json.JsonSerializer.Serialize(rpc);
        }
    }

    public void addRpcMethod(string name, RpcMethod method)
    {
        RPCHandler[name] = method;
    }
}
