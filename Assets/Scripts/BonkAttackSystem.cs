using UnityEngine;

public class BonkAttackSystem : MonoBehaviour
{
    [Header("Attack Settings")]
    [Tooltip("Maximum time a charge attack can be held")]
    [Range(0.5f, 5f)]
    public float maxChargeTime = 2f;

    [Tooltip("Cooldown between attacks")]
    [Range(0.1f, 1f)]
    public float attackCooldown = 0.25f;

    [Tooltip("Speed multiplier for basic attack animation")]
    [Range(1f, 3f)]
    public float basicAttackSpeedMultiplier = 1.5f;

    [Tooltip("Speed multiplier for charge attack animation")]
    [Range(0.5f, 1f)]
    public float chargeAttackSpeedMultiplier = 0.7f;

    [Tooltip("Whether player can move during attacks")]
    public bool allowMovementDuringAttack = false;

    [Header("Aim Settings")]
    [Tooltip("Enable mouse-based aim control during attacks")]
    public bool useMouseAim = false;

    [Tooltip("How fast the mallet rotates to face mouse position")]
    [Range(360f, 1440f)]
    public float attackRotationSpeed = 720f;

    [Tooltip("Layers that can be aimed at")]
    public LayerMask aimLayer;

    [Header("Detection Settings")]
    [Tooltip("Size of the attack hitbox")]
    public Vector3 attackBoxSize = new Vector3(1f, 1f, 1f);

    [Tooltip("Transform where attack detection originates")]
    public Transform detectionPoint;

    [Header("References")]
    public PlayerMovement playerMovement;
    public MalletRotation malletRotation;
    public CameraFollow cameraFollow;
    public LayerMask bonkableLayer;

    [Header("Debug Visualization")]
    [SerializeField] private Color normalGizmoColor = Color.blue;
    [SerializeField] private Color activeGizmoColor = Color.red;

    // private variables
    [SerializeField] private float holdThreshold = 0.2f;
    private const float MIN_CHARGE_VALUE = 0.1f;
    private Transform parentMallet;
    private Camera mainCamera;
    private Animator animator;
    private CursorLockMode previousCursorState;
    private Color currentGizmoColor;
    private float currentChargeTime = 0f;
    private float cooldownTimer = 0f;
    private float holdTime = 0f;
    private float finalCharge = 0f;
    private bool isCharging = false;
    private bool isAttacking = false;
    private bool canStartNewAttack = true;
    private bool wasAutoReleased = false;
    private bool isChargedAttack = false;

    // animation hash ids for performance
    private readonly int IsAttackingHash = Animator.StringToHash("IsAttacking");
    private readonly int IsChargingHash = Animator.StringToHash("IsCharging");
    private readonly int TriggerWindupHash = Animator.StringToHash("TriggerWindup");
    private readonly int TriggerSlamHash = Animator.StringToHash("TriggerSlam");

    private void Start()
    {
        mainCamera = Camera.main;
        animator = GetComponent<Animator>();
        currentGizmoColor = normalGizmoColor;

        if (parentMallet == null)
            parentMallet = transform.parent;

    }

    private void Update()
    {
        HandleCooldown();

        HandleAiming();

        HandleInput();
    }

    private void BonkInputLogic()
    {
        // start attack
        if (Input.GetMouseButtonDown(0) && canStartNewAttack && !isAttacking)
        {
            StartAttack();
        }
        // start charge if hold time is greater than threshold
        else if (Input.GetMouseButton(0) && isAttacking && !isCharging && holdTime > holdThreshold)
        {
            StartCharge();
        }
        // handle charge
        else if (Input.GetMouseButton(0) && isCharging)
        {
            HandleCharge();
        }
        // release charge
        else if (Input.GetMouseButtonUp(0) && isCharging && !wasAutoReleased)
        {
            ReleaseCharge();
            holdTime = 0f;
        }
    }

    private void UpdateMalletAim()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, aimLayer))
        {
            Vector3 targetPoint = hit.point;
            Vector3 direction = (targetPoint - transform.position).normalized;
            direction.y = 0; // Keep rotation on Y axis only

            // Create the target rotation 
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

            // Smoothly rotate towards target
            parentMallet.rotation = Quaternion.RotateTowards(
                parentMallet.rotation,
                targetRotation,
                attackRotationSpeed * Time.deltaTime
            );
        }
    }

    private void StartAttack()
    {
        isAttacking = true;
        canStartNewAttack = false;
        wasAutoReleased = false;

        // instantly reset mallet to upright position
        ResetMalletRotation();

        if (useMouseAim)
        {
            UnlockCursor();
            if (cameraFollow != null)
                cameraFollow.enabled = false;
        }

        animator.SetFloat("AttackSpeed", basicAttackSpeedMultiplier);

        // trigger windup animation
        animator.SetBool(IsAttackingHash, true);
        animator.SetTrigger(TriggerWindupHash);

        HandleMovementRestrictions();
    }

    private void StartCharge()
    {
        isCharging = true;
        isChargedAttack = true;
        animator.SetBool(IsChargingHash, true);
        animator.SetFloat("AttackSpeed", chargeAttackSpeedMultiplier);

    }

    private void HandleCharge()
    {
        if (currentChargeTime < maxChargeTime)
        {
            currentChargeTime += Time.deltaTime;
            animator.SetBool(IsChargingHash, true);

            // if we just hit max charge, release
            if (currentChargeTime >= maxChargeTime)
            {
                wasAutoReleased = true;
                ReleaseCharge();
            }
        }
    }

    private void ReleaseCharge()
    {
        if (isCharging)
        {
            isCharging = false;
            animator.SetBool(IsChargingHash, false);
            animator.SetTrigger(TriggerSlamHash);

            finalCharge = CalculateChargeValue();

            currentChargeTime = 0f;
        }
    }

    private void HandleCooldown()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        if (cooldownTimer <= 0 && !canStartNewAttack)
        {
            canStartNewAttack = true;
        }
    }

    private void HandleAiming()
    {
        if (useMouseAim && isAttacking)
        {
            UpdateMalletAim();
        }
    }

    private void HandleInput()
    {
        // left mouse button input handling
        if (Input.GetMouseButton(0))
        {
            holdTime += Time.deltaTime;
        }
        else
        {
            holdTime = 0f;
        }

        BonkInputLogic();
    }

    private float CalculateChargeValue()
    {
        float normalizedValue = Mathf.Clamp01(currentChargeTime / maxChargeTime);
        return Mathf.Lerp(MIN_CHARGE_VALUE, 1f, normalizedValue);
    }

    private void HandleMovementRestrictions()
    {
        if (!allowMovementDuringAttack)
        {
            playerMovement.enabled = false;
        }
        malletRotation.enabled = false;
    }

    private void ResetMalletRotation()
    {
        if (parentMallet != null)
        {
            Transform orientation = playerMovement.transform;
            parentMallet.forward = orientation.forward;
            transform.localRotation = Quaternion.identity;
        }
    }

    private void DetectBonkableObjects()
    {
        // safety check for detection point reference
        if (detectionPoint == null)
        {
            Debug.LogError("Detection point transform is not assigned!");
            return;
        }
        currentGizmoColor = activeGizmoColor;

        float impactValue = isChargedAttack ? finalCharge : 0f; 

        // detect objects using the transform's position and rotation
        Collider[] hitColliders = Physics.OverlapBox(
            detectionPoint.position,
            attackBoxSize / 2f,
            detectionPoint.rotation,
            bonkableLayer
        );

        // log detected objects for debugging
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent<IBonkable>(out var bonkable))
            {
                bonkable.OnBonked(impactValue, transform.position);
                Debug.Log($"Detected bonkable object: {hitCollider.name}");
            }
        }

        Debug.Log($"Impact Value sent to IBonkable: {impactValue:F2}");

        // reset gizmo color after a short delay
        Invoke(nameof(ResetGizmoColor), 0.1f);
    }

    private void ResetGizmoColor()
    {
        currentGizmoColor = normalGizmoColor;
    }

    public void OnSlam()
    {
        DetectBonkableObjects();

        if (useMouseAim)
        {
            RestoreCursorState();
            if (cameraFollow != null)
                cameraFollow.enabled = true;
        }
        
    }

    public void OnAttackComplete()
    {
        isAttacking = false;
        animator.SetBool(IsAttackingHash, false);
        animator.ResetTrigger(TriggerSlamHash);
        animator.ResetTrigger(TriggerWindupHash);
        animator.SetFloat("AttackSpeed", 1f);
        cooldownTimer = attackCooldown;
        isChargedAttack = false;

        // re-enable movement
        playerMovement.enabled = true;
        malletRotation.enabled = true;
    }

    private void UnlockCursor()
    {
        previousCursorState = Cursor.lockState;
        Cursor.lockState = CursorLockMode.Confined; // Keep cursor in window but visible
        Cursor.visible = true;
    }

    private void RestoreCursorState()
    {
        Cursor.lockState = previousCursorState;
        Cursor.visible = false;
    }

    private void OnDrawGizmos()
    {
        // allow gizmo to show even in edit mode for easier setup
        if (detectionPoint == null) return;

        Gizmos.color = Application.isPlaying ? currentGizmoColor : normalGizmoColor;
        Gizmos.matrix = Matrix4x4.TRS(detectionPoint.position, detectionPoint.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, attackBoxSize);
    }
}