// PlayerComponentAttribute.cs
//
// Add [PlayerComponent] to any MonoBehaviour to include it in the
// Auto Setup Player tool. No shared file ever needs to be edited.
//
// Usage:
//   [PlayerComponent]                           // basic — added to player
//   [PlayerComponent("Combat", order: 20)]      // grouped + ordered in the window
//   [PlayerComponent(optional: true)]           // shown but unchecked by default
//
// That's it. The setup window scans every MonoBehaviour in the project
// for this attribute automatically.

using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class PlayerComponentAttribute : Attribute
{
    /// <summary>Group label shown in the setup window (e.g. "Combat", "Allomancy").</summary>
    public string Group { get; }

    /// <summary>Lower numbers run first within the same group.</summary>
    public int Order { get; }

    /// <summary>If true the checkbox starts unchecked — component is available but not default.</summary>
    public bool Optional { get; }

    public PlayerComponentAttribute(string group = "General", int order = 50, bool optional = false)
    {
        Group    = group;
        Order    = order;
        Optional = optional;
    }
}
