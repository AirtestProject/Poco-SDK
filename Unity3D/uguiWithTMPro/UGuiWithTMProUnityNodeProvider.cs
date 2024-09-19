using UnityEngine;

namespace Poco.UGUIWithTMPro
{
    public class UGuiWithTMProUnityNodeProvider : UnityNodeProvider
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