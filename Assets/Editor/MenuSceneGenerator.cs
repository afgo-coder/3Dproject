using System.Collections.Generic;
using System.IO;
using MiniMart.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniMart.Editor
{
    public static class MenuSceneGenerator
    {
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string SettingsScenePath = "Assets/Scenes/SettingsScene.unity";
        private const string GameScenePath = "Assets/Scenes/Game.unity";
        private const string MainBackgroundPath = "Assets/Image/닌자 편의점 배경.png";
        private const string SettingsBackgroundPath = "Assets/Image/닌자 편의점 배경2.png";
        private const string FontAssetPath = "Assets/Font/RiaSans-Bold SDF Dynamic.asset";

        [MenuItem("Tools/MiniMart/Create Menu Scenes")]
        public static void CreateMenuScenes()
        {
            Directory.CreateDirectory("Assets/Scenes");

            CreateMainMenuScene();
            CreateSettingsScene();
            AddScenesToBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("MiniMart", "메인 메뉴와 설정 씬을 다시 생성했습니다.", "확인");
        }

        private static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            EnsureEventSystem();
            Canvas canvas = CreateCanvas();
            TMP_FontAsset fontAsset = LoadFontAsset();

            CreateBackground(canvas.transform, MainBackgroundPath);
            CreateDimOverlay(canvas.transform, new Color(0.04f, 0.08f, 0.11f, 0.55f));
            CreateAccentStripe(canvas.transform, new Color(0.12f, 0.58f, 0.50f, 0.95f));

            GameObject card = CreatePanel(
                canvas.transform,
                "MainCard",
                new Vector2(0.28f, 0.5f),
                new Vector2(640f, 700f),
                new Color(0.05f, 0.09f, 0.12f, 0.78f));

            CreateTitle(card.transform, fontAsset, "MINI MARKET", new Vector2(0f, 235f), 54);
            CreateBodyText(card.transform, fontAsset, "작은 편의점을 키워나가는 3D 운영 시뮬레이션", new Vector2(0f, 180f), new Vector2(520f, 42f), 22);
            CreateBodyText(card.transform, fontAsset, "발주, 진열, 계산, 확장을 반복하며 나만의 편의점을 키워보세요.", new Vector2(0f, 130f), new Vector2(540f, 58f), 18, new Color(0.82f, 0.90f, 0.90f, 0.92f));

            GameObject controllerObject = new GameObject("MainMenuController");
            MainMenuController controller = controllerObject.AddComponent<MainMenuController>();

            GameObject continueButton = CreateButton(card.transform, fontAsset, "ContinueButton", "이어하기", new Vector2(0f, 30f), new Vector2(320f, 66f), controller, nameof(MainMenuController.ContinueGame));
            CreateButton(card.transform, fontAsset, "StartButton", "새 게임", new Vector2(0f, -55f), new Vector2(320f, 66f), controller, nameof(MainMenuController.StartGame));
            CreateButton(card.transform, fontAsset, "SettingsButton", "설정", new Vector2(0f, -140f), new Vector2(320f, 66f), controller, nameof(MainMenuController.OpenSettings));
            CreateButton(card.transform, fontAsset, "QuitButton", "종료", new Vector2(0f, -225f), new Vector2(320f, 66f), controller, nameof(MainMenuController.QuitGame));

            CreateBodyText(card.transform, fontAsset, "팁: 저장 데이터가 있으면 이어하기 버튼이 활성화됩니다.", new Vector2(0f, -305f), new Vector2(520f, 36f), 16, new Color(0.74f, 0.84f, 0.84f, 0.9f));

            SerializedObject controllerSerializedObject = new SerializedObject(controller);
            controllerSerializedObject.FindProperty("continueButton").objectReferenceValue = continueButton.GetComponent<Button>();
            controllerSerializedObject.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
        }

        private static void CreateSettingsScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            EnsureEventSystem();
            Canvas canvas = CreateCanvas();
            TMP_FontAsset fontAsset = LoadFontAsset();

            CreateBackground(canvas.transform, SettingsBackgroundPath);
            CreateDimOverlay(canvas.transform, new Color(0.05f, 0.08f, 0.11f, 0.68f));
            CreateAccentStripe(canvas.transform, new Color(0.10f, 0.50f, 0.46f, 0.95f));

            GameObject card = CreatePanel(
                canvas.transform,
                "SettingsCard",
                new Vector2(0.5f, 0.5f),
                new Vector2(760f, 620f),
                new Color(0.05f, 0.09f, 0.12f, 0.82f));

            CreateTitle(card.transform, fontAsset, "설정", new Vector2(0f, 220f), 46);
            CreateBodyText(card.transform, fontAsset, "배경음과 효과음 볼륨을 조절할 수 있습니다.", new Vector2(0f, 170f), new Vector2(520f, 36f), 18, new Color(0.82f, 0.90f, 0.90f, 0.92f));

            GameObject controllerObject = new GameObject("SettingsMenuController");
            SettingsMenuController controller = controllerObject.AddComponent<SettingsMenuController>();

            CreateLabeledSlider(card.transform, fontAsset, "배경음", new Vector2(0f, 65f), out Slider bgmSlider, out TextMeshProUGUI bgmValueText);
            CreateLabeledSlider(card.transform, fontAsset, "효과음", new Vector2(0f, -60f), out Slider sfxSlider, out TextMeshProUGUI sfxValueText);

            SerializedObject controllerSerializedObject = new SerializedObject(controller);
            controllerSerializedObject.FindProperty("bgmSlider").objectReferenceValue = bgmSlider;
            controllerSerializedObject.FindProperty("sfxSlider").objectReferenceValue = sfxSlider;
            controllerSerializedObject.FindProperty("bgmValueTmpText").objectReferenceValue = bgmValueText;
            controllerSerializedObject.FindProperty("sfxValueTmpText").objectReferenceValue = sfxValueText;
            controllerSerializedObject.ApplyModifiedPropertiesWithoutUndo();

            CreateButton(card.transform, fontAsset, "BackButton", "뒤로 가기", new Vector2(0f, -210f), new Vector2(260f, 64f), controller, nameof(SettingsMenuController.BackToMainMenu));

            EditorSceneManager.SaveScene(scene, SettingsScenePath);
        }

        private static Canvas CreateCanvas()
        {
            GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static TMP_FontAsset LoadFontAsset()
        {
            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (fontAsset == null)
            {
                throw new FileNotFoundException($"TMP 폰트를 찾을 수 없습니다: {FontAssetPath}");
            }

            return fontAsset;
        }

        private static void CreateBackground(Transform parent, string spritePath)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            GameObject background = new GameObject("Backdrop", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(parent, false);

            RectTransform rect = background.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = background.GetComponent<Image>();
            image.color = Color.white;
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
        }

        private static void CreateDimOverlay(Transform parent, Color color)
        {
            GameObject overlay = new GameObject("Overlay", typeof(RectTransform), typeof(Image));
            overlay.transform.SetParent(parent, false);

            RectTransform rect = overlay.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            overlay.GetComponent<Image>().color = color;
        }

        private static void CreateAccentStripe(Transform parent, Color color)
        {
            GameObject stripe = new GameObject("AccentStripe", typeof(RectTransform), typeof(Image));
            stripe.transform.SetParent(parent, false);

            RectTransform rect = stripe.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, 8f);
            rect.anchoredPosition = Vector2.zero;

            stripe.GetComponent<Image>().color = color;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 anchor, Vector2 size, Color color)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);

            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;

            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private static TextMeshProUGUI CreateTitle(Transform parent, TMP_FontAsset fontAsset, string text, Vector2 position, int fontSize)
        {
            return CreateTmpText(parent, "Title", text, fontAsset, position, new Vector2(520f, 64f), fontSize, FontStyles.Bold, TextAlignmentOptions.Center);
        }

        private static TextMeshProUGUI CreateBodyText(Transform parent, TMP_FontAsset fontAsset, string text, Vector2 position, Vector2 size, int fontSize, Color? color = null)
        {
            TextMeshProUGUI tmp = CreateTmpText(parent, "BodyText", text, fontAsset, position, size, fontSize, FontStyles.Normal, TextAlignmentOptions.Center);
            tmp.color = color ?? new Color(0.94f, 0.96f, 0.96f, 1f);
            return tmp;
        }

        private static TextMeshProUGUI CreateTmpText(
            Transform parent,
            string name,
            string text,
            TMP_FontAsset fontAsset,
            Vector2 position,
            Vector2 size,
            int fontSize,
            FontStyles fontStyle,
            TextAlignmentOptions alignment)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            TextMeshProUGUI tmp = textObject.GetComponent<TextMeshProUGUI>();
            tmp.font = fontAsset;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = fontStyle;
            tmp.color = new Color(0.96f, 0.96f, 0.92f, 1f);
            tmp.alignment = alignment;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            return tmp;
        }

        private static GameObject CreateButton(Transform parent, TMP_FontAsset fontAsset, string name, string label, Vector2 position, Vector2 size, Object target, string methodName)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.12f, 0.52f, 0.46f, 0.96f);

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.12f, 0.52f, 0.46f, 0.96f);
            colors.highlightedColor = new Color(0.20f, 0.68f, 0.60f, 1f);
            colors.pressedColor = new Color(0.08f, 0.37f, 0.31f, 1f);
            colors.selectedColor = colors.normalColor;
            button.colors = colors;

            TextMeshProUGUI labelText = CreateTmpText(buttonObject.transform, "Label", label, fontAsset, Vector2.zero, size, 24, FontStyles.Bold, TextAlignmentOptions.Center);
            labelText.color = Color.white;

            switch (methodName)
            {
                case nameof(MainMenuController.StartGame):
                    UnityEventTools.AddPersistentListener(button.onClick, ((MainMenuController)target).StartGame);
                    break;
                case nameof(MainMenuController.ContinueGame):
                    UnityEventTools.AddPersistentListener(button.onClick, ((MainMenuController)target).ContinueGame);
                    break;
                case nameof(MainMenuController.OpenSettings):
                    UnityEventTools.AddPersistentListener(button.onClick, ((MainMenuController)target).OpenSettings);
                    break;
                case nameof(MainMenuController.QuitGame):
                    UnityEventTools.AddPersistentListener(button.onClick, ((MainMenuController)target).QuitGame);
                    break;
                case nameof(SettingsMenuController.BackToMainMenu):
                    UnityEventTools.AddPersistentListener(button.onClick, ((SettingsMenuController)target).BackToMainMenu);
                    break;
            }

            return buttonObject;
        }

        private static void CreateLabeledSlider(Transform parent, TMP_FontAsset fontAsset, string label, Vector2 position, out Slider slider, out TextMeshProUGUI valueText)
        {
            CreateTmpText(parent, $"{label}Label", label, fontAsset, new Vector2(-220f, position.y + 24f), new Vector2(160f, 32f), 24, FontStyles.Bold, TextAlignmentOptions.Left);

            GameObject sliderObject = new GameObject($"{label}Slider", typeof(RectTransform), typeof(Slider));
            sliderObject.transform.SetParent(parent, false);

            RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
            sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
            sliderRect.pivot = new Vector2(0.5f, 0.5f);
            sliderRect.sizeDelta = new Vector2(380f, 32f);
            sliderRect.anchoredPosition = position;

            GameObject background = new GameObject("Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(sliderObject.transform, false);
            RectTransform backgroundRect = background.GetComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0f, 0.25f);
            backgroundRect.anchorMax = new Vector2(1f, 0.75f);
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            background.GetComponent<Image>().color = new Color(0.18f, 0.24f, 0.28f, 1f);

            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObject.transform, false);
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
            fillAreaRect.offsetMin = new Vector2(10f, 0f);
            fillAreaRect.offsetMax = new Vector2(-10f, 0f);

            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            fill.GetComponent<Image>().color = new Color(0.15f, 0.62f, 0.52f, 1f);

            GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(sliderObject.transform, false);
            RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(10f, 0f);
            handleAreaRect.offsetMax = new Vector2(-10f, 0f);

            GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(22f, 42f);
            handle.GetComponent<Image>().color = new Color(0.94f, 0.96f, 0.96f, 1f);

            slider = sliderObject.GetComponent<Slider>();
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.7f;

            valueText = CreateTmpText(parent, $"{label}Value", "70%", fontAsset, new Vector2(250f, position.y + 24f), new Vector2(120f, 32f), 20, FontStyles.Bold, TextAlignmentOptions.Left);
        }

        private static void AddScenesToBuildSettings()
        {
            EditorBuildSettingsScene[] existing = EditorBuildSettings.scenes;
            bool hasMainMenu = false;
            bool hasSettings = false;
            bool hasGameScene = false;

            for (int i = 0; i < existing.Length; i++)
            {
                if (existing[i].path == MainMenuScenePath) hasMainMenu = true;
                if (existing[i].path == SettingsScenePath) hasSettings = true;
                if (existing[i].path == GameScenePath) hasGameScene = true;
            }

            List<EditorBuildSettingsScene> merged = new(existing);
            if (!hasMainMenu)
            {
                merged.Add(new EditorBuildSettingsScene(MainMenuScenePath, true));
            }

            if (!hasSettings)
            {
                merged.Add(new EditorBuildSettingsScene(SettingsScenePath, true));
            }

            if (!hasGameScene)
            {
                merged.Add(new EditorBuildSettingsScene(GameScenePath, true));
            }

            EditorBuildSettings.scenes = merged.ToArray();
        }
    }
}
