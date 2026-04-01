using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Mistborn → Setup Combat Animator
/// Adds combat parameters and placeholder states to the player's Animator Controller.
/// Run once after importing the project, then replace placeholder states with real clips.
/// </summary>
public class CombatAnimatorSetup : EditorWindow
{
    private AnimatorController _controller;

    [MenuItem("Mistborn/Setup Combat Animator")]
    public static void Open()
    {
        var win = GetWindow<CombatAnimatorSetup>("Combat Animator Setup");
        win.minSize = new Vector2(380, 200);

        // Auto-find the player controller
        string[] guids = AssetDatabase.FindAssets("PlayerController t:AnimatorController");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            win._controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        }
    }

    void OnGUI()
    {
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Combat Animator Setup", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Adds the following to the Animator Controller:\n" +
            "Parameters: Attack (Trigger), HeavyAttack (Trigger), Parry (Trigger), IsBlocking (Bool)\n" +
            "States:     Attack, HeavyAttack, Parry, Block (placeholder — swap in real clips after)",
            MessageType.Info);

        EditorGUILayout.Space(6);
        _controller = (AnimatorController)EditorGUILayout.ObjectField(
            "Animator Controller", _controller, typeof(AnimatorController), false);

        EditorGUILayout.Space(8);
        GUI.enabled = _controller != null;
        if (GUILayout.Button("Apply Combat Setup", GUILayout.Height(32)))
            Apply();
        GUI.enabled = true;

        if (_controller == null)
            EditorGUILayout.HelpBox("Assign the player's Animator Controller above.", MessageType.Warning);
    }

    void Apply()
    {
        Undo.RecordObject(_controller, "Combat Animator Setup");

        EnsureParameter(_controller, "Attack",       AnimatorControllerParameterType.Trigger);
        EnsureParameter(_controller, "HeavyAttack",  AnimatorControllerParameterType.Trigger);
        EnsureParameter(_controller, "Parry",        AnimatorControllerParameterType.Trigger);
        EnsureParameter(_controller, "IsBlocking",   AnimatorControllerParameterType.Bool);

        AnimatorStateMachine sm = _controller.layers[0].stateMachine;

        AnimatorState attackState      = EnsureState(sm, "Attack");
        AnimatorState heavyAttackState = EnsureState(sm, "HeavyAttack");
        AnimatorState parryState       = EnsureState(sm, "Parry");
        AnimatorState blockState       = EnsureState(sm, "Block");

        // Transitions from Any State
        EnsureAnyStateTransition(sm, attackState,      "Attack",      isTrigger: true);
        EnsureAnyStateTransition(sm, heavyAttackState, "HeavyAttack", isTrigger: true);
        EnsureAnyStateTransition(sm, parryState,       "Parry",       isTrigger: true);
        EnsureAnyStateBlockTransition(sm, blockState);

        // Exit transitions back to entry (let the clip finish, then return)
        EnsureExitTransition(attackState,      0.9f);
        EnsureExitTransition(heavyAttackState, 0.9f);
        EnsureExitTransition(parryState,       0.9f);

        EditorUtility.SetDirty(_controller);
        AssetDatabase.SaveAssets();

        Debug.Log("[CombatAnimatorSetup] Done — parameters and states added to " + _controller.name +
                  ". Swap placeholder states with real animation clips in the Animator window.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static void EnsureParameter(AnimatorController ctrl, string name, AnimatorControllerParameterType type)
    {
        foreach (var p in ctrl.parameters)
            if (p.name == name) return;
        ctrl.AddParameter(name, type);
    }

    static AnimatorState EnsureState(AnimatorStateMachine sm, string name)
    {
        foreach (var s in sm.states)
            if (s.state.name == name) return s.state;

        AnimatorState state = sm.AddState(name);
        // Position states in a readable column in the Animator window
        int idx = sm.states.Length - 1;
        sm.states[idx] = new ChildAnimatorState
        {
            state    = state,
            position = new Vector3(400f, 100f + idx * 70f, 0f)
        };
        return state;
    }

    static void EnsureAnyStateTransition(AnimatorStateMachine sm, AnimatorState dest,
                                         string paramName, bool isTrigger)
    {
        foreach (var t in sm.anyStateTransitions)
            if (t.destinationState == dest) return;

        AnimatorStateTransition trans = sm.AddAnyStateTransition(dest);
        trans.hasExitTime = false;
        trans.duration    = 0.1f;
        if (isTrigger)
            trans.AddCondition(AnimatorConditionMode.If, 0, paramName);
    }

    static void EnsureAnyStateBlockTransition(AnimatorStateMachine sm, AnimatorState dest)
    {
        foreach (var t in sm.anyStateTransitions)
            if (t.destinationState == dest) return;

        AnimatorStateTransition trans = sm.AddAnyStateTransition(dest);
        trans.hasExitTime = false;
        trans.duration    = 0.1f;
        trans.AddCondition(AnimatorConditionMode.If, 0, "IsBlocking");
    }

    static void EnsureExitTransition(AnimatorState state, float exitTime)
    {
        if (state.transitions.Length > 0) return;
        AnimatorStateTransition t = state.AddExitTransition();
        t.hasExitTime = true;
        t.exitTime    = exitTime;
        t.duration    = 0.1f;
    }
}
