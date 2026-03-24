using MiniMart.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace MiniMart.Editor
{
    public static class GameUiPolishTool
    {
        private const string FontAssetPath = "Assets/Font/RiaSans-Bold SDF.asset";
        private static readonly Vector2 CenterAnchor = new(0.5f, 0.5f);

        [MenuItem("Tools/MiniMart/Polish Game UI")]
        public static void PolishGameUi()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("MiniMart", "현재 씬에서 Canvas를 찾지 못했습니다.", "확인");
                return;
            }

            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (fontAsset == null)
            {
                EditorUtility.DisplayDialog("MiniMart", $"TMP 폰트를 찾지 못했습니다.\n{FontAssetPath}", "확인");
                return;
            }

            EnsureHudTextWithComponent<TutorialUI>(canvas.transform, "TutorialText", fontAsset);
            EnsureHudTextWithComponent<DailyGoalUI>(canvas.transform, "GoalText", fontAsset);
            EnsureHudTextWithComponent<ShadowWorkerStatusUI>(canvas.transform, "WorkerStatusText", fontAsset);

            StyleHud(canvas.transform, fontAsset);
            StyleResultPanel(canvas.transform, fontAsset);
            StyleOrderTerminalPanel(canvas.transform, fontAsset);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorUtility.DisplayDialog("MiniMart", "게임 UI를 다시 정리했습니다.", "확인");
        }

        private static void StyleHud(Transform canvas, TMP_FontAsset fontAsset)
        {
            StyleTextByName(canvas, "MoneyText", fontAsset, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(280f, 40f), new Vector2(24f, -20f), 22, TextAlignmentOptions.Left);
            StyleTextByName(canvas, "DayTimerText", fontAsset, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(280f, 40f), new Vector2(0f, -20f), 22, TextAlignmentOptions.Center);
            StyleTextByName(canvas, "DayCount", fontAsset, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(180f, 40f), new Vector2(-24f, -20f), 22, TextAlignmentOptions.Right);
            StyleTextByName(canvas, "TutorialText", fontAsset, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(420f, 220f), new Vector2(24f, -90f), 18, TextAlignmentOptions.TopLeft);
            StyleTextByName(canvas, "GoalText", fontAsset, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(360f, 200f), new Vector2(-24f, 40f), 18, TextAlignmentOptions.TopLeft);
            StyleTextByName(canvas, "WorkerStatusText", fontAsset, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(280f, 120f), new Vector2(24f, -300f), 18, TextAlignmentOptions.TopLeft);
            StyleTextByName(canvas, "InteractionPromptText", fontAsset, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(880f, 48f), new Vector2(0f, 88f), 22, TextAlignmentOptions.Center);
            StyleTextByName(canvas, "StatusText", fontAsset, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(720f, 36f), new Vector2(24f, 24f), 18, TextAlignmentOptions.Left);
        }

        private static void StyleResultPanel(Transform canvas, TMP_FontAsset fontAsset)
        {
            Transform result = canvas.Find("Result");
            if (result == null)
            {
                return;
            }

            RectTransform resultRect = result.GetComponent<RectTransform>();
            if (resultRect != null)
            {
                resultRect.anchorMin = CenterAnchor;
                resultRect.anchorMax = CenterAnchor;
                resultRect.pivot = CenterAnchor;
                resultRect.sizeDelta = new Vector2(760f, 520f);
                resultRect.anchoredPosition = Vector2.zero;
            }

            Image panelImage = result.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = new Color(0.06f, 0.09f, 0.11f, 0.94f);
            }

            StyleTextByName(result, "Text (Legacy)", fontAsset, CenterAnchor, CenterAnchor, new Vector2(640f, 360f), Vector2.zero, 24, TextAlignmentOptions.TopLeft);
            StyleTextByName(result, "SummaryText", fontAsset, CenterAnchor, CenterAnchor, new Vector2(640f, 360f), Vector2.zero, 24, TextAlignmentOptions.TopLeft);

            Button button = result.GetComponentInChildren<Button>(true);
            if (button != null)
            {
                RectTransform rect = button.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(1f, 0f);
                rect.anchorMax = new Vector2(1f, 0f);
                rect.pivot = new Vector2(1f, 0f);
                rect.sizeDelta = new Vector2(180f, 52f);
                rect.anchoredPosition = new Vector2(-28f, 24f);
                StyleButton(button, fontAsset, "다음 날", 20);
            }
        }

        private static void StyleOrderTerminalPanel(Transform canvas, TMP_FontAsset fontAsset)
        {
            Transform panel = canvas.Find("OrderTerminalPanel");
            if (panel == null)
            {
                return;
            }

            RectTransform panelRect = panel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.anchorMin = CenterAnchor;
                panelRect.anchorMax = CenterAnchor;
                panelRect.pivot = CenterAnchor;
            }

            Image panelImage = panel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = new Color(0.08f, 0.12f, 0.15f, 0.88f);
            }

            EnsureLabel(panel, "PanelTitle", fontAsset, "운영 패널", CenterAnchor, CenterAnchor, new Vector2(320f, 46f), new Vector2(0f, 235f), 30, TextAlignmentOptions.Center, false);
            EnsureLabel(panel, "SectionOrder", fontAsset, "발주", CenterAnchor, CenterAnchor, new Vector2(140f, 32f), new Vector2(-360f, 165f), 24, TextAlignmentOptions.Center, false);
            EnsureLabel(panel, "SectionPlacement", fontAsset, "배치", CenterAnchor, CenterAnchor, new Vector2(140f, 32f), new Vector2(-360f, -90f), 24, TextAlignmentOptions.Center, false);
            EnsureLabel(panel, "SectionExpansion", fontAsset, "확장", CenterAnchor, CenterAnchor, new Vector2(140f, 32f), new Vector2(150f, 165f), 24, TextAlignmentOptions.Center, false);
            EnsureLabel(panel, "SectionShadow", fontAsset, "분신", CenterAnchor, CenterAnchor, new Vector2(140f, 32f), new Vector2(330f, -90f), 24, TextAlignmentOptions.Center, false);

            StylePanelTextOnly(panel, "SelectedProductText", fontAsset, 24, TextAlignmentOptions.Center);
            StylePanelTextOnly(panel, "ProductCostText", fontAsset, 18, TextAlignmentOptions.Center);
            StylePanelTextOnly(panel, "PlacementInfoText", fontAsset, 18, TextAlignmentOptions.Center);
            StylePanelTextOnly(panel, "ExpansionInfoText", fontAsset, 18, TextAlignmentOptions.Center);
            StylePanelTextOnly(panel, "ShadowWorkerInfoText", fontAsset, 18, TextAlignmentOptions.Center);
            StylePanelTextOnly(panel, "ShadowWorker", fontAsset, 18, TextAlignmentOptions.Center);

            foreach (Button button in panel.GetComponentsInChildren<Button>(true))
            {
                StylePanelButton(button, fontAsset);
            }
        }

        private static void EnsureHudTextWithComponent<T>(Transform canvas, string name, TMP_FontAsset fontAsset) where T : Component
        {
            Transform found = canvas.Find(name);
            if (found == null)
            {
                GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer));
                go.transform.SetParent(canvas, false);
                Undo.AddComponent<TextMeshProUGUI>(go);
                found = go.transform;
            }

            EnsureTextAsTmp(found.gameObject, fontAsset);

            if (found.GetComponent<T>() == null)
            {
                Undo.AddComponent<T>(found.gameObject);
            }
        }

        private static void EnsureLabel(Transform parent, string name, TMP_FontAsset fontAsset, string content, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 anchoredPosition, int fontSize, TextAlignmentOptions alignment, bool forceLayout = true)
        {
            Transform found = parent.Find(name);
            bool created = false;
            if (found == null)
            {
                GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer));
                go.transform.SetParent(parent, false);
                found = go.transform;
                created = true;
            }

            TMP_Text tmp = EnsureTextAsTmp(found.gameObject, fontAsset);
            RectTransform rect = found.GetComponent<RectTransform>();
            if (forceLayout || created)
            {
                rect.anchorMin = anchorMin;
                rect.anchorMax = anchorMax;
                rect.pivot = CenterAnchor;
                rect.sizeDelta = size;
                rect.anchoredPosition = anchoredPosition;
            }

            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = new Color(0.96f, 0.96f, 0.92f, 1f);
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.raycastTarget = false;
        }

        private static void StyleTextByName(Transform parent, string name, TMP_FontAsset fontAsset, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 anchoredPosition, int fontSize, TextAlignmentOptions alignment)
        {
            Transform found = parent.Find(name);
            if (found == null)
            {
                return;
            }

            TMP_Text tmp = EnsureTextAsTmp(found.gameObject, fontAsset);
            RectTransform rect = found.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = anchorMin;
                rect.anchorMax = anchorMax;
                rect.pivot = CenterAnchor;
                rect.sizeDelta = size;
                rect.anchoredPosition = anchoredPosition;
            }

            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = new Color(0.95f, 0.95f, 0.92f, 1f);
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;
        }

        private static void StylePanelTextOnly(Transform parent, string name, TMP_FontAsset fontAsset, int fontSize, TextAlignmentOptions alignment)
        {
            Transform found = parent.Find(name);
            if (found == null)
            {
                return;
            }

            TMP_Text tmp = EnsureTextAsTmp(found.gameObject, fontAsset);
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = new Color(0.95f, 0.95f, 0.92f, 1f);
            tmp.raycastTarget = false;
        }

        private static void StylePanelButton(Button button, TMP_FontAsset fontAsset)
        {
            string label = ResolveButtonLabel(button);
            if (string.IsNullOrWhiteSpace(label))
            {
                TMP_Text existing = button.GetComponentInChildren<TMP_Text>(true);
                label = existing != null ? existing.text : button.gameObject.name;
            }

            int fontSize = label.Length >= 6 ? 16 : 18;
            if (label.Contains("분신"))
            {
                fontSize = 16;
            }

            StyleButton(button, fontAsset, label, fontSize);
        }

        private static string ResolveButtonLabel(Button button)
        {
            string name = button.gameObject.name;
            return name switch
            {
                "Prev" => "이전",
                "Next" => "다음",
                "Order" => "발주",
                "PrevPlace" => "이전",
                "NextPlace" => "다음",
                "EnterPlace" => "배치 시작",
                "ExitPlace" => "배치 종료",
                "Expand" => "확장",
                "Stocker" => "운반 분신",
                "Cashier" => "계산 분신",
                "Close" => "닫기",
                _ => ResolveButtonLabelFromOnClick(button)
            };
        }

        private static string ResolveButtonLabelFromOnClick(Button button)
        {
            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            {
                string method = button.onClick.GetPersistentMethodName(i);
                switch (method)
                {
                    case "SelectPreviousProduct":
                    case "SelectPreviousFurniture":
                        return "이전";
                    case "SelectNextProduct":
                    case "SelectNextFurniture":
                        return "다음";
                    case "OrderSelectedProduct":
                        return "발주";
                    case "EnterPlacementMode":
                        return "배치 시작";
                    case "ExitPlacementMode":
                        return "배치 종료";
                    case "BuyNextExpansion":
                        return "확장";
                    case "HireStockerShadow":
                        button.gameObject.name = "Stocker";
                        return "운반 분신";
                    case "HireCashierShadow":
                        button.gameObject.name = "Cashier";
                        return "계산 분신";
                    case "Close":
                    case "OnNextDayButtonClicked":
                        return button.gameObject.name == "Button" ? "다음 날" : "닫기";
                }
            }

            return string.Empty;
        }

        private static void StyleButton(Button button, TMP_FontAsset fontAsset, string label, int fontSize)
        {
            button.transition = Selectable.Transition.ColorTint;

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(0.10f, 0.45f, 0.38f, 1f);
                button.targetGraphic = image;
            }

            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.10f, 0.45f, 0.38f, 1f);
            colors.highlightedColor = new Color(0.16f, 0.58f, 0.48f, 1f);
            colors.pressedColor = new Color(0.08f, 0.32f, 0.26f, 1f);
            colors.selectedColor = colors.normalColor;
            colors.disabledColor = new Color(0.78f, 0.78f, 0.78f, 0.5f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            button.colors = colors;

            TMP_Text tmp = EnsureButtonLabel(button, fontAsset);
            TMP_Text[] allTexts = button.GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text childText in allTexts)
            {
                childText.font = fontAsset;
                childText.fontSize = fontSize;
                childText.alignment = TextAlignmentOptions.Center;
                childText.textWrappingMode = TextWrappingModes.NoWrap;
                childText.overflowMode = TextOverflowModes.Ellipsis;
                childText.raycastTarget = false;
                childText.color = new Color(0.92f, 0.96f, 0.94f, 1f);
            }

            tmp.text = label;
        }

        private static TMP_Text EnsureButtonLabel(Button button, TMP_FontAsset fontAsset)
        {
            TMP_Text tmp = button.GetComponentInChildren<TMP_Text>(true);
            if (tmp != null)
            {
                tmp.font = fontAsset;
                return tmp;
            }

            Text legacy = button.GetComponentInChildren<Text>(true);
            if (legacy != null)
            {
                GameObject labelObject = legacy.gameObject;
                Undo.DestroyObjectImmediate(legacy);
                tmp = labelObject.GetComponent<TextMeshProUGUI>();
                if (tmp == null)
                {
                    tmp = Undo.AddComponent<TextMeshProUGUI>(labelObject);
                }

                RectTransform labelRect = labelObject.GetComponent<RectTransform>();
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;
                tmp.font = fontAsset;
                return tmp;
            }

            GameObject go = new("Text (TMP)", typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(button.transform, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            tmp = Undo.AddComponent<TextMeshProUGUI>(go);
            tmp.font = fontAsset;
            return tmp;
        }

        private static TMP_Text EnsureTextAsTmp(GameObject gameObject, TMP_FontAsset fontAsset)
        {
            TMP_Text tmp = gameObject.GetComponent<TMP_Text>();
            if (tmp == null)
            {
                Text legacy = gameObject.GetComponent<Text>();
                if (legacy != null)
                {
                    string previousText = legacy.text;
                    Color previousColor = legacy.color;
                    int previousFontSize = legacy.fontSize;
                    TextAnchor previousAlignment = legacy.alignment;
                    Undo.DestroyObjectImmediate(legacy);
                    tmp = Undo.AddComponent<TextMeshProUGUI>(gameObject);
                    tmp.text = previousText;
                    tmp.color = previousColor;
                    tmp.fontSize = previousFontSize > 0 ? previousFontSize : 20f;
                    tmp.alignment = ConvertAlignment(previousAlignment);
                }
                else
                {
                    tmp = Undo.AddComponent<TextMeshProUGUI>(gameObject);
                }
            }

            tmp.font = fontAsset;
            return tmp;
        }

        private static TextAlignmentOptions ConvertAlignment(TextAnchor anchor)
        {
            return anchor switch
            {
                TextAnchor.UpperLeft => TextAlignmentOptions.TopLeft,
                TextAnchor.UpperCenter => TextAlignmentOptions.Top,
                TextAnchor.UpperRight => TextAlignmentOptions.TopRight,
                TextAnchor.MiddleLeft => TextAlignmentOptions.Left,
                TextAnchor.MiddleCenter => TextAlignmentOptions.Center,
                TextAnchor.MiddleRight => TextAlignmentOptions.Right,
                TextAnchor.LowerLeft => TextAlignmentOptions.BottomLeft,
                TextAnchor.LowerCenter => TextAlignmentOptions.Bottom,
                TextAnchor.LowerRight => TextAlignmentOptions.BottomRight,
                _ => TextAlignmentOptions.Center,
            };
        }
    }
}
