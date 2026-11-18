#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace EnhancedConsole.Editor.Window.Toolbar
{
    /// <summary>
    /// The section filter toggle, showing active/total sections, and its menu of sections and filter modes.
    /// </summary>
    internal sealed class SectionFilterButton
    {
        private static readonly GUIContent DISABLED_CONTENT = new("SL (--/--)");

        private readonly ConsoleContext context;
        private readonly ToolbarToggle toggle = new(new GUIContent(), ToolbarControls.BUTTON_WIDTH);
        private Rect menuScreenRect;

        public SectionFilterButton(ConsoleContext consoleContext)
        {
            context = consoleContext;
        }

        public void Draw()
        {
            if (context.SectionConfig == null)
            {
                DrawDisabled();
                return;
            }

            DrawToggle();
            DrawMenuButton();
        }

        private void DrawDisabled()
        {
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = context.Colors.DisabledUIElement;
            GUI.enabled = false;
            GUILayout.Button(DISABLED_CONTENT, EditorStyles.toolbarButton,
                GUILayout.Width(ToolbarControls.BUTTON_WIDTH));
            GUI.enabled = true;
            GUI.backgroundColor = originalColor;
        }

        private void DrawToggle()
        {
            SectionedLogOrganizer sections = context.Sections;
            int active = sections.GetActiveSectionCount();
            int total = sections.GetTotalSectionCount();
            toggle.Content.text = $"SL ({active}/{total})";

            bool isActive = toggle.Draw(sections.IsFilterActive, GetToggleColor(active == total));
            if (isActive == sections.IsFilterActive)
                return;

            sections.SetFilterActive(isActive);
            context.Preferences.SectionFilterActive.Set(isActive);
            context.Repaint();
        }

        private Color GetToggleColor(bool allSectionsActive)
        {
            if (!context.Sections.IsFilterActive)
                return context.Colors.DisabledButton;

            return allSectionsActive ? Color.white : context.Colors.ToggleButtonActive;
        }

        private void DrawMenuButton()
        {
            Rect dropdownRect = ToolbarControls.ReserveDropdownRect();
            if (Event.current.type == EventType.Repaint)
                menuScreenRect = GUIUtility.GUIToScreenRect(dropdownRect);

            if (ToolbarControls.DrawDropdownButton(dropdownRect))
                CreateMenu().DropDown(dropdownRect);
        }

        private GenericMenu CreateMenu()
        {
            GenericMenu menu = new();
            menu.AddItem(new GUIContent("Sections..."), false, ShowSectionMenu);
            menu.AddSeparator("");
            AddFilterModeItem(menu, "Stack And Content", SectionFilterMode.StackAndContent);
            AddFilterModeItem(menu, "Only Stack", SectionFilterMode.OnlyStack);
            AddFilterModeItem(menu, "Only Content", SectionFilterMode.OnlyContent);
            return menu;
        }

        private void AddFilterModeItem(GenericMenu menu, string label, SectionFilterMode mode)
        {
            menu.AddItem(new GUIContent(label), context.Sections.FilterMode == mode, OnFilterModeSelected, mode);
        }

        private void OnFilterModeSelected(object mode)
        {
            context.Sections.SetFilterMode((SectionFilterMode)mode);
            context.Preferences.SetSectionFilterMode((SectionFilterMode)mode);
            context.Repaint();
        }

        private void ShowSectionMenu()
        {
            if (context.SectionConfig != null)
                SectionLogsMenuWindow.Show(menuScreenRect, context.SectionConfig.GetAllSections(), OnSectionsChanged);
        }

        private void OnSectionsChanged()
        {
            if (context.SectionConfig == null)
                return;

            EditorUtility.SetDirty(context.SectionConfig);
            context.Sections.RefreshSectionStates();
            context.Repaint();
        }
    }
}
#endif
