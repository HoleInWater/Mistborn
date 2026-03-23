using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SteelPush : MonoBehaviour
{
    [Header("Settings")]
    public float referenceMass = 80f;
    public float referenceDistance = 3f;
    public float minDistance = 1f;
    public float maxRange = 30f;
    public float metalCostPerSecond = 2f;
    public float pushCooldown = 0.2f;
    
    [Header("Allomancy Physics (Lore-Accurate)")]
    public float allomanticStrength = 50f;
    public float maxCoinVelocity = 20f;
    [Range(1f, 2f)]
    public float distanceExponent = 1f;
    [Range(0f, 1f)]
    public float velocityDamping = 0.5f;
    
    [Header("Flaring")]
    [Range(1.5f, 4f)]
    public float maxFlareMultiplier = 2.5f;
    [Range(1f, 5f)]
    public float flaringMetalCostMultiplier = 3f;
    
    [Header("Steel Bubble")]
    [Tooltip("Enable steel bubble (defensive push field)")]
    public bool enableSteelBubble = true;
    public KeyCode steelBubbleKey = KeyCode.F;
    public float steelBubbleRadius = 2.5f;
    public float steelBubbleForce = 50f;
    public float steelBubbleCooldown = 0.5f;
    public float steelBubbleMetalCostMultiplier = 1.5f;
    
    [Header("References")]
    public Camera playerCamera;
    public LayerMask metalLayer;
    public Allomancer allomancer;
    public Rigidbody playerRigidbody;
    public Transform chestTransform;
    
    [Header("Visual Effects")]
    public GameObject pushEffectPrefab;
    public float shakeMagnitude = 0.1f;
    public float shakeDuration = 0.1f;
    public float shakeForceThreshold = 100f;
    
    [Header("Prediction")]
    public bool enablePushPrediction = true;
    public Color predictionColor = Color.red;
    public int predictionPoints = 20;
    public float predictionTimeStep = 0.1f;
    
    [Header("Debug")]
    public bool debugPushOperations = false;
    
    private bool isBurning = false;
    private bool isFlaring = false;
    private bool eKeyWasPressed = false;
    private bool bubbleAppliedThisPress = false;
    private Coroutine vignetteCoroutine;
    private float cooldownTimer = 0f;
    private float steelBubbleCooldownTimer = 0f;
    
    private RaycastHit currentTargetHit;
    private AllomanticTarget currentTarget;
    private Rigidbody currentTargetRigidbody;
    private bool hasCurrentTarget = false;
    
    private float cooldown = 0f;
    private Coroutine pushTintCoroutine;
    private Color currentPushTint = Color.clear;
    
    private LineRenderer predictionLine;
    private bool isPredictionActive = false;
    
    void Start()
    {
        if (playerRigidbody == null)
            playerRigidbody = GetComponentInParent<Rigidbody>();
        
        if (playerCamera == null)
            playerCamera = Camera.main;
        
        if (allomancer == null)
            allomancer = GetComponentInParent<Allomancer>();
        
        if (metalLayer.value == 0)
            metalLayer = LayerMask.GetMask("Metal");
        
        if (chestTransform == null)
            chestTransform = playerRigidbody != null ? playerRigidbody.transform : transform;
        
        CreatePredictionLine();
    }
    
    void CreatePredictionLine()
    {
        GameObject lineObj = new GameObject("PushPredictionLine");
        predictionLine = lineObj.AddComponent<LineRenderer>();
        
        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
            predictionLine.material = new Material(shader);
        else
            predictionLine.material = new Material(Shader.Find("Unlit/Color"));
        
        predictionLine.startColor = predictionColor;
        predictionLine.endColor = predictionColor;
        predictionLine.startWidth = 0.03f;
        predictionLine.endWidth = 0.01f;
        predictionLine.positionCount = predictionPoints;
        predictionLine.useWorldSpace = true;
        predictionLine.gameObject.SetActive(false);
    }
    
    void Update()
    {
        if (allomancer != null && !allomancer.canBurnMetal)
        {
            if (isBurning) StopBurning();
            return;
        }
        
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
        if (steelBubbleCooldownTimer > 0f) steelBubbleCooldownTimer -= Time.deltaTime;
        
        UpdateTargetedMetal();
        
        bool eKeyDown = Input.GetKeyDown(KeyCode.E);
        bool eKeyUp = Input.GetKeyUp(KeyCode.E);
        
        if (eKeyDown && !eKeyWasPressed && cooldownTimer <= 0f)
        {
            if (isFlaring)
            {
                eKeyWasPressed = true;
                if (!isBurning) StartBurning();
                PushMetals();
                DrainMetal(flaringMetalCostMultiplier);
                StartFlaringVignette();
            }
        }
        
        if (eKeyUp)
        {
            eKeyWasPressed = false;
            StopBurning();
        }
        
        if (enableSteelBubble && Input.GetKeyDown(steelBubbleKey))
        {
            if (isFlaring && steelBubbleCooldownTimer <= 0f)
            {
                if (!isBurning) StartBurning();
                if (!bubbleAppliedThisPress)
                {
                    PushMetalsInBubble();
                    DrainMetal(steelBubbleMetalCostMultiplier);
                    steelBubbleCooldownTimer = steelBubbleCooldown;
                    bubbleAppliedThisPress = true;
                }
            }
        }
        
        UpdatePrediction();
    }
    
    void StartBurning()
    {
        if (isBurning) return;
        isBurning = true;
        if (allomancer != null)
            allomancer.StartBurning(AllomancySkill.MetalType.Steel);
    }
    
    void StopBurning()
    {
        if (!isBurning) return;
        isBurning = false;
        cooldownTimer = pushCooldown;
        if (allomancer != null)
            allomancer.StopBurning();
    }
    
    void UpdateTargetedMetal()
    {
        hasCurrentTarget = false;
        currentTarget = null;
        currentTargetRigidbody = null;
        
        if (playerCamera == null) return;
        
        float closestDist = maxRange;
        var allTargets = FindObjectsOfType<AllomanticTarget>();
        Collider[] colliders = Physics.OverlapSphere(playerCamera.transform.position, maxRange, metalLayer);
        
        foreach (var metal in allTargets)
        {
            if (metal == null) continue;
            Rigidbody rb = metal.GetComponent<Rigidbody>();
            if (rb == null || rb == playerRigidbody) continue;
            if (!metal.canBePushed) continue;
            
            float dist = Vector3.Distance(rb.position, playerCamera.transform.position);
            if (dist < closestDist && dist > 0.1f)
            {
                closestDist = dist;
                currentTargetRigidbody = rb;
                currentTarget = metal;
                hasCurrentTarget = true;
            }
        }
        
        foreach (Collider col in colliders)
        {
            if (col == null) continue;
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb == null || rb == playerRigidbody) continue;
            
            float dist = Vector3.Distance(rb.position, playerCamera.transform.position);
            if (dist < closestDist && dist > 0.1f && dist <= maxRange)
            {
                closestDist = dist;
                currentTargetRigidbody = rb;
                currentTarget = col.GetComponent<AllomanticTarget>();
                hasCurrentTarget = true;
            }
        }
    }
    
    void UpdatePrediction()
    {
        bool shouldShowPrediction = enablePushPrediction && hasCurrentTarget && currentTarget != null && currentTarget.canBePushed;
        
        if (shouldShowPrediction)
            DrawPredictionLine();
        else if (isPredictionActive)
        {
            predictionLine.gameObject.SetActive(false);
            isPredictionActive = false;
        }
    }
    
    void DrawPredictionLine()
    {
        if (predictionLine == null || currentTargetRigidbody == null) return;
        
        float distance = Vector3.Distance(playerRigidbody.position, currentTargetRigidbody.position);
        Vector3 pushDirection = (currentTargetRigidbody.position - playerRigidbody.position).normalized;
        
        float weightFactor = playerRigidbody.mass / referenceMass;
        float strength = allomanticStrength * weightFactor;
        if (isFlaring) strength *= maxFlareMultiplier;
        
        float distanceFactor = 1f - (distance / maxRange);
        distanceFactor = Mathf.Clamp01(distanceFactor);
        
        float force = strength * distanceFactor;
        float initialSpeed = Mathf.Min(force * 0.1f, maxCoinVelocity);
        Vector3 initialVelocity = pushDirection * initialSpeed;
        
        Vector3[] points = new Vector3[predictionPoints];
        Vector3 startPos = currentTargetRigidbody.position;
        Vector3 velocity = initialVelocity;
        float timeStep = predictionTimeStep;
        
        for (int i = 0; i < predictionPoints; i++)
        {
            points[i] = startPos;
            startPos += velocity * timeStep;
            velocity += Physics.gravity * timeStep;
        }
        
        Color lineColor = isFlaring ? Color.yellow : predictionColor;
        predictionLine.startColor = lineColor;
        predictionLine.endColor = lineColor;
        predictionLine.positionCount = predictionPoints;
        predictionLine.SetPositions(points);
        predictionLine.gameObject.SetActive(true);
        isPredictionActive = true;
    }
    
    void PushMetals()
    {
        if (playerRigidbody == null) return;
        if (!hasCurrentTarget || currentTargetRigidbody == null) return;
        
        Vector3 pushOrigin = playerRigidbody.position;
        Rigidbody targetRigidbody = currentTargetRigidbody;
        AllomanticTarget target = currentTarget;
        
        if (targetRigidbody == playerRigidbody) return;
        if (target != null && !target.canBePushed) return;
        
        float distance = Vector3.Distance(pushOrigin, targetRigidbody.position);
        Vector3 directionToTarget = targetRigidbody.position - pushOrigin;
        bool isAnchored = (target != null && target.isAnchored) || targetRigidbody.isKinematic;
        
        float weightFactor = playerRigidbody.mass / referenceMass;
        float strength = allomanticStrength * weightFactor;
        if (isFlaring) strength *= maxFlareMultiplier;
        
        float distanceFactor = 1f - (distance / maxRange);
        distanceFactor = Mathf.Clamp01(distanceFactor);
        
        float force = strength * distanceFactor;
        
        if (isAnchored)
        {
            playerRigidbody.AddForce(-directionToTarget.normalized * force);
        }
        else if (force > 1f)
        {
            targetRigidbody.AddForce(directionToTarget.normalized * force, ForceMode.Impulse);
        }
        
        if (force > shakeForceThreshold)
        {
            ShakeCamera(shakeMagnitude);
            TriggerPushTint(force);
        }
    }
    
    void PushMetalsInBubble()
    {
        if (playerRigidbody == null) return;
        
        Collider[] colliders = Physics.OverlapSphere(playerRigidbody.position, steelBubbleRadius, metalLayer);
        
        foreach (Collider collider in colliders)
        {
            Rigidbody targetRigidbody = collider.attachedRigidbody;
            if (targetRigidbody == null || targetRigidbody == playerRigidbody) continue;
            
            AllomanticTarget target = collider.GetComponent<AllomanticTarget>();
            if (target != null && !target.canBePushed) continue;
            
            float force = steelBubbleForce;
            Vector3 direction = (targetRigidbody.position - playerRigidbody.position).normalized;
            targetRigidbody.AddForce(direction * force, ForceMode.Impulse);
        }
    }
    
    void DrainMetal(float multiplier = 1f)
    {
        if (allomancer == null) return;
        float drainAmount = metalCostPerSecond * Time.deltaTime * multiplier;
        if (isFlaring) drainAmount *= 3f;
        float actionDrain = metalCostPerSecond * 0.5f * multiplier;
        allomancer.DrainMetal(AllomancySkill.MetalType.Steel, drainAmount + actionDrain);
    }
    
    void ShakeCamera(float magnitude)
    {
        if (playerCamera == null || magnitude <= 0f) return;
        StartCoroutine(ShakeCoroutine(magnitude));
    }
    
    IEnumerator ShakeCoroutine(float magnitude)
    {
        Vector3 originalPos = playerCamera.transform.localPosition;
        float elapsed = 0f;
        
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            playerCamera.transform.localPosition = originalPos + new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        playerCamera.transform.localPosition = originalPos;
    }
    
    void TriggerPushTint(float force)
    {
        if (pushTintCoroutine != null) StopCoroutine(pushTintCoroutine);
        pushTintCoroutine = StartCoroutine(PushTintCoroutine(force));
    }
    
    IEnumerator PushTintCoroutine(float force)
    {
        Color tintColor = new Color(1f, 0.5f, 0f, 0.2f);
        float elapsed = 0f;
        while (elapsed < 0.2f)
        {
            float alpha = Mathf.Lerp(tintColor.a, 0f, elapsed / 0.2f);
            currentPushTint = new Color(tintColor.r, tintColor.g, tintColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        currentPushTint = Color.clear;
        pushTintCoroutine = null;
    }
    
    void StartFlaringVignette()
    {
        if (vignetteCoroutine != null) StopCoroutine(vignetteCoroutine);
        vignetteCoroutine = StartCoroutine(FlareVignetteRoutine());
    }
    
    IEnumerator FlareVignetteRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        vignetteCoroutine = null;
    }
    
    void OnGUI()
    {
        if (currentPushTint.a > 0.01f)
        {
            GUI.color = currentPushTint;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
    
    void OnDestroy()
    {
        if (predictionLine != null)
            Destroy(predictionLine.gameObject);
    }
}
