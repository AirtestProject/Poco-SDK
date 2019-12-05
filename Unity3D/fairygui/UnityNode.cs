using System;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace Poco
{
    public class UnityNode : AbstractNode
    {
        public static string DefaultTypeName = "GameObject";
        private GameObject gameObject;
        private DisplayObject displayObject;
        private GObject gObject;
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

            var info = gameObject.GetComponent<DisplayObjectInfo>();
            if (info != null)
            {
                displayObject = info.displayObject;
                gObject = displayObject.gOwner;
            }

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

            if (gObject != null)
            {
                if (!(gObject is GComponent))
                    return children;

                Container container = ((GComponent)gObject).container;
                int cnt = container.numChildren;
                for (int i = 0; i < cnt; i++)
                    children.Add(new UnityNode(container.GetChildAt(i).gameObject));
            }
            else
            {
                foreach (Transform child in gameObject.transform)
                {
                    children.Add(new UnityNode(child.gameObject));
                }
            }

            return children;
        }

        public override object getAttr(string attrName)
        {
            if (gObject != null)
            {
                switch (attrName)
                {
                    case "name":
                        return string.IsNullOrEmpty(gObject.name) ? gameObject.name : gObject.name;
                    case "type":
                        return gObject.GetType().Name.Substring(1);
                    case "visible":
                        return gObject.onStage && gObject.visible;
                    case "pos":
                        {
                            Vector2 vec2 = gObject.LocalToGlobal(Vector2.zero);
                            return new float[] { vec2.x / (float)Screen.width, vec2.y / (float)Screen.height };
                        }
                    case "size":
                        {
                            Rect rect = gObject.TransformRect(new Rect(0, 0, gObject.width, gObject.height), null);
                            return new float[] { rect.width / (float)Screen.width, rect.height / (float)Screen.height };
                        }
                    case "scale":
                        return new List<float>() { 1.0f, 1.0f };
                    case "anchorPoint":
                        return new float[] { gObject.pivotX, gObject.pivotY };
                    case "zOrders":
                        return GameObjectzOrders();
                    case "clickable":
                        return gObject.touchable;
                    case "text":
                        return gObject.text;
                    case "components":
                        return components;
                    case "texture":
                        return (gObject.displayObject != null && gObject.displayObject.graphics != null && gObject.displayObject.graphics.texture != null && gObject.displayObject.graphics.texture.nativeTexture != null) ? gObject.displayObject.graphics.texture.nativeTexture.name : null;
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
            else
            {
                switch (attrName)
                {
                    case "name":
                        return gameObject.name;
                    case "type":
                        return displayObject != null ? displayObject.GetType().Name : DefaultTypeName;
                    case "visible":
                        return displayObject != null ? (displayObject.stage != null && displayObject.visible) : true;
                    case "pos":
                        return new float[] { 0.0f, 0.0f };
                    case "size":
                        return new float[] { 0.0f, 0.0f };
                    case "scale":
                        return new List<float>() { 1.0f, 1.0f };
                    case "anchorPoint":
                        return new float[] { 0.0f, 0.0f };
                    case "zOrders":
                        return new Dictionary<string, object>() { { "local", 0 }, { "global", 0 } };
                    case "clickable":
                        return false;
                    case "components":
                        return components;
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
        }

        static string[] ATTR_KEYS = new string[] {"name","type","visible","pos","size","scale", "anchorPoint","zOrders",
                "clickable","text","components",  "texture","tag","_ilayer","layer", "_instanceId" };
        public override Dictionary<string, object> enumerateAttrs()
        {
            Dictionary<string, object> ret = new Dictionary<string, object>();
            foreach (string key in ATTR_KEYS)
            {
                object value = getAttr(key);
                if (value != null)
                {
                    ret.Add(key, value);
                }
            }
            return ret;
        }

        private int GameObjectLayer()
        {
            return gameObject.layer;
        }

        private string GameObjectLayerName()
        {
            return LayerMask.LayerToName(gameObject.layer);
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

        public static bool SetText(GameObject go, string textVal)
        {
            if (go != null)
            {
                var info = go.GetComponent<DisplayObjectInfo>();
                if (info != null && info.displayObject.gOwner != null)
                {
                    info.displayObject.gOwner.text = textVal;
                    return true;
                }
            }
            return false;
        }
    }
}
