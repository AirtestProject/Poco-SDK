using System;
using System.Collections.Generic;
#if UNITY_2021_1_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace Poco.ugui
{
#if UNITY_2021_1_OR_NEWER
    public class UnityVisualElement : AbstractNode
    {
        public static Dictionary<UnityVisualElement, int> instanceIDs = new Dictionary<UnityVisualElement, int>();

        private VisualElement visualElement;

        public UnityVisualElement(VisualElement obj)
        {
            visualElement = obj;
            instanceIDs[this] = (int.MaxValue / 2) + instanceIDs.Count;
        }

        public override AbstractNode getParent()
        {
            VisualElement parentObj = visualElement.parent;
            return new UnityVisualElement(parentObj);
        }

        public override List<AbstractNode> getChildren()
        {
            List<AbstractNode> children = new List<AbstractNode>();
            foreach (VisualElement child in visualElement.Children())
            {
                children.Add(new UnityVisualElement(child));
            }
            return children;
        }

        public override object getAttr(string attrName)
        {
            switch (attrName)
            {
                case "name":
                    return visualElement.name;
                case "type":
                    return visualElement.GetType().Name;
                case "visible":
                    return VisualElementVisible();
                case "pos":
                case "anchorPoint":
                    return new ValueTuple<float, float>(visualElement.resolvedStyle.left, visualElement.resolvedStyle.top);
                case "size":
                    return new ValueTuple<float, float>(visualElement.resolvedStyle.width, visualElement.resolvedStyle.height);
                case "zOrders":
                    return GameObjectzOrders();
                case "clickable":
                    return VisualElementClickable();
                case "text":
                    return GetVisualElementText();
                case "texture":
                    return GetImageSourceTexture();
                case "_instanceId":
                    return instanceIDs[this];
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
                { "name", visualElement.name },
                { "type", visualElement.GetType().Name },
                { "visible", VisualElementVisible () },
                { "pos", new ValueTuple<float, float>(visualElement.resolvedStyle.left, visualElement.resolvedStyle.top) },
                { "anchorPoint", new ValueTuple<float, float>(visualElement.resolvedStyle.left, visualElement.resolvedStyle.top) },
                { "size", new ValueTuple<float, float>(visualElement.resolvedStyle.width, visualElement.resolvedStyle.height) },
                { "scale", new List<float> (){ 1.0f, 1.0f } },
                { "zOrders", GameObjectzOrders () },
                { "clickable", VisualElementClickable () },
                { "text", GetVisualElementText () },
                { "texture", GetImageSourceTexture () },
                { "_instanceId", instanceIDs[this] },
            };
            return payload;
        }

        private bool VisualElementVisible()
        {
            return visualElement.resolvedStyle.display != DisplayStyle.None && visualElement.style.visibility != Visibility.Hidden;
        }

        private bool VisualElementClickable()
        {
            return visualElement.pickingMode != PickingMode.Ignore;
        }

        private string GetVisualElementText()
        {
            if (visualElement is TextField textField) return textField.text;
            if (visualElement is TextElement textElement) return textElement.text;
            return null;
        }

        private Dictionary<string, float> GameObjectzOrders()
        {
            int localIndex = 0;
            if (visualElement.parent is not null) localIndex = visualElement.parent.IndexOf(visualElement);
            Dictionary<string, float> zOrders = new Dictionary<string, float>() {
                { "global", 0f },
                { "local", localIndex }
            };
            return zOrders;
        }

        private string GetImageSourceTexture()
        {
            if (visualElement is Image image) return image.image.name;
            if (visualElement.resolvedStyle.backgroundImage.texture) return visualElement.resolvedStyle.backgroundImage.texture.name;
            return null;
        }

        public static bool SetText(UnityVisualElement element, string textVal)
        {
            if (element.visualElement is TextField textField) textField.value = textVal;
            if (element.visualElement is TextElement textElement) textElement.text = textVal;
            return false;
        }
    }
#endif
}