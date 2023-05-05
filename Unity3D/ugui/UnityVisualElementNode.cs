using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_2021_1_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace Poco.ugui
{
#if UNITY_2021_1_OR_NEWER
    public class UnityVisualElement : AbstractNode
    {
        public static Dictionary<Type, string> TypeNames = new Dictionary<Type, string>() {
            { typeof(TextElement), "Text" },
            { typeof(Image), "Image" },
            { typeof(Button), "Button" },
            { typeof(TextField), "InputField" },
            { typeof(Toggle), "Toggle" },
            { typeof(Slider), "Slider" },
            { typeof(Scroller), "ScrollBar" },
            { typeof(ScrollView), "ScrollRect" }
        };
        public static string DefaultTypeName = "Node";

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

        VisualElement Root(VisualElement element)
        {
            if (element.parent is not null) return Root(element.parent);
            return element;
        }
        Vector2 GetPos(VisualElement element)
        {
            VisualElement root = Root(element);
            return new Vector2(element.worldBound.x / root.worldBound.width, element.worldBound.y / root.worldBound.height);
        }
        Vector2 GetSize(VisualElement element)
        {
            VisualElement root = Root(element);
            return new Vector2(element.worldBound.width / root.worldBound.width, element.worldBound.height / root.worldBound.height);
        }

        public override object getAttr(string attrName)
        {
            switch (attrName)
            {
                case "name":
                    return string.IsNullOrEmpty(visualElement.name) ? visualElement.ToString() : visualElement.name;
                case "type":
                    return GuessObjectTypeFromComponentNames(visualElement.GetType());
                case "classList":
                    return visualElement.GetClasses();
                case "visible":
                    return VisualElementVisible();
                case "pos":
                    Vector2 pos = GetPos(visualElement);
                    return new List<float>() { pos.x, pos.y };
                case "size":
                    Vector2 size = GetSize(visualElement);
                    return new List<float>() { size.x, size.y };
                case "scale":
                    return new List<float>() { 1.0f, 1.0f };
                case "anchorPoint":
                    return new List<float>() { 0.0f, 0.0f };
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
            Vector2 pos = GetPos(visualElement);
            Vector2 size = GetSize(visualElement);
            Dictionary<string, object> payload = new Dictionary<string, object>() {
                { "name", string.IsNullOrEmpty(visualElement.name) ? visualElement.ToString() : visualElement.name },
                { "type", GuessObjectTypeFromComponentNames(visualElement.GetType()) },
                { "classList", visualElement.GetClasses() },
                { "visible", VisualElementVisible () },
                { "pos", new List<float>() { pos.x, pos.y } },
                { "size", new List<float>() { size.x, size.y } },
                { "scale", new List<float>() { 1.0f, 1.0f } },
                { "anchorPoint", new List<float>() { 0.0f, 0.0f } },
                { "zOrders", GameObjectzOrders () },
                { "clickable", VisualElementClickable () },
                { "text", GetVisualElementText () },
                { "texture", GetImageSourceTexture () },
                { "_instanceId", instanceIDs[this] },
            };
            return payload;
        }

        private string GuessObjectTypeFromComponentNames(Type type)
        {
            foreach (var item in TypeNames)
            {
                if (item.Key == type || item.Key.IsAssignableFrom(type)) return item.Value;
            }
            return DefaultTypeName;
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