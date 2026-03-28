// IPlayerSetupModule.cs
//
// Every setup module implements this interface.
// The window discovers all implementations via reflection — adding a new
// module means creating ONE new file, touching ZERO existing files.
// That is the entire conflict-prevention strategy.

using UnityEngine;
using System.Collections.Generic;

namespace MistbornEditor
{
    /// <summary>
    /// Implement this interface in its own file (e.g. PlayerSetup_YourSystem.cs)
    /// to add your system to the Auto Setup Player tool.
    /// The window will find it automatically — no registration needed.
    /// </summary>
    public interface IPlayerSetupModule
    {
        /// <summary>Short display name shown in the window (e.g. "Movement").</summary>
        string ModuleName { get; }

        /// <summary>One-line description shown as a tooltip.</summary>
        string Description { get; }

        /// <summary>
        /// Perform setup on the player GameObject.
        /// Use log.Add() to report what you did. Use Util.Ensure to add components
        /// only when missing.
        /// </summary>
        void Setup(GameObject player, SetupLog log);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Shared utilities used by every module — static so there is no inheritance
    // coupling between module files.
    // ─────────────────────────────────────────────────────────────────────────

    public static class Util
    {
        /// <summary>
        /// Returns the existing component of type T, or adds and returns a new one.
        /// Logs what it did to the provided SetupLog.
        /// </summary>
        public static T Ensure<T>(GameObject go, SetupLog log) where T : Component
        {
            T existing = go.GetComponent<T>();
            if (existing != null)
            {
                log.Skip(typeof(T).Name);
                return existing;
            }
            T added = go.AddComponent<T>();
            log.Add(typeof(T).Name);
            return added;
        }

        /// <summary>
        /// Tries to find a child transform by name (case-insensitive). Returns null if not found.
        /// </summary>
        public static Transform FindChild(Transform root, string name)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                if (string.Equals(child.name, name, System.StringComparison.OrdinalIgnoreCase))
                    return child;
            return null;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Simple log collector — each module writes to this, the window displays it.
    // ─────────────────────────────────────────────────────────────────────────

    public class SetupLog
    {
        public readonly List<(string module, string message, bool isNew)> Entries = new();

        private string _currentModule = "";

        public void BeginModule(string moduleName) => _currentModule = moduleName;

        public void Add(string componentName) =>
            Entries.Add((_currentModule, $"+ Added   {componentName}", true));

        public void Skip(string componentName) =>
            Entries.Add((_currentModule, $"  Exists  {componentName}", false));

        public void Info(string message) =>
            Entries.Add((_currentModule, $"  {message}", false));

        public void Warn(string message) =>
            Entries.Add((_currentModule, $"⚠ {message}", false));
    }
}
