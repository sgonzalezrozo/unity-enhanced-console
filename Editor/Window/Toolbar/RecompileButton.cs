#if UNITY_EDITOR
using EnhancedConsole.Editor.Preferences;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace EnhancedConsole.Editor.Window.Toolbar
{
    /// <summary>
    /// Requests a full recompile, optionally after an asset refresh. Shown outside Play Mode.
    /// </summary>
    internal sealed class RecompileButton
    {
        private const float LABEL_PADDING = 10f;
        private static readonly GUIContent RECOMPILE_CONTENT = new("Recompile");
        private static readonly GUIContent RECOMPILE_WITH_REFRESH_CONTENT = new("Recompile + Asset Refresh");

        private readonly BoolPreference withAssetRefresh;

        public RecompileButton(ConsolePreferences preferences)
        {
            withAssetRefresh = preferences.RecompileWithAssetRefresh;
        }

        public void Draw()
        {
            GUIContent content = withAssetRefresh.Value ? RECOMPILE_WITH_REFRESH_CONTENT : RECOMPILE_CONTENT;
            float width = EditorStyles.toolbarButton.CalcSize(content).x + LABEL_PADDING;
            if (GUILayout.Button(content, EditorStyles.toolbarButton, GUILayout.Width(width)))
                Recompile();

            DrawOptionsMenu();
        }

        private void DrawOptionsMenu()
        {
            Rect dropdownRect = ToolbarControls.ReserveDropdownRect();
            if (!ToolbarControls.DrawDropdownButton(dropdownRect))
                return;

            GenericMenu menu = new();
            menu.AddItem(new GUIContent("Recompile Only"), !withAssetRefresh.Value, SetRecompileOnly);
            menu.AddItem(new GUIContent("Recompile and Asset Refresh"), withAssetRefresh.Value,
                SetRecompileWithAssetRefresh);
            menu.DropDown(dropdownRect);
        }

        private void SetRecompileOnly()
        {
            withAssetRefresh.Set(false);
        }

        private void SetRecompileWithAssetRefresh()
        {
            withAssetRefresh.Set(true);
        }

        private void Recompile()
        {
            if (withAssetRefresh.Value)
            {
                Debug.Log("[EnhancedConsole] Refreshing assets and recompiling all scripts.");
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }
            else
            {
                Debug.Log("[EnhancedConsole] Recompiling all scripts.");
            }

            CompilationPipeline.RequestScriptCompilation();
        }
    }
}
#endif
