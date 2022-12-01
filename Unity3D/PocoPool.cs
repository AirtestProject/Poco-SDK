using System;
using System.Collections.Generic;

namespace Game.SDKs.PocoSDK
{
    public interface PocoPoolable
    {
        void Clear();
    }

    public class PocoPool<T> : CustomReleasePool<T> where T:  PocoPoolable, new()
    {
        public static PocoPool<T> inst = new PocoPool<T>();
        
        public override void Release(T target)
        {
            target.Clear();
            base.Release(target);
        }
    }
}