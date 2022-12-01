using System;
using System.Collections.Generic;
using Game.SDKs.PocoSDK;
using UnityEngine.UI;
using UnityEngine;


namespace Poco
{
    public class UnityNode : AbstractNode
    {
        public static string DefaultTypeName = "GameObject";
       
        public NodeParams paramInfo;
        //public GameObject gameObject;
        //private Renderer renderer;
        //private RectTransform rectTransform;
        
        public List<Transform> children = new List<Transform>();
        public Dictionary<string, System.Object> payLoad = new Dictionary<string, object>();
        private Rect rect;
        private Vector2 objectPos;
        private Camera camera;
        private static Camera MainCamera;

        private static Camera[] otherCameras;

        public UnityNode()
        {
            
        }

        /// <summary>
        /// 帧初始化时调用, 记录当前帧的摄像机
        /// </summary>
        public static void FrameInit()
        {
            MainCamera = Camera.main;
            otherCameras = Camera.allCameras;
        }

        /// <summary>
        /// 每个物体调用, 计算当前照射物体的摄像机
        /// </summary>
        public void Added_SetCamera()
        { 
            int len = otherCameras.Length;
            for ( int i = 0; i < len; i++ )
            {
                // skip the main camera
                // we want to use specified camera first then fallback to main camera if no other cameras
                // for further advanced cases, we could test whether the game object is visible within the camera
                /*
                if ( otherCameras[i] == MainCamera )
                {
                    continue;
                }*/
                if ( ( otherCameras[i].cullingMask &  paramInfo.layer ) != 0 )
                {
                    camera = otherCameras[i];
                }
            }
        }

        /// <summary>
        /// 计算区域
        /// </summary>
        /// <param name="param"></param>
        public void Added_CalculateData(NodeParams param)
        {
            rect = GameObjectRect( param.collider, param.rectTransform);
            objectPos = param.collider ? WorldToGUIPoint(camera, param.collider.bounds.center) : Vector2.zero;
        }

        public T GetCustomComponent<T>(int index) where T : Component
        {
            var components = paramInfo.customComponents;
            if (index >= components.Count || index < 0)
            {
                return null;
            }

            return components[index] as T;
        }
        
        private List<string> components = new List<string>();

        /// <summary>
        /// 记录各种属性以及相应的查询方法
        /// 需要注意 该方法分配的内存需要做到线程安全 因为Dump是在主线程 而序列化是在子线程 因此不能为同一片区域
        /// 之后如果为了降低开发复杂度 可以不允许Dump和序列化并行 变为Dump为Dump 序列化为序列化
        /// </summary>
        public static List<(string PropName, Func<UnityNode, object> GetPropFunc) > GetPlayload = new List<(string, Func<UnityNode, object>) >()
        {
            ( "name", (node)=> node.paramInfo.name ),
            ( "type",  (node)=>node.paramInfo.type),  
           // { "type",  (node)=>node.GuessObjectTypeFromComponentNames (node.paramInfo.components) },    //应无必要判断
            ( "visible", (node)=>true ),            //应都可见
            ( "pos", (node)=>node.GameObjectPosInScreen (node.objectPos, node.paramInfo.collider, node.paramInfo.rectTransform, node.rect) ),
            ( "size", (node)=>node.GameObjectSizeInScreen (node.rect, node.paramInfo.rectTransform) ), 
           // { "scale", new List<float> (){ 1.0f, 1.0f } },        //默认值, 则无意义
            ( "anchorPoint", (node)=>node.GameObjectAnchorInScreen (node.paramInfo.collider, node.rect, node.objectPos) ),
            ( "zOrders", (node)=>node.GameObjectzOrders () ),
            ( "clickable", (node)=>node.GameObjectClickable (node.components) ),
            ( "text", (node)=>node.GameObjectText () ),
            ( "components", (node)=>node.components ),
            ( "texture", (node)=>node.GetImageSourceTexture () ),
           // { "tag", (node)=>node.GameObjectTag () },        //应不会判断
            ( "_ilayer", (node)=>node.GameObjectLayer() ),
           // { "layer", (node)=>node.GameObjectLayerName() },    //可以由上一个获取
           // { "_instanceId", (node)=>node.gameObject.GetInstanceID () }    //应不会判断
        };

        /// <summary>
        /// 填充Payload
        /// </summary>
        /// <param name="param"></param>
        /// <param name="dict"></param>
        public void FillPayload( NodeParams param, Dictionary<string, object> dict)
        {
            paramInfo = param;
            Added_SetCamera();
            Added_CalculateData(param);
            
            //填充属性
            foreach (var pair in GetPlayload)
            {
                dict[pair.PropName] = pair.GetPropFunc(this);
            }
        }

        /// <summary>
        /// 获取layer
        /// </summary>
        /// <returns></returns>
        private int GameObjectLayer()
        {
            return paramInfo.layer;
        }

        /// <summary>
        /// 获取Clickable
        /// </summary>
        /// <param name="components"></param>
        /// <returns></returns>
        private bool GameObjectClickable(List<string> components)
        {
            Button button = paramInfo.button;
            return button ? button.isActiveAndEnabled : false;
        }

        /// <summary>
        /// 获取Text
        /// </summary>
        /// <returns></returns>
        private string GameObjectText()
        {
            Text text = paramInfo.text;
            return text ? text.text : null;
        }

        /// <summary>
        /// 获取Z顺序
        /// </summary>
        private Dictionary<string, float> GameObjectzOrders()
        {
            Dictionary<string, float> zOrders = new Dictionary<string, float>();
            float CameraViewportPoint = 0;
            if (camera != null)
            {
                CameraViewportPoint = Math.Abs(camera.WorldToViewportPoint(paramInfo.transform.position).z);
            }

            zOrders["global"] = 0f;
            zOrders["local"] = -1 * CameraViewportPoint;
            
            return zOrders;
        }

        /// <summary>
        /// 获取区域
        /// </summary>
        /// <param name="collider"></param>
        /// <param name="rectTransform"></param>
        /// <returns></returns>
        private Rect GameObjectRect(Collider collider, RectTransform rectTransform)
        {
            Rect rect = new Rect(0, 0, 0, 0);
            if (collider)
            {
                rect = RendererToScreenSpace(camera, collider);
            }
            else if (rectTransform)
            {
                rect = RectTransformToScreenSpace(rectTransform);
            }
            return rect;
        }

        /// <summary>
        /// 获取Pos
        /// </summary>
        /// <param name="objectPos"></param>
        /// <param name="collider"></param>
        /// <param name="rectTransform"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        private float[] GameObjectPosInScreen(Vector3 objectPos, Collider collider, RectTransform rectTransform, Rect rect)
        {
            float[] pos = { 0f, 0f };

            if (collider)
            {
                // 3d object
                pos[0] = objectPos.x / (float)Screen.width;
                pos[1] = objectPos.y / (float)Screen.height;
            }
            else if (rectTransform)
            {
                // ui object (rendered on screen space, other render modes may be different)
                // use center pos for now
                Canvas rootCanvas = GetRootCanvas( paramInfo.gameObject);
                RenderMode renderMode = rootCanvas != null ? rootCanvas.renderMode : new RenderMode();
                switch (renderMode)
                {
                    case RenderMode.ScreenSpaceCamera:
                        //上一个方案经过实际测试发现还有两个问题存在
                        //1.在有Canvas Scaler修改了RootCanvas的Scale的情况下坐标的抓取仍然不对，影响到了ScreenSpaceCameram模式在不同分辨率和屏幕比例下识别的兼容性。
                        //2.RectTransformUtility转的 rectTransform.transform.position本质上得到的是RectTransform.pivot中心轴在屏幕上的坐标，如果pivot不等于(0.5,0.5)，
                        //那么获取到的position就不等于图形的中心点。
                        //试了一晚上，找到了解决办法。

                        //用MainCanvas转一次屏幕坐标
                        Vector2 position = RectTransformUtility.WorldToScreenPoint(rootCanvas.worldCamera, rectTransform.transform.position);
                        //注意: 这里的position其实是Pivot点在Screen上的坐标，并不是图形意义上的中心点,在经过下列玄学公式换算才是真的图形中心在屏幕的位置。
                        //公式内算上了rootCanvas.scaleFactor 缩放因子，经测试至少在Canvas Scaler.Expand模式下，什么分辨率和屏幕比都抓的很准，兼容性很强，其他的有待测试。
                        //由于得出来的坐标是左下角为原点，触控输入是左上角为原点，所以要上下反转一下Poco才能用,所以y坐标用Screen.height减去。
                        
                        //2021/4/21 LBS修改 由于编辑器里会根据pivot进行中心点位置计算, 原poco代码在C#部分又做了一遍, 反而导致对不上, 因此, 只在此处做y坐标变换
                        // position.Set(
                        //     position.x - rectTransform.rect.width * rootCanvas.scaleFactor * (rectTransform.pivot.x - 0.5f),
                        //     Screen.height - (position.y - rectTransform.rect.height * rootCanvas.scaleFactor * (rectTransform.pivot.y - 0.5f))
                        //     );
                        position.y = Screen.height - position.y;
                        
                        pos[0] = position.x / Screen.width;
                        pos[1] = position.y / Screen.height;
                        break;
                    case RenderMode.WorldSpace:
                        Vector2 _pos = RectTransformUtility.WorldToScreenPoint(rootCanvas.worldCamera, rectTransform.transform.position);
                        pos[0] = _pos.x / Screen.width;
                        pos[1] = (Screen.height - _pos.y) / Screen.height;
                        break;
                    default:
                        pos[0] = rect.center.x / (float)Screen.width;
                        pos[1] = rect.center.y / (float)Screen.height;
                        break;
                }
            }
            return pos;
        }

        /// <summary>
        /// 获取RootCanvas
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        private Canvas GetRootCanvas(GameObject gameObject)
        {
            return paramInfo.rootCanvas;
            Canvas canvas = gameObject.GetComponentInParent<Canvas>();
            // 如果unity版本小于unity5.5，就用递归的方式取吧，没法直接取rootCanvas
            // 如果有用到4.6以下版本的话就自己手动在这里添加条件吧
#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4
			if (canvas && canvas.isRootCanvas) {
				return canvas;
			} else {
				if (gameObject.transform.parent.gameObject != null) {
					return GetRootCanvas(gameObject.transform.parent.gameObject);
				} else {
					return null;
				}
			}
#else
            if (canvas && canvas.isRootCanvas)
            {
                return canvas;
            }
            else if (canvas)
            {
                return canvas.rootCanvas;
            }
            else
            {
                return null;
            }
#endif
        }
        
        
        /// <summary>
        /// 计算Size
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="rectTransform"></param>
        /// <returns></returns>
        private float[] GameObjectSizeInScreen(Rect rect, RectTransform rectTransform)
        {
            float[] size = {0f, 1f};
            
            if (rectTransform)
            {
                Canvas rootCanvas = GetRootCanvas(paramInfo.gameObject);
                RenderMode renderMode = rootCanvas != null ? rootCanvas.renderMode : new RenderMode();
                switch (renderMode)
                {
                    case RenderMode.ScreenSpaceCamera:
                        Rect _rect = RectTransformUtility.PixelAdjustRect(rectTransform, rootCanvas);
                        size[0] = _rect.width * rootCanvas.scaleFactor / (float) Screen.width;
                        size[1] = _rect.height * rootCanvas.scaleFactor / (float) Screen.height;
                        break;
                    case RenderMode.WorldSpace:
                        Rect rect_ = rectTransform.rect;
                        RectTransform canvasTransform = paramInfo.rootCanvasRectTransform;
                        size[0] = rect_.width / canvasTransform.rect.width;
                        size[1] = rect_.height / canvasTransform.rect.height;
                        break;
                    default:
                        size[0] = rect.width / (float) Screen.width;
                        size[1] = rect.height / (float)Screen.height;
                        break;
                }
            }
            else
            {
                size[0] = rect.width / (float) Screen.width;
                size[1] = rect.height / (float)Screen.height;
            }
            return size;
        }

        /// <summary>
        /// 计算Anchor
        /// </summary>
        /// <param name="collider"></param>
        /// <param name="rect"></param>
        /// <param name="objectPos"></param>
        /// <returns></returns>
        private float[] GameObjectAnchorInScreen(Collider collider, Rect rect, Vector3 objectPos)
        {
            float[] defaultValue = { 0.5f, 0.5f };
            if ( paramInfo.rectTransform)
            {
                Vector2 data = paramInfo.rectTransform.pivot;
                defaultValue[0] = data[0];
                defaultValue[1] = 1 - data[1];
                return defaultValue;
            }
            if (!collider)
            {
                //<Modified> some object do not have renderer
                return defaultValue;
            }
            float[] anchor = { (objectPos.x - rect.xMin) / rect.width, (objectPos.y - rect.yMin) / rect.height };
            if (Double.IsNaN(anchor[0]) || Double.IsNaN(anchor[1]))
            {
                return defaultValue;
            }
            else if (Double.IsPositiveInfinity(anchor[0]) || Double.IsPositiveInfinity(anchor[1]))
            {
                return defaultValue;
            }
            else if (Double.IsNegativeInfinity(anchor[0]) || Double.IsNegativeInfinity(anchor[1]))
            {
                return defaultValue;
            }
            else
            {
                return anchor;
            }
        }

        /// <summary>
        /// 计算Image
        /// </summary>
        /// <returns></returns>
        private string GetImageSourceTexture()
        {
            Image image = paramInfo.image;
            if (image != null && image.sprite != null)
            {
                return image.sprite.name;
            }

            RawImage rawImage = paramInfo.rawImage;
            if (rawImage != null && rawImage.texture != null)
            {
                return rawImage.texture.name;
            }

            SpriteRenderer spriteRenderer = paramInfo.spriteRenderer;
            if (spriteRenderer != null) 
            {
                var spr = spriteRenderer.sprite;
                return spr == null? null : spr.name;
            }

            // Renderer render = gameObject.GetComponent<Renderer>();
            // if (collider != null && collider.material != null && render.material.HasProperty("_Color"))
            // {
            //     return collider.material.color.ToString();
            // }

            return null;
        }

        /// <summary>
        /// 计算坐标
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="world"></param>
        /// <returns></returns>
        protected static Vector2 WorldToGUIPoint(Camera camera, Vector3 world)
        {
            Vector2 screenPoint = Vector2.zero;
            if (camera != null)
            {
                screenPoint = camera.WorldToScreenPoint(world);
                screenPoint.y = (float)Screen.height - screenPoint.y;
            }
            return screenPoint;
        }

        /// <summary>
        /// 计算大小
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="collider"></param>
        /// <returns></returns>
        protected static Rect RendererToScreenSpace(Camera camera, Collider collider)
        {
            Vector3 cen = collider.bounds.center;
            Vector3 ext = collider.bounds.extents;
            Vector2[] extentPoints = new Vector2[8] {
                WorldToGUIPoint (camera, new Vector3 (cen.x - ext.x, cen.y - ext.y, cen.z - ext.z)),
                WorldToGUIPoint (camera, new Vector3 (cen.x + ext.x, cen.y - ext.y, cen.z - ext.z)),
                WorldToGUIPoint (camera, new Vector3 (cen.x - ext.x, cen.y - ext.y, cen.z + ext.z)),
                WorldToGUIPoint (camera, new Vector3 (cen.x + ext.x, cen.y - ext.y, cen.z + ext.z)),
                WorldToGUIPoint (camera, new Vector3 (cen.x - ext.x, cen.y + ext.y, cen.z - ext.z)),
                WorldToGUIPoint (camera, new Vector3 (cen.x + ext.x, cen.y + ext.y, cen.z - ext.z)),
                WorldToGUIPoint (camera, new Vector3 (cen.x - ext.x, cen.y + ext.y, cen.z + ext.z)),
                WorldToGUIPoint (camera, new Vector3 (cen.x + ext.x, cen.y + ext.y, cen.z + ext.z))
            };
            Vector2 min = extentPoints[0];
            Vector2 max = extentPoints[0];
            foreach (Vector2 v in extentPoints)
            {
                min = Vector2.Min(min, v);
                max = Vector2.Max(max, v);
            }
            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }

        /// <summary>
        /// 计算Rect
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <returns></returns>
        protected static Rect RectTransformToScreenSpace(RectTransform rectTransform)
        {
            Vector2 size = Vector2.Scale(rectTransform.rect.size, rectTransform.lossyScale);
            Rect rect = new Rect(rectTransform.position.x, Screen.height - rectTransform.position.y, size.x, size.y);
            rect.x -= (rectTransform.pivot.x * size.x);
            rect.y -= ((1.0f - rectTransform.pivot.y) * size.y);
            return rect;
        }

        /// <summary>
        /// 设置Text
        /// </summary>
        /// <param name="go"></param>
        /// <param name="textVal"></param>
        /// <returns></returns>
        public static bool SetText(GameObject go, string textVal)
        {
            if (go != null)
            {
                var inputField = go.GetComponent<InputField>();
                if (inputField != null)
                {
                    inputField.text = textVal;
                    return true;
                }
            }
            return false;
        }
    }
    
    public static class TypeNeedField
    {
        public static List<string> btnField = new List<string>() 
        {
            "name", 
            "type",
            "pos",
            "visible",
            "size",
            "scale",
            "anchorPoint",
            "zOrders",
            "text"
        };
        
        public static List<string> textField = new List<string>() 
        {
            "name", 
            "type",
            "pos",
            "visible",
            "size",
            "scale",
            "anchorPoint",
            "zOrders",
            "text"
        };
        
        public static List<string> colliderField = new List<string>() 
        {
            "name", 
            "type",
            "pos",
            "visible",
            "size",
            "scale",
            "anchorPoint",
            "zOrders",
            "text"
        };
        
        public static List<string> defaultField = new List<string>() 
        {
            "name", 
            "type",
            "pos",
            "visible",
            "size",
            "scale",
            "anchorPoint",
            "zOrders",
            "text"
        };
        
    }
}
