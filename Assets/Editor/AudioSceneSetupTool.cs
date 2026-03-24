using MiniMart.Managers;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MiniMart.Editor
{
    public static class AudioSceneSetupTool
    {
        private const string ButtonClickPath = "Assets/Audio/GeneratedSfx/click_double_on.wav";
        private const string OrderPlacedPath = "Assets/Audio/GeneratedSfx/order_placed.wav";
        private const string FurniturePlacedPath = "Assets/Audio/GeneratedSfx/furniture_placed.wav";
        private const string CheckoutPath = "Assets/Audio/GeneratedSfx/checkout.wav";
        private const string ExpansionPath = "Assets/Audio/GeneratedSfx/expansion.wav";

        [MenuItem("Tools/MiniMart/Setup Audio Manager In Open Scene")]
        public static void SetupAudioManagerInOpenScene()
        {
            AudioManager manager = Object.FindFirstObjectByType<AudioManager>();
            if (manager == null)
            {
                GameObject audioManagerObject = new GameObject("AudioManager");
                manager = audioManagerObject.AddComponent<AudioManager>();
            }

            SerializedObject serializedObject = new SerializedObject(manager);
            serializedObject.FindProperty("buttonClickSfx").objectReferenceValue = LoadClip(ButtonClickPath);
            serializedObject.FindProperty("orderPlacedSfx").objectReferenceValue = LoadClip(OrderPlacedPath);
            serializedObject.FindProperty("furniturePlacedSfx").objectReferenceValue = LoadClip(FurniturePlacedPath);
            serializedObject.FindProperty("checkoutSfx").objectReferenceValue = LoadClip(CheckoutPath);
            serializedObject.FindProperty("expansionSfx").objectReferenceValue = LoadClip(ExpansionPath);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(manager);
            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);

            Selection.activeObject = manager.gameObject;
            EditorGUIUtility.PingObject(manager.gameObject);
            EditorUtility.DisplayDialog("MiniMart", "현재 씬에 AudioManager를 준비했습니다.", "확인");
        }

        private static AudioClip LoadClip(string assetPath)
        {
            return AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
        }
    }
}
