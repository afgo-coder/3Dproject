using MiniMart.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace MiniMart.Editor
{
    public static class PauseMenuPolishTool
    {
        private const string FontAssetPath = "Assets/Font/RiaSans-Bold SDF Dynamic.asset";

        [MenuItem("Tools/MiniMart/Polish Pause Menu")]
        public static void PolishPauseMenu()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("MiniMart", "현재 씬에서 Canvas를 찾을 수 없습니다.", "확인");
                return;
            }

            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (fontAsset == null)
            {
                EditorUtility.DisplayDialog("MiniMart", $"TMP 폰트를 찾을 수 없습니다.\n{FontAssetPath}", "확인");
                return;
            }

            GameObject pausePanel = FindOrCreatePanel(canvas.transform);
            PauseMenuController controller = pausePanel.GetComponent<PauseMenuController>();
            if (controller == null)
            {
                controller = Undo.AddComponent<PauseMenuController>(pausePanel);
            }

            StylePanel(pausePanel);
            CreateOrUpdateContents(pausePanel.transform, controller, fontAsset);

            SerializedObject controllerObject = new SerializedObject(controller);
            controllerObject.FindProperty("panel").objectReferenceValue = pausePanel;
            controllerObject.ApplyModifiedPropertiesWithoutUndo();

            pausePanel.SetActive(false);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorUtility.DisplayDialog("MiniMart", "PausePanel UI를 정리했습니다.", "확인");
        }

        private static GameObject FindOrCreatePanel(Transform canvas)
        {
            Transform found = canvas.Find("PausePanel");
            if (found != null)
            {
                return found.gameObject;
            }

            GameObject panel = new GameObject("PausePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(canvas, false);
            return panel;
        }

        private static void StylePanel(GameObject panel)
        {
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = panel.GetComponent<Image>();
            image.color = new Color(0.04f, 0.07f, 0.10f, 0.72f);
        }

        private static void CreateOrUpdateContents(Transform panel, PauseMenuController controller, TMP_FontAsset fontAsset)
        {
            GameObject card = EnsureChild(panel, "PauseCard", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(640f, 520f);
            cardRect.anchoredPosition = Vector2.zero;
            card.GetComponent<Image>().color = new Color(0.06f, 0.10f, 0.13f, 0.92f);

            CreateOrUpdateLabel(card.transform, "Title", "일시정지", fontAsset, new Vector2(0f, 170f), new Vector2(360f, 52f), 34, FontStyles.Bold);
            CreateOrUpdateLabel(card.transform, "Subtitle", "게임을 잠시 멈췄습니다.\n원하는 메뉴를 선택하세요.", fontAsset, new Vector2(0f, 112f), new Vector2(420f, 56f), 18, FontStyles.Normal, new Color(0.83f, 0.90f, 0.90f, 0.92f));

            CreateOrUpdateButton(card.transform, controller, fontAsset, "ResumeButton", "게임으로 돌아가기", new Vector2(0f, 20f), nameof(PauseMenuController.ResumeGame));
            CreateOrUpdateButton(card.transform, controller, fontAsset, "SettingsButton", "설정", new Vector2(0f, -60f), nameof(PauseMenuController.OpenSettings));
            CreateOrUpdateButton(card.transform, controller, fontAsset, "SaveMenuButton", "저장 후 메인메뉴", new Vector2(0f, -140f), nameof(PauseMenuController.SaveAndReturnToMainMenu));
            CreateOrUpdateButton(card.transform, controller, fontAsset, "SaveQuitButton", "저장 후 종료", new Vector2(0f, -220f), nameof(PauseMenuController.SaveAndQuit));
        }

        private static void CreateOrUpdateButton(Transform parent, PauseMenuController controller, TMP_FontAsset fontAsset, string name, string label, Vector2 position, string methodName)
        {
            GameObject buttonObject = EnsureChild(parent, name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(320f, 60f);
            rect.anchoredPosition = position;

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.13f, 0.49f, 0.43f, 0.96f);

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.13f, 0.49f, 0.43f, 0.96f);
            colors.highlightedColor = new Color(0.20f, 0.66f, 0.58f, 1f);
            colors.pressedColor = new Color(0.09f, 0.36f, 0.31f, 1f);
            colors.selectedColor = colors.normalColor;
            colors.disabledColor = new Color(0.68f, 0.68f, 0.68f, 0.45f);
            button.colors = colors;
            button.targetGraphic = image;

            button.onClick = new Button.ButtonClickedEvent();
            switch (methodName)
            {
                case nameof(PauseMenuController.ResumeGame):
                    UnityEventTools.AddPersistentListener(button.onClick, controller.ResumeGame);
                    break;
                case nameof(PauseMenuController.OpenSettings):
                    UnityEventTools.AddPersistentListener(button.onClick, controller.OpenSettings);
                    break;
                case nameof(PauseMenuController.SaveAndReturnToMainMenu):
                    UnityEventTools.AddPersistentListener(button.onClick, controller.SaveAndReturnToMainMenu);
                    break;
                case nameof(PauseMenuController.SaveAndQuit):
                    UnityEventTools.AddPersistentListener(button.onClick, controller.SaveAndQuit);
                    break;
            }

            GameObject labelObject = EnsureChild(buttonObject.transform, "Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = labelObject.GetComponent<TextMeshProUGUI>();
            tmp.font = fontAsset;
            tmp.text = label;
            tmp.fontSize = 24;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;
        }

        private static void CreateOrUpdateLabel(Transform parent, string name, string text, TMP_FontAsset fontAsset, Vector2 position, Vector2 size, int fontSize, FontStyles style, Color? color = null)
        {
            GameObject labelObject = EnsureChild(parent, name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            RectTransform rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            TextMeshProUGUI tmp = labelObject.GetComponent<TextMeshProUGUI>();
            tmp.font = fontAsset;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color ?? new Color(0.96f, 0.96f, 0.92f, 1f);
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;
        }

        private static GameObject EnsureChild(Transform parent, string name, params System.Type[] components)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject go = new GameObject(name, components);
            go.transform.SetParent(parent, false);
            return go;
        }
    }
}
