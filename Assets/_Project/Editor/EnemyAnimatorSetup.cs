using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;

/// <summary>
/// Rebuilds Assets/_Project/Animations/EnemyController.controller with a full
/// locomotion + combat state machine mirroring the player controller.
///
/// Run via:  Tools → Mistborn → Rebuild Enemy Animator Controller
///
/// Parameters written (must match EnemyAnimationController.SyncLocoParams):
///   Speed    (float)  — raw m/s from NavAgent
///   Velocity (float)  — 0-1 normalised
///   IsWalking, IsRunning, IsChasing, IsPatrolling, IsFleeing  (bool)
///   IsAttacking, IsDead  (bool)
/// </summary>
public static class EnemyAnimatorSetup
{
    const string CONTROLLER_PATH = "Assets/_Project/Animations/EnemyController.controller";

    // Clip asset paths — same Mixamo clips as the player
    const string ANIM_DIR = "Assets/_Project/Animations/";
    static readonly Dictionary<string, string> ClipPaths = new Dictionary<string, string>
    {
        { "Idle",        ANIM_DIR + "Breathing Idle.anim"                      },
        { "Walk",        ANIM_DIR + "Walking (1).anim"                         },
        { "Run",         ANIM_DIR + "Running.anim"                             },
        { "Jump",        ANIM_DIR + "Jump.anim"                                },
        { "RunJump",     ANIM_DIR + "Running Jump.anim"                        },
        { "Attack",      ANIM_DIR + "Standing Melee Attack Horizontal.anim"    },
        { "HeavyAttack", ANIM_DIR + "Standing Melee Attack 360 High.anim"      },
        { "Block",       ANIM_DIR + "Blocking.anim"                            },
        { "Hit",         ANIM_DIR + "Standing Block React Large.anim"          },
    };

    [MenuItem("Tools/Mistborn/Rebuild Enemy Animator Controller")]
    public static void Rebuild()
    {
        // ── Create or overwrite controller ────────────────────────────────────
        AnimatorController ctrl = AnimatorController.CreateAnimatorControllerAtPath(CONTROLLER_PATH);

        // ── Parameters ───────────────────────────────────────────────────────
        ctrl.AddParameter("Speed",          AnimatorControllerParameterType.Float);
        ctrl.AddParameter("Velocity",       AnimatorControllerParameterType.Float);
        ctrl.AddParameter("IsWalking",      AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("IsRunning",      AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("IsChasing",      AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("IsPatrolling",   AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("IsFleeing",      AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("IsAttacking",    AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("IsDead",         AnimatorControllerParameterType.Bool);

        AnimatorStateMachine root = ctrl.layers[0].stateMachine;

        // ── States ────────────────────────────────────────────────────────────
        AnimatorState idle        = AddState(root, ctrl, "Idle",        "Idle",        new Vector3(-200,  0));
        AnimatorState walk        = AddState(root, ctrl, "Walk",        "Walk",        new Vector3( 100, -120));
        AnimatorState run         = AddState(root, ctrl, "Run",         "Run",         new Vector3( 400, -120));
        AnimatorState attack      = AddState(root, ctrl, "Attack",      "Attack",      new Vector3( 100,  120));
        AnimatorState heavyAttack = AddState(root, ctrl, "HeavyAttack", "HeavyAttack", new Vector3( 400,  120));
        AnimatorState block       = AddState(root, ctrl, "Block",       "Block",       new Vector3( 100,  280));
        AnimatorState hit         = AddState(root, ctrl, "Hit",         "Hit",         new Vector3( 400,  280));
        AnimatorState dead        = AddState(root, ctrl, "Dead",        null,          new Vector3(-200, -250));

        // Death has no dedicated clip — holds last pose. Assign in Inspector if one is imported.
        dead.motion = null;

        root.defaultState = idle;

        // ── Transitions ───────────────────────────────────────────────────────

        // Idle → Walk (moving but not running)
        var t = idle.AddTransition(walk);
        t.hasExitTime = false; t.duration = 0.15f;
        t.AddCondition(AnimatorConditionMode.If,    0, "IsWalking");

        // Idle → Run (chasing or running)
        t = idle.AddTransition(run);
        t.hasExitTime = false; t.duration = 0.15f;
        t.AddCondition(AnimatorConditionMode.If,    0, "IsRunning");

        // Idle → Run when chasing (chasing implies running speed)
        t = idle.AddTransition(run);
        t.hasExitTime = false; t.duration = 0.15f;
        t.AddCondition(AnimatorConditionMode.If,    0, "IsChasing");

        // Walk → Idle
        t = walk.AddTransition(idle);
        t.hasExitTime = false; t.duration = 0.15f;
        t.AddCondition(AnimatorConditionMode.IfNot, 0, "IsWalking");
        t.AddCondition(AnimatorConditionMode.IfNot, 0, "IsRunning");
        t.AddCondition(AnimatorConditionMode.IfNot, 0, "IsChasing");

        // Walk → Run
        t = walk.AddTransition(run);
        t.hasExitTime = false; t.duration = 0.1f;
        t.AddCondition(AnimatorConditionMode.If,    0, "IsRunning");

        // Walk → Run (chasing)
        t = walk.AddTransition(run);
        t.hasExitTime = false; t.duration = 0.1f;
        t.AddCondition(AnimatorConditionMode.If,    0, "IsChasing");

        // Run → Walk
        t = run.AddTransition(walk);
        t.hasExitTime = false; t.duration = 0.2f;
        t.AddCondition(AnimatorConditionMode.IfNot, 0, "IsRunning");
        t.AddCondition(AnimatorConditionMode.IfNot, 0, "IsChasing");
        t.AddCondition(AnimatorConditionMode.If,    0, "IsWalking");

        // Run → Idle
        t = run.AddTransition(idle);
        t.hasExitTime = false; t.duration = 0.2f;
        t.AddCondition(AnimatorConditionMode.IfNot, 0, "IsRunning");
        t.AddCondition(AnimatorConditionMode.IfNot, 0, "IsChasing");
        t.AddCondition(AnimatorConditionMode.IfNot, 0, "IsWalking");

        // Any State → Attack
        var anyAttack = root.AddAnyStateTransition(attack);
        anyAttack.hasExitTime = false; anyAttack.duration = 0.1f;
        anyAttack.AddCondition(AnimatorConditionMode.If,    0, "IsAttacking");
        anyAttack.canTransitionToSelf = false;

        // Attack → Idle (exit time — clip finishes)
        t = attack.AddTransition(idle);
        t.hasExitTime = true; t.exitTime = 0.85f; t.duration = 0.15f;
        t.AddCondition(AnimatorConditionMode.IfNot, 0, "IsAttacking");

        // Any State → HeavyAttack — driven by EnemyAnimationController Playable layer,
        // but keep state here so the Animator window reflects it correctly.
        // HeavyAttack → Idle
        t = heavyAttack.AddTransition(idle);
        t.hasExitTime = true; t.exitTime = 0.85f; t.duration = 0.2f;

        // Idle → Block
        t = idle.AddTransition(block);
        t.hasExitTime = false; t.duration = 0.1f;
        // Block has no bool param — driven externally by EnemyAnimationController Playables layer

        // Block → Idle (exit time)
        t = block.AddTransition(idle);
        t.hasExitTime = true; t.exitTime = 1f; t.duration = 0.15f;

        // Hit → Idle (exit time)
        t = hit.AddTransition(idle);
        t.hasExitTime = true; t.exitTime = 1f; t.duration = 0.15f;

        // Any State → Dead (highest priority — no return)
        var anyDead = root.AddAnyStateTransition(dead);
        anyDead.hasExitTime = false; anyDead.duration = 0.25f;
        anyDead.AddCondition(AnimatorConditionMode.If, 0, "IsDead");
        anyDead.canTransitionToSelf = false;

        // ── Save ──────────────────────────────────────────────────────────────
        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[EnemyAnimatorSetup] Rebuilt '{CONTROLLER_PATH}' — assign it to the enemy Animator component.");
        EditorUtility.DisplayDialog(
            "Enemy Animator Rebuilt",
            $"Controller saved to:\n{CONTROLLER_PATH}\n\n" +
            "Assign it to your enemy's Animator component in the Inspector.\n" +
            "A Death clip slot is left empty — assign one from your animation pack if available.",
            "OK");
    }

    static AnimatorState AddState(AnimatorStateMachine sm, AnimatorController ctrl,
                                  string stateName, string clipKey, Vector3 pos)
    {
        AnimatorState state = sm.AddState(stateName, pos);

        if (clipKey != null && ClipPaths.TryGetValue(clipKey, out string path))
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip != null)
                state.motion = clip;
            else
                Debug.LogWarning($"[EnemyAnimatorSetup] Clip not found at: {path}");
        }

        return state;
    }
}
