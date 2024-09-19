using UnityEngine;

namespace Poco
{
    public abstract class UnityNodeProvider: ScriptableObject
    {
        public abstract AbstractNode CreateNode(GameObject gameObject);
        public abstract bool SetText(GameObject go, string textVal);
    }
}