using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;


namespace Poco
{
	public class UnityNode: AbstractNode
	{
		private GameObject gameObject;

		public UnityNode (GameObject obj)
		{
			gameObject = obj;
		}

		public override AbstractNode getParent ()
		{ //<Modified> 不要用new, 这样会掩盖原来的结构, 多态就没有意义了
			GameObject parentObj = gameObject.transform.parent.gameObject;
			return new UnityNode (parentObj);
		}

		public override List<AbstractNode> getChildren ()
		{ //<Modified> 不要用new, 这样会掩盖原来的结构, 多态就没有意义了
			List<AbstractNode> children = new List<AbstractNode> ();
			foreach (Transform child in gameObject.transform) {
				children.Add (new UnityNode (child.gameObject));
			}
			return children;
		}

		public override object getAttr (string attrName)
		{
			Dictionary<string, object> payload = enumerateAttrs ();
			return payload.ContainsKey (attrName) ? payload [attrName] : null;
		}

		public override Dictionary<string, object> enumerateAttrs ()
		{
			Bounds bounds = NGUIMath.CalculateAbsoluteWidgetBounds (gameObject.transform);
			Rect rect = GetRectInScreen (bounds);
			Vector2 objectPos = WorldToGUIPoint (bounds.center);
			Dictionary<string, object> payload = new Dictionary<string, object> () {
				{ "name", gameObject.name },
				{ "type", gameObject.GetType ().Name },
				{ "visible", GameObjectVisible () },
				{ "pos", GetPosInScreen (objectPos) },
				{ "size", GetSizeInScreen (rect) },
				{ "scale", new List<float> (){ 1.0f, 1.0f } },
				{ "anchorPoint", GetAnchorInScreen (rect, objectPos) },
				{ "zOrders", GetzOrders () },
				{ "clickable", GameObjectClickable () },
				{ "text", GameObjectText () },
				{ "components", GetAllComponents () }
			};
			return payload;
		}

		public bool GameObjectVisible ()
		{
			return gameObject.activeInHierarchy ? true : false;
		}

		public bool GameObjectClickable ()
		{
			UIButton b = gameObject.GetComponent<UIButton> ();
			BoxCollider bc = gameObject.GetComponent<BoxCollider> ();
			return b && b.isEnabled && bc ? true : false;
		}

		public string GameObjectText ()
		{
			UILabel t = gameObject.GetComponent<UILabel> ();
			return t ? t.text : null;
		}

		public List<string> GetAllComponents ()
		{
			List<string> components = new List<string> ();
			Component[] allComponents = gameObject.GetComponents<Component> ();
			foreach (Component ac in allComponents) {
				components.Add (ac.GetType ().Name);
			}
			return components;
		}

		public Dictionary<string, float> GetzOrders ()
		{
			float CameraViewportPoint = 0;
			if (UICamera.currentCamera != null) {
				CameraViewportPoint = Math.Abs (UICamera.currentCamera.WorldToViewportPoint (gameObject.transform.position).z);
			}
			Dictionary<string, float> zOrders = new Dictionary<string, float> () {
				{ "global", 0f },
				{ "local", -1 * CameraViewportPoint }
			};
			return zOrders;
		}

		public static float[] GetPosInScreen (Vector2 objectPos)
		{
			float[] pos = { objectPos.x / (float)Screen.width, objectPos.y / (float)Screen.height };
			return pos;
		}

		public static float[] GetSizeInScreen (Rect rect)
		{
			float[] size = { rect.width / (float)Screen.width, rect.height / (float)Screen.height };
			return size;
		}

		public static float[] GetAnchorInScreen (Rect rect, Vector2 objectPos)
		{
			float[] anchor = { (objectPos.x - rect.xMin) / rect.width, (objectPos.y - rect.yMin) / rect.height };
			if (Double.IsNaN (anchor [0]) || Double.IsNaN (anchor [1])) {
				return new float[] { 0.5f, 0.5f };
			}
			return anchor;
		}

		public Rect GetRectInScreen (Bounds bounds)
		{
			Vector3 cen;
			Vector3 ext;
			Renderer r = gameObject.GetComponent<Renderer> ();
			cen = r ? r.bounds.center : bounds.center;
			ext = r ? r.bounds.extents : bounds.extents;
			Vector2[] extentPoints = new Vector2[8] {
				WorldToGUIPoint (new Vector3 (cen.x - ext.x, cen.y - ext.y, cen.z - ext.z)),
				WorldToGUIPoint (new Vector3 (cen.x + ext.x, cen.y - ext.y, cen.z - ext.z)),
				WorldToGUIPoint (new Vector3 (cen.x - ext.x, cen.y - ext.y, cen.z + ext.z)),
				WorldToGUIPoint (new Vector3 (cen.x + ext.x, cen.y - ext.y, cen.z + ext.z)),
				WorldToGUIPoint (new Vector3 (cen.x - ext.x, cen.y + ext.y, cen.z - ext.z)),
				WorldToGUIPoint (new Vector3 (cen.x + ext.x, cen.y + ext.y, cen.z - ext.z)),
				WorldToGUIPoint (new Vector3 (cen.x - ext.x, cen.y + ext.y, cen.z + ext.z)),
				WorldToGUIPoint (new Vector3 (cen.x + ext.x, cen.y + ext.y, cen.z + ext.z))
			};
			Vector2 min = extentPoints [0];
			Vector2 max = extentPoints [0];
			foreach (Vector2 v in extentPoints) {
				min = Vector2.Min (min, v);
				max = Vector2.Max (max, v);
			}
			return new Rect (min.x, min.y, max.x - min.x, max.y - min.y);
		}

		public static Vector2 WorldToGUIPoint (Vector3 world)
		{
			Vector2 screenPoint = UICamera.currentCamera.WorldToScreenPoint (world);
			screenPoint.y = (float)Screen.height - screenPoint.y;
			return screenPoint;
		}
	}
}
