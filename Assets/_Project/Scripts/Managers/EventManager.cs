using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Global event system for broadcasting game events between decoupled systems.
/// </summary>
public class EventManager : MonoBehaviour
{
    private static EventManager _instance;
    public static EventManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<EventManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("EventManager");
                    _instance = go.AddComponent<EventManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private Dictionary<string, List<System.Delegate>> listeners = new Dictionary<string, List<System.Delegate>>();

    public static void RegisterEvent(string eventName, System.Action callback)
    {
        if (!Instance.listeners.ContainsKey(eventName))
            Instance.listeners[eventName] = new List<System.Delegate>();
        Instance.listeners[eventName].Add(callback);
    }

    public static void RegisterEvent<T>(string eventName, System.Action<T> callback)
    {
        if (!Instance.listeners.ContainsKey(eventName))
            Instance.listeners[eventName] = new List<System.Delegate>();
        Instance.listeners[eventName].Add(callback);
    }

    public static void UnregisterEvent(string eventName, System.Action callback)
    {
        if (Instance.listeners.ContainsKey(eventName))
            Instance.listeners[eventName].Remove(callback);
    }

    public static void TriggerEvent(string eventName, Dictionary<string, object> data = null)
    {
        if (!Instance.listeners.ContainsKey(eventName)) return;
        foreach (System.Delegate d in Instance.listeners[eventName])
        {
            try { ((System.Action)d)?.Invoke(); }
            catch (System.Exception e) { Debug.LogError($"[EVENT] Error in {eventName}: {e.Message}"); }
        }
    }

    public static void TriggerEvent<T>(string eventName, T param)
    {
        if (!Instance.listeners.ContainsKey(eventName)) return;
        foreach (System.Delegate d in Instance.listeners[eventName])
        {
            try { ((System.Action<T>)d)?.Invoke(param); }
            catch (System.Exception e) { Debug.LogError($"[EVENT] Error in {eventName}: {e.Message}"); }
        }
    }
}
