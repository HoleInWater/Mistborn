#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Mistborn > Auto Setup Player
/// Scans every MonoBehaviour for [PlayerComponent] and shows a grouped,
/// checkbox-driven window. Click "Apply to Player" to add checked components.
/// </summary>
public class PlayerAutoSetupWizard : EditorWindow
{
    // ── Data ─────────────────────────────────────────────────────────────────

    private class ComponentEntry
    {
        public Type   type;
        public string group;
        public int    order;
        public bool   optional;
        public bool   enabled;
    }

    private Dictionary<string, List<ComponentEntry>> _groups;
    private Vector2 _scroll;
    private GameObject _target;

    // ── Open ─────────────────────────────────────────────────────────────────

    [MenuItem("Mistborn/Auto Setup Player")]
    public static void Open()
    {
        var w = GetWindow<PlayerAutoSetupWizard>("Auto Setup Player");
        w.minSize = new Vector2(340, 480);
        w.Scan();
    }

    // ── Scan all assemblies for [PlayerComponent] ─────────────────────────

    void Scan()
    {
        _groups = new Dictionary<string, List<ComponentEntry>>();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsSubclassOf(typeof(MonoBehaviour))) continue;
                var attr = type.GetCustomAttribute<PlayerComponentAttribute>();
                if (attr == null) continue;

                var entry = new ComponentEntry
                {
                    type     = type,
                    group    = attr.Group,
                    order    = attr.Order,
                    optional = attr.Optional,
                    enabled  = !attr.Optional   // default: checked unless optional
                };

                if (!_groups.ContainsKey(attr.Group))
                    _groups[attr.Group] = new List<ComponentEntry>();

                _groups[attr.Group].Add(entry);
            }
        }

        // Sort within each group by order
        foreach (var list in _groups.Values)
            list.Sort((a, b) => a.order.CompareTo(b.order));
    }

    // ── GUI ───────────────────────────────────────────────────────────────

    void OnGUI()
    {
        if (_groups == null) Scan();

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Auto Setup Player", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Select a Player GameObject, check the components you want, then click Apply.",
            MessageType.Info);
        EditorGUILayout.Space(4);

        _target = (GameObject)EditorGUILayout.ObjectField("Player GameObject", _target, typeof(GameObject), true);

        // Try to auto-find if empty
        if (_target == null)
            _target = GameObject.FindWithTag("Player");

        EditorGUILayout.Space(6);

        // ── Buttons ───────────────────────────────────────────────────────
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Select All"))
                SetAll(true);
            if (GUILayout.Button("Deselect All"))
                SetAll(false);
            if (GUILayout.Button("Refresh"))
                Scan();
        }

        EditorGUILayout.Space(4);

        // ── Scrollable component list ─────────────────────────────────────
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        foreach (var groupName in _groups.Keys.OrderBy(k => k))
        {
            EditorGUILayout.LabelField(groupName, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            foreach (var entry in _groups[groupName])
            {
                bool alreadyAttached = _target != null && _target.GetComponent(entry.type) != null;

                using (new EditorGUI.DisabledScope(alreadyAttached))
                {
                    string label = entry.type.Name;
                    if (alreadyAttached) label += "  ✓";
                    else if (entry.optional) label += "  (optional)";

                    entry.enabled = EditorGUILayout.ToggleLeft(label, entry.enabled || alreadyAttached);
                }
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space(4);
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(6);

        // ── Apply ─────────────────────────────────────────────────────────
        using (new EditorGUI.DisabledScope(_target == null))
        {
            if (GUILayout.Button("Apply to Player", GUILayout.Height(32)))
                Apply();
        }

        if (_target == null)
            EditorGUILayout.HelpBox("No GameObject tagged \"Player\" found in scene.", MessageType.Warning);
    }

    // ── Apply ──────────────────────────────────────────────────────────────

    void Apply()
    {
        if (_target == null) return;

        Undo.RecordObject(_target, "Auto Setup Player Components");

        int added = 0;
        foreach (var list in _groups.Values)
        {
            foreach (var entry in list)
            {
                if (!entry.enabled) continue;
                if (_target.GetComponent(entry.type) != null) continue;

                _target.AddComponent(entry.type);
                added++;
            }
        }

        EditorUtility.SetDirty(_target);
        Debug.Log($"[Auto Setup Player] Added {added} component(s) to {_target.name}.");
    }

    void SetAll(bool value)
    {
        foreach (var list in _groups.Values)
            foreach (var entry in list)
                entry.enabled = value;
    }
}
#endif
