using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;


namespace Poco
{
    public class UnityNode : AbstractNode
    {
        public static Dictionary<string, string> TypeNames = new Dictionary<string, string>() {
            { "UI2DSprite", "UI2DSprite" },
            { "UI2DSpriteAnimation", "UI2DSpriteAnimation" },
            { "UIAnchor", "UIAnchor" },
            { "UIAtlas", "UIAtlas" },
            { "UICamera", "UICamera" },
            { "UIFont", "UIFont" },
            { "UIInput", "UIInput" },
            { "UILabel", "UILabel" },
            { "UILocalize", "UILocalize" },
            { "UIOrthoCamera", "UIOrthoCamera" },
            { "UIPanel", "UIPanel" },
            { "UIRoot", "UIRoot" },
            { "UISprite", "UISprite" },
            { "UISpriteAnimation", "UISpriteAnimation" },
            { "UISpriteData", "UISpriteData" },
            { "UIStretch", "UIStretch" },
            { "UITextList", "UITextList" },
            { "UITexture", "UITexture" },
            { "UITooltip", "UITooltip" },
            { "UIViewport", "UIViewport" },
            { "Camera", "Camera" },
            { "Transform", "Node" },
        };
        public static string DefaultTypeName = "GameObject";
        private GameObject gameObject;
        private Camera camera;
        private Bounds bounds;
        private Rect rect;
        private Vector2 objectPos;
        private List<string> components;

        public UnityNode(GameObject obj)
        {
            gameObject = obj;
            camera = GetCamera();
            bounds = NGUIMath.CalculateAbsoluteWidgetBounds(gameObject.transform);
            rect = BoundsToScreenSpace(bounds);
            objectPos = WorldToGUIPoint(bounds.center);
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
                    return GameObjectVisible(components);
                case "pos":
                    return GameObjectPosInScreen(objectPos);
                case "size":
                    return GameObjectSizeInScreen(rect);
                case "scale":
                    return new List<float>() { 1.0f, 1.0f };
                case "anchorPoint":
                    return GameObjectAnchorInScreen(rect, objectPos);
                case "zOrders":
                    return GameObjectzOrders();
                case "clickable":
                    return GameObjectClickable();
                case "text":
                    return GameObjectText();
                case "components":
                    return components;
                case "texture":
                    return GetImageSourceTexture();
                case "tag":
                    return GameObjectTag();
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

        private Camera GetCamera()
        {
            // it seems that NGUI has it own camera culling mask.
            // so we don't need to test within which camera a game object is visible
            return UICamera.currentCamera != null ? UICamera.currentCamera : UICamera.mainCamera;
        }

        private Dictionary<string, object> GetPayload()
        {
            Dictionary<string, object> payload = new Dictionary<string, object>() {
                { "name", gameObject.name },
                { "type", GuessObjectTypeFromComponentNames (components) },
                { "visible", GameObjectVisible (components) },
                { "pos", GameObjectPosInScreen (objectPos) },
                { "size", GameObjectSizeInScreen (rect) },
                { "scale", new List<float> (){ 1.0f, 1.0f } },
                { "anchorPoint", GameObjectAnchorInScreen (rect, objectPos) },
                { "zOrders", GameObjectzOrders () },
                { "clickable", GameObjectClickable () },
                { "text", GameObjectText () },
                { "components", components },
                { "texture", GetImageSourceTexture () },
                { "tag", GameObjectTag () },
                { "_instanceId", gameObject.GetInstanceID() },
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

        private bool GameObjectVisible(List<string> components)
        {
            if (gameObject.activeInHierarchy)
            {
                bool drawcall = components.Contains("UIDrawCall");
                bool light = components.Contains("Light");
                // bool mesh = components.Contains ("MeshRenderer") && components.Contains ("MeshFilter");
                bool particle = components.Contains("ParticleSystem") && components.Contains("ParticleSystemRenderer");
                return drawcall || light || particle ? false : true;
            }
            else
            {
                return false;
            }
        }

        private bool GameObjectClickable()
        {
            UIButton button = gameObject.GetComponent<UIButton>();
            BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
            return button && button.isEnabled && boxCollider ? true : false;
        }

        private string GameObjectText()
        {
            UILabel text = gameObject.GetComponent<UILabel>();
            return text ? text.text : null;
        }

        private string GameObjectTag()
        {
            string tag;
            try
            {
                tag = !gameObject.CompareTag("Untagged") ? gameObject.tag : null;
            }
            catch (UnityException e)
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

        private float[] GameObjectPosInScreen(Vector2 objectPos)
        {
            float[] pos = { objectPos.x / (float)Screen.width, objectPos.y / (float)Screen.height };
            return pos;
        }

        private float[] GameObjectSizeInScreen(Rect rect)
        {
            float[] size = { rect.width / (float)Screen.width, rect.height / (float)Screen.height };
            return size;
        }

        private float[] GameObjectAnchorInScreen(Rect rect, Vector2 objectPos)
        {
            float[] defaultValue = { 0.5f, 0.5f };
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
            UISprite sprite = gameObject.GetComponent<UISprite>();
            if (sprite != null)
            {
                return sprite.spriteName;
            }

            UITexture texture = gameObject.GetComponent<UITexture>();
            if (texture != null && texture.mainTexture != null)
            {
                return texture.mainTexture.name;
            }

            return null;
        }

        private Rect BoundsToScreenSpace(Bounds bounds)
        {
            Vector3 cen;
            Vector3 ext;
            Renderer renderer = gameObject.GetComponent<Renderer>();
            cen = renderer ? renderer.bounds.center : bounds.center;
            ext = renderer ? renderer.bounds.extents : bounds.extents;
            Vector2[] extentPoints = new Vector2[8] {
                WorldToGUIPoint (new Vector3 (cen.x - ext.x, cen.y - ext.y, cen.z - ext.z)),
                WorldToGUIPoint (new Vector3 (cen.x + ext.x, cen.y - ext.y, cen.z - ext.z)),
                WorldToGUIPoint (new Vector3 (cen.x - ext.x, cen.y - ext.y, cen.z + ext.z)),
                WorldToGUIPoint (new Vector3 (cen.x + ext.x, cen.y - ext.y, cen.z + ext.z)),
                WorldToGUIPoint (new Vector3 (cen.x - ext.x, cen.y + ext.y, cen.z - ext.z)),
                WorldToGUIPoint (new Vector3 (cen.x + ext.x, cen.y + ext.y, cen.z - ext.z)),
                WorldToGUIPoint (new Vector3 (cen.x - ext.x, cen.y + ext.y, cen.z + ext.z)),
                WorldToGUIPoint (new Vector3 (cen.x + ext.x, cen.y + ext.y, cen.z + ext.z))
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

        private Vector2 WorldToGUIPoint(Vector3 world)
        {
            Vector2 screenPoint = Vector2.zero;
            if (camera != null)
            {
                screenPoint = camera.WorldToScreenPoint(world);
                screenPoint.y = (float)Screen.height - screenPoint.y;
            }
            return screenPoint;
        }

        public static bool SetText(GameObject go, string textVal)
        {
            if (go != null)
            {
                var inputField = go.GetComponent<UIInput>();
                if (inputField != null)
                {
                    // 这一行未测试，给输入框设置文本
                    inputField.text = textVal;
                    return true;
                }
            }
            return false;
        }
    }
}
