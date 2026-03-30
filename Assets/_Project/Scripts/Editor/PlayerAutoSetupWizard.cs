#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Mistborn > Auto Setup Player
/// Scans every MonoBehaviour in the project for [PlayerComponent] and shows
/// a grouped checkbox list. Drag your Player into the slot, check what you
/// want, and click Apply.
/// </summary>
public class PlayerAutoSetupWizard : EditorWindow
{
    private struct Entry
    {
        public Type   type;
        public string group;
        public int    order;
        public bool   optional;
        public bool   enabled;
    }

    private List<Entry>                   _entries  = new List<Entry>();
    private Dictionary<string, List<int>> _groupMap = new Dictionary<string, List<int>>();
    private Vector2                       _scroll;
    private GameObject                    _target;
    private string                        _statusMsg = "";

    // ── Open ─────────────────────────────────────────────────────────────────

    [MenuItem("Mistborn/Auto Setup Player")]
    public static void Open()
    {
        PlayerAutoSetupWizard w = GetWindow<PlayerAutoSetupWizard>("Auto Setup Player");
        w.minSize = new Vector2(320, 400);
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void OnEnable()
    {
        DoScan();
    }

    // ── Scan ──────────────────────────────────────────────────────────────────

    void DoScan()
    {
        _entries.Clear();
        _groupMap.Clear();
        _statusMsg = "";

        try
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = asm.GetTypes(); }
                catch { continue; }

                foreach (Type t in types)
                {
                    if (!t.IsClass || t.IsAbstract) continue;
                    if (!typeof(MonoBehaviour).IsAssignableFrom(t)) continue;

                    object[] attrs = t.GetCustomAttributes(typeof(PlayerComponentAttribute), false);
                    if (attrs.Length == 0) continue;

                    PlayerComponentAttribute attr = (PlayerComponentAttribute)attrs[0];

                    Entry e = new Entry
                    {
                        type     = t,
                        group    = attr.Group ?? "General",
                        order    = attr.Order,
                        optional = attr.Optional,
                        enabled  = !attr.Optional
                    };

                    int idx = _entries.Count;
                    _entries.Add(e);

                    if (!_groupMap.ContainsKey(e.group))
                        _groupMap[e.group] = new List<int>();
                    _groupMap[e.group].Add(idx);
                }
            }

            // Sort each group by order
            foreach (var list in _groupMap.Values)
                list.Sort((a, b) => _entries[a].order.CompareTo(_entries[b].order));

            _statusMsg = $"Found {_entries.Count} component(s) in {_groupMap.Count} group(s).";
        }
        catch (Exception ex)
        {
            _statusMsg = "Scan error: " + ex.Message;
        }

        Repaint();
    }

    // ── GUI ───────────────────────────────────────────────────────────────────

    void OnGUI()
    {
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Auto Setup Player", EditorStyles.boldLabel);
        EditorGUILayout.Space(2);

        // Target field
        GameObject prev = _target;
        _target = (GameObject)EditorGUILayout.ObjectField("Player GameObject", _target, typeof(GameObject), true);
        if (_target == null)
            _target = GameObject.FindWithTag("Player");
        if (_target != prev) Repaint();

        EditorGUILayout.Space(4);

        // Toolbar
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Select All",   GUILayout.Height(22))) SetAll(true);
        if (GUILayout.Button("Deselect All", GUILayout.Height(22))) SetAll(false);
        if (GUILayout.Button("Rescan",       GUILayout.Height(22))) DoScan();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);

        if (_entries.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "No scripts with [PlayerComponent] found. Click Rescan or check for compile errors.",
                MessageType.Warning);
        }
        else
        {
            // Component list
            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));

            List<string> sortedGroups = new List<string>(_groupMap.Keys);
            sortedGroups.Sort();

            foreach (string groupName in sortedGroups)
            {
                EditorGUILayout.LabelField(groupName, EditorStyles.boldLabel);

                foreach (int idx in _groupMap[groupName])
                {
                    Entry e = _entries[idx];

                    bool attached = _target != null && _target.GetComponent(e.type) != null;

                    string label = "  " + e.type.Name;
                    if (attached)        label += "   [already on player]";
                    else if (e.optional) label += "   (optional)";

                    if (attached)
                    {
                        // Already on the player — show a locked checked toggle
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.ToggleLeft(label, true);
                        EditorGUI.EndDisabledGroup();
                    }
                    else
                    {
                        // Not yet attached — fully interactive toggle
                        bool newVal = EditorGUILayout.ToggleLeft(label, e.enabled);
                        if (newVal != e.enabled)
                        {
                            Entry updated = e;
                            updated.enabled = newVal;
                            _entries[idx] = updated;
                            Repaint();
                        }
                    }
                }

                EditorGUILayout.Space(2);
            }

            EditorGUILayout.EndScrollView();
        }

        EditorGUILayout.Space(4);

        // Apply button — always visible, shows error if no target
        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
        if (GUILayout.Button("Apply to Player", GUILayout.Height(34)))
        {
            if (_target == null)
                EditorUtility.DisplayDialog("No Target",
                    "Drag your Player GameObject into the 'Player GameObject' field first.", "OK");
            else
                DoApply();
        }
        GUI.backgroundColor = Color.white;

        if (_target == null)
            EditorGUILayout.HelpBox("No Player GameObject assigned (also checks for 'Player' tag).", MessageType.Warning);

        EditorGUILayout.Space(2);
        EditorGUILayout.LabelField(_statusMsg, EditorStyles.miniLabel);
    }

    // ── Apply ─────────────────────────────────────────────────────────────────

    void DoApply()
    {
        Undo.RecordObject(_target, "Auto Setup Player");
        int added = 0;

        List<string> sortedGroups = new List<string>(_groupMap.Keys);
        sortedGroups.Sort();

        foreach (string g in sortedGroups)
        {
            foreach (int idx in _groupMap[g])
            {
                Entry e = _entries[idx];
                if (!e.enabled) continue;
                if (_target.GetComponent(e.type) != null) continue;
                _target.AddComponent(e.type);
                added++;
            }
        }

        EditorUtility.SetDirty(_target);
        _statusMsg = $"Done — added {added} component(s) to {_target.name}.";
        Repaint();
    }

    void SetAll(bool value)
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            Entry e = _entries[i];
            e.enabled = value;
            _entries[i] = e;
        }
        Repaint();
    }
}
#endif
