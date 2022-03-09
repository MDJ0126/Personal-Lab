using UnityEditor;
using UnityEngine;

namespace MaskTextureMaker
{
    public static class MaskedTextureMakerUtils
    {
        public const string MENU_NAME = "MaskedTextureMaker/View Debug OnGUI";
        public static bool isOnDebugGUI = false;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            isOnDebugGUI = EditorPrefs.GetBool(MENU_NAME, false);
            EditorApplication.delayCall += () =>
            {
                PerformAction(isOnDebugGUI);
            };
        }

        [MenuItem(MENU_NAME)]
        private static void ViewDebugOnGUIMenu()
        {
            PerformAction(!isOnDebugGUI);
        }

        public static void PerformAction(bool enabled)
        {
            Menu.SetChecked(MENU_NAME, enabled);
            EditorPrefs.SetBool(MENU_NAME, enabled);
            isOnDebugGUI = enabled;
            MaskedTextureMaker.isOnDebugGUI = isOnDebugGUI;
        }
    }
}