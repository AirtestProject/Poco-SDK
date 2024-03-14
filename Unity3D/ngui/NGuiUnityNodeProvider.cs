using UnityEngine;

namespace Poco.NGUI
{
    public class NGuiUnityNodeProvider : UnityNodeProvider
    {
        public override AbstractNode CreateNode(GameObject gameObject)
        {
            return new UnityNode(gameObject);
        }

        public override bool SetText(GameObject go, string textVal)
        {
            return UnityNode.SetText(go, textVal);
        }
    }
}