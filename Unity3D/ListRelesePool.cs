using System;
using System.Collections.Generic;

namespace Game.SDKs.PocoSDK
{
    public class ListRelesePool 
    {
        public static ListRelesePool inst = new ListRelesePool();
        List<List<object>> list = new List<List<object>>();


        public List<object> Alloc(int count)
        {
            List<object> target;
            lock (this)
            {
                int index = BinarySearch(count);
                if (index >=0 && index < list.Count)
                {
                    target = list[ index ];
                    list.RemoveAt(index);
                }
                else
                {
                    target = NewObject();
                }

                // if (list.Count > 0)
                // {
                //     int index = BinarySearch(count);
                //     target = list[ index ];
                //     list.RemoveAt(index);
                // }
                // else
                // {
                //     target = NewObject();
                // }
            }

            return target;
        }
    
    
        protected  List<object> NewObject()
        {
            return new List<object>();
        }
    
        public virtual void Release(List<object> target)
        {
            lock (this)
            {
                target.Clear();
                int newIndex = BinarySearch(target.Capacity);
                list.Insert(newIndex, target);
            }
        }
        
        int BinarySearch( int value)
        {
            
            int index = 0;
            int length = list.Count;
            int num1 = index;
            int num2 = index + length - 1;
            while (num1 <= num2)
            {
                int index1 = num1 + (num2 - num1 >> 1);
                int curCount = list[index1].Capacity;
                int num3 = curCount.CompareTo(value);
                if (num3 == 0)
                    return index1;
                if (num3 < 0)
                    num1 = index1 + 1;
                else
                    num2 = index1 - 1;
            }
            return num1;
        }
    }
}