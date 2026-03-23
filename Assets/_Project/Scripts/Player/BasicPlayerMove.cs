// NOTE: Line 41 contains Debug.LogError which should be removed for production
// NOTE: Consider adding [RequireComponent(typeof(Rigidbody))] attribute for physics
void HandleMovement()
{
    float x = Input.GetAxis("Horizontal");
    float z = Input.GetAxis("Vertical");

    float currentActiveSpeed = moveSpeed;
    bool isMoving = (Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f);
    bool isTryingToSprint = Input.GetKey(KeyCode.LeftShift) && isMoving;
    bool hasStamina = staminaSystem != null && staminaSystem.currentStamina > 1f;

    if (isTryingToSprint && hasStamina)
    {
        currentActiveSpeed = sprintSpeed;
        staminaSystem.DrainStamina(drainRate);
    }

    Vector3 forward = cameraPivot.forward;
    Vector3 right = cameraPivot.right;
    forward.y = 0;
    right.y = 0;
    forward.Normalize();
    right.Normalize();

    Vector3 moveDirection = (forward * z + right * x).normalized;

    // Store for use in FixedUpdate
    _moveDirection = moveDirection;
    _currentActiveSpeed = currentActiveSpeed;

    // Rotation is fine to keep here
    if (moveDirection.magnitude >= 0.1f)
    {
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}

// Add these two fields at the top of the class with your other private fields:
private Vector3 _moveDirection;
private float _currentActiveSpeed;

void FixedUpdate()
{
    // Apply horizontal movement via Rigidbody, preserving vertical velocity (gravity/jump)
    Vector3 horizontalVelocity = _moveDirection * _currentActiveSpeed;
    rb.velocity = new Vector3(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.z);

    // Gravity scaling
    if (rb.velocity.y < 0)
    {
        rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
    }
    else if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
    {
        rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
    }
}
