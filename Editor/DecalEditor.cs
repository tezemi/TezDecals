using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using TezDecals.Runtime;

namespace TezDecals.Editor 
{
	[CustomEditor(typeof(Decal))]
    internal class DecalEditor : UnityEditor.Editor 
    {
        public override void OnInspectorGUI() 
        {
            var decal = target as Decal;

			decal.FixedAspect = EditorGUILayout.Toggle("Fixed Aspect", decal.FixedAspect);
			decal.LayerMask = LayerMaskField("Layer Mask", decal.LayerMask);
			decal.MaxAngle = (int)EditorGUILayout.Slider("Max Angle", decal.MaxAngle, Decal.MinMaxAngle, Decal.MaxMaxAngle);
			decal.Offset = EditorGUILayout.Slider("Offset", decal.Offset, Decal.MinOffset, Decal.MaxOffset);

			EditorGUILayout.Separator();

			decal.Material = (Material)EditorGUILayout.ObjectField("Material", decal.Material, typeof(Material), false);

            if (decal.Material != null) 
            {
                if (decal.Material.mainTexture != null)
                {
					var path = AssetDatabase.GetAssetPath(decal.Material.mainTexture);
					var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
					var spriteList = sprites.Select(s => s.name).ToArray();

					if (sprites != null && sprites.Length > 0)
					{
						if (decal.Sprite == null || !sprites.Contains(decal.Sprite))
						{
							decal.Sprite = sprites[0];
						}

						decal.Sprite = sprites[EditorGUILayout.Popup("Sprite", Array.IndexOf(sprites, decal.Sprite), spriteList)];

						var foldout = EditorGUILayout.BeginFoldoutHeaderGroup(EditorPrefs.GetBool(nameof(DecalEditor) + ".ShowSprites"), "Sprites");
						EditorPrefs.SetBool(nameof(DecalEditor) + ".ShowSprites", foldout);

						if (foldout)
						{
							decal.Sprite = sprites[GUILayout.SelectionGrid
							(
								Array.IndexOf(sprites, decal.Sprite),
								sprites.Select(s => AssetPreview.GetAssetPreview(s)).ToArray(),
								5
							)];
						}

						EditorGUILayout.EndFoldoutHeaderGroup();
					}
					else
					{
						GUILayout.Label($@"The selected material, ""{decal.Material.name},"" doesn't have any sprites. Make the texture a sprite-sheet to select a sprite.", EditorStyles.wordWrappedLabel);
					}
				}
				else
				{
					GUILayout.Label($@"The selected material, ""{decal.Material.name},"" doesn't have a main texture.", EditorStyles.wordWrappedLabel);
				} 
            }
            else
            {
				GUILayout.Label("Select a material.", EditorStyles.wordWrappedLabel);
			}

            EditorGUILayout.Separator();

            if (GUILayout.Button("Generate"))
            {
                decal.GenerateAndSetDirty();
            }

            if (GUI.changed) 
            {
                decal.OnValidate();
                decal.GenerateAndSetDirty();

                GUI.changed = false;
            }
        }

        private LayerMask LayerMaskField(string label, LayerMask mask)
		{
			var names = Enumerable.Range(0, 32).Select(i => LayerMask.LayerToName(i)).Reverse().SkipWhile(s => string.IsNullOrEmpty(s)).Reverse().ToArray();

			return EditorGUILayout.MaskField(label, mask.value, names);
		}
	}
}
