using System.Collections.Generic;
using Poco;

namespace Game.SDKs.PocoSDK
{
    public  class PayloadDictionaryPool : CustomReleasePool<Dictionary<string, object>>
    {
        public static PayloadDictionaryPool inst = new PayloadDictionaryPool();

        protected override Dictionary<string, object> NewObject()
        {
            var obj = new Dictionary<string, object>();
            Init(obj);
            return obj;
        }

        void Init(Dictionary<string, object> target)
        {
            foreach (var pair in UnityNode.GetPlayload)
            {
                target[pair.PropName] = "";
            }
        }

        public override void Release(Dictionary<string, object> target)
        {
            Init(target);
            base.Release(target);
        }

    }
}