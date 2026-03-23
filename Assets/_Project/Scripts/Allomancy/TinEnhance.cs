using UnityEngine;

public class TinEnhance : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public MetalReserve metalReserve;

    [Header("FOV Settings")]
    public float focusedFOV = 45f;
    public float transitionSpeed = 8f;

    [Header("Burn Settings")]
    public float metalCostPerSecond = 5f;

    private bool isBurning = false;
    private float originalFOV;

    void Start()
    {
        if (playerCamera != null)
            originalFOV = playerCamera.fieldOfView;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && metalReserve.currentMetal > 0f)
            StartBurning();

        if (Input.GetKey(KeyCode.E) && isBurning)
        {
            EnhanceSenses();
            DrainMetal();
        }
        else
        {
            RestoreSenses();
        }

        if (Input.GetKeyUp(KeyCode.E))
            StopBurning();
    }

    void StartBurning() => isBurning = true;

    void StopBurning() => isBurning = false;

    void EnhanceSenses()
    {
        if (playerCamera != null)
            playerCamera.fieldOfView = Mathf.Lerp(
                playerCamera.fieldOfView,
                focusedFOV,
                transitionSpeed * Time.deltaTime);
    }

    void RestoreSenses()
    {
        if (playerCamera != null)
            playerCamera.fieldOfView = Mathf.Lerp(
                playerCamera.fieldOfView,
                originalFOV,
                transitionSpeed * Time.deltaTime);
    }

    void DrainMetal()
    {
        metalReserve.Drain(metalCostPerSecond * Time.deltaTime);

        if (metalReserve.currentMetal <= 0f)
            StopBurning();
    }
}
