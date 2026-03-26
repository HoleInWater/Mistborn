using UnityEngine;

/// <summary>
/// Sprint controller with Pewter and Steel Feruchemy integration.
/// Pewter burning reduces stamina drain and increases sprint speed.
/// Steel Feruchemy tapping further boosts speed.
/// </summary>
public class Sprint : MonoBehaviour
{
    [Header("Speed Settings — WorldScale: 2u = 5ft")]
    public float walkSpeed = 1.8f; // ~4.5 ft/s
    public float sprintSpeed = 6f; // ~15 ft/s run

    [Header("Pewter Enhancement")]
    [Tooltip("Sprint speed bonus when burning Pewter (S = S_base × (1 + k×P), k=1)")]
    public float pewterSprintBonus = 0.5f;
    [Tooltip("Stamina drain reduction when burning Pewter")]
    public float pewterDrainReduction = 0.5f;

    [Header("Stamina Costs")]
    public float drainRate = 25f;

    [HideInInspector]
    public float currentSpeed;

    private PlayerStamina staminaSystem;
    private Allomancer allomancer;
    private Feruchemist feruchemist;
    private bool isSprinting = false;

    void Start()
    {
        staminaSystem = GetComponent<PlayerStamina>();
        allomancer = GetComponent<Allomancer>();
        feruchemist = GetComponent<Feruchemist>();
        currentSpeed = walkSpeed;
    }

    void Update()
    {
<<<<<<< HEAD
        bool isTryingToSprint = Input.GetKey(KeyCode.LeftShift);
=======
        bool isTryingToSprint = Input.GetKey(Keybinds.Sprint);
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
        bool hasStamina = staminaSystem != null && staminaSystem.currentStamina > 0.5f;

        if (isTryingToSprint && hasStamina)
        {
            isSprinting = true;
            float speed = sprintSpeed;
            float drain = drainRate;

            // Pewter burning: faster sprint, less stamina drain
            if (allomancer != null && allomancer.IsMetalBurning(AllomancySkill.MetalType.Pewter))
            {
                float flare = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
                float P = Mathf.Clamp01(flare / 2.5f);
                speed *= (1f + pewterSprintBonus * P);
                drain *= (1f - pewterDrainReduction * P);
            }

            // Steel Feruchemy tapping: speed boost from stored speed
            if (feruchemist != null)
            {
                float speedMod = feruchemist.GetAttributeModifier(FeruchemicalAttribute.Speed);
                if (speedMod > 1f)
                    speed *= speedMod;
            }

            // Skill tree bonus
            if (AllomanticSkillTree.Instance != null)
            {
                float moveBonus = AllomanticSkillTree.Instance.GetSkillValue("Move_Speed1")
                                + AllomanticSkillTree.Instance.GetSkillValue("Move_Speed2");
                speed *= (1f + moveBonus);
            }

            currentSpeed = speed;
            if (staminaSystem != null)
                staminaSystem.DrainStamina(drain);
        }
        else
        {
            isSprinting = false;
            currentSpeed = walkSpeed;
        }
    }

    public bool IsSprinting() => isSprinting;
    public float GetSprintMultiplier() => isSprinting ? currentSpeed / walkSpeed : 1f;
}
