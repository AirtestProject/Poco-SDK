using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


  namespace Poco
{
    public class AbstractDumper : IDumper
    {
        public virtual AbstractNode getRoot()
        {
            return null;
        }

        Dictionary<string, object> blankDict = new Dictionary<string, object>();
        
        public Dictionary<string, object> dumpHierarchy()
        {
            // 短路 不再调用
            return blankDict;
            // return dumpHierarchyImpl(getRoot(), true);
        }
        public Dictionary<string, object> dumpHierarchy(bool onlyVisibleNode)
        {
            // 短路 不再调用
            return blankDict;
            // return dumpHierarchyImpl(getRoot(), onlyVisibleNode);
        }

        // private Dictionary<string, object> dumpHierarchyImpl(AbstractNode node, bool onlyVisibleNode)
        // {
            // if (node == null)
            // {
            //     return null;
            // }
            //
            // Dictionary<string, object> payload = node.enumerateAttrs();
            // Dictionary<string, object> result = new Dictionary<string, object>();
            // string name = (string)node.getAttr("name");
            // result.Add("name", name);
            // result.Add("payload", payload);
            //
            // List<object> children = new List<object>();
            // List<AbstractNode> nodeList = new List<AbstractNode>();
            // foreach (AbstractNode child in node.getChildren())
            // {
            //     nodeList.Add( child );
            //     if (!onlyVisibleNode || (bool)child.getAttr("visible"))
            //     {
            //         // if (count >= 10)
            //         // {
            //         //     return result;
            //         // }
            //         children.Add(dumpHierarchyImpl(child, onlyVisibleNode));
            //     }
            // }
            // if (children.Count > 0)
            // {
            //     result.Add("children", children);
            // }
            // foreach( AbstractNode child in nodeList)
            // {
            //     UnityNodePool.Release( child );
            // }
            // return result;
        // }

        public virtual List<float> getPortSize()
        {
            return null;
        }
    }

    public interface IDumper
    {
        AbstractNode getRoot();

        Dictionary<string, object> dumpHierarchy();
        Dictionary<string, object> dumpHierarchy(bool onlyVisibleNode);

        List<float> getPortSize();
    }
}
