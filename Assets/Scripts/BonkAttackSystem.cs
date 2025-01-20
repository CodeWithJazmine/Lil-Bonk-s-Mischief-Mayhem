using UnityEngine;

public class BonkAttackSystem : MonoBehaviour
{
    [Header("Attack Settings")]
    public float maxChargeTime = 2f;
    public float attackCooldown = 0.5f;
    public bool allowMovementDuringAttack = false;

    private float holdTime = 0f;
    private float holdThreshold = 0.15f;

    [Header("References")]
    public PlayerMovement playerMovement;
    public MalletRotation malletRotation;
    public Transform parentMallet;

    private Animator animator;
    private float currentChargeTime = 0f;
    private float cooldownTimer = 0f;
    private bool isCharging = false;
    private bool isAttacking = false;
    private bool canStartNewAttack = true;
    private Quaternion originalRotation;

    // animation Hash IDs for performance
    private readonly int IsAttackingHash = Animator.StringToHash("IsAttacking");
    private readonly int IsChargingHash = Animator.StringToHash("IsCharging");
    private readonly int TriggerWindupHash = Animator.StringToHash("TriggerWindup");
    private readonly int TriggerSlamHash = Animator.StringToHash("TriggerSlam");

    private void Start()
    {
        animator = GetComponent<Animator>();

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


        // start attack
        if (Input.GetMouseButtonDown(0) && canStartNewAttack && !isAttacking)
        {
            StartAttack();
        }
        // start charge if hold time is greater than threshold
        else if (Input.GetMouseButton(0) && isAttacking && !isCharging && holdTime > holdThreshold)
        {
            isCharging = true;
            animator.SetBool(IsChargingHash, true);
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

    // called via Animation Event at the end of slam animation
    public void OnAttackComplete()
    {
        isAttacking = false;
        animator.SetBool(IsAttackingHash, false);
        cooldownTimer = attackCooldown;
    }

    // optional: add visual feedback or effects during attacks
    public void OnAnimationComplete()
    {
        // re-enable movement
        playerMovement.enabled = true;
        malletRotation.enabled = true;
    }
}