#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EnhancedConsole.Editor.LogManagement;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace EnhancedConsole.Editor
{
    /// <summary>
    /// EnhancedConsoleWindow: Main window coordinator for the Enhanced Console.
    /// Responsibility: Orchestrate window lifecycle and delegate to focused components.
    /// </summary>
    public class EnhancedConsoleWindow : EditorWindow
    {
        private const int TOOLBAR_HEIGHT = 20;
        private const int LOG_ENTRY_HEIGHT = 32;
        private const float DEFAULT_DETAILS_HEIGHT = 200f;
        private const string PREF_CLEAR_ON_PLAY = "EnhancedConsole_ClearOnPlay";
        internal const string PREF_CLEAR_ON_BUILD = "EnhancedConsole_ClearOnBuild";
        private const string PREF_CLEAR_ON_RECOMPILE = "EnhancedConsole_ClearOnRecompile";
        private const string PREF_COLLAPSE = "EnhancedConsole_Collapse";
        private const string PREF_COLLAPSE_EXACT_STACKTRACE = "EnhancedConsole_CollapseExactStacktrace";
        private const string PREF_SHOW_TIME = "EnhancedConsole_ShowTime";
        private const string PREF_SHOW_STACK_TRACE = "EnhancedConsole_ShowStackTrace";
        private const string PREF_DETAILS_HEIGHT = "EnhancedConsole_DetailsHeight";
        private const string PREF_SL_FILTER_MODE = "EnhancedConsole_SL_FilterMode";
        private const string PREF_RECOMPILE_MODE = "EnhancedConsole_RecompileMode";
        private const float STACK_TRACE_LINE_HEIGHT = 16f;

        // New logs mark the window dirty; it repaints at most this often instead of once (plus a
        // queued delayCall) per log, and only when something changed.
        private const double REPAINT_INTERVAL_SECONDS = 1d / 30d;

        private static EnhancedConsoleWindow instance;

        // Core Managers (Extracted)
        private LogEntryManager logManager;
        [SerializeField] private List<LogEntry> _persistedLogs = new();

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

        // Preferences
        private SelectiveLogsScriptableObject _selectiveLogsConfig;

        // Components (SRP - Single Responsibility Principle)
        private LogLevelColorScheme _colorScheme;
        private LogEntryFormatter _formatter;
        private StackTraceParser _stackTraceParser;
        private LogFilterManager _filterManager;
        private SectionedLogOrganizer _sectionOrganizer;
        private bool _isStylesInitialized;
        private GUIStyle _logMessageStyle;
        private GUIStyle _logTimeStyle;
        private GUIStyle _logCountStyle;
        private GUIStyle _stackTraceLineStyle;
        private GUIStyle _stackTraceLineClickableStyle;
        private bool _repaintPending;
        private double _lastRepaintTime;
        private bool _guiErrorLogged;

        // Selection is stored by index into the filtered view; the entry is kept so the selection
        // can be found again when the view is rebuilt (filter change, collapse, pruning).
        private LogEntry _selectedEntry;
        private int _seenFilteredResetCount = -1;

        // Stack trace panel cache, rebuilt only when the selected stack trace changes.
        private string _cachedStackTrace;
        private string[] _stackTraceLines = Array.Empty<string>();
        private StackTraceLineInfo[] _stackTraceLineInfos = Array.Empty<StackTraceLineInfo>();

        // Toolbar content, updated only when the values change.
        private readonly GUIContent _infoToggleContent = new();
        private readonly GUIContent _warningToggleContent = new();
        private readonly GUIContent _errorToggleContent = new();
        private int _shownInfoCount = -1;
        private int _shownWarningCount = -1;
        private int _shownErrorCount = -1;
        private string _shownTitle;


        // UI State
        private bool _previousCollapseState;
        private Rect _sectionMenuScreenRect;

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
            RestorePersistedLogs();
        }

        private void InitializeComponents()
        {
            _colorScheme = new();
            _stackTraceParser = new();
            _formatter ??= new(true, false);
            _filterManager = new();
            _sectionOrganizer = _filterManager.GetSectionOrganizer();

            logManager = new(_filterManager);
        }

        private void RegisterEventHandlers()
        {
            LogContextHelper.Initialize();
            LogContextHelper.ClearLogContext();
            Application.logMessageReceived += HandleLog;
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            CompilationPipeline.compilationStarted += OnCompilationStarted;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
        }

        private void OnDisable()
        {
            UnregisterEventHandlers();
            SavePreferences();
            _isStylesInitialized = false;
        }

        private void UnregisterEventHandlers()
        {
            Application.logMessageReceived -= HandleLog;
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            CompilationPipeline.compilationStarted -= OnCompilationStarted;
            CompilationPipeline.compilationFinished -= OnCompilationFinished;
        }


        internal static void ClearForBuild()
        {
            if (instance != null)
                instance.ClearLogs();
        }

        internal static void RepaintIfOpen()
        {
            if (instance != null)
                instance.Repaint();
        }

        private void RestorePersistedLogs()
        {
            logManager.Restore(_persistedLogs);
            _persistedLogs.Clear();
        }

        private void LoadPreferences()
        {
            _clearOnPlay = EditorPrefs.GetBool(PREF_CLEAR_ON_PLAY, false);
            _clearOnBuild = EditorPrefs.GetBool(PREF_CLEAR_ON_BUILD, false);
            _clearOnRecompile = EditorPrefs.GetBool(PREF_CLEAR_ON_RECOMPILE, false);
            _previousCollapseState = EditorPrefs.GetBool(PREF_COLLAPSE, false);
            _filterManager.SetCollapse(_previousCollapseState);
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
            _selectiveLogsConfig = SelectiveLogsScriptableObject.FindConfig();

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
            EditorPrefs.SetBool(PREF_COLLAPSE, _filterManager.IsCollapseEnabled);
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
            bool isRecompile = !EditorApplication.isPlayingOrWillChangePlaymode;
            if (_clearOnRecompile && isRecompile)
            {
                ForceClearLogs();
                _persistedLogs.Clear();
                return;
            }

            _persistedLogs = logManager.GetPersistableEntries();
        }

        private void ForceClearLogs()
        {
            ClearUnityConsole();
            logManager.Clear();
            _selectedIndex = -1;
            _selectedEntry = null;
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
                    if (instance != null)
                    {
                        instance.Focus();
                        instance.Repaint();
                    }
                    break;

                case PlayModeStateChange.ExitingPlayMode:
                    if (instance != null)
                        instance.Repaint();
                    break;

                case PlayModeStateChange.ExitingEditMode:
                    if (_clearOnPlay)
                        ClearLogs();
                    break;
            }
        }

        private void OnEditorUpdate()
        {
            if (!_repaintPending)
                return;

            double now = EditorApplication.timeSinceStartup;
            if (now - _lastRepaintTime < REPAINT_INTERVAL_SECONDS)
                return;

            _repaintPending = false;
            _lastRepaintTime = now;
            Repaint();
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
            // The context handler runs just before this callback on the same thread.
            int contextInstanceId = LogContextHelper.ConsumeLogContextInstanceId();
            logManager.AddLog(message, stackTrace, type, contextInstanceId);
            _repaintPending = true;
        }


        private void OnGUI()
        {
            try
            {
                if (logManager == null)
                    InitializeComponents();

                // Lazy initialize styles when EditorStyles are ready (first GUI call)
                if (!_isStylesInitialized)
                {
                    InitializeStyles();
                    _isStylesInitialized = true;
                }

                HandleWindowFocus();

                DrawToolbar();
                DrawLogList();

                if (!_showStackTrace)
                    return;

                DrawDetailsPanel();
                DrawResizeHandle();
            }
            catch (ExitGUIException)
            {
                // Unity uses this to abort the current GUI event on purpose.
                throw;
            }
            catch (Exception ex)
            {
                // Drawing a HelpBox here would unbalance the layout groups that were open when the
                // exception was thrown. Log once instead of on every repaint, then abort the event cleanly.
                if (!_guiErrorLogged)
                {
                    _guiErrorLogged = true;
                    Debug.LogException(ex);
                }

                GUIUtility.ExitGUI();
            }
        }

        private void HandleWindowFocus()
        {
            // Searching the AssetDatabase is slow in large projects, so only look again when there's no config.
            if (!_hadFocus && hasFocus && _selectiveLogsConfig == null)
                LoadSelectiveLogsConfig();

            _hadFocus = hasFocus;

            // Cheap: hashes the section fields and recompiles only when the config was edited.
            _sectionOrganizer.RefreshSectionStates();
        }

        private void DrawToolbar()
        {
            UpdateTitle();

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
                SavePreferences();
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
            bool newShowTime = GUILayout.Toggle(_showTime, "Time", EditorStyles.toolbarButton, GUILayout.Width(50));
            if (newShowTime != _showTime)
            {
                _showTime = newShowTime;
                SavePreferences();
            }
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

        private void UpdateTitle()
        {
            string title = Application.isPlaying
                ? $" Console | Total: {logManager.Entries.Count} - Filtered: {GetFilteredLogs().Count} "
                : " Console  ";

            if (title == _shownTitle)
                return;

            _shownTitle = title;
            titleContent = new GUIContent(title);
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
            UpdateCountContent(_infoToggleContent, ref _shownInfoCount, logManager.InfoCount, LogType.Log);
            UpdateCountContent(_warningToggleContent, ref _shownWarningCount, logManager.WarningCount, LogType.Warning);
            UpdateCountContent(_errorToggleContent, ref _shownErrorCount, logManager.ErrorCount, LogType.Error);

            Color originalColor = GUI.backgroundColor;

            GUI.backgroundColor = _filterManager.IsShowingInfo ? Color.white : _colorScheme.DisabledButton;
            bool newShowInfo = GUILayout.Toggle(_filterManager.IsShowingInfo,
                _infoToggleContent,
                EditorStyles.toolbarButton, GUILayout.Width(50));
            if (newShowInfo != _filterManager.IsShowingInfo)
                _filterManager.SetShowInfo(newShowInfo);

            GUI.backgroundColor = _filterManager.IsShowingWarnings ? Color.white : _colorScheme.DisabledButton;
            bool newShowWarnings = GUILayout.Toggle(_filterManager.IsShowingWarnings,
                _warningToggleContent,
                EditorStyles.toolbarButton, GUILayout.Width(50));
            if (newShowWarnings != _filterManager.IsShowingWarnings)
                _filterManager.SetShowWarnings(newShowWarnings);

            GUI.backgroundColor = _filterManager.IsShowingErrors ? Color.white : _colorScheme.DisabledButton;
            bool newShowErrors = GUILayout.Toggle(_filterManager.IsShowingErrors,
                _errorToggleContent,
                EditorStyles.toolbarButton, GUILayout.Width(50));
            if (newShowErrors != _filterManager.IsShowingErrors)
                _filterManager.SetShowErrors(newShowErrors);

            GUI.backgroundColor = originalColor;
        }

        private static void UpdateCountContent(GUIContent content, ref int shownCount, int count, LogType type)
        {
            content.image = LogEntryFormatter.GetIconForLogType(type);
            if (count == shownCount)
                return;

            shownCount = count;
            content.text = " " + count;
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

            int activeSections = _sectionOrganizer.GetActiveSectionCount();
            int totalSections = _sectionOrganizer.GetTotalSectionCount();

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
            if (Event.current.type == EventType.Repaint)
                _sectionMenuScreenRect = GUIUtility.GUIToScreenRect(dropdownRect);

            if (GUI.Button(dropdownRect, "▼", EditorStyles.toolbarDropDown))
            {
                GenericMenu menu = new();
                menu.AddItem(new GUIContent("Sections..."), false, ShowSectionMenu);
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Stack And Content"), _sectionOrganizer.GetFilterMode == SLFilterMode.StackAndContent, SetSectionFilterMode_StackAndContent);
                menu.AddItem(new GUIContent("Only Stack"), _sectionOrganizer.GetFilterMode == SLFilterMode.OnlyStack, SetSectionFilterMode_OnlyStack);
                menu.AddItem(new GUIContent("Only Content"), _sectionOrganizer.GetFilterMode == SLFilterMode.OnlyContent, SetSectionFilterMode_OnlyContent);
                menu.DropDown(dropdownRect);
            }
        }

        private void ShowSectionMenu()
        {
            if (_selectiveLogsConfig == null)
                return;

            SelectiveLogSection[] sections = _selectiveLogsConfig.GetAllSections();
            Dictionary<string, bool> states = new();
            foreach (SelectiveLogSection section in sections)
            {
                if (section != null && section.name != null)
                    states[section.name] = section.enabled;
            }

            SectionLogsMenuWindow.Show(_sectionMenuScreenRect, sections, states, () => ApplySectionStates(sections, states));
        }

        private void ApplySectionStates(SelectiveLogSection[] sections, Dictionary<string, bool> states)
        {
            if (_selectiveLogsConfig == null)
                return;

            foreach (SelectiveLogSection section in sections)
            {
                if (section != null && section.name != null && states.TryGetValue(section.name, out bool enabled))
                    section.enabled = enabled;
            }

            EditorUtility.SetDirty(_selectiveLogsConfig);
            _sectionOrganizer.RefreshSectionStates();
            Repaint();
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

            _selectedEntry = _selectedIndex >= 0 && _selectedIndex < filteredLogs.Count
                ? filteredLogs[_selectedIndex]
                : null;
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
            GUI.DrawTexture(iconRect, LogEntryFormatter.GetIconForLogType(log.Type));
        }

        private void DrawLogMessage(Rect entryRect, LogEntry log)
        {
            float textStartX = entryRect.x + 15 + 16 + 5;
            float rightPadding = 5f;
            float countCircleSize = 20f;
            float timeWidth = _showTime ? 60f : 0f;
            float availableTextWidth = entryRect.width - textStartX - rightPadding - countCircleSize - timeWidth;

            Rect textRect = new(textStartX, entryRect.y + 2, availableTextWidth, LOG_ENTRY_HEIGHT);

            GUI.Label(textRect, log.DisplayText, _logMessageStyle);
        }

        private void DrawLogTime(Rect entryRect, LogEntry log)
        {
            if (!_showTime)
                return;

            float timeWidth = 60f;
            Rect timeRect = new(entryRect.xMax - 5 - 20 - timeWidth, entryRect.y + 2, timeWidth - 5, LOG_ENTRY_HEIGHT);

            GUI.Label(timeRect, log.TimeText, _logTimeStyle);
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

            EnsureStackTraceCache(stackTrace);
            float totalContentHeight = _stackTraceLines.Length * STACK_TRACE_LINE_HEIGHT;

            try
            {
                _detailsScrollPosition = GUI.BeginScrollView(
                    scrollViewRect,
                    _detailsScrollPosition,
                    new Rect(0, 0, scrollViewRect.width - 20, totalContentHeight),
                    false,
                    totalContentHeight > scrollViewRect.height
                );

                DrawStackTraceLines(scrollViewRect);
            }
            finally
            {
                GUI.EndScrollView();
            }
        }

        // Splitting and parsing (which includes an AssetDatabase lookup per line) happen once per
        // selected stack trace, not once per line on every GUI event.
        private void EnsureStackTraceCache(string stackTrace)
        {
            if (stackTrace == _cachedStackTrace)
                return;

            _cachedStackTrace = stackTrace;
            _stackTraceLines = stackTrace.Split('\n');
            _stackTraceLineInfos = new StackTraceLineInfo[_stackTraceLines.Length];
            for (int i = 0; i < _stackTraceLines.Length; i++)
                _stackTraceLineInfos[i] = _stackTraceParser.ParseLine(_stackTraceLines[i]);
        }

        private void DrawStackTraceLines(Rect scrollViewRect)
        {
            // Only the visible lines are drawn.
            int first = Mathf.Max(0, Mathf.FloorToInt(_detailsScrollPosition.y / STACK_TRACE_LINE_HEIGHT));
            int last = Mathf.Min(_stackTraceLines.Length - 1,
                first + Mathf.CeilToInt(scrollViewRect.height / STACK_TRACE_LINE_HEIGHT) + 1);

            for (int i = first; i <= last; i++)
            {
                string line = _stackTraceLines[i];
                if (string.IsNullOrEmpty(line))
                    continue;

                Rect lineRect = new(0, i * STACK_TRACE_LINE_HEIGHT, scrollViewRect.width - 20, STACK_TRACE_LINE_HEIGHT);
                DrawStackTraceLine(lineRect, line, _stackTraceLineInfos[i], _stackTraceLineStyle, _stackTraceLineClickableStyle);
            }
        }

        private void DrawStackTraceLine(Rect lineRect, string line, StackTraceLineInfo lineInfo, GUIStyle normalStyle, GUIStyle clickableStyle)
        {
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
                Repaint();
                currentEvent.Use();
            }
            else if (currentEvent.type == EventType.MouseUp)
            {
                // Writing EditorPrefs on every drag event is slow, so save once at the end.
                _isResizingDetails = false;
                SavePreferences();
                currentEvent.Use();
            }
        }

        /// <summary>Cached filtered view; cheap to call several times per GUI event. Don't modify the list.</summary>
        private List<LogEntry> GetFilteredLogs()
        {
            List<LogEntry> logs = logManager.GetFilteredEntries();

            // When the view was rebuilt, indices point at different logs: find the selected log again.
            if (logManager.FilteredResetCount != _seenFilteredResetCount)
            {
                _seenFilteredResetCount = logManager.FilteredResetCount;
                _selectedIndex = _selectedEntry != null ? logs.IndexOf(_selectedEntry) : -1;
                _multiSelectedIndices.Clear();
            }

            return logs;
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
            // O(1) lookup instead of walking every loaded scene and then every object.
            UnityEngine.Object target = LogContextHelper.ResolveContextObject(logEntry.ContextGameObjectInstanceId);
            if (target == null)
                return;

            EditorGUIUtility.PingObject(target);
            Selection.activeObject = target;
        }
    }
}
#endif
