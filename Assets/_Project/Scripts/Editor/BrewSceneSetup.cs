using Brew.Core;
using Brew.Data;
using Brew.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Brew.Editor
{
    public static class BrewSceneSetup
    {
        private static readonly Color WorkshopWalnut = new(0.243f, 0.173f, 0.118f); // #3E2C1E
        private static readonly Color WarmCream = new(0.961f, 0.925f, 0.843f);       // #F5ECD7
        private static readonly Color CauldronAmber = new(0.910f, 0.643f, 0.290f);   // #E8A44A
        private static readonly Color Ink = new(0.180f, 0.125f, 0.098f);             // #2E2019

        [MenuItem("Brew/Create Gameplay Scene")]
        public static void CreateGameplayScene()
        {
            const string scenesFolder = "Assets/_Project/Scenes";
            const string scenePath = scenesFolder + "/Gameplay.unity";

            if (!AssetDatabase.IsValidFolder("Assets/_Project/Scenes"))
                AssetDatabase.CreateFolder("Assets/_Project", "Scenes");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SetupGameplayScene();
            EditorSceneManager.SaveScene(scene, scenePath);

            var buildScenes = EditorBuildSettings.scenes;
            bool alreadyInBuild = false;
            foreach (var s in buildScenes)
            {
                if (s.path == scenePath) { alreadyInBuild = true; break; }
            }
            if (!alreadyInBuild)
            {
                var newScenes = new EditorBuildSettingsScene[buildScenes.Length + 1];
                buildScenes.CopyTo(newScenes, 0);
                newScenes[buildScenes.Length] = new EditorBuildSettingsScene(scenePath, true);
                EditorBuildSettings.scenes = newScenes;
            }

            Debug.Log($"[Brew] Created Gameplay scene at {scenePath} and added to build settings.");
        }

        [MenuItem("Brew/Setup Gameplay Scene")]
        public static void SetupGameplayScene()
        {
            CreateCamera();
            CreateUIPrefabs();
            var tokenPrefab = CreateTokenPrefab();
            var boardGo = CreateBoardPresenter(tokenPrefab);

            var canvas = CreateCanvas();
            var hudPanel = CreateHudPanel(canvas.transform);
            var levelSelectPanel = CreateLevelSelectPanel(canvas.transform);
            var levelCompletePanel = CreateLevelCompletePanel(canvas.transform);
            var levelFailPanel = CreateLevelFailPanel(canvas.transform);
            var boosterBarPanel = CreateBoosterBarPanel(canvas.transform);
            var gameplayPanel = CreateGameplayPanel(canvas.transform);

            CreateEventSystem();
            var flowController = CreateGameFlowController(
                boardGo, hudPanel, levelSelectPanel,
                levelCompletePanel, levelFailPanel, gameplayPanel);

            Debug.Log("[Brew] Gameplay scene setup complete.");
        }

        [MenuItem("Brew/Generate Placeholder Token Sprites")]
        public static void GeneratePlaceholderSprites()
        {
            var colors = new (IngredientColor type, Color color)[]
            {
                (IngredientColor.Ember, new Color(0.878f, 0.353f, 0.227f)),
                (IngredientColor.Frost, new Color(0.435f, 0.722f, 0.851f)),
                (IngredientColor.Vine, new Color(0.427f, 0.686f, 0.369f)),
                (IngredientColor.Sun, new Color(0.949f, 0.780f, 0.271f)),
                (IngredientColor.Shadow, new Color(0.420f, 0.306f, 0.608f))
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
            cam.backgroundColor = WorkshopWalnut;
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

            EnsureFolder("Assets/_Project/Prefabs/Tokens");

            var go = new GameObject("Token");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Default";
            go.AddComponent<TokenView>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);

            return prefab.GetComponent<TokenView>();
        }

        private static void CreateUIPrefabs()
        {
            CreateRecipeVialPrefab();
            CreateLevelButtonPrefab();
        }

        private static void CreateRecipeVialPrefab()
        {
            const string prefabPath = "Assets/_Project/Prefabs/UI/RecipeVialUI.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null) return;

            EnsureFolder("Assets/_Project/Prefabs/UI");

            var go = new GameObject("RecipeVialUI", typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(60, 120);

            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(go.transform, false);
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            var bgImg = bg.GetComponent<Image>();
            bgImg.color = new Color(1f, 1f, 1f, 0.15f);

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(go.transform, false);
            var fillRt = fill.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.sizeDelta = Vector2.zero;
            var fillImg = fill.GetComponent<Image>();
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Vertical;
            fillImg.fillOrigin = 0;

            var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(go.transform, false);
            var iconRt = icon.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.5f, 0.8f);
            iconRt.anchorMax = new Vector2(0.5f, 0.8f);
            iconRt.sizeDelta = new Vector2(30, 30);

            var countText = new GameObject("Count", typeof(RectTransform), typeof(Text));
            countText.transform.SetParent(go.transform, false);
            var countRt = countText.GetComponent<RectTransform>();
            countRt.anchorMin = new Vector2(0.5f, 0.1f);
            countRt.anchorMax = new Vector2(0.5f, 0.1f);
            countRt.sizeDelta = new Vector2(50, 30);
            var countTxt = countText.GetComponent<Text>();
            countTxt.text = "0";
            countTxt.alignment = TextAnchor.MiddleCenter;
            countTxt.fontSize = 18;
            countTxt.color = Color.white;

            var stamp = new GameObject("CompleteStamp", typeof(RectTransform), typeof(Image));
            stamp.transform.SetParent(go.transform, false);
            var stampRt = stamp.GetComponent<RectTransform>();
            stampRt.anchorMin = new Vector2(0.5f, 0.5f);
            stampRt.anchorMax = new Vector2(0.5f, 0.5f);
            stampRt.sizeDelta = new Vector2(50, 50);
            stamp.SetActive(false);

            var vialComponent = go.AddComponent<RecipeVialUI>();
            var so = new SerializedObject(vialComponent);
            so.FindProperty("_fillImage").objectReferenceValue = fillImg;
            so.FindProperty("_countText").objectReferenceValue = countTxt;
            so.FindProperty("_iconImage").objectReferenceValue = icon.GetComponent<Image>();
            so.FindProperty("_completeStamp").objectReferenceValue = stamp;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            Debug.Log("[Brew] Created RecipeVialUI prefab.");
        }

        private static void CreateLevelButtonPrefab()
        {
            const string prefabPath = "Assets/_Project/Prefabs/UI/LevelButton.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null) return;

            EnsureFolder("Assets/_Project/Prefabs/UI");

            var go = new GameObject("LevelButton", typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(80, 80);

            var btnImg = go.AddComponent<Image>();
            btnImg.color = CauldronAmber;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = btnImg;

            var numText = new GameObject("LevelNumber", typeof(RectTransform), typeof(Text));
            numText.transform.SetParent(go.transform, false);
            var numRt = numText.GetComponent<RectTransform>();
            numRt.anchorMin = Vector2.zero;
            numRt.anchorMax = Vector2.one;
            numRt.sizeDelta = Vector2.zero;
            var numTxt = numText.GetComponent<Text>();
            numTxt.text = "1";
            numTxt.alignment = TextAnchor.MiddleCenter;
            numTxt.fontSize = 24;
            numTxt.fontStyle = FontStyle.Bold;
            numTxt.color = Ink;

            var lockIcon = new GameObject("LockIcon", typeof(RectTransform), typeof(Image));
            lockIcon.transform.SetParent(go.transform, false);
            var lockRt = lockIcon.GetComponent<RectTransform>();
            lockRt.anchorMin = new Vector2(0.5f, 0.5f);
            lockRt.anchorMax = new Vector2(0.5f, 0.5f);
            lockRt.sizeDelta = new Vector2(30, 30);
            lockIcon.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 0.8f);

            var starsContainer = new GameObject("Stars", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            starsContainer.transform.SetParent(go.transform, false);
            var starsRt = starsContainer.GetComponent<RectTransform>();
            starsRt.anchorMin = new Vector2(0.5f, 0f);
            starsRt.anchorMax = new Vector2(0.5f, 0f);
            starsRt.pivot = new Vector2(0.5f, 0f);
            starsRt.sizeDelta = new Vector2(60, 16);
            starsRt.anchoredPosition = new Vector2(0, 4);
            var layout = starsContainer.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 2;
            layout.childAlignment = TextAnchor.MiddleCenter;

            var starObjects = new GameObject[3];
            for (int i = 0; i < 3; i++)
            {
                var star = new GameObject($"Star{i + 1}", typeof(RectTransform), typeof(Image));
                star.transform.SetParent(starsContainer.transform, false);
                var starRt = star.GetComponent<RectTransform>();
                starRt.sizeDelta = new Vector2(16, 16);
                star.GetComponent<Image>().color = new Color(1f, 0.85f, 0.2f);
                star.SetActive(false);
                starObjects[i] = star;
            }

            var levelButton = go.AddComponent<LevelButton>();
            var lbSo = new SerializedObject(levelButton);
            lbSo.FindProperty("_levelNumberText").objectReferenceValue = numTxt;
            lbSo.FindProperty("_lockIcon").objectReferenceValue = lockIcon;
            lbSo.FindProperty("_button").objectReferenceValue = btn;
            var starsProp = lbSo.FindProperty("_starObjects");
            starsProp.arraySize = 3;
            for (int i = 0; i < 3; i++)
                starsProp.GetArrayElementAtIndex(i).objectReferenceValue = starObjects[i];
            lbSo.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            Debug.Log("[Brew] Created LevelButton prefab.");
        }

        private static GameObject CreateBoardPresenter(TokenView tokenPrefab)
        {
            var existing = Object.FindAnyObjectByType<BoardPresenter>();
            if (existing != null) return existing.gameObject;

            var boardGo = new GameObject("Board");
            var presenter = boardGo.AddComponent<BoardPresenter>();
            var inputController = boardGo.AddComponent<InputController>();

            var container = new GameObject("TokenContainer");
            container.transform.SetParent(boardGo.transform);

            var so = new SerializedObject(presenter);
            so.FindProperty("_tokenPrefab").objectReferenceValue = tokenPrefab;
            so.FindProperty("_tokenContainer").objectReferenceValue = container.transform;
            so.FindProperty("_inputController").objectReferenceValue = inputController;

            var boardConfig = LoadOrCreateBoardConfig();
            so.FindProperty("_boardConfig").objectReferenceValue = boardConfig;
            so.ApplyModifiedProperties();

            return boardGo;
        }

        private static Canvas CreateCanvas()
        {
            var existing = Object.FindAnyObjectByType<Canvas>();
            if (existing != null) return existing;

            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            return canvas;
        }

        private static GameObject CreateHudPanel(Transform parent)
        {
            var hud = CreateUIPanel("HUD", parent);
            var hudRt = hud.GetComponent<RectTransform>();
            hudRt.anchorMin = new Vector2(0, 0.85f);
            hudRt.anchorMax = Vector2.one;
            hudRt.offsetMin = new Vector2(20, 0);
            hudRt.offsetMax = new Vector2(-20, -20);

            var moveCount = CreateTextObject("MoveCount", hud.transform, "15", 36, FontStyle.Bold);
            var moveRt = moveCount.GetComponent<RectTransform>();
            moveRt.anchorMin = new Vector2(0, 0.5f);
            moveRt.anchorMax = new Vector2(0.2f, 1f);
            moveRt.offsetMin = Vector2.zero;
            moveRt.offsetMax = Vector2.zero;

            var score = CreateTextObject("Score", hud.transform, "0", 24, FontStyle.Bold);
            var scoreRt = score.GetComponent<RectTransform>();
            scoreRt.anchorMin = new Vector2(0.3f, 0.5f);
            scoreRt.anchorMax = new Vector2(0.7f, 1f);
            scoreRt.offsetMin = Vector2.zero;
            scoreRt.offsetMax = Vector2.zero;
            score.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

            var vialContainer = new GameObject("VialContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            vialContainer.transform.SetParent(hud.transform, false);
            var vialRt = vialContainer.GetComponent<RectTransform>();
            vialRt.anchorMin = new Vector2(0.7f, 0f);
            vialRt.anchorMax = Vector2.one;
            vialRt.offsetMin = Vector2.zero;
            vialRt.offsetMax = Vector2.zero;
            var vialLayout = vialContainer.GetComponent<HorizontalLayoutGroup>();
            vialLayout.spacing = 8;
            vialLayout.childAlignment = TextAnchor.MiddleRight;

            var hudCtrl = hud.AddComponent<HudController>();
            var hudSo = new SerializedObject(hudCtrl);
            hudSo.FindProperty("_moveCountText").objectReferenceValue = moveCount.GetComponent<Text>();
            hudSo.FindProperty("_scoreText").objectReferenceValue = score.GetComponent<Text>();
            hudSo.FindProperty("_vialContainer").objectReferenceValue = vialContainer.transform;

            var vialPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/RecipeVialUI.prefab");
            if (vialPrefab != null)
                hudSo.FindProperty("_vialPrefab").objectReferenceValue = vialPrefab.GetComponent<RecipeVialUI>();
            hudSo.ApplyModifiedProperties();

            return hud;
        }

        private static GameObject CreateLevelSelectPanel(Transform parent)
        {
            var panel = CreateUIPanel("LevelSelectScreen", parent);
            var panelRt = panel.GetComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;

            var bg = panel.AddComponent<Image>();
            bg.color = WorkshopWalnut;

            var title = CreateTextObject("Title", panel.transform, "Select Level", 32, FontStyle.Bold);
            var titleRt = title.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 0.9f);
            titleRt.anchorMax = Vector2.one;
            titleRt.offsetMin = Vector2.zero;
            titleRt.offsetMax = new Vector2(0, -40);
            title.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

            var scrollView = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect));
            scrollView.transform.SetParent(panel.transform, false);
            var svRt = scrollView.GetComponent<RectTransform>();
            svRt.anchorMin = new Vector2(0.05f, 0.02f);
            svRt.anchorMax = new Vector2(0.95f, 0.88f);
            svRt.offsetMin = Vector2.zero;
            svRt.offsetMax = Vector2.zero;

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollView.transform, false);
            var vpRt = viewport.GetComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero;
            vpRt.anchorMax = Vector2.one;
            vpRt.sizeDelta = Vector2.zero;
            viewport.GetComponent<Image>().color = new Color(1, 1, 1, 0.01f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.sizeDelta = new Vector2(0, 0);
            var gridLayout = content.GetComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(80, 80);
            gridLayout.spacing = new Vector2(12, 12);
            gridLayout.padding = new RectOffset(10, 10, 10, 10);
            gridLayout.childAlignment = TextAnchor.UpperCenter;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 5;
            var fitter = content.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var sv = scrollView.GetComponent<ScrollRect>();
            sv.viewport = vpRt;
            sv.content = contentRt;
            sv.horizontal = false;
            sv.vertical = true;

            var levelSelect = panel.AddComponent<LevelSelectScreen>();
            var lsSo = new SerializedObject(levelSelect);
            lsSo.FindProperty("_levelButtonContainer").objectReferenceValue = content.transform;
            var btnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/LevelButton.prefab");
            if (btnPrefab != null)
                lsSo.FindProperty("_levelButtonPrefab").objectReferenceValue = btnPrefab.GetComponent<LevelButton>();
            lsSo.ApplyModifiedProperties();

            return panel;
        }

        private static GameObject CreateLevelCompletePanel(Transform parent)
        {
            var panel = CreateUIPanel("LevelCompleteScreen", parent);
            var panelRt = panel.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.1f, 0.25f);
            panelRt.anchorMax = new Vector2(0.9f, 0.75f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;

            var bg = panel.AddComponent<Image>();
            bg.color = WarmCream;

            var titleObj = CreateTextObject("Title", panel.transform, "Level Complete!", 28, FontStyle.Bold);
            titleObj.GetComponent<Text>().color = Ink;
            var tRt = titleObj.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0, 0.8f);
            tRt.anchorMax = Vector2.one;
            tRt.offsetMin = Vector2.zero;
            tRt.offsetMax = Vector2.zero;
            titleObj.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

            var scoreText = CreateTextObject("ScoreText", panel.transform, "0", 22, FontStyle.Normal);
            scoreText.GetComponent<Text>().color = Ink;
            var sRt = scoreText.GetComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0.1f, 0.55f);
            sRt.anchorMax = new Vector2(0.9f, 0.7f);
            sRt.offsetMin = Vector2.zero;
            sRt.offsetMax = Vector2.zero;
            scoreText.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

            var bonusText = CreateTextObject("BonusText", panel.transform, "", 18, FontStyle.Normal);
            bonusText.GetComponent<Text>().color = Ink;
            var bRt = bonusText.GetComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0.1f, 0.45f);
            bRt.anchorMax = new Vector2(0.9f, 0.55f);
            bRt.offsetMin = Vector2.zero;
            bRt.offsetMax = Vector2.zero;
            bonusText.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

            var starsRow = new GameObject("Stars", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            starsRow.transform.SetParent(panel.transform, false);
            var starsRt = starsRow.GetComponent<RectTransform>();
            starsRt.anchorMin = new Vector2(0.2f, 0.7f);
            starsRt.anchorMax = new Vector2(0.8f, 0.82f);
            starsRt.offsetMin = Vector2.zero;
            starsRt.offsetMax = Vector2.zero;
            var sl = starsRow.GetComponent<HorizontalLayoutGroup>();
            sl.spacing = 10;
            sl.childAlignment = TextAnchor.MiddleCenter;

            var starObjects = new GameObject[3];
            for (int i = 0; i < 3; i++)
            {
                var star = new GameObject($"Star{i + 1}", typeof(RectTransform), typeof(Image));
                star.transform.SetParent(starsRow.transform, false);
                var starRt2 = star.GetComponent<RectTransform>();
                starRt2.sizeDelta = new Vector2(40, 40);
                star.GetComponent<Image>().color = new Color(1f, 0.85f, 0.2f);
                star.SetActive(false);
                starObjects[i] = star;
            }

            var nextBtn = CreateButton("NextButton", panel.transform, "Next Level", CauldronAmber);
            var nextRt = nextBtn.GetComponent<RectTransform>();
            nextRt.anchorMin = new Vector2(0.55f, 0.08f);
            nextRt.anchorMax = new Vector2(0.9f, 0.22f);
            nextRt.offsetMin = Vector2.zero;
            nextRt.offsetMax = Vector2.zero;

            var replayBtn = CreateButton("ReplayButton", panel.transform, "Replay", new Color(0.6f, 0.6f, 0.6f));
            var replayRt = replayBtn.GetComponent<RectTransform>();
            replayRt.anchorMin = new Vector2(0.1f, 0.08f);
            replayRt.anchorMax = new Vector2(0.45f, 0.22f);
            replayRt.offsetMin = Vector2.zero;
            replayRt.offsetMax = Vector2.zero;

            var completeScreen = panel.AddComponent<LevelCompleteScreen>();
            var csSo = new SerializedObject(completeScreen);
            csSo.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<Text>();
            csSo.FindProperty("_bonusText").objectReferenceValue = bonusText.GetComponent<Text>();
            csSo.FindProperty("_nextLevelButton").objectReferenceValue = nextBtn.GetComponent<Button>();
            csSo.FindProperty("_replayButton").objectReferenceValue = replayBtn.GetComponent<Button>();
            var starsProp = csSo.FindProperty("_starObjects");
            starsProp.arraySize = 3;
            for (int i = 0; i < 3; i++)
                starsProp.GetArrayElementAtIndex(i).objectReferenceValue = starObjects[i];
            csSo.ApplyModifiedProperties();

            panel.SetActive(false);
            return panel;
        }

        private static GameObject CreateLevelFailPanel(Transform parent)
        {
            var panel = CreateUIPanel("LevelFailScreen", parent);
            var panelRt = panel.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.1f, 0.3f);
            panelRt.anchorMax = new Vector2(0.9f, 0.7f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;

            var bg = panel.AddComponent<Image>();
            bg.color = WarmCream;

            var msgText = CreateTextObject("Message", panel.transform, "Out of Moves!", 26, FontStyle.Bold);
            msgText.GetComponent<Text>().color = Ink;
            var mRt = msgText.GetComponent<RectTransform>();
            mRt.anchorMin = new Vector2(0, 0.5f);
            mRt.anchorMax = Vector2.one;
            mRt.offsetMin = Vector2.zero;
            mRt.offsetMax = Vector2.zero;
            msgText.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

            var retryBtn = CreateButton("RetryButton", panel.transform, "Retry", CauldronAmber);
            var rRt = retryBtn.GetComponent<RectTransform>();
            rRt.anchorMin = new Vector2(0.25f, 0.1f);
            rRt.anchorMax = new Vector2(0.75f, 0.3f);
            rRt.offsetMin = Vector2.zero;
            rRt.offsetMax = Vector2.zero;

            var failScreen = panel.AddComponent<LevelFailScreen>();
            var fsSo = new SerializedObject(failScreen);
            fsSo.FindProperty("_messageText").objectReferenceValue = msgText.GetComponent<Text>();
            fsSo.FindProperty("_retryButton").objectReferenceValue = retryBtn.GetComponent<Button>();
            fsSo.ApplyModifiedProperties();

            panel.SetActive(false);
            return panel;
        }

        private static GameObject CreateBoosterBarPanel(Transform parent)
        {
            var panel = CreateUIPanel("BoosterBar", parent);
            var panelRt = panel.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.15f, 0.02f);
            panelRt.anchorMax = new Vector2(0.85f, 0.08f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;

            var hlg = panel.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;

            string[] boosterNames = { "Shake", "Catalyst", "ExtraMoves" };
            foreach (var name in boosterNames)
            {
                var slot = new GameObject(name, typeof(RectTransform), typeof(Image));
                slot.transform.SetParent(panel.transform, false);
                slot.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.2f);
                var slotRt = slot.GetComponent<RectTransform>();
                slotRt.sizeDelta = new Vector2(70, 50);

                var label = CreateTextObject("Label", slot.transform, name, 12, FontStyle.Normal);
                var lrt = label.GetComponent<RectTransform>();
                lrt.anchorMin = Vector2.zero;
                lrt.anchorMax = Vector2.one;
                lrt.sizeDelta = Vector2.zero;
                label.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            }

            return panel;
        }

        private static GameObject CreateGameplayPanel(Transform parent)
        {
            var panel = CreateUIPanel("GameplayPanel", parent);
            var panelRt = panel.GetComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;
            panel.SetActive(false);
            return panel;
        }

        private static GameFlowController CreateGameFlowController(
            GameObject boardGo,
            GameObject hudPanel,
            GameObject levelSelectPanel,
            GameObject levelCompletePanel,
            GameObject levelFailPanel,
            GameObject gameplayPanel)
        {
            var existing = Object.FindAnyObjectByType<GameFlowController>();
            if (existing != null) return existing;

            var go = new GameObject("GameFlowController");
            var controller = go.AddComponent<GameFlowController>();

            var so = new SerializedObject(controller);
            so.FindProperty("_levelSelectPanel").objectReferenceValue = levelSelectPanel;
            so.FindProperty("_gameplayPanel").objectReferenceValue = gameplayPanel;
            so.FindProperty("_levelCompleteScreen").objectReferenceValue = levelCompletePanel.GetComponent<LevelCompleteScreen>();
            so.FindProperty("_levelFailScreen").objectReferenceValue = levelFailPanel.GetComponent<LevelFailScreen>();
            so.FindProperty("_boardPresenter").objectReferenceValue = boardGo.GetComponent<BoardPresenter>();
            so.FindProperty("_hudController").objectReferenceValue = hudPanel.GetComponent<HudController>();
            so.FindProperty("_levelSelectScreen").objectReferenceValue = levelSelectPanel.GetComponent<LevelSelectScreen>();
            so.ApplyModifiedProperties();

            return controller;
        }

        private static void CreateEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null) return;

            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        private static GameObject CreateUIPanel(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static GameObject CreateTextObject(string name, Transform parent, string text, int fontSize, FontStyle style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var txt = go.GetComponent<Text>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.fontStyle = style;
            txt.color = Color.white;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return go;
        }

        private static GameObject CreateButton(string name, Transform parent, string label, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.color = color;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            var text = CreateTextObject("Label", go.transform, label, 18, FontStyle.Bold);
            var tRt = text.GetComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.sizeDelta = Vector2.zero;
            text.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            text.GetComponent<Text>().color = Color.white;

            return go;
        }

        private static BoardConfigSO LoadOrCreateBoardConfig()
        {
            const string configPath = "Assets/_Project/ScriptableObjects/Config/BoardConfig.asset";

            var existing = AssetDatabase.LoadAssetAtPath<BoardConfigSO>(configPath);
            if (existing != null)
                return existing;

            EnsureFolder("Assets/_Project/ScriptableObjects/Config");

            var config = ScriptableObject.CreateInstance<BoardConfigSO>();
            AssetDatabase.CreateAsset(config, configPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Brew] Created BoardConfig asset at {configPath}");
            return config;
        }

        private static void EnsureFolder(string path)
        {
            var parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
