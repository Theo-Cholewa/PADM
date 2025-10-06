using UnityEditor;
using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.TextCore.LowLevel; // <- important

public class GeneratePiecesOfEightFontAsset
{
    [MenuItem("Tools/Generate Pieces of Eight Font Asset")]
    public static void GenerateFontAsset()
    {
        // Essaie de trouver automatiquement la police dans le projet
        string[] guids = AssetDatabase.FindAssets("Pieces of Eight t:Font");
        if (guids == null || guids.Length == 0)
        {
            Debug.LogError("❌ Police introuvable. Place 'Pieces of Eight.ttf' dans Assets/Fonts/ ou renomme-la correctement.");
            return;
        }

        string fontPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
        if (sourceFont == null)
        {
            Debug.LogError("❌ Impossible de charger la font à: " + fontPath);
            return;
        }

        string dir = Path.GetDirectoryName(fontPath).Replace('\\','/');
        string assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(dir, "Pieces of Eight SDF.asset"));

        // TMP 3.x : surcharge complète
        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
            sourceFont,
            90,                         // samplingPointSize (net pour un titre)
            8,                          // padding
            GlyphRenderMode.SDFAA,      // rendu lissé
            1024, 1024,                 // atlas width/height
            AtlasPopulationMode.Dynamic,
            true                        // multi-atlas
        );

        if (fontAsset == null)
        {
            Debug.LogError("❌ Échec création TMP_FontAsset.");
            return;
        }

        AssetDatabase.CreateAsset(fontAsset, assetPath);
        AssetDatabase.SaveAssets();

        // Matériau doré avec léger contour brun
        Material mat = new Material(fontAsset.material);
        mat.name = "Pieces of Eight SDF Gold";
        mat.SetColor(ShaderUtilities.ID_FaceColor, new Color(0.83f, 0.72f, 0.21f)); // doré
        mat.SetColor(ShaderUtilities.ID_OutlineColor, new Color(0.25f, 0.16f, 0.05f)); // brun
        mat.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.1f);

        string matPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(dir, mat.name + ".mat"));
        AssetDatabase.CreateAsset(mat, matPath);

        // Associer le preset au font asset
        fontAsset.material = mat;
        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();

        Debug.Log($"✅ Créé : {assetPath}\n🌟 Matériau : {matPath}");
    }
}
