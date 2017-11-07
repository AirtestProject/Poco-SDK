using System;
using System.Collections.Generic;
using UnityEngine;

namespace Poco
{
	public class UnityDumper: AbstractDumper
	{
		public override AbstractNode getRoot ()
		{
			return new RootNode ();
		}

		public new List<float> getPortSize ()
		{
			return new List<float>{ (float)Screen.width, (float)Screen.height };
		}
	}

	public class RootNode: AbstractNode
	{
		public override List<AbstractNode> getChildren () //<Modified> 
		{
			List<AbstractNode> children = new List<AbstractNode> ();
			foreach (GameObject obj in UnityEngine.Object.FindObjectsOfType(typeof(GameObject))) {
				if (obj.transform.parent == null) {
					children.Add (new UnityNode (obj));
				}
			}
			return children;
		}
	}
}

