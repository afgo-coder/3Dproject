using System.Collections.Generic;
using System.IO;
using MiniMart.UI;
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
        private const string SampleScenePath = "Assets/Scenes/SampleScene.unity";

        [MenuItem("Tools/MiniMart/Create Menu Scenes")]
        public static void CreateMenuScenes()
        {
            Directory.CreateDirectory("Assets/Scenes");

            CreateMainMenuScene();
            CreateSettingsScene();
            AddScenesToBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("MiniMart", "MainMenu와 SettingsScene 생성을 완료했습니다.", "확인");
        }

        private static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            EnsureEventSystem();
            Canvas canvas = CreateCanvas();
            CreateFullScreenPanel(canvas.transform, "Backdrop", new Color(0.09f, 0.13f, 0.16f, 0.92f));

            CreateText(canvas.transform, "Title", "Mini Convenience Store Simulator", new Vector2(0.5f, 0.78f), new Vector2(700f, 90f), 36, FontStyle.Bold);
            CreateText(canvas.transform, "Subtitle", "작은 편의점을 키워나가는 운영 시뮬레이션", new Vector2(0.5f, 0.70f), new Vector2(560f, 40f), 20, FontStyle.Normal);

            GameObject controllerObject = new GameObject("MainMenuController");
            MainMenuController controller = controllerObject.AddComponent<MainMenuController>();

            CreateButton(canvas.transform, "StartButton", "게임 시작", new Vector2(0.5f, 0.50f), new Vector2(220f, 56f), controller, nameof(MainMenuController.StartGame));
            CreateButton(canvas.transform, "SettingsButton", "설정", new Vector2(0.5f, 0.40f), new Vector2(220f, 56f), controller, nameof(MainMenuController.OpenSettings));
            CreateButton(canvas.transform, "QuitButton", "종료", new Vector2(0.5f, 0.30f), new Vector2(220f, 56f), controller, nameof(MainMenuController.QuitGame));

            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
        }

        private static void CreateSettingsScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            EnsureEventSystem();
            Canvas canvas = CreateCanvas();
            CreateFullScreenPanel(canvas.transform, "Backdrop", new Color(0.13f, 0.12f, 0.10f, 0.94f));

            CreateText(canvas.transform, "Title", "설정", new Vector2(0.5f, 0.78f), new Vector2(320f, 80f), 34, FontStyle.Bold);
            CreateText(canvas.transform, "Placeholder", "설정 화면은 다음 단계에서 확장할 수 있습니다.", new Vector2(0.5f, 0.58f), new Vector2(580f, 40f), 20, FontStyle.Normal);

            GameObject controllerObject = new GameObject("SettingsMenuController");
            SettingsMenuController controller = controllerObject.AddComponent<SettingsMenuController>();

            CreateButton(canvas.transform, "BackButton", "뒤로 가기", new Vector2(0.5f, 0.32f), new Vector2(220f, 56f), controller, nameof(SettingsMenuController.BackToMainMenu));

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

        private static void CreateFullScreenPanel(Transform parent, string name, Color color)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);

            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            panel.GetComponent<Image>().color = color;
        }

        private static GameObject CreateText(Transform parent, string name, string text, Vector2 anchor, Vector2 size, int fontSize, FontStyle fontStyle)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;

            Text label = textObject.GetComponent<Text>();
            label.text = text;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.fontStyle = fontStyle;
            label.color = new Color(0.96f, 0.96f, 0.92f, 1f);
            label.alignment = TextAnchor.MiddleCenter;
            return textObject;
        }

        private static void CreateButton(Transform parent, string name, string label, Vector2 anchor, Vector2 size, Object target, string methodName)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.18f, 0.55f, 0.48f, 0.96f);

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.18f, 0.55f, 0.48f, 0.96f);
            colors.highlightedColor = new Color(0.24f, 0.68f, 0.60f, 1f);
            colors.pressedColor = new Color(0.12f, 0.40f, 0.35f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            GameObject textObject = CreateText(buttonObject.transform, "Label", label, new Vector2(0.5f, 0.5f), size, 22, FontStyle.Bold);
            textObject.GetComponent<Text>().color = Color.white;

            switch (methodName)
            {
                case nameof(MainMenuController.StartGame):
                    UnityEventTools.AddPersistentListener(button.onClick, ((MainMenuController)target).StartGame);
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
        }

        private static void AddScenesToBuildSettings()
        {
            EditorBuildSettingsScene[] existing = EditorBuildSettings.scenes;
            bool hasMainMenu = false;
            bool hasSettings = false;
            bool hasSampleScene = false;

            for (int i = 0; i < existing.Length; i++)
            {
                if (existing[i].path == MainMenuScenePath) hasMainMenu = true;
                if (existing[i].path == SettingsScenePath) hasSettings = true;
                if (existing[i].path == SampleScenePath) hasSampleScene = true;
            }

            List<EditorBuildSettingsScene> merged = new List<EditorBuildSettingsScene>(existing);
            if (!hasMainMenu)
            {
                merged.Add(new EditorBuildSettingsScene(MainMenuScenePath, true));
            }

            if (!hasSettings)
            {
                merged.Add(new EditorBuildSettingsScene(SettingsScenePath, true));
            }

            if (!hasSampleScene)
            {
                merged.Add(new EditorBuildSettingsScene(SampleScenePath, true));
            }

            EditorBuildSettings.scenes = merged.ToArray();
        }
    }
}
