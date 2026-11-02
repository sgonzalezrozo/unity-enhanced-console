#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Tools.EnhanceConsole.Editor.LogManagement;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Tools.EnhanceConsole.Editor
{
    /// <summary>
    /// EnhancedConsoleWindow: Main window coordinator for the Enhanced Console.
    /// Responsibility: Orchestrate window lifecycle and delegate to focused components.
    /// This class should remain ~100-120 lines following SOLID principles.
    /// </summary>
    public class EnhancedConsoleWindow : EditorWindow
    {
        private const int TOOLBAR_HEIGHT = 20;
        private const int LOG_ENTRY_HEIGHT = 32;
        private const float DEFAULT_DETAILS_HEIGHT = 200f;
        private const string PREF_CLEAR_ON_PLAY = "EnhancedConsole_ClearOnPlay";
        private const string PREF_CLEAR_ON_BUILD = "EnhancedConsole_ClearOnBuild";
        private const string PREF_CLEAR_ON_RECOMPILE = "EnhancedConsole_ClearOnRecompile";
        private const string PREF_COLLAPSE_EXACT_STACKTRACE = "EnhancedConsole_CollapseExactStacktrace";
        private const string PREF_SHOW_TIME = "EnhancedConsole_ShowTime";
        private const string PREF_SHOW_STACK_TRACE = "EnhancedConsole_ShowStackTrace";
        private const string PREF_DETAILS_HEIGHT = "EnhancedConsole_DetailsHeight";
        private const string PREF_SL_FILTER_MODE = "EnhancedConsole_SL_FilterMode";
        private const string PREF_RECOMPILE_MODE = "EnhancedConsole_RecompileMode";
        private const double PLAY_MODE_REPAINT_INTERVAL_SECONDS = 0.1d;

        private static EnhancedConsoleWindow instance;
        private static bool buildPlayerHandlerRegistered;

        // Core Managers (Extracted)
        private LogEntryManager logManager;
        private CompilationStateTracker _compilationTracker;

        // Selection & View State
        private int _selectedIndex = -1;
        private readonly HashSet<int> _multiSelectedIndices = new();
        private Vector2 _scrollPosition;
        private Vector2 _detailsScrollPosition;

        // Configuration State
        private bool _clearOnPlay;
        private bool _clearOnBuild;
        private bool _clearOnRecompile;
        private bool _showTime;
        private bool _showStackTrace;
        private bool _isResizingDetails;
        private bool _hadFocus = true;
        private bool _recompileWithAssetRefresh;

        private float _detailsHeight = DEFAULT_DETAILS_HEIGHT;
        private bool _isInPlayMode;

        // Preferences
        private SelectiveLogsScriptableObject _selectiveLogsConfig;

        // Components (SRP - Single Responsibility Principle)
        private LogLevelColorScheme _colorScheme;
        private LogEntryFormatter _formatter;
        private StackTraceParser _stackTraceParser;
        private ConsoleViewRenderer _viewRenderer;
        private LogFilterManager _filterManager;
        private SectionedLogOrganizer _sectionOrganizer;
        private bool _isStylesInitialized;
        private GUIStyle _logMessageStyle;
        private GUIStyle _logTimeStyle;
        private GUIStyle _logCountStyle;
        private GUIStyle _stackTraceLineStyle;
        private GUIStyle _stackTraceLineClickableStyle;
        private double _lastPlayModeRepaintTime;


        // UI State
        private bool _previousCollapseState;

        [MenuItem("Window/Enhanced Console")]
        public static void ShowWindow()
        {
            instance = GetWindow<EnhancedConsoleWindow>("Enhanced Console");
            instance.minSize = new Vector2(400, 300);
            instance.Show();
        }

        public EnhancedConsoleWindow()
        {
            _selectedIndex = -1;
        }

        private void OnEnable()
        {
            InitializeComponents();
            instance = this;
            RegisterEventHandlers();
            LoadPreferences();
            _compilationTracker.CheckForExistingErrors();
        }

        private void InitializeComponents()
        {
            _colorScheme = new();
            _stackTraceParser = new();
            _viewRenderer = new(_colorScheme, _formatter ?? (_formatter = new(true, false)));
            _filterManager = new();
            _sectionOrganizer = _filterManager.GetSectionOrganizer();

            logManager = new(_filterManager);
            _compilationTracker = new(logManager);
        }

        private void RegisterEventHandlers()
        {
            LogContextHelper.Initialize();
            Application.logMessageReceived += HandleLog;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            CompilationPipeline.compilationStarted += OnCompilationStarted;
            CompilationPipeline.compilationFinished += OnCompilationFinished;

            if (!buildPlayerHandlerRegistered)
            {
                BuildPlayerWindow.RegisterBuildPlayerHandler(OnBuildPlayer);
                buildPlayerHandlerRegistered = true;
            }
        }

        private void OnDisable()
        {
            UnregisterEventHandlers();
            SavePreferences();
            _isStylesInitialized = false;

            // Clean up play mode refresh handler
            if (_isInPlayMode)
                EditorApplication.update -= RefreshDuringPlayMode;
        }

        private void UnregisterEventHandlers()
        {
            Application.logMessageReceived -= HandleLog;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            CompilationPipeline.compilationStarted -= OnCompilationStarted;
            CompilationPipeline.compilationFinished -= OnCompilationFinished;
        }


        private void OnBuildPlayer(BuildPlayerOptions options)
        {
            if (_clearOnBuild)
                ClearLogs();

            BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
        }

        private void LoadPreferences()
        {
            _clearOnPlay = EditorPrefs.GetBool(PREF_CLEAR_ON_PLAY, false);
            _clearOnBuild = EditorPrefs.GetBool(PREF_CLEAR_ON_BUILD, false);
            _clearOnRecompile = EditorPrefs.GetBool(PREF_CLEAR_ON_RECOMPILE, false);
            _filterManager.SetCollapseExactStacktrace(EditorPrefs.GetBool(PREF_COLLAPSE_EXACT_STACKTRACE, false));
            _showTime = EditorPrefs.GetBool(PREF_SHOW_TIME, false);
            _showStackTrace = EditorPrefs.GetBool(PREF_SHOW_STACK_TRACE, false);
            _detailsHeight = EditorPrefs.GetFloat(PREF_DETAILS_HEIGHT, DEFAULT_DETAILS_HEIGHT);
            _recompileWithAssetRefresh = EditorPrefs.GetBool(PREF_RECOMPILE_MODE, false);

            SLFilterMode filterMode = (SLFilterMode)EditorPrefs.GetInt(PREF_SL_FILTER_MODE, (int)SLFilterMode.StackAndContent);
            _sectionOrganizer.SetFilterMode(filterMode);

            // Default: section filter should be OFF to show all logs
            bool sectionLogsFilterActive = EditorPrefs.GetBool("EnhancedConsole_SL_Active", false);
            _sectionOrganizer.SetFilterActive(sectionLogsFilterActive);

            // Sync formatter with loaded preferences
            if (_formatter != null)
                _formatter.SetShowStackTrace(_showStackTrace);

            LoadSelectiveLogsConfig();
        }

        private void LoadSelectiveLogsConfig()
        {
            _selectiveLogsConfig = Resources.Load<SelectiveLogsScriptableObject>("Tools/EnhanceConsole/SelectiveLogsConfig");

            if (_selectiveLogsConfig == null)
            {
                // Ensure section filter is disabled if config is missing
                _sectionOrganizer.SetFilterActive(false);
                return;
            }

            _filterManager.InitializeSectionOrganizer(_selectiveLogsConfig);
            _sectionOrganizer.RefreshSectionStates();
        }

        private void SavePreferences()
        {
            EditorPrefs.SetBool(PREF_CLEAR_ON_PLAY, _clearOnPlay);
            EditorPrefs.SetBool(PREF_CLEAR_ON_BUILD, _clearOnBuild);
            EditorPrefs.SetBool(PREF_CLEAR_ON_RECOMPILE, _clearOnRecompile);
            EditorPrefs.SetBool(PREF_COLLAPSE_EXACT_STACKTRACE, _filterManager.IsCollapseExactStacktraceEnabled);
            EditorPrefs.SetBool(PREF_SHOW_TIME, _showTime);
            EditorPrefs.SetBool(PREF_SHOW_STACK_TRACE, _showStackTrace);
            EditorPrefs.SetFloat(PREF_DETAILS_HEIGHT, _detailsHeight);
            EditorPrefs.SetBool(PREF_RECOMPILE_MODE, _recompileWithAssetRefresh);
            EditorPrefs.SetBool("EnhancedConsole_SL_Active", _sectionOrganizer.IsFilterActive);
            EditorPrefs.SetInt(PREF_SL_FILTER_MODE, (int)_sectionOrganizer.GetFilterMode);
        }

        private void OnBeforeAssemblyReload()
        {
            if (_clearOnRecompile)
                ForceClearLogs();
        }

        private void ForceClearLogs()
        {
            ClearUnityConsole();
            logManager.Clear();
            _selectedIndex = -1;
            _multiSelectedIndices.Clear();
            Repaint();
        }

        private void ClearUnityConsole()
        {
            try
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetAssembly(typeof(SceneView));
                if (assembly == null)
                {
                    Debug.LogWarning("[EnhancedConsole] Could not access UnityEditor assembly");
                    return;
                }

                Type logEntries = assembly.GetType("UnityEditor.LogEntries");
                if (logEntries == null)
                {
                    Debug.LogWarning("[EnhancedConsole] Could not find LogEntries type");
                    return;
                }

                MethodInfo clearMethod = logEntries.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
                if (clearMethod == null)
                {
                    Debug.LogWarning("[EnhancedConsole] Could not find Clear method on LogEntries");
                    return;
                }

                clearMethod.Invoke(null, null);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[EnhancedConsole] Failed to clear Unity console: {ex.Message}");
            }
        }

        private void OnPlayModeChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    _isInPlayMode = true;
                    if (instance != null)
                    {
                        instance.Focus();
                        instance.Repaint();
                    }

                    // Add frequent refresh updates during play mode
                    EditorApplication.update += RefreshDuringPlayMode;
                    break;

                case PlayModeStateChange.ExitingPlayMode:
                    _isInPlayMode = false;
                    EditorApplication.update -= RefreshDuringPlayMode;
                    if (instance != null)
                        instance.Repaint();
                    break;

                case PlayModeStateChange.ExitingEditMode:
                    if (_clearOnPlay)
                        ClearLogs();
                    break;
            }
        }

        private void RefreshDuringPlayMode()
        {
            if (!_isInPlayMode || instance == null)
                return;

            double now = EditorApplication.timeSinceStartup;
            if (now - _lastPlayModeRepaintTime < PLAY_MODE_REPAINT_INTERVAL_SECONDS)
                return;

            _lastPlayModeRepaintTime = now;
            instance.Repaint();
        }

        private void Update()
        {
        }

        private void OnCompilationStarted(object context)
        {
            if (_clearOnRecompile)
                logManager.Clear();
        }

        private void OnCompilationFinished(object context)
        {
            EditorApplication.delayCall += RepaintOnceMore;
            Repaint();
        }

        private void RepaintOnceMore()
        {
            EditorApplication.delayCall -= RepaintOnceMore;
            if (instance != null)
                instance.Repaint();
        }



        private void HandleLog(string message, string stackTrace, LogType type)
        {
            logManager.AddLog(message, stackTrace, type);
            RepaintWindow();
        }

        private void RepaintWindow()
        {
            if (instance != null)
            {
                instance.Repaint();
                EditorApplication.delayCall += DelayedRepaint;
            }
        }

        private void DelayedRepaint()
        {
            EditorApplication.delayCall -= DelayedRepaint;
            if (instance != null)
                instance.Repaint();
        }


        private void OnGUI()
        {
            try
            {
                if (_viewRenderer == null)
                    InitializeComponents();

                // Lazy initialize styles when EditorStyles are ready (first GUI call)
                if (!_isStylesInitialized)
                {
                    _viewRenderer.InitializeStyles();
                    InitializeStyles();
                    _isStylesInitialized = true;
                }

                HandleWindowFocus();
                _compilationTracker.UpdateErrorStatus();

                DrawToolbar();
                DrawLogList();

                if (!_showStackTrace)
                    return;

                DrawDetailsPanel();
                DrawResizeHandle();
            }
            catch (System.Exception ex)
            {
                EditorGUILayout.HelpBox($"EnhancedConsole Error: {ex.Message}\n{ex.StackTrace}", MessageType.Error);
            }
        }

        private void HandleWindowFocus()
        {
            if (!_hadFocus && hasFocus)
            {
                _hadFocus = true;
                LoadSelectiveLogsConfig();
                _sectionOrganizer.RefreshSectionStates();
            }

            _hadFocus = hasFocus;

            if (_selectiveLogsConfig != null)
                _sectionOrganizer.RefreshSectionStates();
        }

        private void DrawToolbar()
        {
            string compilationStatus = GetCompilationStatusForTitle();
            titleContent = new GUIContent($" Console {compilationStatus} ");

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button(new GUIContent("Clear", "Clear all logs"), EditorStyles.toolbarButton, GUILayout.Width(70)))
                ClearLogs();

            DrawClearOptionsMenu();
            DrawCollapseToggle();
            DrawCollapseOptionsMenu();
            DrawStackTraceToggle();
            DrawTimeToggle();

            if (!Application.isPlaying)
            {
                DrawForceRecompileButton();
                DrawForceRecompileOptionsMenu();
            }

            GUILayout.FlexibleSpace();

            DrawSearchField();
            DrawLogTypeToggles();
            DrawSectionLogsButton();

            EditorGUILayout.EndHorizontal();
        }

        private void ToggleClearOnPlay()
        {
            _clearOnPlay = !_clearOnPlay;
            SavePreferences();
        }

        private void ToggleClearOnBuild()
        {
            _clearOnBuild = !_clearOnBuild;
            SavePreferences();
        }

        private void ToggleClearOnRecompile()
        {
            _clearOnRecompile = !_clearOnRecompile;
            SavePreferences();
        }

        private void DrawClearOptionsMenu()
        {
            Rect dropdownRect = GUILayoutUtility.GetRect(new GUIContent("▼"), EditorStyles.toolbarDropDown, GUILayout.Width(15));
            if (GUI.Button(dropdownRect, "▼", EditorStyles.toolbarDropDown))
            {
                GenericMenu menu = new();
                menu.AddItem(new GUIContent("Clear on Play"), _clearOnPlay, ToggleClearOnPlay);
                menu.AddItem(new GUIContent("Clear on Build"), _clearOnBuild, ToggleClearOnBuild);
                menu.AddItem(new GUIContent("Clear on Recompile"), _clearOnRecompile, ToggleClearOnRecompile);
                menu.DropDown(dropdownRect);
            }
        }

        private void DrawCollapseToggle()
        {
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = _filterManager.IsCollapseEnabled ? _colorScheme.ToggleButtonActive : Color.white;

            bool newCollapseState = GUILayout.Toggle(
                _filterManager.IsCollapseEnabled,
                "Collapse",
                EditorStyles.toolbarButton,
                GUILayout.Width(70));

            if (newCollapseState != _previousCollapseState)
            {
                _filterManager.SetCollapse(newCollapseState);
                _previousCollapseState = newCollapseState;
                logManager.RebuildWithCollapse();
            }
            else
            {
                _filterManager.SetCollapse(newCollapseState);
            }

            GUI.backgroundColor = originalColor;
        }

        private void DrawCollapseOptionsMenu()
        {
            Rect dropdownRect = GUILayoutUtility.GetRect(new GUIContent("▼"), EditorStyles.toolbarDropDown, GUILayout.Width(15));
            if (GUI.Button(dropdownRect, "▼", EditorStyles.toolbarDropDown))
            {
                GenericMenu menu = new();
                menu.AddItem(new GUIContent("Exact Stacktrace"), _filterManager.IsCollapseExactStacktraceEnabled, ToggleCollapseExactStacktrace);
                menu.DropDown(dropdownRect);
            }
        }

        private void ToggleCollapseExactStacktrace()
        {
            _filterManager.SetCollapseExactStacktrace(!_filterManager.IsCollapseExactStacktraceEnabled);
            SavePreferences();
            logManager.RebuildWithCollapse();
        }

        private void DrawStackTraceToggle()
        {
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = _showStackTrace ? _colorScheme.ToggleButtonActive : Color.white;
            bool newShowStackTrace = GUILayout.Toggle(_showStackTrace, "Stack Trace", EditorStyles.toolbarButton, GUILayout.Width(80));
            if (newShowStackTrace != _showStackTrace)
            {
                _showStackTrace = newShowStackTrace;
                _formatter.SetShowStackTrace(_showStackTrace);
                SavePreferences();
            }
            GUI.backgroundColor = originalColor;
        }

        private void DrawTimeToggle()
        {
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = _showTime ? _colorScheme.ToggleButtonActive : Color.white;
            _showTime = GUILayout.Toggle(_showTime, "Time", EditorStyles.toolbarButton, GUILayout.Width(50));
            if (GUI.changed)
                SavePreferences();
            GUI.backgroundColor = originalColor;
        }

        private void DrawForceRecompileButton()
        {
            string buttonLabel = _recompileWithAssetRefresh ? "Recompile + Asset Refresh" : "Recompile";
            GUIContent buttonContent = new(buttonLabel);
            float buttonWidth = EditorStyles.toolbarButton.CalcSize(buttonContent).x + 10;
            if (GUILayout.Button(buttonContent, EditorStyles.toolbarButton, GUILayout.Width(buttonWidth)))
                ExecuteRecompile();
        }

        private void DrawForceRecompileOptionsMenu()
        {
            Rect dropdownRect = GUILayoutUtility.GetRect(new GUIContent("▼"), EditorStyles.toolbarDropDown, GUILayout.Width(15));
            if (GUI.Button(dropdownRect, "▼", EditorStyles.toolbarDropDown))
            {
                GenericMenu menu = new();
                menu.AddItem(new GUIContent("Recompile Only"), !_recompileWithAssetRefresh, ToggleRecompileMode_OnlyRecompile);
                menu.AddItem(new GUIContent("Recompile and Asset Refresh"), _recompileWithAssetRefresh, ToggleRecompileMode_WithAssetRefresh);
                menu.DropDown(dropdownRect);
            }
        }

        private void ToggleRecompileMode_OnlyRecompile()
        {
            _recompileWithAssetRefresh = false;
            SavePreferences();
        }

        private void ToggleRecompileMode_WithAssetRefresh()
        {
            _recompileWithAssetRefresh = true;
            SavePreferences();
        }

        private void ExecuteRecompile()
        {
            if (_recompileWithAssetRefresh)
                ForceRecompileWithAssetRefresh();
            else
                ForceRecompileAll();
        }

        private void ForceRecompileAll()
        {
            Debug.Log("[Force Recompile] Starting full recompilation of all assemblies...");
            CompilationPipeline.RequestScriptCompilation();
            Debug.Log("[Force Recompile] Recompilation requested. Unity will recompile all scripts.");
            Debug.Log("[Force Recompile] Wait for compilation to finish (check spinning icon in bottom-right).");
        }

        private void ForceRecompileWithAssetRefresh()
        {
            Debug.Log("[Force Recompile] Starting full recompilation with asset database refresh...");
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            CompilationPipeline.RequestScriptCompilation();
            Debug.Log("[Force Recompile] Asset database refreshed and recompilation requested.");
            Debug.Log("[Force Recompile] This may take a moment. Check Console for 'Compilation finished'.");
        }

        private string GetCompilationStatusForTitle()
        {
            if (!Application.isPlaying)
                return "";

            return $"| Total: {logManager.GetEntries().Count} - Filtered: {GetFilteredLogs().Count}";
        }

        private void DrawSearchField()
        {
            GUILayout.Label("Search:", EditorStyles.miniLabel, GUILayout.Width(60));
            string newSearch = EditorGUILayout.TextField(_filterManager.GetSearchFilter(), EditorStyles.toolbarSearchField, GUILayout.Width(200));
            if (newSearch != _filterManager.GetSearchFilter())
                _filterManager.SetSearchFilter(newSearch);

            if (GUILayout.Button("×", EditorStyles.toolbarButton, GUILayout.Width(20)))
                _filterManager.SetSearchFilter("");
        }

        private void DrawLogTypeToggles()
        {
            List<LogEntry> logs = logManager.GetEntries();
            int infoCount = _filterManager.GetInfoCount(logs);
            int warningCount = _filterManager.GetWarningCount(logs);
            int errorCount = _filterManager.GetErrorCount(logs);

            Color originalColor = GUI.backgroundColor;

            GUI.backgroundColor = _filterManager.IsShowingInfo ? Color.white : _colorScheme.DisabledButton;
            bool newShowInfo = GUILayout.Toggle(_filterManager.IsShowingInfo,
                new GUIContent($" {infoCount}", EditorGUIUtility.IconContent("console.infoicon").image),
                EditorStyles.toolbarButton, GUILayout.Width(50));
            if (newShowInfo != _filterManager.IsShowingInfo)
                _filterManager.SetShowInfo(newShowInfo);

            GUI.backgroundColor = _filterManager.IsShowingWarnings ? Color.white : _colorScheme.DisabledButton;
            bool newShowWarnings = GUILayout.Toggle(_filterManager.IsShowingWarnings,
                new GUIContent($" {warningCount}", EditorGUIUtility.IconContent("console.warnicon").image),
                EditorStyles.toolbarButton, GUILayout.Width(50));
            if (newShowWarnings != _filterManager.IsShowingWarnings)
                _filterManager.SetShowWarnings(newShowWarnings);

            GUI.backgroundColor = _filterManager.IsShowingErrors ? Color.white : _colorScheme.DisabledButton;
            bool newShowErrors = GUILayout.Toggle(_filterManager.IsShowingErrors,
                new GUIContent($" {errorCount}", EditorGUIUtility.IconContent("console.erroricon").image),
                EditorStyles.toolbarButton, GUILayout.Width(50));
            if (newShowErrors != _filterManager.IsShowingErrors)
                _filterManager.SetShowErrors(newShowErrors);

            GUI.backgroundColor = originalColor;
        }

        private void DrawSectionLogsButton()
        {
            Color originalColor = GUI.backgroundColor;

            if (_selectiveLogsConfig == null)
            {
                GUI.backgroundColor = _colorScheme.DisabledUIElement;
                GUI.enabled = false;
                GUILayout.Button("SL (--/--)", EditorStyles.toolbarButton, GUILayout.Width(70));
                GUI.enabled = true;
                GUI.backgroundColor = originalColor;
                return;
            }

            SelectiveLogSection[] allSections = _selectiveLogsConfig.GetAllSections();
            int activeSections = _sectionOrganizer.GetActiveSectionCount();
            int totalSections = allSections.Length;

            bool allActive = activeSections == totalSections;
            GUI.backgroundColor = _sectionOrganizer.IsFilterActive
                ? (allActive ? Color.white : _colorScheme.ToggleButtonActive)
                : _colorScheme.DisabledButton;

            bool newFilterActive = GUILayout.Toggle(_sectionOrganizer.IsFilterActive,
                $"SL ({activeSections}/{totalSections})",
                EditorStyles.toolbarButton,
                GUILayout.Width(70));

            if (newFilterActive != _sectionOrganizer.IsFilterActive)
            {
                _sectionOrganizer.SetFilterActive(newFilterActive);
                SavePreferences();
                Repaint();
            }

            GUI.backgroundColor = originalColor;

            Rect dropdownRect = GUILayoutUtility.GetRect(new GUIContent("▼"), EditorStyles.toolbarDropDown, GUILayout.Width(15));
            if (GUI.Button(dropdownRect, "▼", EditorStyles.toolbarDropDown))
            {
                GenericMenu menu = new();
                menu.AddItem(new GUIContent("Stack And Content"), _sectionOrganizer.GetFilterMode == SLFilterMode.StackAndContent, SetSectionFilterMode_StackAndContent);
                menu.AddItem(new GUIContent("Only Stack"), _sectionOrganizer.GetFilterMode == SLFilterMode.OnlyStack, SetSectionFilterMode_OnlyStack);
                menu.AddItem(new GUIContent("Only Content"), _sectionOrganizer.GetFilterMode == SLFilterMode.OnlyContent, SetSectionFilterMode_OnlyContent);
                menu.DropDown(dropdownRect);
            }
        }

        private void SetSectionFilterMode_StackAndContent()
        {
            _sectionOrganizer.SetFilterMode(SLFilterMode.StackAndContent);
            EditorPrefs.SetInt(PREF_SL_FILTER_MODE, (int)SLFilterMode.StackAndContent);
            Repaint();
        }

        private void SetSectionFilterMode_OnlyStack()
        {
            _sectionOrganizer.SetFilterMode(SLFilterMode.OnlyStack);
            EditorPrefs.SetInt(PREF_SL_FILTER_MODE, (int)SLFilterMode.OnlyStack);
            Repaint();
        }

        private void SetSectionFilterMode_OnlyContent()
        {
            _sectionOrganizer.SetFilterMode(SLFilterMode.OnlyContent);
            EditorPrefs.SetInt(PREF_SL_FILTER_MODE, (int)SLFilterMode.OnlyContent);
            Repaint();
        }

        private void DrawLogList()
        {
            List<LogEntry> filteredLogs = GetFilteredLogs();

            float minLogListHeight = 120f;
            float availableHeight = _showStackTrace
                ? position.height - TOOLBAR_HEIGHT - _detailsHeight - 10
                : position.height - TOOLBAR_HEIGHT - 10;
            float totalContentHeight = Mathf.Max(filteredLogs.Count * LOG_ENTRY_HEIGHT, minLogListHeight);
            float constrainedHeight = Mathf.Min(availableHeight, totalContentHeight);

            Rect scrollViewRect = GUILayoutUtility.GetRect(0, constrainedHeight, GUILayout.ExpandWidth(true));
            float contentHeight = filteredLogs.Count * LOG_ENTRY_HEIGHT;

            try
            {
                _scrollPosition = GUI.BeginScrollView(scrollViewRect, _scrollPosition,
                    new Rect(0, 0, scrollViewRect.width - 20, contentHeight));

                int firstVisibleIndex = Mathf.Max(0, Mathf.FloorToInt(_scrollPosition.y / LOG_ENTRY_HEIGHT));
                int visibleRowCount = Mathf.CeilToInt(scrollViewRect.height / LOG_ENTRY_HEIGHT) + 1;
                int lastVisibleIndex = Mathf.Min(filteredLogs.Count - 1, firstVisibleIndex + visibleRowCount);

                for (int i = firstVisibleIndex; i <= lastVisibleIndex; i++)
                    DrawLogListEntry(filteredLogs, i, scrollViewRect);
            }
            finally
            {
                GUI.EndScrollView();
            }

            HandleLogListInput(filteredLogs);
        }

        private void DrawLogListEntry(List<LogEntry> filteredLogs, int i, Rect scrollViewRect)
        {
            LogEntry log = filteredLogs[i];
            Rect entryRect = new(0, i * LOG_ENTRY_HEIGHT, scrollViewRect.width - 20, LOG_ENTRY_HEIGHT);

            Color backgroundColor = _selectedIndex == i || _multiSelectedIndices.Contains(i)
                ? _colorScheme.SelectedRowBackground
                : (i % 2 == 0 ? _colorScheme.EvenRowBackground : _colorScheme.OddRowBackground);

            EditorGUI.DrawRect(entryRect, backgroundColor);

            DrawSectionBubble(entryRect, log);
            DrawLogIcon(entryRect, log);
            DrawLogMessage(entryRect, log);
            DrawLogTime(entryRect, log);
            DrawCollapseCount(entryRect, log, i);
        }

        private void DrawSectionBubble(Rect entryRect, LogEntry log)
        {
            SelectiveLogSection section = _sectionOrganizer.GetSectionForLog(log);
            if (section == null)
                return;

            Rect bubbleRect = new(entryRect.x + 2, entryRect.y + 10, 8, 12);
            EditorGUI.DrawRect(bubbleRect, section.color);
        }

        private void DrawLogIcon(Rect entryRect, LogEntry log)
        {
            Rect iconRect = new(entryRect.x + 15, entryRect.y + 8, 16, 16);
            GUI.DrawTexture(iconRect, _formatter.GetIconForLogType(log.Type));
        }

        private void DrawLogMessage(Rect entryRect, LogEntry log)
        {
            float textStartX = entryRect.x + 15 + 16 + 5;
            float rightPadding = 5f;
            float countCircleSize = 20f;
            float timeWidth = _showTime ? 60f : 0f;
            float availableTextWidth = entryRect.width - textStartX - rightPadding - countCircleSize - timeWidth;

            Rect textRect = new(textStartX, entryRect.y + 2, availableTextWidth, LOG_ENTRY_HEIGHT);

            string displayText = _formatter.TruncateMessage(log.Message);

            GUI.Label(textRect, displayText, _logMessageStyle);
        }

        private void DrawLogTime(Rect entryRect, LogEntry log)
        {
            if (!_showTime)
                return;

            float timeWidth = 60f;
            Rect timeRect = new(entryRect.xMax - 5 - 20 - timeWidth, entryRect.y + 2, timeWidth - 5, LOG_ENTRY_HEIGHT);

            string timeText = _formatter.FormatTimeDisplay(log.RealtimeSinceStartup);
            GUI.Label(timeRect, timeText, _logTimeStyle);
        }

        private void DrawCollapseCount(Rect entryRect, LogEntry log, int index)
        {
            if (!_filterManager.IsCollapseEnabled || log.Count <= 1)
                return;

            float countCircleSize = 20f;
            Rect circleRect = new(entryRect.xMax - 5 - countCircleSize, entryRect.y + (LOG_ENTRY_HEIGHT - countCircleSize) * 0.5f, countCircleSize, countCircleSize);

            Color circleColor = _selectedIndex == index
                ? _colorScheme.SelectedCountCircle
                : (index % 2 == 0 ? _colorScheme.CountCircleBackground : _colorScheme.CountCircleBackgroundDark);

            EditorGUI.DrawRect(circleRect, circleColor);

            GUI.Label(circleRect, log.Count.ToString(), _logCountStyle);
        }

        private void HandleLogListInput(List<LogEntry> filteredLogs)
        {
            Event e = Event.current;

            HandleMouseInput(e, filteredLogs);
            HandleKeyboardShortcuts(e, filteredLogs);
            HandleArrowKeyNavigation(e, filteredLogs);
        }

        private void HandleMouseInput(Event e, List<LogEntry> filteredLogs)
        {
            if (e.type != EventType.MouseDown)
                return;

            Rect scrollArea = GUILayoutUtility.GetLastRect();
            if (!scrollArea.Contains(e.mousePosition))
                return;

            int clickedIndex = (int)((e.mousePosition.y - scrollArea.y + _scrollPosition.y) / LOG_ENTRY_HEIGHT);
            if (clickedIndex < 0 || clickedIndex >= filteredLogs.Count)
                return;

            if (e.clickCount == 2)
            {
                LogEntry logEntry = filteredLogs[clickedIndex];
                OpenScriptAtLogLocation(logEntry);
            }
            else
            {
                HandleLogEntrySelection(clickedIndex, e);
                TrySelectGameObjectInHierarchy(filteredLogs[clickedIndex]);
            }
        }

        private void HandleKeyboardShortcuts(Event e, List<LogEntry> filteredLogs)
        {
            if (e.type != EventType.KeyDown || (!e.control && !e.command))
                return;

            if (e.keyCode == KeyCode.A && filteredLogs.Count > 0)
                SelectAllLogs(filteredLogs, e);
            else if (e.keyCode == KeyCode.C && (_selectedIndex >= 0 || _multiSelectedIndices.Count > 0))
            {
                CopySelectedEntries(filteredLogs);
                e.Use();
            }
        }

        private void SelectAllLogs(List<LogEntry> filteredLogs, Event e)
        {
            _multiSelectedIndices.Clear();
            for (int i = 0; i < filteredLogs.Count; i++)
                _multiSelectedIndices.Add(i);
            _selectedIndex = 0;
            e.Use();
            Repaint();
        }

        private void HandleArrowKeyNavigation(Event e, List<LogEntry> filteredLogs)
        {
            if (e.type != EventType.KeyDown || filteredLogs.Count == 0)
                return;

            if (e.keyCode == KeyCode.UpArrow && _selectedIndex > 0)
            {
                _selectedIndex--;
                _multiSelectedIndices.Clear();
                e.Use();
            }
            else if (e.keyCode == KeyCode.DownArrow && _selectedIndex < filteredLogs.Count - 1)
            {
                _selectedIndex++;
                _multiSelectedIndices.Clear();
                e.Use();
            }
        }

        private void HandleLogEntrySelection(int index, Event currentEvent)
        {
            if (currentEvent.shift && _selectedIndex >= 0)
            {
                int start = Mathf.Min(_selectedIndex, index);
                int end = Mathf.Max(_selectedIndex, index);
                for (int j = start; j <= end; j++)
                    _multiSelectedIndices.Add(j);
            }
            else if (currentEvent.control || currentEvent.command)
            {
                if (_multiSelectedIndices.Contains(index))
                    _multiSelectedIndices.Remove(index);
                else
                    _multiSelectedIndices.Add(index);
            }
            else
            {
                _multiSelectedIndices.Clear();
            }

            _selectedIndex = index;
            GUI.FocusControl(null);
            currentEvent.Use();
        }

        private void DrawDetailsPanel()
        {
            List<LogEntry> filteredLogs = GetFilteredLogs();

            if (_selectedIndex < 0 || _selectedIndex >= filteredLogs.Count)
                return;

            LogEntry selectedLog = filteredLogs[_selectedIndex];

            if (string.IsNullOrEmpty(selectedLog.StackTrace))
                return;

            Rect detailsRect = new(0, position.height - _detailsHeight, position.width, _detailsHeight);

            DrawDetailsSeparator(detailsRect);
            DrawMessagePanel(detailsRect);
            DrawStackTracePanel(detailsRect, selectedLog.StackTrace);
        }

        private void DrawDetailsSeparator(Rect detailsRect)
        {
            Rect separatorLine1 = new(0, detailsRect.y - 3, position.width, 1);
            Rect separatorLine2 = new(0, detailsRect.y - 1, position.width, 1);
            EditorGUI.DrawRect(separatorLine1, _colorScheme.SeparatorLineDim);
            EditorGUI.DrawRect(separatorLine2, _colorScheme.SeparatorLineDim);
        }

        private void DrawMessagePanel(Rect detailsRect)
        {
            EditorGUI.LabelField(new Rect(detailsRect.x + 10, detailsRect.y, detailsRect.width - 10, 24), "Stacktrace:");
        }

        private void DrawStackTracePanel(Rect detailsRect, string stackTrace)
        {
            float stackTraceStartY = detailsRect.y + 24 + 10;
            float stackTraceHeight = detailsRect.yMax - stackTraceStartY;

            Rect stackTraceRect = new(detailsRect.x, stackTraceStartY, detailsRect.width, stackTraceHeight);
            EditorGUI.DrawRect(stackTraceRect, _colorScheme.StackTracePanelBackground);

            Rect scrollViewRect = new(stackTraceRect.x + 4, stackTraceRect.y + 4, stackTraceRect.width - 8, stackTraceRect.height - 8);

            string[] stackTraceLines = stackTrace.Split('\n');
            float totalContentHeight = stackTraceLines.Length * 16f;

            try
            {
                _detailsScrollPosition = GUI.BeginScrollView(
                    scrollViewRect,
                    _detailsScrollPosition,
                    new Rect(0, 0, scrollViewRect.width - 20, totalContentHeight),
                    false,
                    totalContentHeight > scrollViewRect.height
                );

                DrawStackTraceLines(stackTraceLines, scrollViewRect);
            }
            finally
            {
                GUI.EndScrollView();
            }
        }

        private void DrawStackTraceLines(string[] stackTraceLines, Rect scrollViewRect)
        {
            float lineHeight = 16f;

            for (int i = 0; i < stackTraceLines.Length; i++)
            {
                string line = stackTraceLines[i];
                if (string.IsNullOrEmpty(line))
                    continue;

                Rect lineRect = new(0, i * lineHeight, scrollViewRect.width - 20, lineHeight);
                DrawStackTraceLine(lineRect, line, _stackTraceLineStyle, _stackTraceLineClickableStyle);
            }
        }

        private void DrawStackTraceLine(Rect lineRect, string line, GUIStyle normalStyle, GUIStyle clickableStyle)
        {
            StackTraceLineInfo lineInfo = _stackTraceParser.ParseLine(line);

            if (lineInfo.HasFileInfo)
            {
                if (GUI.Button(lineRect, "", GUIStyle.none))
                    OpenScriptAtLocation(lineInfo.FilePath, lineInfo.LineNumber);

                EditorGUIUtility.AddCursorRect(lineRect, MouseCursor.Link);
                GUI.Label(lineRect, line, clickableStyle);
            }
            else
            {
                GUI.Label(lineRect, line, normalStyle);
            }
        }

        private GUIStyle CreateStackTraceLineStyle()
        {
            return new(EditorStyles.label)
            {
                padding = new RectOffset(4, 4, 2, 2),
                fontSize = 11,
                wordWrap = false,
                richText = true
            };
        }

        private GUIStyle CreateStackTraceLineStyleClickable(GUIStyle baseStyle)
        {
            return new(baseStyle)
            {
                normal = { textColor = _colorScheme.TextLink },
                hover = { textColor = _colorScheme.TextLinkHover }
            };
        }

        private void InitializeStyles()
        {
            _logMessageStyle = new GUIStyle(EditorStyles.label)
            {
                richText = true,
                wordWrap = false
            };
            _logMessageStyle.normal.textColor = _colorScheme.TextDefault;

            _logTimeStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight
            };
            _logTimeStyle.normal.textColor = _colorScheme.TextMuted;

            _logCountStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _logCountStyle.normal.textColor = _colorScheme.CountCircleText;

            _stackTraceLineStyle = CreateStackTraceLineStyle();
            _stackTraceLineClickableStyle = CreateStackTraceLineStyleClickable(_stackTraceLineStyle);
        }

        private void DrawResizeHandle()
        {
            if (!_showStackTrace)
                return;

            Rect resizeRect = new(0, position.height - _detailsHeight - 5, position.width, 5);

            EditorGUIUtility.AddCursorRect(resizeRect, MouseCursor.ResizeVertical);

            float centerY = position.height - _detailsHeight - 2.5f;
            float centerX = position.width * 0.5f;

            Rect separatorRect = new(0, centerY - 0.5f, position.width, 1f);
            EditorGUI.DrawRect(separatorRect, _colorScheme.SeparatorLine);

            float gripSpacing = 4f;
            float gripSize = 3f;

            for (int i = -1; i <= 1; i++)
            {
                Rect gripRect = new(centerX + (i * gripSpacing) - gripSize * 0.5f, centerY - gripSize * 0.5f, gripSize, gripSize);
                EditorGUI.DrawRect(gripRect, _colorScheme.GripHandle);
            }

            Event currentEvent = Event.current;

            if (currentEvent.type == EventType.MouseDown && resizeRect.Contains(currentEvent.mousePosition))
            {
                _isResizingDetails = true;
                currentEvent.Use();
            }

            if (!_isResizingDetails)
                return;

            if (currentEvent.type == EventType.MouseDrag)
            {
                float minLogListHeight = 120f;
                float maxDetailsHeight = position.height - TOOLBAR_HEIGHT - minLogListHeight - 10;
                _detailsHeight = Mathf.Clamp(position.height - currentEvent.mousePosition.y, 100f, maxDetailsHeight);
                SavePreferences();
                Repaint();
                currentEvent.Use();
            }
            else if (currentEvent.type == EventType.MouseUp)
            {
                _isResizingDetails = false;
                currentEvent.Use();
            }
        }

        private List<LogEntry> GetFilteredLogs()
        {
            return _filterManager.FilterLogs(logManager.GetEntries());
        }

        private void CopySelectedEntries(List<LogEntry> filteredLogs)
        {
            if (_selectedIndex < 0 && _multiSelectedIndices.Count == 0)
                return;

            StringBuilder clipboard = new();

            HashSet<int> indicesToCopy = new(_multiSelectedIndices);
            if (_selectedIndex >= 0)
                indicesToCopy.Add(_selectedIndex);

            List<int> sortedIndices = new(indicesToCopy);
            sortedIndices.Sort();

            for (int i = 0; i < sortedIndices.Count; i++)
            {
                int index = sortedIndices[i];
                if (index < 0 || index >= filteredLogs.Count)
                    continue;

                if (i > 0)
                    clipboard.AppendLine();

                LogEntry log = filteredLogs[index];
                clipboard.Append(_formatter.FormatForClipboard(log));
            }

            EditorGUIUtility.systemCopyBuffer = clipboard.ToString();
        }

        private void ClearLogs()
        {
            ForceClearLogs();
        }

        private void OpenScriptAtLogLocation(LogEntry log)
        {
            if (!string.IsNullOrEmpty(log.StackTrace))
            {
                if (_stackTraceParser.TryParseFirstLine(log.StackTrace, out string filePath, out int lineNumber))
                {
                    OpenScriptAtLocation(filePath, lineNumber);
                    return;
                }
            }

            if (!string.IsNullOrEmpty(log.Message))
                TryOpenFromCompilerError(log.Message);
        }

        private void TryOpenFromCompilerError(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            int openParenIndex = message.IndexOf('(');
            int closeParenIndex = message.IndexOf(')');

            if (openParenIndex < 0 || closeParenIndex < 0 || closeParenIndex <= openParenIndex)
                return;

            string beforeParen = message.Substring(0, openParenIndex);
            string insideParen = message.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1);

            if (!beforeParen.StartsWith("Assets/") && !beforeParen.StartsWith("Packages/"))
                return;

            string[] coordinates = insideParen.Split(',');
            if (coordinates.Length < 1 || !int.TryParse(coordinates[0].Trim(), out int lineNumber))
                return;

            string assetPath = beforeParen.Trim();
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (asset != null)
                OpenScriptAtLocation(assetPath, lineNumber);
        }

        private void OpenScriptAtLocation(string filePath, int lineNumber)
        {
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(filePath, lineNumber);
        }

        private void TrySelectGameObjectInHierarchy(LogEntry logEntry)
        {
            if (logEntry.ContextGameObjectInstanceId == 0)
                return;

            GameObject targetGameObject = null;

            // Search in ALL loaded scenes
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (!scene.isLoaded)
                    continue;

                foreach (GameObject go in scene.GetRootGameObjects())
                {
                    targetGameObject = FindGameObjectByInstanceId(go, logEntry.ContextGameObjectInstanceId);
                    if (targetGameObject != null)
                        break;
                }

                if (targetGameObject != null)
                    break;
            }

             // If not found in scenes, search in DontDestroyOnLoad
             if (targetGameObject == null)
             {
                 GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                 foreach (GameObject go in allObjects)
                {
                    if (go.GetInstanceID() == logEntry.ContextGameObjectInstanceId)
                    {
                        targetGameObject = go;
                        break;
                    }
                }
            }

            if (targetGameObject == null)
                return;

            EditorGUIUtility.PingObject(targetGameObject);
            Selection.activeGameObject = targetGameObject;
        }

        private GameObject FindGameObjectByInstanceId(GameObject root, int instanceId)
        {
            if (root.GetInstanceID() == instanceId)
                return root;

            foreach (Transform child in root.transform)
            {
                GameObject result = FindGameObjectByInstanceId(child.gameObject, instanceId);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}
#endif
