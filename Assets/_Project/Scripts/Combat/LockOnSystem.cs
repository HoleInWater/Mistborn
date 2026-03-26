using UnityEngine;

/// <summary>
/// Enemy targeting and lock-on camera. Middle click to toggle.
/// Atium burning extends lock-on range and shows target's predicted movement.
/// Tab key to cycle between targets while locked on.
/// </summary>
public class LockOnSystem : MonoBehaviour
{
    [Header("Settings")]
    public float searchRadius = 20f;
    public float atiumSearchBonus = 15f;
    public LayerMask enemyLayer;
    public KeyCode lockOnKey = KeyCode.Mouse2;
    public KeyCode cycleKey = KeyCode.Tab;

    [Header("Camera")]
    public float lockOnCameraSmoothSpeed = 5f;

    [Header("References")]
    public BasicPlayerMove playerMove;
    public Camera mainCamera;
    public Allomancer allomancer;

    private Transform currentTarget;
    public Transform CurrentTarget => currentTarget;

    private Collider[] cachedEnemies = new Collider[20];
    private int currentTargetIndex = -1;
    private float scanTimer;

    void Start()
    {
<<<<<<< HEAD
=======
        lockOnKey = Keybinds.Grapple;
        cycleKey = Keybinds.MetalWheel;
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
        if (playerMove == null) playerMove = GetComponent<BasicPlayerMove>();
        if (mainCamera == null) mainCamera = Camera.main;
        if (allomancer == null) allomancer = GetComponent<Allomancer>();
        if (enemyLayer == 0) enemyLayer = LayerMask.GetMask("Enemy");
        if (enemyLayer == 0) enemyLayer = ~0;
    }

    void Update()
    {
        if (Input.GetKeyDown(lockOnKey))
            ToggleLockOn();

        // Cycle targets with Tab
        if (Input.GetKeyDown(cycleKey) && currentTarget != null)
            CycleTarget();

        // Validate current target
        if (currentTarget != null)
        {
            if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
            {
                ClearLockOn();
                return;
            }

            float maxDist = searchRadius * 1.5f;
            if (allomancer != null && allomancer.IsMetalBurning(AllomancySkill.MetalType.Atium))
                maxDist += atiumSearchBonus;

            if (Vector3.Distance(transform.position, currentTarget.position) > maxDist)
                ClearLockOn();
        }

        // Feed lock-on target to player camera
        if (playerMove != null)
            playerMove.lockOnTarget = currentTarget;
    }

    public void ToggleLockOn()
    {
        if (currentTarget != null)
            ClearLockOn();
        else
            FindTarget();
    }

    void FindTarget()
    {
        float range = searchRadius;
        if (allomancer != null && allomancer.IsMetalBurning(AllomancySkill.MetalType.Atium))
            range += atiumSearchBonus;

        int count = Physics.OverlapSphereNonAlloc(transform.position, range, cachedEnemies, enemyLayer);
        float bestWeight = float.MaxValue;
        Transform bestTarget = null;
        int bestIndex = -1;

        for (int i = 0; i < count; i++)
        {
            Collider col = cachedEnemies[i];
            if (col == null || col.transform == transform) continue;

            // Must be alive
            IDamageable hp = col.GetComponentInParent<IDamageable>();
            if (hp != null && hp.GetCurrentHealth() <= 0) continue;

            Vector3 viewPos = mainCamera.WorldToViewportPoint(col.transform.position);
            bool onScreen = viewPos.z > 0 && viewPos.x > 0 && viewPos.x < 1 && viewPos.y > 0 && viewPos.y < 1;
            if (!onScreen) continue;

            float screenDist = Vector2.Distance(new Vector2(viewPos.x, viewPos.y), new Vector2(0.5f, 0.5f));
            float worldDist = Vector3.Distance(transform.position, col.transform.position);
            float weight = screenDist * 10f + worldDist;

            if (weight < bestWeight)
            {
                bestWeight = weight;
                bestTarget = col.transform;
                bestIndex = i;
            }
        }

        if (bestTarget != null)
        {
            currentTarget = bestTarget;
            currentTargetIndex = bestIndex;
            SoundManager.Instance?.PlayNotification();
        }
    }

    void CycleTarget()
    {
        float range = searchRadius;
        if (allomancer != null && allomancer.IsMetalBurning(AllomancySkill.MetalType.Atium))
            range += atiumSearchBonus;

        int count = Physics.OverlapSphereNonAlloc(transform.position, range, cachedEnemies, enemyLayer);
        if (count <= 1) return;

        // Find next valid target
        for (int attempt = 0; attempt < count; attempt++)
        {
            currentTargetIndex = (currentTargetIndex + 1) % count;
            Collider col = cachedEnemies[currentTargetIndex];
            if (col != null && col.transform != transform && col.transform != currentTarget)
            {
                currentTarget = col.transform;
                SoundManager.Instance?.PlayNotification();
                return;
            }
        }
    }

    public void ClearLockOn()
    {
        currentTarget = null;
        currentTargetIndex = -1;
        if (playerMove != null) playerMove.lockOnTarget = null;
    }

    public bool IsLockedOn() => currentTarget != null;
}
