using Brew.Core;
using Brew.Presentation;
using UnityEditor;
using UnityEngine;

namespace Brew.Editor
{
    /// <summary>
    /// Editor utility to bootstrap the Gameplay scene with all required objects.
    /// Run via menu: Brew > Setup Gameplay Scene.
    /// </summary>
    public static class BrewSceneSetup
    {
        [MenuItem("Brew/Setup Gameplay Scene")]
        public static void SetupGameplayScene()
        {
            CreateCamera();
            var tokenPrefab = CreateTokenPrefab();
            CreateBoardPresenter(tokenPrefab);

            Debug.Log("[Brew] Gameplay scene setup complete. Assign a BoardConfigSO in BoardPresenter.");
        }

        [MenuItem("Brew/Generate Placeholder Token Sprites")]
        public static void GeneratePlaceholderSprites()
        {
            var colors = new (IngredientColor type, Color color)[]
            {
                (IngredientColor.Ember, new Color(0.878f, 0.251f, 0.251f)),
                (IngredientColor.Frost, new Color(0.251f, 0.502f, 0.878f)),
                (IngredientColor.Vine, new Color(0.251f, 0.690f, 0.251f)),
                (IngredientColor.Sun, new Color(0.878f, 0.753f, 0.125f)),
                (IngredientColor.Shadow, new Color(0.502f, 0.251f, 0.753f))
            };

            const int size = 64;
            const string folder = "Assets/_Project/Art/Tokens";

            if (!AssetDatabase.IsValidFolder("Assets/_Project/Art/Tokens"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Project/Art"))
                    AssetDatabase.CreateFolder("Assets/_Project", "Art");
                AssetDatabase.CreateFolder("Assets/_Project/Art", "Tokens");
            }

            foreach (var (type, color) in colors)
            {
                var texture = CreateCircleTexture(size, color);
                var path = $"{folder}/Token_{type}.png";
                System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path);

                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spritePixelsPerUnit = 64;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.SaveAndReimport();
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"[Brew] Generated {colors.Length} placeholder token sprites in {folder}/");
        }

        private static Texture2D CreateCircleTexture(int size, Color fillColor)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size / 2f;
            float radius = size / 2f - 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist < radius - 1)
                        tex.SetPixel(x, y, fillColor);
                    else if (dist < radius)
                        tex.SetPixel(x, y, Color.Lerp(fillColor, Color.clear, dist - radius + 1));
                    else
                        tex.SetPixel(x, y, Color.clear);
                }
            }

            tex.Apply();
            return tex;
        }

        private static void CreateCamera()
        {
            var existing = Camera.main;
            if (existing != null) return;

            var cameraGo = new GameObject("Main Camera");
            var cam = cameraGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 7f;
            cam.backgroundColor = new Color(0.12f, 0.12f, 0.18f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.tag = "MainCamera";
            cameraGo.transform.position = new Vector3(0, 0, -10);
        }

        private static TokenView CreateTokenPrefab()
        {
            const string prefabPath = "Assets/_Project/Prefabs/Tokens/Token.prefab";

            var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
                return existingPrefab.GetComponent<TokenView>();

            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/Tokens"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs"))
                    AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Tokens");
            }

            var go = new GameObject("Token");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Default";
            var tokenView = go.AddComponent<TokenView>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);

            return prefab.GetComponent<TokenView>();
        }

        private static void CreateBoardPresenter(TokenView tokenPrefab)
        {
            var existing = Object.FindAnyObjectByType<BoardPresenter>();
            if (existing != null) return;

            var boardGo = new GameObject("Board");
            var presenter = boardGo.AddComponent<BoardPresenter>();
            var inputController = boardGo.AddComponent<InputController>();

            var container = new GameObject("TokenContainer");
            container.transform.SetParent(boardGo.transform);

            var so = new SerializedObject(presenter);
            so.FindProperty("_tokenPrefab").objectReferenceValue = tokenPrefab;
            so.FindProperty("_tokenContainer").objectReferenceValue = container.transform;
            so.FindProperty("_inputController").objectReferenceValue = inputController;
            so.ApplyModifiedProperties();

            Debug.Log("[Brew] Created Board GameObject with BoardPresenter and InputController.");
        }
    }
}
