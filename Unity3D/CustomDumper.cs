using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Game.SDKs.PocoSDK;
using Poco;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

 public class CustomDumper 
{
    /// <summary>
    /// Transform树
    /// </summary>
    public Dictionary<Transform, NodeInfo> trmTree = new Dictionary<Transform, NodeInfo>();
    public List<NodeInfo> list = new List<NodeInfo>();
    private ConcurrentQueue<Dictionary<Transform, NodeInfo>> tQueue;
    public ConcurrentQueue<Dictionary<string, System.Object>> rQueue;
    public HashSet<NodeParams> canvasList = new HashSet<NodeParams>();
    private List<Type> customTypes = new List<Type>();
    
    public void RegisteCustomHandler<T>( string dumpName, Func<UnityNode, object> act, string hiearchyName = "") where T: Component
    {
        customTypes.Add(typeof(T) );
        UnityNode.GetPlayload.Add( (dumpName, act) );
        InitNameDict<T>(hiearchyName);
    }

    private bool OnHandleCustomType<T>(T t, NodeParams nodeParams) where T : Component
    {
        nodeParams.customComponents.Add(t);
        return true;
    }

    public CustomDumper()
    {
        // tQueue = new ConcurrentQueue<Dictionary<Transform, NodeInfo>>();
        rQueue = new ConcurrentQueue<Dictionary<string, object>>();
        
        InitNameDict<Collider>();
        InitNameDict<Text>();
        InitNameDict<Image>();
        InitNameDict<Button>();
        InitNameDict<RawImage>("Raw.Image");
        InitNameDict<SpriteRenderer>();
        InitNameDict<Canvas>();
    }
    
    private string GetName(Type t)
    {
        return nameDict[t];
    }

    void InitNameDict<T>(string cutomName = "")
    {
        nameDict.Add(typeof(T), cutomName == "" ? typeof(T).Name: cutomName);
    }
    
    private static Dictionary<Type, string> nameDict = new Dictionary<Type, string>();


    private bool handleImage(Image arg1, NodeParams arg2)
    {
        arg2.image = arg1;
        return true;
    }

    private bool handleText(Text arg1, NodeParams arg2)
    {
        arg2.text = arg1;
        return true;
    }

    private bool handleCollider(Collider arg, NodeParams trm)
    {
        /*
        if (arg.isTrigger == false)
        {
            return false;
        }
        */

        trm.collider = arg;
        return true;
    }
    
    private bool handleButton(Button arg, NodeParams trm)
    {
        trm.button = arg;
        return true;
    }
    
    private bool handleRawImage(RawImage arg, NodeParams trm)
    {
        trm.rawImage = arg;
        return true;
    }
    

    public void Dump()
    {
        HandleType<Button>(handleButton);
        HandleType<Text>(handleText);
        HandleType<Image>(handleImage);
        HandleType<RawImage>(handleRawImage);
        
        HandleType<SpriteRenderer>(handleSpriteRenderer);
        HandleType<Canvas>(handleCanvas);
        
        HandleType<Collider>(handleCollider);

        foreach (var type in customTypes)
        {
            HandleCustomType(type);
        }

        foreach (var param in canvasList)
        {
            var canvasInfo = trmTree[param.transform];
            var curTrm = canvasInfo.parent;
            
            bool isRootCanvas = true;
            while (curTrm != null)
            {
                var info = trmTree[curTrm];
                if (info.param.canvas != null)
                {
                    isRootCanvas = false;
                    break;
                }

                curTrm = info.parent;
            }

            if (isRootCanvas == false)
            {
                continue;
            }

            SetRootCanvas(canvasInfo, param.canvas, param.rectTransform);
        }
        
        //tQueue.Enqueue(new Dictionary<Transform, NodeInfo>(trmTree));

    }

    private void SetRootCanvas(NodeInfo trm, Canvas paramCanvas, RectTransform rectTransform)
    {
        trm.param.rootCanvas = paramCanvas;
        trm.param.rootCanvasRectTransform = rectTransform;

        foreach (var child in trm.childTransforms)
        {
            SetRootCanvas(child, paramCanvas, rectTransform);
        }
    }

    private bool handleCanvas(Canvas arg1, NodeParams arg2)
    {
        arg2.canvas = arg1;
        canvasList.Add(arg2);
        return true;
    }

    private bool handleSpriteRenderer(SpriteRenderer arg1, NodeParams arg2)
    {
        arg2.spriteRenderer = arg1;
        return true;
    }

    // thread call
    public void GenDumpResult()
    {
        // while (true)
        // {
        //     if (tQueue.IsEmpty)
        //     {
        //         System.Threading.Thread.Sleep(100);
        //         continue;
        //     }
        // tQueue.TryDequeue(out trmTree);

        foreach (var pair in trmTree)
        {
            // 拿到第二层的所有节点
            if (pair.Value.parent == null)
            {
                list.Add(pair.Value);
            }
        }

        list.Sort((a, b) => a.param.name.CompareTo(b.param.name));

        //每此Dump初始化一次
        UnityNode.FrameInit();

        Dictionary<string, System.Object> dict = ResultDictionaryPool.inst.Alloc();
        var childrenList = ListRelesePool.inst.Alloc(list.Count);
        dict["name"] = "<root>";
        var rootDict = RootNode.GetDict();
        dict["payload"] = rootDict;

        foreach (var trmIn2ndFloor in list)
        {
            var NodeInfo = trmTree[trmIn2ndFloor.param.transform];
            childrenList.Add(NodeInfo.FillInfo());
        }

        dict["children"] = childrenList;
        rQueue.Enqueue(dict);
    }

    void HandleCustomType(Type t)
    {
        var ts = Object.FindObjectsOfType(t) as Component[];
        HandleTypeImp(ts, OnHandleCustomType);
    }
    
    void HandleType<T>(Func< T, NodeParams, bool> func ) where T: Component
    {
        var ts = Object.FindObjectsOfType<T>();
        HandleTypeImp(ts, func);
    }

    void HandleTypeImp<T>(T[] ts, Func<T, NodeParams, bool> func) where T : Component
    {
        foreach (var t in ts)
       {
           Transform curVisitTrm = t.transform;
           NodeInfo currVisitNode = null;
           if (!trmTree.TryGetValue(curVisitTrm, out currVisitNode))
           {
               currVisitNode = PocoPool<NodeInfo>.inst.Alloc();
               currVisitNode.param.type = GetName(typeof(T));
           }
           
           bool result = func(t, currVisitNode.param);
           if (result == false)
           {
               continue;
           }

           NodeInfo lastVisitNode = null;

           do
           {
               NodeParams nodeParams = currVisitNode.param;

               nodeParams.transform = curVisitTrm;
               var rectTransform = curVisitTrm as RectTransform;
               if (rectTransform != null)
               {
                   nodeParams.rectTransform = rectTransform;
               }

               nodeParams.name = curVisitTrm.name;
               nodeParams.gameObject = curVisitTrm.gameObject;
               nodeParams.layer = nodeParams.gameObject.layer;

               currVisitNode.parent = curVisitTrm.parent;

               
               if (lastVisitNode != null)
               {
                   currVisitNode.childTransforms.Add(lastVisitNode);
               }

               trmTree[curVisitTrm] = currVisitNode;
               lastVisitNode = currVisitNode;

               curVisitTrm = currVisitNode.parent;

               
               if (curVisitTrm == null)
               {
                   //证明遍历到了最上层， 退出
                   currVisitNode = null;
               }
               else
               {
                   if (trmTree.ContainsKey(curVisitTrm))
                   {
                       //证明父亲之前已经被创建了
                       //挂载父子关系即可， 并退出循环
                        trmTree[curVisitTrm].childTransforms.Add(lastVisitNode);
                        currVisitNode = null;
                   }
                   else
                   {
                       //证明父亲不存在 继续循环
                       currVisitNode = PocoPool<NodeInfo>.inst.Alloc();
                   }
               }
           } while (currVisitNode!=null);
    
       }
    }


    public void Clear()
    {
        foreach (var pair in trmTree)
        {
            PocoPool<NodeInfo>.inst.Release(pair.Value);
        }
        trmTree.Clear();
        list.Clear();
        canvasList.Clear();
    }
}
