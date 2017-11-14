using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Poco
{
	public class AbstractDumper: IDumper
	{
		public virtual AbstractNode getRoot ()
		{
			return null;
		}

		public Dictionary<string, object> dumpHierarchy ()
		{
			return dumpHierarchyImpl (getRoot ());
		}

		private Dictionary<string, object> dumpHierarchyImpl (AbstractNode node)
		{
			if (node == null) {
				return null;
			}

			Dictionary<string, object> payload = node.enumerateAttrs ();
			Dictionary<string, object> result = new Dictionary<string, object> ();
			string name = (string)node.getAttr ("name");
			result.Add ("name", name);
			result.Add ("payload", payload);

			List<object> children = new List<object> ();
			foreach (AbstractNode child in node.getChildren()) {
				if ((bool)child.getAttr ("visible")) {
					children.Add (dumpHierarchyImpl (child));
				}
			}
			if (children.Count > 0) {
				result.Add ("children", children);
			}
			return result;
		}

		public virtual List<float> getPortSize ()
		{
			return null;
		}
	}

	public interface IDumper
	{
		AbstractNode getRoot ();

		Dictionary<string, object> dumpHierarchy ();

		List<float> getPortSize ();
	}
}
