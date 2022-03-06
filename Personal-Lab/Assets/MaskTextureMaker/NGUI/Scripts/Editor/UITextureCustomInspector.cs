//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2020 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Inspector class used to edit UITextureCustomInspector.
/// </summary>

[CanEditMultipleObjects]
[CustomEditor(typeof(UITextureCustom), true)]
public class UITextureCustomInspector : UITextureInspector
{
	UITextureCustom mTexCustom;

	protected override void OnEnable()
	{
		base.OnEnable();
		mTexCustom = target as UITextureCustom;
	}

	protected override bool ShouldDrawProperties()
	{
		if (target == null) return false;

		SerializedProperty sp = NGUIEditorTools.DrawProperty("Texture", serializedObject, "mTextureObject");
		NGUIEditorTools.DrawProperty("Material", serializedObject, "mMat");

		if (sp != null)
		{
			Texture texture = sp.objectReferenceValue as Texture;
			if (texture == null)
			{
				var maskTextureScriptableObject = sp.objectReferenceValue as MaskTextureData;
				if (maskTextureScriptableObject != null)
				{
					maskTextureScriptableObject.RequestMaskTexture((texture2D) =>
					{
						mTexCustom.mainTexture = texture2D;
						NGUISettings.texture = texture2D;
					});
				}
			}
			else
				NGUISettings.texture = texture;
		}

		if (mTexCustom != null && (mTexCustom.material == null || serializedObject.isEditingMultipleObjects))
		{
			NGUIEditorTools.DrawProperty("Shader", serializedObject, "mShader");
		}

		EditorGUI.BeginDisabledGroup(mTexCustom == null || mTexCustom.mainTexture == null || serializedObject.isEditingMultipleObjects);

		NGUIEditorTools.DrawRectProperty("UV Rect", serializedObject, "mRect");

		sp = serializedObject.FindProperty("mFixedAspect");
		bool before = sp.boolValue;
		NGUIEditorTools.DrawProperty("Fixed Aspect", sp);
		if (sp.boolValue != before) (target as UIWidget).drawRegion = new Vector4(0f, 0f, 1f, 1f);

		if (sp.boolValue)
		{
			EditorGUILayout.HelpBox("Note that Fixed Aspect mode is not compatible with Draw Region modifications done by sliders and progress bars.", MessageType.Info);
		}

		EditorGUI.EndDisabledGroup();
		return true;
	}

	/// <summary>
	/// Allow the texture to be previewed.
	/// </summary>

	public override bool HasPreviewGUI()
	{
		return (Selection.activeGameObject == null || Selection.gameObjects.Length == 1) &&
			(mTexCustom != null) && (mTexCustom.mainTexture as Texture2D != null);
	}

	/// <summary>
	/// Draw the sprite preview.
	/// </summary>

	public override void OnPreviewGUI(Rect rect, GUIStyle background)
	{
		Texture2D tex = mTexCustom.mainTexture as Texture2D;

		if (tex != null)
		{
			Rect tc = mTexCustom.uvRect;
			tc.xMin *= tex.width;
			tc.xMax *= tex.width;
			tc.yMin *= tex.height;
			tc.yMax *= tex.height;
			NGUIEditorTools.DrawSprite(tex, rect, mTexCustom.color, tc, mTexCustom.border);
		}
	}
}
