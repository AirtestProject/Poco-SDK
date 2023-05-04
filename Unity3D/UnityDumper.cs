using System;
using System.Collections.Generic;
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
        private List<AbstractNode> children = null;

        public RootNode()
        {
            children = new List<AbstractNode>();
            foreach (GameObject obj in Transform.FindObjectsOfType(typeof(GameObject)))
            {
                if (obj.transform.parent == null)
                {
                    #if PACKAGE_NGUI
                    children.Add(new ngui.UnityNode(obj));
                    #elif PACKAGE_FAIRYGUI
                    children.Add(new fairygui.UnityNode(obj));
                    #elif PACKAGE_TMPRO
                    children.Add(new uguiWithTMPro.UnityNode(obj));
                    #else
                    children.Add(new ugui.UnityNode(obj));
                    #endif
                }
            }
        }

        public override List<AbstractNode> getChildren() //<Modified>
        {
            return children;
        }
    }
}
