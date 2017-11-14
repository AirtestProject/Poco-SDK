using System;
using System.Collections.Generic;
using UnityEngine;


namespace Poco
{
	public class AbstractNode
	{
		private List<string> requiredAttrs = new List<string> {
			"name",
			"type",
			"visible",
			"pos",
			"size",
			"scale",
			"anchorPoint",
			"zOrders"
		};

		public virtual AbstractNode getParent ()
		{
			return null;
		}

		public virtual List<AbstractNode> getChildren ()
		{
			return null;
		}

		public virtual object getAttr (string attrName)
		{
			Dictionary<string, object> defaultAttrs = new Dictionary<string, object> () {
				{ "name", "<Root>" },
				{ "type", "Root" },
				{ "visible", true },
				{ "pos", new List<float> (){ 0.0f, 0.0f } },
				{ "size", new List<float> (){ 0.0f, 0.0f } },
				{ "scale", new List<float> (){ 1.0f, 1.0f } },
				{ "anchorPoint", new List<float> (){ 0.5f, 0.5f } },
				{ "zOrders", new Dictionary<string, object> (){ { "local", 0 }, { "global", 0 } } }
			};
			return defaultAttrs.ContainsKey (attrName) ? defaultAttrs [attrName] : null;
		}

		public virtual void setAttr (string attrName, object val)
		{

		}

		public virtual Dictionary<string, object> enumerateAttrs ()
		{
			Dictionary<string, object> ret = new Dictionary<string, object> ();
			foreach (string attr in requiredAttrs) {
				ret.Add (attr, getAttr (attr));
			}
			return ret;
		}
	}
}
