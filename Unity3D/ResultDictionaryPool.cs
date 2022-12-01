using System.Collections.Generic;

namespace Game.SDKs.PocoSDK
{
    public class ResultDictionaryPool : CustomReleasePool<Dictionary<string, object>>
    {
        public override void Release(Dictionary<string, object> resultInfo)
        {
            resultInfo["name"] = null;
            resultInfo["payload"] = null;
            resultInfo["children"] = null;
            base.Release(resultInfo);
            
        }

        public static ResultDictionaryPool inst = new ResultDictionaryPool();
    }
}