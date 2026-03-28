// MistbornSetupWindow.cs
//
// Opens from Mistborn > Auto Setup Player.
//
// HOW IT WORKS — zero-conflict design:
//   Scans every MonoBehaviour in the project for [PlayerComponent].
//   Adding a new component to the setup means adding ONE attribute to
//   the component's own script — a file the dev already owns.
//   This file never needs to be edited by anyone.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;

public class MistbornSetupWindow : EditorWindow
{
    // ── Discovered component list ─────────────────────────────────────────────

    private struct Entry
    {
        public Type   ComponentType;
        public string Group;
        public int    Order;
        public bool   Enabled;   // checkbox state
    }

    private List<Entry>                   _entries;
    private Dictionary<string, bool>      _groupFoldouts = new();
    private Vector2                       _scroll;

    // ── Setup state ───────────────────────────────────────────────────────────

    private GameObject _player;
    private List<(string label, bool isNew)> _log;
    private bool _ran;

    // ── Menu ──────────────────────────────────────────────────────────────────

    [MenuItem("Mistborn/Auto Setup Player")]
    public static void Open()
    {
        var w = GetWindow<MistbornSetupWindow>("Auto Setup Player");
        w.minSize = new Vector2(400, 520);
        w.Show();
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void OnEnable()
    {
        ScanComponents();
        TryFindPlayer();
    }

    void ScanComponents()
    {
        _entries = new List<Entry>();

        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            IEnumerable<Type> types;
            try { types = assembly.GetTypes(); }
            catch { continue; }

            foreach (var type in types)
            {
                if (!typeof(MonoBehaviour).IsAssignableFrom(type)) continue;
                var attr = type.GetCustomAttribute<PlayerComponentAttribute>();
                if (attr == null) continue;

                _entries.Add(new Entry
                {
                    ComponentType = type,
                    Group         = attr.Group,
                    Order         = attr.Order,
                    Enabled       = !attr.Optional,
                });
            }
        }

        _entries = _entries
            .OrderBy(e => e.Group)
            .ThenBy(e => e.Order)
            .ThenBy(e => e.ComponentType.Name)
            .ToList();
    }

    // ── GUI ───────────────────────────────────────────────────────────────────

    void OnGUI()
    {
        DrawHeader();
        GUILayout.Space(4);
        DrawPlayerPicker();
        GUILayout.Space(4);

        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        DrawComponentList();
        EditorGUILayout.EndScrollView();

        GUILayout.Space(4);
        DrawRunButton();

        if (_ran && _log != null)
        {
            GUILayout.Space(6);
            DrawLog();
        }
    }

    void DrawHeader()
    {
        GUILayout.Space(6);
        var h = new GUIStyle(EditorStyles.boldLabel) { fontSize = 13, alignment = TextAnchor.MiddleCenter };
        GUILayout.Label("Mistborn — Auto Setup Player", h);

        EditorGUILayout.HelpBox(
            "Adds [PlayerComponent]-tagged scripts to the player. " +
            "Existing components are never duplicated. Safe to run repeatedly.",
            MessageType.Info);
    }

    void DrawPlayerPicker()
    {
        EditorGUILayout.LabelField("Player", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            _player = (GameObject)EditorGUILayout.ObjectField(_player, typeof(GameObject), true);
            if (GUILayout.Button("Find by Tag", GUILayout.Width(100)))
                TryFindPlayer();
        }
        if (_player == null)
            EditorGUILayout.HelpBox("Assign a player or click Find by Tag.", MessageType.Warning);
    }

    void DrawComponentList()
    {
        if (_entries.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "No [PlayerComponent] attributes found. Add [PlayerComponent] to any " +
                "MonoBehaviour to include it here.",
                MessageType.Warning);
            return;
        }

        // Select-all / none row
        EditorGUILayout.LabelField($"Components  ({_entries.Count} found)", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("All",  EditorStyles.miniButtonLeft,  GUILayout.Width(44)))
                SetAll(true);
            if (GUILayout.Button("None", EditorStyles.miniButtonRight, GUILayout.Width(44)));
                SetAll(false);
        }
        GUILayout.Space(3);

        // Draw grouped
        string currentGroup = null;
        bool   groupEnabled = true;

        for (int i = 0; i < _entries.Count; i++)
        {
            var e = _entries[i];

            if (e.Group != currentGroup)
            {
                currentGroup = e.Group;
                if (!_groupFoldouts.ContainsKey(currentGroup))
                    _groupFoldouts[currentGroup] = true;

                groupEnabled = _groupFoldouts[currentGroup] =
                    EditorGUILayout.Foldout(_groupFoldouts[currentGroup], currentGroup, true,
                        EditorStyles.foldoutHeader);
            }

            if (!groupEnabled) continue;

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(16);
                var entry = _entries[i];
                entry.Enabled = EditorGUILayout.ToggleLeft(entry.ComponentType.Name, entry.Enabled);
                _entries[i] = entry;

                // Show "already on player" hint
                if (_player != null && _player.GetComponent(entry.ComponentType) != null)
                {
                    var s = new GUIStyle(EditorStyles.miniLabel);
                    s.normal.textColor = new Color(0.4f, 0.7f, 0.4f);
                    GUILayout.Label("✓", s, GUILayout.Width(16));
                }
            }
        }
    }

    void DrawRunButton()
    {
        GUI.enabled = _player != null;
        var prev = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.35f, 0.65f, 1f);
        var btn = new GUIStyle(GUI.skin.button) { fontSize = 12, fixedHeight = 32 };
        if (GUILayout.Button("▶  Add Components to Player", btn))
            RunSetup();
        GUI.backgroundColor = prev;
        GUI.enabled = true;
    }

    void DrawLog()
    {
        int added   = _log.Count(l => l.isNew);
        int skipped = _log.Count(l => !l.isNew);

        var addStyle = new GUIStyle(EditorStyles.boldLabel);
        addStyle.normal.textColor = new Color(0.25f, 0.85f, 0.25f);

        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label($"✓ {added} added", addStyle);
            GUILayout.Space(10);
            GUILayout.Label($"{skipped} already present", EditorStyles.miniLabel);
        }

        foreach (var (label, isNew) in _log)
        {
            var s = new GUIStyle(EditorStyles.miniLabel);
            if (isNew) s.normal.textColor = new Color(0.3f, 0.9f, 0.3f);
            EditorGUILayout.LabelField(label, s);
        }
    }

    // ── Logic ─────────────────────────────────────────────────────────────────

    void TryFindPlayer()
    {
        if (_player != null) return;
        _player = GameObject.FindGameObjectWithTag("Player");
    }

    void SetAll(bool value)
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            var e = _entries[i];
            e.Enabled = value;
            _entries[i] = e;
        }
    }

    void RunSetup()
    {
        if (_player == null) return;

        Undo.RegisterFullObjectHierarchyUndo(_player, "Mistborn Auto Setup Player");

        _log = new List<(string, bool)>();
        _ran = true;

        foreach (var entry in _entries)
        {
            if (!entry.Enabled) continue;

            if (_player.GetComponent(entry.ComponentType) != null)
            {
                _log.Add(($"  {entry.ComponentType.Name}", false));
            }
            else
            {
                _player.AddComponent(entry.ComponentType);
                _log.Add(($"+ {entry.ComponentType.Name}", true));
            }
        }

        EditorUtility.SetDirty(_player);
        Repaint();
    }
}
