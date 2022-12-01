using System.Collections.Generic;
using Poco;
using UnityEngine;
using UnityEngine.UI;

namespace Game.SDKs.PocoSDK
{
    /// <summary>
    /// 节点的参数, 记录了节点所需的各项属性
    /// </summary>
    public class NodeParams
    {
        public Transform transform;
        public string name;
        public GameObject gameObject;
        public Image image;
        public Text text;
        public Collider collider;
        public Button button;
        public RawImage rawImage;
        public RectTransform rectTransform;
        public SpriteRenderer spriteRenderer;
        public int layer;
        public Canvas canvas;
        public Canvas rootCanvas;
        public RectTransform rootCanvasRectTransform;
        public string type = "GameObject";
        
        //由项目指定的Components
        public List<Component> customComponents = new List<Component>();

        public void clear()
        {
            transform = null;
            name = "";
            gameObject = null;
            image = null;
            text = null;
            collider = null;
            button = null;
            rawImage = null;
            rectTransform = null;
            spriteRenderer = null;
            layer = 0;
            canvas = null;
            rootCanvas = null;
            rootCanvasRectTransform = null;
            customComponents.Clear();
            type = "GameObject";
        }

       
    }

    /// <summary>
    /// 节点信息, 记录了节点的层级关系和NodeParams
    /// </summary>
    public class NodeInfo : PocoPoolable
    {
        static UnityNode node = new UnityNode();
        
        public NodeParams param = new NodeParams();
        
        public Transform parent = null;
        public HashSet<NodeInfo> childTransforms = new HashSet<NodeInfo>();
        
        public void Clear()
        {
            parent = null;
            childTransforms.Clear();
            param.clear();
        }

        /// <summary>
        /// 递归填充结果并返回
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> FillInfo()
        {
            Dictionary<string, object> resultInfo = ResultDictionaryPool.inst.Alloc();
            Dictionary<string, object> payLoadInfo = PayloadDictionaryPool.inst.Alloc();
            var childrenInfoList = ListRelesePool.inst.Alloc(childTransforms.Count);
            
            node.FillPayload(param,  payLoadInfo);
            foreach (var childInfo in childTransforms)
            {
                childrenInfoList.Add(childInfo.FillInfo());
            }

            resultInfo["name"] = param.name;
            resultInfo["payload"] = payLoadInfo;
            resultInfo["children"] = childrenInfoList;

            return resultInfo;
        }
    }
}