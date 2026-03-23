using UnityEngine;

public class TinEnhance : MonoBehaviour
{
    [Header("Settings")]
    public float focusedFOV = 45f;
    public float transitionSpeed = 8f;
    public float metalCostPerSecond = 2f;
    public float hearingRange = 50f;
    public float sightRange = 100f;

    [Header("References")]
    public Camera playerCamera;

    private float metalReserve = 100f;
    private bool isBurning = false;
    private float originalFOV;

    void Start()
    {
        if (playerCamera != null)
            originalFOV = playerCamera.fieldOfView;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && metalReserve > 0f)
            StartBurning();

        if (Input.GetKey(KeyCode.E) && isBurning)
            EnhanceSenses();
        else
            RestoreSenses();

        if (Input.GetKeyUp(KeyCode.E))
            StopBurning();
    }

    void StartBurning() => isBurning = true;

    void StopBurning()
    {
        isBurning = false;
    }

    void EnhanceSenses()
    {
        if (playerCamera != null)
            playerCamera.fieldOfView = Mathf.Lerp(
                playerCamera.fieldOfView, focusedFOV, transitionSpeed * Time.deltaTime);

        DrainMetal();
    }

    void RestoreSenses()
    {
        if (playerCamera != null)
            playerCamera.fieldOfView = Mathf.Lerp(
                playerCamera.fieldOfView, originalFOV, transitionSpeed * Time.deltaTime);
    }

    void DrainMetal()
    {
        metalReserve -= metalCostPerSecond * Time.deltaTime;
        if (metalReserve <= 0)
        {
            metalReserve = 0;
            StopBurning();
        }
    }

    public float GetMetalReserve() => metalReserve;
    public void RefillMetal(float amount) => metalReserve = Mathf.Min(metalReserve + amount, 100f);
}
