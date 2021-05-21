using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;


namespace Poco
{
    public class UnityNode : AbstractNode
    {
        public static Dictionary<string, string> TypeNames = new Dictionary<string, string>() {
            { "Text", "Text" },
            { "Gradient Text", "Gradient.Text" },
            { "Image", "Image" },
            { "RawImage", "Raw.Image" },
            { "Mask", "Mask" },
            { "2DRectMask", "2D-Rect.Mask" },
            { "Button", "Button" },
            { "InputField", "InputField" },
            { "Toggle", "Toggle" },
            { "Toggle Group", "ToggleGroup" },
            { "Slider", "Slider" },
            { "ScrollBar", "ScrollBar" },
            { "DropDown", "DropDown" },
            { "ScrollRect", "ScrollRect" },
            { "Selectable", "Selectable" },
            { "Camera", "Camera" },
            { "RectTransform", "Node" },
            { "TextMeshProUGUI","TMPROUGUI" },
            { "TMP_Text","TMPRO" },
        };
        public static string DefaultTypeName = "GameObject";
        private GameObject gameObject;
        private Renderer renderer;
        private RectTransform rectTransform;
        private Rect rect;
        private Vector2 objectPos;
        private List<string> components;
        private Camera camera;


        public UnityNode(GameObject obj)
        {
            gameObject = obj;
            camera = Camera.main;
            foreach (var cam in Camera.allCameras)
            {
                // skip the main camera
                // we want to use specified camera first then fallback to main camera if no other cameras
                // for further advanced cases, we could test whether the game object is visible within the camera
                if (cam == Camera.main)
                {
                    continue;
                }
                if ((cam.cullingMask & (1 << gameObject.layer)) != 0)
                {
                    camera = cam;
                }
            }

            renderer = gameObject.GetComponent<Renderer>();
            rectTransform = gameObject.GetComponent<RectTransform>();
            rect = GameObjectRect(renderer, rectTransform);
            objectPos = renderer ? WorldToGUIPoint(camera, renderer.bounds.center) : Vector2.zero;
            components = GameObjectAllComponents();
        }

        public override AbstractNode getParent()
        {
            GameObject parentObj = gameObject.transform.parent.gameObject;
            return new UnityNode(parentObj);
        }

        public override List<AbstractNode> getChildren()
        {
            List<AbstractNode> children = new List<AbstractNode>();
            foreach (Transform child in gameObject.transform)
            {
                children.Add(new UnityNode(child.gameObject));
            }
            return children;
        }

        public override object getAttr(string attrName)
        {
            switch (attrName)
            {
                case "name":
                    return gameObject.name;
                case "type":
                    return GuessObjectTypeFromComponentNames(components);
                case "visible":
                    return GameObjectVisible(renderer, components);
                case "pos":
                    return GameObjectPosInScreen(objectPos, renderer, rectTransform, rect);
                case "size":
                    return GameObjectSizeInScreen(rect, rectTransform);
                case "scale":
                    return new List<float>() { 1.0f, 1.0f };
                case "anchorPoint":
                    return GameObjectAnchorInScreen(renderer, rect, objectPos);
                case "zOrders":
                    return GameObjectzOrders();
                case "clickable":
                    return GameObjectClickable(components);
                case "text":
                    return GameObjectText();
                case "components":
                    return components;
                case "texture":
                    return GetImageSourceTexture();
                case "tag":
                    return GameObjectTag();
                case "layer":
                    return GameObjectLayerName();
                case "_ilayer":
                    return GameObjectLayer();
                case "_instanceId":
                    return gameObject.GetInstanceID();
                default:
                    return null;
            }
        }


        public override Dictionary<string, object> enumerateAttrs()
        {
            Dictionary<string, object> payload = GetPayload();
            Dictionary<string, object> ret = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> p in payload)
            {
                if (p.Value != null)
                {
                    ret.Add(p.Key, p.Value);
                }
            }
            return ret;
        }


        private Dictionary<string, object> GetPayload()
        {
            Dictionary<string, object> payload = new Dictionary<string, object>() {
                { "name", gameObject.name },
                { "type", GuessObjectTypeFromComponentNames (components) },
                { "visible", GameObjectVisible (renderer, components) },
                { "pos", GameObjectPosInScreen (objectPos, renderer, rectTransform, rect) },
                { "size", GameObjectSizeInScreen (rect, rectTransform) },
                { "scale", new List<float> (){ 1.0f, 1.0f } },
                { "anchorPoint", GameObjectAnchorInScreen (renderer, rect, objectPos) },
                { "zOrders", GameObjectzOrders () },
                { "clickable", GameObjectClickable (components) },
                { "text", GameObjectText () },
                { "components", components },
                { "texture", GetImageSourceTexture () },
                { "tag", GameObjectTag () },
                { "_ilayer", GameObjectLayer() },
                { "layer", GameObjectLayerName() },
                { "_instanceId", gameObject.GetInstanceID () },
            };
            return payload;
        }

        private string GuessObjectTypeFromComponentNames(List<string> components)
        {
            List<string> cns = new List<string>(components);
            cns.Reverse();
            foreach (string name in cns)
            {
                if (TypeNames.ContainsKey(name))
                {
                    return TypeNames[name];
                }
            }
            return DefaultTypeName;
        }

        private bool GameObjectVisible(Renderer renderer, List<string> components)
        {
            if (gameObject.activeInHierarchy)
            {
                bool light = components.Contains("Light");
                // bool mesh = components.Contains ("MeshRenderer") && components.Contains ("MeshFilter");
                bool particle = components.Contains("ParticleSystem") && components.Contains("ParticleSystemRenderer");
                if (light || particle)
                {
                    return false;
                }
                else
                {
                    return renderer ? renderer.isVisible : true;
                }
            }
            else
            {
                return false;
            }
        }

        private int GameObjectLayer()
        {
            return gameObject.layer;
        }
        private string GameObjectLayerName()
        {
            return LayerMask.LayerToName(gameObject.layer);
        }

        private bool GameObjectClickable(List<string> components)
        {
            Button button = gameObject.GetComponent<Button>();
            return button ? button.isActiveAndEnabled : false;
        }

        private string GameObjectText()
        {
            TMP_Text tmpText = gameObject.GetComponent<TMP_Text>();
            if (tmpText)
            {
                return tmpText.GetParsedText();
            }
            TextMeshProUGUI tmpUIText = gameObject.GetComponent<TextMeshProUGUI>();
            if (tmpUIText)
            {
                return tmpUIText.GetParsedText();
            }
            Text text = gameObject.GetComponent<Text>();
            return text ? text.text : null;
        }

        private string GameObjectTag()
        {
            string tag;
            try
            {
                tag = !gameObject.CompareTag("Untagged") ? gameObject.tag : null;
            }
            catch (UnityException)
            {
                tag = null;
            }
            return tag;
        }

        private List<string> GameObjectAllComponents()
        {
            List<string> components = new List<string>();
            Component[] allComponents = gameObject.GetComponents<Component>();
            if (allComponents != null)
            {
                foreach (Component ac in allComponents)
                {
                    if (ac != null)
                    {
                        components.Add(ac.GetType().Name);
                    }
                }
            }
            return components;
        }

        private Dictionary<string, float> GameObjectzOrders()
        {
            float CameraViewportPoint = 0;
            if (camera != null)
            {
                CameraViewportPoint = Math.Abs(camera.WorldToViewportPoint(gameObject.transform.position).z);
            }
            Dictionary<string, float> zOrders = new Dictionary<string, float>() {
                { "global", 0f },
                { "local", -1 * CameraViewportPoint }
            };
            return zOrders;
        }

        private Rect GameObjectRect(Renderer renderer, RectTransform rectTransform)
        {
            Rect rect = new Rect(0, 0, 0, 0);
            if (renderer)
            {
                rect = RendererToScreenSpace(camera, renderer);
            }
            else if (rectTransform)
            {
                rect = RectTransformToScreenSpace(rectTransform);
            }
            return rect;
        }

        private float[] GameObjectPosInScreen(Vector3 objectPos, Renderer renderer, RectTransform rectTransform, Rect rect)
        {
            float[] pos = { 0f, 0f };

            if (renderer)
            {
                // 3d object
                pos[0] = objectPos.x / (float)Screen.width;
                pos[1] = objectPos.y / (float)Screen.height;
            }
            else if (rectTransform)
            {
                // ui object (rendered on screen space, other render modes may be different)
                // use center pos for now
                Canvas rootCanvas = GetRootCanvas(gameObject);
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
                        position.Set(
                            position.x - rectTransform.rect.width * rootCanvas.scaleFactor * (rectTransform.pivot.x - 0.5f),
                            Screen.height - (position.y - rectTransform.rect.height * rootCanvas.scaleFactor * (rectTransform.pivot.y - 0.5f))
                            );
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

        private Canvas GetRootCanvas(GameObject gameObject)
        {
            Canvas canvas = gameObject.GetComponentInParent<Canvas>();
            // 如果unity版本小于unity5.5，就用递归的方式取吧，没法直接取rootCanvas
            // 如果有用到4.6以下版本的话就自己手动在这里添加条件吧
#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4
			if (canvas && canvas.isRootCanvas)
			{
				return canvas;
			}
			else
			{
				if (gameObject.transform.parent.gameObject != null)
				{
					return GetRootCanvas(gameObject.transform.parent.gameObject);
				}
				else
				{
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

        private float[] GameObjectSizeInScreen(Rect rect, RectTransform rectTransform)
        {
            float[] size = { 0f, 0f };
            if (rectTransform)
            {
                Canvas rootCanvas = GetRootCanvas(gameObject);
                RenderMode renderMode = rootCanvas != null ? rootCanvas.renderMode : new RenderMode();
                switch (renderMode)
                {
                    case RenderMode.ScreenSpaceCamera:
                        Rect _rect = RectTransformUtility.PixelAdjustRect(rectTransform, rootCanvas);
                        size = new float[] {
                            _rect.width * rootCanvas.scaleFactor / (float)Screen.width,
                            _rect.height * rootCanvas.scaleFactor / (float)Screen.height
                        };
                        break;
                    case RenderMode.WorldSpace:
                        Rect rect_ = rectTransform.rect;
                        RectTransform canvasTransform = rootCanvas.GetComponent<RectTransform>();
                        size = new float[] { rect_.width / canvasTransform.rect.width, rect_.height / canvasTransform.rect.height };
                        break;
                    default:
                        size = new float[] { rect.width / (float)Screen.width, rect.height / (float)Screen.height };
                        break;
                }
            }
            else
            {
                size = new float[] { rect.width / (float)Screen.width, rect.height / (float)Screen.height };
            }
            return size;
        }

        private float[] GameObjectAnchorInScreen(Renderer renderer, Rect rect, Vector3 objectPos)
        {
            float[] defaultValue = { 0.5f, 0.5f };
            if (rectTransform)
            {
                Vector2 data = rectTransform.pivot;
                defaultValue[0] = data[0];
                defaultValue[1] = 1 - data[1];
                return defaultValue;
            }
            if (!renderer)
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

        private string GetImageSourceTexture()
        {
            Image image = gameObject.GetComponent<Image>();
            if (image != null && image.sprite != null)
            {
                return image.sprite.name;
            }

            RawImage rawImage = gameObject.GetComponent<RawImage>();
            if (rawImage != null && rawImage.texture != null)
            {
                return rawImage.texture.name;
            }

            SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                return spriteRenderer.sprite.name;
            }

            Renderer render = gameObject.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                return renderer.material.color.ToString();
            }

            return null;
        }

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

        protected static Rect RendererToScreenSpace(Camera camera, Renderer renderer)
        {
            Vector3 cen = renderer.bounds.center;
            Vector3 ext = renderer.bounds.extents;
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

        protected static Rect RectTransformToScreenSpace(RectTransform rectTransform)
        {
            Vector2 size = Vector2.Scale(rectTransform.rect.size, rectTransform.lossyScale);
            Rect rect = new Rect(rectTransform.position.x, Screen.height - rectTransform.position.y, size.x, size.y);
            rect.x -= (rectTransform.pivot.x * size.x);
            rect.y -= ((1.0f - rectTransform.pivot.y) * size.y);
            return rect;
        }

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
}
