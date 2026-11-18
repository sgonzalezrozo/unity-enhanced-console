#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using EnhancedConsole.Editor.LogCapture;
using EnhancedConsole.Editor.Window;
using EnhancedConsole.Editor.Window.Details;
using EnhancedConsole.Editor.Window.LogList;
using EnhancedConsole.Editor.Window.Toolbar;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace EnhancedConsole.Editor
{
    /// <summary>
    /// The Enhanced Console window. Creates the console's parts, connects them to Editor events and draws them.
    /// </summary>
    public class EnhancedConsoleWindow : EditorWindow, IHasCustomMenu
    {
        private static readonly Vector2 MIN_WINDOW_SIZE = new(400, 300);

        private static EnhancedConsoleWindow instance;

        [FormerlySerializedAs("_persistedLogs")]
        [SerializeField] private List<LogEntry> persistedLogs = new();

        private ConsoleContext context;
        private LogReceiver logReceiver;
        private CompilerMessageListener compilerListener;
        private ConsoleToolbar toolbar;
        private LogListView listView;
        private LogListInput listInput;
        private StackTracePanel stackTracePanel;
        private DetailsResizeHandle resizeHandle;
        private WindowMenu windowMenu;
        private bool hadFocus = true;
        private bool guiErrorLogged;
        private string shownTitle;

        [MenuItem("Window/Enhanced Console")]
        public static void ShowWindow()
        {
            instance = GetWindow<EnhancedConsoleWindow>("Enhanced Console");
            instance.minSize = MIN_WINDOW_SIZE;
            instance.Show();
        }

        internal static void ClearForBuild()
        {
            if (instance != null)
                instance.context.ClearLogs();
        }

        internal static void RepaintIfOpen()
        {
            if (instance != null)
                instance.Repaint();
        }

        private void OnEnable()
        {
            instance = this;
            CreateParts();
            RegisterEvents();
            context.Logs.Restore(persistedLogs);
            persistedLogs.Clear();
        }

        private void OnDisable()
        {
            logReceiver.Unregister();
            compilerListener.Unregister();
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            context.Styles.Invalidate();
        }

        private void OnGUI()
        {
            try
            {
                DrawWindow();
            }
            catch (ExitGUIException)
            {
                // Unity uses this to abort the current GUI event on purpose.
                throw;
            }
            catch (Exception exception)
            {
                // Drawing a HelpBox here would unbalance the layout groups that were open when the exception was
                // thrown. Log once instead of on every repaint, then abort the event cleanly.
                if (!guiErrorLogged)
                {
                    guiErrorLogged = true;
                    Debug.LogError($"[EnhancedConsole] {exception.Message}\n{exception.StackTrace}");
                }

                GUIUtility.ExitGUI();
            }
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            windowMenu.AddItems(menu);
        }

        private void CreateParts()
        {
            context = new ConsoleContext(this);
            logReceiver = new LogReceiver(context);
            compilerListener = new CompilerMessageListener(context);
            toolbar = new ConsoleToolbar(context);
            listView = new LogListView(context);
            listInput = new LogListInput(context, listView);
            stackTracePanel = new StackTracePanel(context);
            resizeHandle = new DetailsResizeHandle(context);
            windowMenu = new WindowMenu(context, listView);
        }

        private void RegisterEvents()
        {
            logReceiver.Register();
            compilerListener.Register();
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        }

        private void DrawWindow()
        {
            context.Styles.EnsureBuilt();
            RefreshSectionConfig();
            SearchField.HandleFindCommand(context);
            listInput.HandleOpenKey(context.GetFilteredLogs());
            UpdateTitle();
            toolbar.Draw();
            DrawLogList();

            if (!context.Preferences.ShowStackTrace.Value)
                return;

            stackTracePanel.Draw(context.GetFilteredLogs());
            resizeHandle.Draw();
        }

        private void DrawLogList()
        {
            List<LogEntry> logs = context.GetFilteredLogs();
            listView.Draw(logs);
            listInput.Handle(logs);
            context.Selection.RememberSelectedEntry(logs);
        }

        private void RefreshSectionConfig()
        {
            // Searching the AssetDatabase is slow in large projects, so only look again when there's no config.
            if (!hadFocus && hasFocus && context.SectionConfig == null)
                context.LoadSectionConfig();

            hadFocus = hasFocus;
            // Cheap: hashes the section fields and recompiles only when the config was edited.
            context.Sections.RefreshSectionStates();
        }

        private void UpdateTitle()
        {
            string title = Application.isPlaying
                ? $" Console | Total: {context.Logs.Entries.Count} - Filtered: {context.GetFilteredLogs().Count} "
                : " Console  ";
            if (title == shownTitle)
                return;

            shownTitle = title;
            titleContent = new GUIContent(title);
        }

        private void OnEditorUpdate()
        {
            logReceiver.Update();
            if (context.Repaints.TryConsume(EditorApplication.timeSinceStartup))
                Repaint();
        }

        private void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
                Focus();

            if (state is PlayModeStateChange.EnteredPlayMode or PlayModeStateChange.ExitingPlayMode)
                Repaint();
            else if (state == PlayModeStateChange.ExitingEditMode && context.Preferences.ClearOnPlay.Value)
                context.ClearLogs();
        }

        private void OnBeforeAssemblyReload()
        {
            bool isRecompile = !EditorApplication.isPlayingOrWillChangePlaymode;
            if (context.Preferences.ClearOnRecompile.Value && isRecompile)
            {
                context.ClearLogs();
                persistedLogs.Clear();
                return;
            }

            persistedLogs = context.Logs.GetPersistableEntries();
        }
    }
}
#endif
