using UnityEditor;
using UnityEngine;

public class NGUIShortcut
{
	[MenuItem("NGUI/Create/Texture Custom &#r", false, 6)]
	static public void AddTexture()
	{
		GameObject go = NGUIEditorTools.SelectedRoot(true);

		if (go != null)
		{
			Selection.activeGameObject = AddTexture(go).gameObject;
		}
		else Debug.Log("You must select a game object first.");
	}

	/// <summary>
	/// Convenience method -- add a texture.
	/// </summary>

	static public UITextureCustom AddTexture(GameObject go)
	{
		UITextureCustom w = NGUITools.AddWidget<UITextureCustom>(go);
		w.name = "Texture";
		w.pivot = NGUISettings.pivot;
		w.mainTexture = NGUISettings.texture;
		w.width = 100;
		w.height = 100;
		return w;
	}
}