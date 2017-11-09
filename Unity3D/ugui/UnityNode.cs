using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;


namespace Poco
{
	public class UnityNode: AbstractNode
	{
		public static Dictionary<string, string> TypeNames = new Dictionary<string, string> () {
			{"Text", "Text"},
			{"Gradient Text", "Gradient.Text"}, 
			{"Image", "Image"}, 
			{"RawImage", "Raw.Image"},
			{"Mask", "Mask"},
			{"2DRectMask", "2D-Rect.Mask"},
			{"Button", "Button"},
			{"InputField", "InputField"},
			{"Toggle", "Toggle"},
			{"Toggle Group", "ToggleGroup"},
			{"Slider", "Slider"},
			{"ScrollBar", "ScrollBar"},
			{"DropDown", "DropDown"},
			{"ScrollRect", "ScrollRect"},
			{"Selectable", "Selectable"},
			{"Camera", "Camera"},
			{"RectTransform", "Node"},
		};
		public static string DefaultTypeName = "GameObject";

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
			Renderer renderer = gameObject.GetComponent<Renderer> ();
			RectTransform rectTransform = gameObject.GetComponent<RectTransform> ();
			
			Rect rect = GameObjectRect (renderer, rectTransform);
			Vector2 objectPos = renderer ? WorldToGUIPoint (renderer.bounds.center) : Vector2.zero;
			List<string> components = GameObjectAllComponents ();
			switch (attrName) {
				case "name":
					return gameObject.name;
				case "type":
					return GuessObjectTypeFromComponentNames(components);
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
				default:
					return null;
			}
		}

		public override Dictionary<string, object> enumerateAttrs ()
		{
			Renderer renderer = gameObject.GetComponent<Renderer> ();
			RectTransform rectTransform = gameObject.GetComponent<RectTransform> ();
			Rect rect = GameObjectRect (renderer, rectTransform);
			Vector2 objectPos = renderer ? WorldToGUIPoint (renderer.bounds.center) : Vector2.zero;
			List<string> components = GameObjectAllComponents ();
			Dictionary<string, object> payload = new Dictionary<string, object> () {
				{ "name", gameObject.name },
				{ "type", GuessObjectTypeFromComponentNames(components) },
				{ "visible", GameObjectVisible (renderer, components) },
				{ "pos", GameObjectPosInScreen (objectPos, renderer, rectTransform, rect) },
				{ "size", GameObjectSizeInScreen (rect) },
				{ "scale", new float[] { 1.0f, 1.0f } },
				{ "anchorPoint", GameObjectAnchorInScreen (renderer, rect, objectPos) },
				{ "zOrders", GameObjectzOrders () },
				{ "clickable", GameObjectClickable (components) },
				{ "text", GameObjectText () },
				{ "components", components },
				{ "texture", GetImageSourceTexture () },
			};


			if (gameObject.tag != null && !gameObject.tag.Equals("Untagged")) {
				payload.Add ("tag", gameObject.tag);
			}
			return payload;
		}

		private string GuessObjectTypeFromComponentNames (List<string> componentNames) 
		{
			var cns = new List<string> (componentNames);
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
				bool mesh = components.Contains ("MeshRenderer") && components.Contains ("MeshFilter");
				bool particle = components.Contains ("ParticleSystem") && components.Contains ("ParticleSystemRenderer");
				if (mesh || particle) {
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
			;
			return size;
		}

		private float[] GameObjectAnchorInScreen (Renderer renderer, Rect rect, Vector3 objectPos)
		{
			float[] anchor = { 0.5f, 0.5f };

			if (!renderer) { //<Modified> some object do not have renderer
				return anchor;
			}
			anchor [0] = (objectPos.x - rect.xMin) / rect.width;
			anchor [1] = (objectPos.y - rect.yMin) / rect.height;
			if (Double.IsNaN (anchor [0]) || Double.IsNaN (anchor [1])) {
				return new float[] { 0.5f, 0.5f };
			}
			return anchor;
		}

		private string GetImageSourceTexture ()
		{
			var cImage = gameObject.GetComponent<Image> ();
			if (cImage != null && cImage.sprite != null) {
				return cImage.sprite.name;
			}

			var cRawImage = gameObject.GetComponent<RawImage> ();
			if (cRawImage != null && cRawImage.texture != null) {
				return cRawImage.texture.name;
			}

			var cSpriteRenderer = gameObject.GetComponent<SpriteRenderer> ();
			if (cSpriteRenderer != null && cSpriteRenderer.sprite != null) {
				return cSpriteRenderer.sprite.name;
			}

			return null;
		}

		protected static Vector2 WorldToGUIPoint (Vector3 world)
		{
			Vector2 screenPoint = Camera.main.WorldToScreenPoint (world);
			screenPoint.y = (float)Screen.height - screenPoint.y;
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
	}
}

