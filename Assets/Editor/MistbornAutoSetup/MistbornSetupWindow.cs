// MistbornSetupWindow.cs
//
// Main EditorWindow. Opens from Mistborn > Auto Setup Player.
// Discovers all IPlayerSetupModule implementations via reflection — no manual
// registration, zero conflicts when adding new modules.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using MistbornEditor;

public class MistbornSetupWindow : EditorWindow
{
    // ── Module discovery ──────────────────────────────────────────────────────

    private List<IPlayerSetupModule> _modules;
    private Dictionary<IPlayerSetupModule, bool> _enabled; // per-module toggle

    private void DiscoverModules()
    {
        _modules = System.AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
            .Where(t => typeof(IPlayerSetupModule).IsAssignableFrom(t)
                     && !t.IsInterface && !t.IsAbstract)
            .Select(t => (IPlayerSetupModule)System.Activator.CreateInstance(t))
            .OrderBy(m => m.ModuleName)
            .ToList();

        _enabled = _modules.ToDictionary(m => m, _ => true);
    }

    // ── State ─────────────────────────────────────────────────────────────────

    private GameObject     _player;
    private SetupLog       _lastLog;
    private Vector2        _logScroll;
    private bool           _ran = false;

    // ── Menu entry ────────────────────────────────────────────────────────────

    [MenuItem("Mistborn/Auto Setup Player")]
    public static void Open()
    {
        var win = GetWindow<MistbornSetupWindow>("Mistborn — Auto Setup Player");
        win.minSize = new Vector2(420, 540);
        win.Show();
    }

    // ── GUI ───────────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        DiscoverModules();
        TryAutoFindPlayer();
    }

    private void OnGUI()
    {
        DrawHeader();
        GUILayout.Space(6);
        DrawPlayerPicker();
        GUILayout.Space(6);
        DrawModuleList();
        GUILayout.Space(6);
        DrawRunButton();

        if (_ran && _lastLog != null)
        {
            GUILayout.Space(8);
            DrawLog();
        }
    }

    // ── Sections ──────────────────────────────────────────────────────────────

    void DrawHeader()
    {
        var headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize  = 14,
            alignment = TextAnchor.MiddleCenter,
        };
        GUILayout.Space(8);
        GUILayout.Label("⚙  Mistborn Auto Setup Player", headerStyle);

        var subStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            wordWrap  = true,
        };
        GUILayout.Label(
            "Adds all required components and wires up references on the selected player GameObject.",
            subStyle);

        EditorGUILayout.HelpBox(
            "Safe to run multiple times — components that already exist are skipped, "
          + "never duplicated.",
            MessageType.Info);
    }

    void DrawPlayerPicker()
    {
        EditorGUILayout.LabelField("Player GameObject", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            _player = (GameObject)EditorGUILayout.ObjectField(_player, typeof(GameObject), true);
            if (GUILayout.Button("Find by Tag", GUILayout.Width(110)))
                TryAutoFindPlayer();
        }

        if (_player == null)
        {
            EditorGUILayout.HelpBox("Assign a player GameObject or click 'Find by Tag'.",
                MessageType.Warning);
        }
    }

    void DrawModuleList()
    {
        EditorGUILayout.LabelField("Setup Modules", EditorStyles.boldLabel);

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("All",  EditorStyles.miniButtonLeft,  GUILayout.Width(50)))
                    foreach (var k in _modules) _enabled[k] = true;
                if (GUILayout.Button("None", EditorStyles.miniButtonRight, GUILayout.Width(50)))
                    foreach (var k in _modules) _enabled[k] = false;
                GUILayout.FlexibleSpace();
                GUILayout.Label($"{_modules.Count} modules found", EditorStyles.miniLabel);
            }

            GUILayout.Space(3);
            foreach (var m in _modules)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    _enabled[m] = EditorGUILayout.ToggleLeft(
                        new GUIContent(m.ModuleName, m.Description), _enabled[m]);
                }
            }
        }
    }

    void DrawRunButton()
    {
        GUI.enabled = _player != null;

        var btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 13, fixedHeight = 34 };
        Color prev = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.35f, 0.65f, 1f);

        if (GUILayout.Button("▶  Run Setup", btnStyle))
            RunSetup();

        GUI.backgroundColor = prev;
        GUI.enabled = true;
    }

    void DrawLog()
    {
        EditorGUILayout.LabelField("Results", EditorStyles.boldLabel);

        int added   = _lastLog.Entries.Count(e => e.isNew);
        int skipped = _lastLog.Entries.Count(e => !e.isNew);

        using (new EditorGUILayout.HorizontalScope())
        {
            var addStyle  = new GUIStyle(EditorStyles.boldLabel);
            addStyle.normal.textColor = new Color(0.2f, 0.8f, 0.2f);
            var skipStyle = new GUIStyle(EditorStyles.miniLabel);

            GUILayout.Label($"✓ {added} added", addStyle);
            GUILayout.Space(12);
            GUILayout.Label($"{skipped} already present", skipStyle);
        }

        _logScroll = EditorGUILayout.BeginScrollView(_logScroll,
            GUILayout.Height(Mathf.Min(_lastLog.Entries.Count * 17 + 10, 220)));

        string lastModule = "";
        foreach (var (module, message, isNew) in _lastLog.Entries)
        {
            if (module != lastModule)
            {
                GUILayout.Space(4);
                EditorGUILayout.LabelField($"── {module}", EditorStyles.miniBoldLabel);
                lastModule = module;
            }

            var style = new GUIStyle(EditorStyles.miniLabel);
            if (isNew) style.normal.textColor = new Color(0.3f, 0.9f, 0.3f);
            else if (message.StartsWith("⚠")) style.normal.textColor = new Color(1f, 0.7f, 0.2f);

            EditorGUILayout.LabelField(message, style);
        }

        EditorGUILayout.EndScrollView();
    }

    // ── Logic ─────────────────────────────────────────────────────────────────

    void TryAutoFindPlayer()
    {
        if (_player != null) return;
        var go = GameObject.FindGameObjectWithTag("Player");
        if (go != null) _player = go;
    }

    void RunSetup()
    {
        if (_player == null) return;

        Undo.RegisterFullObjectHierarchyUndo(_player, "Mistborn Auto Setup Player");

        _lastLog = new SetupLog();
        _ran     = true;

        foreach (var module in _modules)
        {
            if (!_enabled[module]) continue;
            _lastLog.BeginModule(module.ModuleName);
            try
            {
                module.Setup(_player, _lastLog);
            }
            catch (System.Exception ex)
            {
                _lastLog.Warn($"Exception in {module.ModuleName}: {ex.Message}");
            }
        }

        EditorUtility.SetDirty(_player);
        Debug.Log($"[MistbornAutoSetup] Finished — {_lastLog.Entries.Count(e => e.isNew)} components added.");
        Repaint();
    }
}
