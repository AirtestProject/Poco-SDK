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
                    children.Add(new UnityNode(obj));
                }
            }
        }

        public override List<AbstractNode> getChildren() //<Modified> 
        {
            return children;
        }
    }
}
