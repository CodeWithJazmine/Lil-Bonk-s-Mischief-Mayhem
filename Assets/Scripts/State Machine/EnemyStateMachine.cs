using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent (typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class EnemyStateMachine : MonoBehaviour, IBonkable
{
    public bool bonk, superBonk;
    public delegate void EnemyBonked();
    public event EnemyBonked OnEnemyBonked;

    ChaosMeter ChaosMeter;

    // References & Control variables
    private Transform player;
    private Rigidbody playerRB;
    private Rigidbody rb;
    private PlayerDetector playerDetector;
    [SerializeField] private State currentState;
    [SerializeField] private bool useWaypoints = false;
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float normalSpeed = 2f;
    [SerializeField] private float fleeSpeed = 4f;
    [SerializeField] private float minWaitTime = 2f; // Minimum wait time
    [SerializeField] private float maxWaitTime = 5f; // Maximum wait time
    [SerializeField] private float fleeRadius = 3f;
    [SerializeField] private float bonkResistance = 0;
    [SerializeField] private float bonkResistGain = 0.1f;
    [SerializeField] private float bonkResistMax = 0.75f;
    [SerializeField] private float bonkedBaseForce = 100f; // Base amount of force for velocity change when player bonks enemy
    [SerializeField] private float bonkedTime = 2f;
    [SerializeField] private int bonkPoints = 1;
    [SerializeField] private float getupTime = 1.5f;
    [SerializeField] private float currentBonkedTime = 0;
    [SerializeField] private float currentGetupTime;
    [SerializeField] private bool isAggressive = false;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField, Tooltip("Needs to be very close for melee attackers.")] private float attackRadius = 3f;
    [SerializeField] private float attackTime = 3f;
    [SerializeField] private float pushForce = 1000f; // Default value might be way too high.
    [SerializeField] private GameObject bulletPrefab = null;
    [SerializeField] private Transform shotPoint = null;
    [SerializeField] private ParticleSystem muzzleFlash = null;
    public Transform[] waypoints = null;
    [SerializeField] private int currentWaypoint = 0;

    NavMeshAgent agent;
    Animator animator;

    // Animations
    private static readonly int IdleHash = Animator.StringToHash("Idle");
    private static readonly int WalkHash = Animator.StringToHash("Walk");
    private static readonly int RunHash = Animator.StringToHash("Run");
    private static readonly int BonkedHash = Animator.StringToHash("Bonked");
    private static readonly int SuperBonkedHash = Animator.StringToHash("SuperBonked");
    private static readonly int GetupHash = Animator.StringToHash("Getup");
    private static readonly int BoundHash = Animator.StringToHash("Bound");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    [SerializeField] private float crossFade = 0.1f;
    private bool isWalking = false;
    private bool isRunning = false;
    private bool isIdle = false;
    private bool isChasing = false;
    private bool isAttacking = false;

    private Vector3 wanderTarget;
    private float currentWaitTime;
    private float currentAttackTime;
    private float cachedStoppingDistance;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        playerDetector = GetComponentInChildren<PlayerDetector>();
    }

    void Start()
    {
        ChaosMeter = GameManager.Instance.transform.GetComponent<ChaosMeter>();
        player = GameObject.FindWithTag("Player").transform;
        playerRB = player.GetComponent<Rigidbody>();
        cachedStoppingDistance = agent.stoppingDistance;

        // Initialize with the Wander state
        currentState = State.Wander;
        SetNewWanderTarget();
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Waypoint:
                Waypoint();
                break;
            case State.Wander:
                Wander();
                break;
            case State.Wait:
                Wait();
                break;
            case State.Flee:
                Flee();
                break;
            case State.Getup:
                break;
            case State.Chase:
                Chase();
                break;
            case State.Attack:
                Attack();
                break;

        }

        // Transition logic
        if (PlayerInSight()
            && currentState != State.Bonked
            && currentState != State.Getup
            && currentState != State.Attack)
        {
            if(!isAggressive)
                currentState = State.Flee;
            else
                currentState = State.Chase;
        }

        else if (!useWaypoints
            &&currentState == State.Flee 
            && !PlayerInSight() )
        {
            currentState = State.Wander;
            SetNewWanderTarget();
        }

        else if (useWaypoints
            &&currentState == State.Flee 
            && !PlayerInSight() )
        {
            currentState = State.Waypoint;
            SetNextWaypointTarget();
        }

        else if(currentState == State.Bonked)
        {
            currentBonkedTime += Time.deltaTime;
            if(currentBonkedTime >= bonkedTime)
            {
                currentGetupTime = 0;
                animator.CrossFade(GetupHash, crossFade);
                currentState = State.Getup;
            }
        }

        else if(currentState == State.Getup)
        {
            currentGetupTime += Time.deltaTime;
            if (currentGetupTime >= getupTime)
                currentState = State.Wander;

            agent.isStopped = false;
            agent.ResetPath();
        }

        else if(currentState == State.Attack)
        {
            if(isAttacking)
            {
                currentAttackTime += Time.deltaTime;
                if (currentAttackTime >= attackTime)
                {
                    isAttacking = false;
                    currentState = State.Chase;
                }
            }
        }
    }

    private void Wander()
    {
        agent.speed = normalSpeed;
        agent.stoppingDistance = cachedStoppingDistance;
        agent.updateRotation = true;
        agent.updatePosition = true;

        // Check if the enemy has reached the wander target
        if (HasReachedDestination())
        {
            currentState = State.Wait;
            currentWaitTime = Random.Range(minWaitTime, maxWaitTime); // Set a random wait time
        }

        if(!isWalking)
            animator.CrossFade(WalkHash, crossFade);

        isWalking = true;
        isRunning = false;
        isIdle = false;
    }

    private void Waypoint()
    {
        agent.speed = normalSpeed;
        agent.stoppingDistance = cachedStoppingDistance;
        agent.updateRotation = true;
        agent.updatePosition = true;

        // Check if the enemy has reached the wander target
        if (HasReachedDestination())
        {
            currentWaypoint++;
            if(currentWaypoint >= waypoints.Length) currentWaypoint = 0;
            currentState = State.Wait;
            currentWaitTime = Random.Range(minWaitTime, maxWaitTime); // Set a random wait time
        }

        if (!isWalking)
            animator.CrossFade(WalkHash, crossFade);

        isWalking = true;
        isRunning = false;
        isIdle = false;
    }

    private void Chase()
    {
        if(!isChasing)
        {
            agent.speed = chaseSpeed;
            agent.stoppingDistance = attackRadius;
            agent.updateRotation = true;
            agent.SetDestination(player.position);
            isChasing = true;
        }

        else
        {
            var distanceToPlayer = Vector3.Distance(transform.position, player.position);
            Debug.Log("Distance to Player: " + distanceToPlayer.ToString());
            // If the player is outside of attack range and we've reached our targeted position
            if (HasReachedDestination()
                && distanceToPlayer > attackRadius)
            {
                agent.SetDestination(player.position);
                return;
            }

            else if (distanceToPlayer <= attackRadius)
            {
                agent.SetDestination(transform.position);
                currentState = State.Attack;
                return;
            }

            else if(!HasReachedDestination()
                && !agent.pathPending)
            {
                agent.SetDestination(player.position);
            }
        }

        if (!isRunning)
            animator.CrossFade(RunHash, crossFade);

        isWalking = false;
        isRunning = true;
        isIdle = false;
    }

    void Attack()
    {
        if(!isAttacking)
        {            
            animator.CrossFade(AttackHash, crossFade);
            currentAttackTime = 0;
            isAttacking = true;
        }

        agent.updateRotation = false;
        TurnTowardsPosition(player.position);

        isWalking = false;
        isRunning = false;
        isIdle = false;
        isChasing = false;
    }

    bool PlayerInSight()
    {
        return playerDetector.playerIsDetected;
    }

    bool HasReachedDestination()
    {
        return !agent.pathPending
            && agent.remainingDistance <= agent.stoppingDistance
            && (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);
    }

    private void Wait()
    {
        // Countdown the wait timer
        currentWaitTime -= Time.deltaTime;

        // Transition back to Wander when the timer runs out
        if (currentWaitTime <= 0)
        {
            if(useWaypoints)
            {
                currentState = State.Waypoint;
                SetNextWaypointTarget();
            }

            else
            {
                currentState = State.Wander;
                SetNewWanderTarget();
            }
        }

        if (!isIdle)
            animator.CrossFade(IdleHash, crossFade);

        isWalking = false;
        isRunning = false;
        isIdle = true;
    }

    private void SetNewWanderTarget()
    {
        // Pick a random point within a 10-unit radius
        var randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, 1);
        wanderTarget = hit.position;
        agent.SetDestination(wanderTarget);
    }

    private void SetNextWaypointTarget()
    {
        agent.SetDestination(waypoints[currentWaypoint].position);
    }

    private void Flee()
    {
        agent.speed = fleeSpeed;
        agent.stoppingDistance = cachedStoppingDistance;
        agent.updateRotation = true;
        agent.updatePosition = true;

        if (!agent.hasPath)
        {
            Vector3 fleePosition = GetFleePosition();
            if (fleePosition != Vector3.zero)
            {
                agent.SetDestination(fleePosition);
            }
        }

        if (!isRunning)
            animator.CrossFade(RunHash, crossFade);

        isWalking = false;
        isRunning = true;
        isIdle = false;
    }

    private Vector3 GetFleePosition()
    {
        // Calculate direction away from the player
        Vector3 fleeDirection = (transform.position - player.position).normalized;

        // Propose a flee target by adding fleeDirection scaled by fleeRadius
        Vector3 proposedFleePosition = transform.position + fleeDirection * fleeRadius;

        // Check if the proposed position is on the NavMesh
        if (NavMesh.SamplePosition(proposedFleePosition, out NavMeshHit hit, fleeRadius, NavMesh.AllAreas))
        {
            return hit.position; // Return the closest valid position on the NavMesh
        }

        return Vector3.zero; // Return zero vector if no valid position is found
    }

    public void OnBonked(float value, Vector3 position)
    {
        GameManager.Instance.PlayBonkSound();
        // If already bonked, or getting up play bound animation
        if (currentState == State.Bonked || currentState == State.Getup)
        {
            // DO NOT ADD POINTS FOR THIS OR PLAYERS WILL ABUSE TF OUT OF IT LOL
            agent.isStopped = true;
            agent.Warp(transform.position);
            var forceDir = (transform.position - position).normalized;
            rb.AddForce(forceDir * bonkedBaseForce * value, ForceMode.VelocityChange);
            currentBonkedTime = 0;
            currentGetupTime = 0;
            animator.Play(BoundHash, -1, 0);
            OnEnemyBonked?.Invoke();
            isWalking = false;
            isRunning = false;
            isIdle = false;
        }

        // If value = 1 enemy is super bonked
        else if(value >= 1f)
        {
            agent.isStopped = true;
            agent.Warp(transform.position);
            agent.updatePosition = false;
            agent.updateRotation = false;
            currentState = State.Bonked;
            animator.CrossFade(SuperBonkedHash, crossFade);
            currentBonkedTime = 0;
            // Turn towards position
            TurnTowardsPosition(position);
            var forceDir = (transform.position - position).normalized;
            rb.AddForce(forceDir * bonkedBaseForce * value, ForceMode.VelocityChange);
            isWalking = false;
            isRunning = false;
            isIdle = false;
            OnEnemyBonked?.Invoke();
            ChaosMeter.AddChaos(bonkPoints * 2);
            return;
        }    

        // If value >= bonkResistance, enemy is bonked.
        // Increase bonk resistance
        else if(value >= bonkResistance)
        {
            agent.isStopped = true;
            agent.updatePosition = false;
            agent.updateRotation = false;
            agent.Warp(transform.position);
            currentState = State.Bonked;
            animator.CrossFade(BonkedHash, crossFade);
            currentBonkedTime = 0;
            // Turn towards position
            TurnTowardsPosition(position);
            var forceDir = (transform.position - position).normalized;
            rb.AddForce(forceDir * bonkedBaseForce * value, ForceMode.VelocityChange);
            bonkResistance += bonkResistGain;
            if (bonkResistance >= bonkResistMax) bonkResistance = bonkResistMax;
            isWalking = false;
            isRunning = false;
            isIdle = false;
            OnEnemyBonked?.Invoke();
            ChaosMeter.AddChaos(bonkPoints);
        }
    }

    private void OnPush()
    {
        // Animation event for melee enemy
        // Pushes the player if they are within range
        var distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if(distanceToPlayer <= attackRadius)
        {
            playerRB.AddForce(transform.forward * pushForce, ForceMode.VelocityChange);
        }
    }

    private void OnShoot()
    {
        muzzleFlash?.Play();
        var bullet = Instantiate(bulletPrefab, shotPoint.position, shotPoint.rotation);
        bullet.GetComponent<Bullet>().bulletForce = pushForce;
    }

    private void TurnTowardsPosition(Vector3 position)
    {
        // Calculate the direction to the target
        Vector3 direction = (position - transform.position).normalized;

        // Zero out the Y component to restrict rotation to the Y-axis
        direction.y = 0;

        // Avoid processing if direction is zero (to prevent errors)
        if (direction != Vector3.zero)
        {
            // Calculate the rotation to look at the target
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Apply the rotation to the object, modifying only the Y-axis
            transform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
        }
    }
}
