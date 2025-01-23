using UnityEngine;

public class BonkAttackSystem : MonoBehaviour
{
    [Header("Attack Settings")]
    public float maxChargeTime = 2f;
    public float attackCooldown = 0.5f;
    public bool allowMovementDuringAttack = false;

    [Header("Detection Settings")]
    public Vector3 attackBoxSize = new Vector3(1f, 1f, 1f);
    public Transform detectionPoint;
    public Color normalGizmoColor = Color.blue;
    public Color activeGizmoColor = Color.red;
    private Color currentGizmoColor;

    private float holdTime = 0f;
    private float holdThreshold = 0.15f;
    private const float MIN_CHARGE_VALUE = 0.1f; 

    [Header("References")]
    public PlayerMovement playerMovement;
    public MalletRotation malletRotation;
    public Transform parentMallet;
    public LayerMask bonkableLayer;

    private Animator animator;
    private float currentChargeTime = 0f;
    private float cooldownTimer = 0f;
    private bool isCharging = false;
    private bool isAttacking = false;
    private bool canStartNewAttack = true;
    private bool isDetecting = false;

    // animation Hash IDs for performance
    private readonly int IsAttackingHash = Animator.StringToHash("IsAttacking");
    private readonly int IsChargingHash = Animator.StringToHash("IsCharging");
    private readonly int TriggerWindupHash = Animator.StringToHash("TriggerWindup");
    private readonly int TriggerSlamHash = Animator.StringToHash("TriggerSlam");

    private void Start()
    {
        animator = GetComponent<Animator>();
        currentGizmoColor = normalGizmoColor;

        if (parentMallet == null)
            parentMallet = transform.parent;
    }

    private void Update()
    {
        // handle cooldown
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        // reset can attack flag when cooldown is done
        if (cooldownTimer <= 0 && !canStartNewAttack)
        {
            canStartNewAttack = true;
        }

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
        else if (Input.GetMouseButtonUp(0) && isCharging)
        {
            ReleaseCharge();
            holdTime = 0f;
        }
    }

    private void StartAttack()
    {
        isAttacking = true;
        canStartNewAttack = false;
        currentChargeTime = 0f;

        // instantly reset mallet to upright position
        ResetMalletRotation();

        // trigger windup animation
        animator.SetBool(IsAttackingHash, true);
        animator.SetTrigger(TriggerWindupHash);

        HandleMovementRestrictions();
    }

    private void StartCharge()
    {
        isCharging = true;
        animator.SetBool(IsChargingHash, true);
    }

    private void HandleCharge()
    {
        currentChargeTime += Time.deltaTime;
        animator.SetBool(IsChargingHash, true);

        // auto release if max charge time reached
        if (currentChargeTime >= maxChargeTime)
        {
            ReleaseCharge();
        }
    }

    private void ReleaseCharge()
    {
        isCharging = false;
        animator.SetBool(IsChargingHash, false);
        animator.SetTrigger(TriggerSlamHash);

        float finalCharge = CalculateChargeValue();
        Debug.Log($"Released charge with value: {finalCharge:F2}");
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
        isDetecting = true;
        currentGizmoColor = activeGizmoColor;

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
                Debug.Log($"Detected bonkable object: {hitCollider.name}");
            }
        }

        // reset gizmo color after a short delay
        Invoke(nameof(ResetGizmoColor), 0.1f);
    }

    private void ResetGizmoColor()
    {
        isDetecting = false;
        currentGizmoColor = normalGizmoColor;
    }

    public void OnSlam()
    {
        DetectBonkableObjects();
    }

    // called via Animation Event at the end of attack sequence
    public void OnAttackComplete()
    {
        isAttacking = false;
        animator.SetBool(IsAttackingHash, false);
        animator.ResetTrigger(TriggerSlamHash);  
        animator.ResetTrigger(TriggerWindupHash);
        cooldownTimer = attackCooldown;

        // re-enable movement
        playerMovement.enabled = true;
        malletRotation.enabled = true;
    }

    // visualize the attack detection box
    private void OnDrawGizmos()
    {
        // allow gizmo to show even in edit mode for easier setup
        if (detectionPoint == null) return;

        Gizmos.color = Application.isPlaying ? currentGizmoColor : normalGizmoColor;
        Gizmos.matrix = Matrix4x4.TRS(detectionPoint.position, detectionPoint.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, attackBoxSize);
    }
}