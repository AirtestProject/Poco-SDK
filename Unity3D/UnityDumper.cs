using System.Collections.Generic;
using UnityEngine;

namespace Poco
{
    public class UnityDumper : AbstractDumper
    {
        private readonly UnityNodeProvider nodeProvider;

        public UnityDumper(UnityNodeProvider nodeProvider)
        {
            this.nodeProvider = nodeProvider;
        }
        
        public override AbstractNode getRoot()
        {
            return nodeProvider ? new RootNode(nodeProvider) : null;
        }
    }

    public class RootNode : AbstractNode
    {
        private readonly List<AbstractNode> children = new List<AbstractNode>();

        public RootNode(UnityNodeProvider nodeProvider)
        {
            foreach (GameObject obj in Object.FindObjectsOfType(typeof(GameObject)))
            {
                if (obj.transform.parent == null)
                {
                    children.Add(nodeProvider.CreateNode(obj));
                }
            }
        }

        public override List<AbstractNode> getChildren() //<Modified> 
        {
            return children;
        }
    }
}
