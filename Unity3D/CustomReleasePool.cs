using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Game.SDKs.PocoSDK
{
    /// <summary>
    /// 线程安全的池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    // public class CustomReleasePool<T>  where T: new()
    // {
    //     
    //     ConcurrentQueue<T> list = new ConcurrentQueue<T>();
    //     public virtual T Alloc()
    //     {
    //         T target;
    //         bool result = list.TryDequeue(out target);
    //          
    //         if (result == false)
    //         {
    //             target = NewObject();
    //         }
    //
    //         return target;
    //     }
    //
    //
    //     protected virtual T NewObject()
    //     {
    //         return new T();
    //     }
    //
    //     public virtual void Release(T target)
    //     {
    //         list.Enqueue(target);
    //     }
    // }
    
    public abstract class CustomReleasePool<T>  where T: new()
    {
        
        Queue<T> list = new Queue<T>();
        public virtual T Alloc()
        {

            T target;
            lock (this)
            {
                if (list.Count > 0)
                {
                    target = list.Dequeue();
                }
                else
                {
                    target = NewObject();
                }
            }

            return target;
        }
    
    
        protected virtual T NewObject()
        {
            return new T();
        }
    
        public virtual void Release(T target)
        {
            lock (this)
            {
                list.Enqueue(target);
            }
        }

    }
}