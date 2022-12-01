using System;
using System.Collections.Generic;
using System.Diagnostics;
using Game.SDKs.PocoSDK;
using UnityEngine;

namespace Poco
{
    public class UnityDumper : AbstractDumper
    {
        public override AbstractNode getRoot()
        {
            return new RootNode();
        }
    }

    public class RootNode : AbstractNode
    {
        public List<AbstractNode> children = new List<AbstractNode>();
        public Dictionary<string, System.Object> payLoad;

        // public RootNode()
        // {
        //     children = new List<AbstractNode>();
        //     foreach (GameObject obj in Transform.FindObjectsOfType(typeof(GameObject)))
        //     {
        //         if (obj.transform.parent == null)
        //         {
        //             children.Add(new UnityNode(obj));
        //         }
        //     }
        // }
        
        
        public static readonly Dictionary<string, object> defaultAttrs = new Dictionary<string, object> () {
            { "name", "<Root>" },
            { "type", "Root" },
            { "visible", true },
            { "pos", new List<float> (){ 0.0f, 0.0f } },
            { "size", new List<float> (){ 0.0f, 0.0f } },
            { "scale", new List<float> (){ 1.0f, 1.0f } },
            { "anchorPoint", new List<float> (){ 0.5f, 0.5f } },
            { "zOrders", new Dictionary<string, object> (){ { "local", 0 }, { "global", 0 } } }
        };


        public static Dictionary<string, object> GetDict()
        {
            var dict = PayloadDictionaryPool.inst.Alloc();
         
            foreach (var pair in UnityNode.GetPlayload)
            {
                if (defaultAttrs.ContainsKey(pair.PropName))
                {
                    dict[pair.PropName] = defaultAttrs[pair.PropName];
                }
                else
                {
                    dict[pair.PropName] = null;
                }
            }

            return dict;
        }

        public RootNode()
        {
            payLoad = defaultAttrs; //enumerateAttrs();
        }
        
        public override List<AbstractNode> getChildren() //<Modified> 
        {
            return children;
        }
    }
}
