using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;


namespace Poco
{
	public class UnityNode: AbstractNode
	{
		public static Dictionary<string, string> TypeNames = new Dictionary<string, string> () {
			{ "Text", "Text" },
			{ "Gradient Text", "Gradient.Text" }, 
			{ "Image", "Image" }, 
			{ "RawImage", "Raw.Image" },
			{ "Mask", "Mask" },
			{ "2DRectMask", "2D-Rect.Mask" },
			{ "Button", "Button" },
			{ "InputField", "InputField" },
			{ "Toggle", "Toggle" },
			{ "Toggle Group", "ToggleGroup" },
			{ "Slider", "Slider" },
			{ "ScrollBar", "ScrollBar" },
			{ "DropDown", "DropDown" },
			{ "ScrollRect", "ScrollRect" },
			{ "Selectable", "Selectable" },
			{ "Camera", "Camera" },
			{ "RectTransform", "Node" },
		};
		public static string DefaultTypeName = "GameObject";
		private GameObject gameObject;
		private Renderer renderer;
		private RectTransform rectTransform;
		private Rect rect;
		private Vector2 objectPos;
		private List<string> components;


		public UnityNode (GameObject obj)
		{
			gameObject = obj;
			renderer = gameObject.GetComponent<Renderer> ();
			rectTransform = gameObject.GetComponent<RectTransform> ();
			rect = GameObjectRect (renderer, rectTransform);
			objectPos = renderer ? WorldToGUIPoint (renderer.bounds.center) : Vector2.zero;
			components = GameObjectAllComponents ();
		}

		public override AbstractNode getParent ()
		{
			GameObject parentObj = gameObject.transform.parent.gameObject;
			return new UnityNode (parentObj);
		}

		public override List<AbstractNode> getChildren ()
		{
			List<AbstractNode> children = new List<AbstractNode> ();
			foreach (Transform child in gameObject.transform) {
				children.Add (new UnityNode (child.gameObject));
			}
			return children;
		}

		public override object getAttr (string attrName)
		{
			switch (attrName) {
				case "name":
					return gameObject.name;
				case "type":
					return GuessObjectTypeFromComponentNames (components);
				case "visible":
					return GameObjectVisible (renderer, components);
				case "pos":
					return GameObjectPosInScreen (objectPos, renderer, rectTransform, rect);
				case "size":
					return GameObjectSizeInScreen (rect);
				case "scale":
					return new List<float> (){ 1.0f, 1.0f };
				case "anchorPoint":
					return GameObjectAnchorInScreen (renderer, rect, objectPos);
				case "zOrders":
					return GameObjectzOrders ();
				case "clickable":
					return GameObjectClickable (components);
				case "text":
					return GameObjectText ();
				case "components":
					return components;
				case "texture":
					return GetImageSourceTexture ();
				case "tag":
					return GameObjectTag ();
				case "_instanceId":
					return gameObject.GetInstanceID();
				default:
					return null;
			}
		}

		public override Dictionary<string, object> enumerateAttrs ()
		{
			Dictionary<string, object> payload = GetPayload ();
			Dictionary<string, object> ret = new Dictionary<string, object> ();
			foreach (KeyValuePair<string, object>  p in payload) {
				if (p.Value != null) {
					ret.Add (p.Key, p.Value);
				}
			}
			return ret;
		}

		private Dictionary<string, object> GetPayload ()
		{
			Dictionary<string, object> payload = new Dictionary<string, object> () {
				{ "name", gameObject.name },
				{ "type", GuessObjectTypeFromComponentNames (components) },
				{ "visible", GameObjectVisible (renderer, components) },
				{ "pos", GameObjectPosInScreen (objectPos, renderer, rectTransform, rect) },
				{ "size", GameObjectSizeInScreen (rect) },
				{ "scale", new List<float> (){ 1.0f, 1.0f } },
				{ "anchorPoint", GameObjectAnchorInScreen (renderer, rect, objectPos) },
				{ "zOrders", GameObjectzOrders () },
				{ "clickable", GameObjectClickable (components) },
				{ "text", GameObjectText () },
				{ "components", components },
				{ "texture", GetImageSourceTexture () },
				{ "tag", GameObjectTag () },
				{ "_instanceId", gameObject.GetInstanceID() },
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

		private bool GameObjectVisible (Renderer renderer, List<string> components)
		{
			if (gameObject.activeInHierarchy) {
				bool light = components.Contains ("Light");
				bool mesh = components.Contains ("MeshRenderer") && components.Contains ("MeshFilter");
				bool particle = components.Contains ("ParticleSystem") && components.Contains ("ParticleSystemRenderer");
				if (light || mesh || particle) {
					return false;
				} else {
					return renderer ? renderer.isVisible : true;
				}
			} else {
				return false;
			}
		}

		private bool GameObjectClickable (List<string> components)
		{
			Button button = gameObject.GetComponent<Button> ();
			if (button) {
				return button.isActiveAndEnabled;
			}
			// G42游戏比较特殊，有些event是用lua脚本控制的
			// 这里暂时用lua脚本名去判断是否可点击
			// 后续再看看其它unity游戏是什么情况
			if (components.Contains ("DULuaUIEvents")) {
				return true;
			}
			return false;
		}

		private string GameObjectText ()
		{
			Text text = gameObject.GetComponent<Text> ();
			return text ? text.text : null;
		}

		private string GameObjectTag ()
		{
			string tag;
			try {
				tag = !gameObject.CompareTag ("Untagged") ? gameObject.tag : null;
			} catch (UnityException e) {
				tag = null;
			}
			return tag;
		}

		private List<string> GameObjectAllComponents ()
		{
			List<string> components = new List<string> ();
			Component[] allComponents = gameObject.GetComponents<Component> ();
			if (allComponents != null){
				foreach (Component ac in allComponents) {
					if (ac != null) {
						components.Add (ac.GetType ().Name);
					}
				}
			}
			return components;
		}

		private Dictionary<string, float> GameObjectzOrders ()
		{
			float CameraViewportPoint = 0;
			if (Camera.main != null) {
				CameraViewportPoint = Math.Abs (Camera.main.WorldToViewportPoint (gameObject.transform.position).z);
			}
			Dictionary<string, float> zOrders = new Dictionary<string, float> () {
				{ "global", 0f },
				{ "local", -1 * CameraViewportPoint }
			};
			return zOrders;
		}

		private Rect GameObjectRect (Renderer renderer, RectTransform rectTransform)
		{
			Rect rect = new Rect (0, 0, 0, 0);
			if (renderer) {
				rect = RendererToScreenSpace (renderer);
			} else if (rectTransform) {
				rect = RectTransformToScreenSpace (rectTransform);
			}
			return rect;
		}

		private float[] GameObjectPosInScreen (Vector3 objectPos, Renderer renderer, RectTransform rectTransform, Rect rect)
		{
			float[] pos = { 0f, 0f };

			if (renderer) { 
				// 3d object
				pos [0] = objectPos.x / (float)Screen.width;
				pos [1] = objectPos.y / (float)Screen.height;
			} else if (rectTransform) {
				// ui object (rendered on screen space, other render modes may be different)
				// use center pos for now
				pos [0] = rect.center.x / (float)Screen.width;
				pos [1] = rect.center.y / (float)Screen.height;
			}
			return pos;
		}

		private float[] GameObjectSizeInScreen (Rect rect)
		{
			float[] size = { rect.width / (float)Screen.width, rect.height / (float)Screen.height };
			return size;
		}

		private float[] GameObjectAnchorInScreen (Renderer renderer, Rect rect, Vector3 objectPos)
		{
			float[] defaultValue = { 0.5f, 0.5f };

			if (!renderer) { //<Modified> some object do not have renderer
				return defaultValue;
			}
			float[] anchor = {(objectPos.x - rect.xMin) / rect.width, (objectPos.y - rect.yMin) / rect.height};
			if (Double.IsNaN (anchor [0]) || Double.IsNaN (anchor [1])) {
				return defaultValue;
			} else if (Double.IsPositiveInfinity (anchor [0]) || Double.IsPositiveInfinity (anchor [1])) {
				return defaultValue;
			} else if (Double.IsNegativeInfinity (anchor [0]) || Double.IsNegativeInfinity (anchor [1])) {
				return defaultValue;
			} else {
				return anchor;
			}
		}

		private string GetImageSourceTexture ()
		{
			Image image = gameObject.GetComponent<Image> ();
			if (image != null && image.sprite != null) {
				return image.sprite.name;
			}

			RawImage rawImage = gameObject.GetComponent<RawImage> ();
			if (rawImage != null && rawImage.texture != null) {
				return rawImage.texture.name;
			}

			SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer> ();
			if (spriteRenderer != null && spriteRenderer.sprite != null) {
				return spriteRenderer.sprite.name;
			}

			return null;
		}

		protected static Vector2 WorldToGUIPoint (Vector3 world)
		{
			Vector2 screenPoint = Vector2.zero;
			if (Camera.main != null) {
				screenPoint = Camera.main.WorldToScreenPoint (world);
				screenPoint.y = (float)Screen.height - screenPoint.y;
			}
			return screenPoint;
		}

		protected static Rect RendererToScreenSpace (Renderer renderer)
		{
			Vector3 cen = renderer.bounds.center;
			Vector3 ext = renderer.bounds.extents;
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

		protected static Rect RectTransformToScreenSpace (RectTransform rectTransform)
		{
			Vector2 size = Vector2.Scale (rectTransform.rect.size, rectTransform.lossyScale);
			Rect rect = new Rect (rectTransform.position.x, Screen.height - rectTransform.position.y, size.x, size.y);
			rect.x -= (rectTransform.pivot.x * size.x);
			rect.y -= ((1.0f - rectTransform.pivot.y) * size.y);
			return rect;
		}

		public static bool SetText(GameObject go, string textVal)
		{
			if (go != null) {
				var inputField = go.GetComponent<InputField> ();
				if (inputField != null) {
					inputField.text = textVal;
					return true;
				}
			}
			return false;
		}
	}
}
