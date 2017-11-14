using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;


namespace Poco
{
	public class UnityNode: AbstractNode
	{
		public static Dictionary<string, string> TypeNames = new Dictionary<string, string> () {
			{ "UI2DSprite", "UI2DSprite" },
			{ "UI2DSpriteAnimation", "UI2DSpriteAnimation" },
			{ "UIAnchor", "UIAnchor" },
			{ "UIAtlas", "UIAtlas" },
			{ "UICamera", "UICamera" },
			{ "UIFont", "UIFont" },
			{ "UIInput", "UIInput" },
			{ "UILabel", "UILabel" },
			{ "UILocalize", "UILocalize" },
			{ "UIOrthoCamera", "UIOrthoCamera" },
			{ "UIPanel", "UIPanel" },
			{ "UIRoot", "UIRoot" },
			{ "UISprite", "UISprite" },
			{ "UISpriteAnimation", "UISpriteAnimation" },
			{ "UISpriteData", "UISpriteData" },
			{ "UIStretch", "UIStretch" },
			{ "UITextList", "UITextList" },
			{ "UITexture", "UITexture" },
			{ "UITooltip", "UITooltip" },
			{ "UIViewport", "UIViewport" },
			{ "Camera", "Camera" },
			{ "Transform", "Node" },
		};
		public static string DefaultTypeName = "GameObject";
		private GameObject gameObject;
		private Dictionary<string, object> payload;

		public UnityNode (GameObject obj)
		{
			gameObject = obj;
			payload = GetPayload ();
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
			return payload.ContainsKey (attrName) ? payload[attrName] : null;
		}

		public override Dictionary<string, object> enumerateAttrs ()
		{
			return payload;
		}

		private Dictionary<string, object> GetPayload ()
		{
			Bounds bounds = NGUIMath.CalculateAbsoluteWidgetBounds (gameObject.transform);
			Rect rect = BoundsToScreenSpace (bounds);
			Vector2 objectPos = WorldToGUIPoint (bounds.center);
			List<string> components = GameObjectAllComponents ();
			Dictionary<string, object> payload = new Dictionary<string, object> () {
				{ "name", gameObject.name },
				{ "type", GuessObjectTypeFromComponentNames (components) },
				{ "visible", GameObjectVisible (components) },
				{ "pos", GameObjectPosInScreen (objectPos) },
				{ "size", GameObjectSizeInScreen (rect) },
				{ "scale", new List<float> (){ 1.0f, 1.0f } },
				{ "anchorPoint", GameObjectAnchorInScreen (rect, objectPos) },
				{ "zOrders", GameObjectzOrders () },
				{ "clickable", GameObjectClickable () },
				{ "text", GameObjectText () },
				{ "components", components },
				{ "texture", GetImageSourceTexture () },
				{ "tag", GameObjectTag () }
			};
			return payload;
		}

		private string GuessObjectTypeFromComponentNames (List<string> components)
		{
			List<string> cns = new List<string> (components);
			cns.Reverse ();
			foreach (string name in cns) {
				if (TypeNames.ContainsKey(name)) {
					return TypeNames[name];
				}
			}
			return DefaultTypeName;
		}

		private bool GameObjectVisible (List<string> components)
		{
			if (gameObject.activeInHierarchy) {
				bool drawcall = components.Contains ("UIDrawCall");
				bool mesh = components.Contains ("MeshRenderer") && components.Contains ("MeshFilter");
				bool particle = components.Contains ("ParticleSystem") && components.Contains ("ParticleSystemRenderer");
				return drawcall || mesh || particle ? false : true;
			} else {
				return false;
			}
		}

		private bool GameObjectClickable ()
		{
			UIButton button = gameObject.GetComponent<UIButton> ();
			BoxCollider boxCollider = gameObject.GetComponent<BoxCollider> ();
			return button && button.isEnabled && boxCollider ? true : false;
		}

		private string GameObjectText ()
		{
			UILabel text = gameObject.GetComponent<UILabel> ();
			return text ? text.text : null;
		}

		private string GameObjectTag ()
		{
			return !gameObject.CompareTag ("Untagged") ? gameObject.tag : null;
		}

		private List<string> GameObjectAllComponents ()
		{
			List<string> components = new List<string> ();
			Component[] allComponents = gameObject.GetComponents<Component> ();
			foreach (Component ac in allComponents) {
				components.Add (ac.GetType ().Name);
			}
			return components;
		}

		private Dictionary<string, float> GameObjectzOrders ()
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

		private float[] GameObjectPosInScreen (Vector2 objectPos)
		{
			float[] pos = { objectPos.x / (float)Screen.width, objectPos.y / (float)Screen.height };
			return pos;
		}

		private float[] GameObjectSizeInScreen (Rect rect)
		{
			float[] size = { rect.width / (float)Screen.width, rect.height / (float)Screen.height };
			return size;
		}

		private float[] GameObjectAnchorInScreen (Rect rect, Vector2 objectPos)
		{
			float[] anchor = { (objectPos.x - rect.xMin) / rect.width, (objectPos.y - rect.yMin) / rect.height };
			if (Double.IsNaN (anchor [0]) || Double.IsNaN (anchor [1])) {
				return new float[] { 0.5f, 0.5f };
			}
			return anchor;
		}

		private string GetImageSourceTexture ()
		{
			UISprite sprite = gameObject.GetComponent<UISprite> ();
			if (sprite != null) {
				return sprite.spriteName;
			}

			UITexture texture = gameObject.GetComponent<UITexture> ();
			if (texture != null) {
				return texture.mainTexture.name;
			}

			return null;
		}

		protected Rect BoundsToScreenSpace (Bounds bounds)
		{
			Vector3 cen;
			Vector3 ext;
			Renderer renderer = gameObject.GetComponent<Renderer> ();
			cen = renderer ? renderer.bounds.center : bounds.center;
			ext = renderer ? renderer.bounds.extents : bounds.extents;
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

		protected static Vector2 WorldToGUIPoint (Vector3 world)
		{
			Vector2 screenPoint = UICamera.currentCamera.WorldToScreenPoint (world);
			screenPoint.y = (float)Screen.height - screenPoint.y;
			return screenPoint;
		}
	}
}
