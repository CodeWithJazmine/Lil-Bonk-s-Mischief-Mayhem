using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStateMachine : MonoBehaviour, IBonkable
{
    public bool bonk, superBonk;
    public delegate void EnemyBonked();
    public event EnemyBonked OnEnemyBonked;

    // States
    private enum State
    {
        Wander,
        Wait,
        Flee,
        Bonked,
        Getup
    }

    private State currentState;

    // References
    [SerializeField] private Transform player;
    [SerializeField] private float fleeDistance = 5f; // Distance at which the enemy flees
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float wanderSpeed = 2f;
    [SerializeField] private float fleeSpeed = 4f;
    [SerializeField] private float minWaitTime = 2f; // Minimum wait time
    [SerializeField] private float maxWaitTime = 5f; // Maximum wait time
    [SerializeField] private float fleeRadius = 3f;
    [SerializeField] private float bonkResistance = 0;
    [SerializeField] private float bonkResistGain = 0.1f;
    [SerializeField] private float bonkResistMax = 0.75f;
    [SerializeField] private float bonkedTime = 2f;
    [SerializeField] private float getupTime = 1.5f;
    [SerializeField] float currentBonkedTime = 0;
    [SerializeField] float currentGetupTime;

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

    [SerializeField] float crossFade = 0.1f;
    bool isWalking = false;
    bool isRunning = false;
    bool isIdle = false;    

    private Vector3 wanderTarget;
    private float waitTimer;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        // Initialize with the Wander state
        currentState = State.Wander;
        SetNewWanderTarget();
    }

    void Update()
    {
        if(bonk)
        {
            OnBonked(Random.Range(0f, 1f));
            bonk = false;    
        }

        if (superBonk)
        {
            OnBonked(1f);
            superBonk = false;
        }

        switch (currentState)
        {
            case State.Wander:
                Wander();
                break;
            case State.Wait:
                Wait();
                break;
            case State.Flee:
                Flee();
                break;
            case State.Bonked:
                break;
            case State.Getup:
                break;
        }

        // Transition logic
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer < fleeDistance 
            && currentState != State.Bonked
            && currentState != State.Getup)
        {
            currentState = State.Flee;
        }

        else if (currentState == State.Flee && distanceToPlayer >= fleeDistance)
        {
            currentState = State.Wander;
            SetNewWanderTarget();
        }

        else if(currentState == State.Bonked)
        {
            currentBonkedTime += Time.deltaTime;
            if(currentBonkedTime >= bonkedTime)
            {
                currentState = State.Getup;
                animator.CrossFade(GetupHash, crossFade);
                currentGetupTime = 0;
            }
        }

        else if(currentState == State.Getup)
        {
            currentGetupTime += Time.deltaTime;
            if (currentGetupTime >= getupTime)
                currentState = State.Wander;

            agent.isStopped = false;
        }
    }

    private void Wander()
    {
        agent.speed = wanderSpeed;

        // Check if the enemy has reached the wander target
        if (HasReachedDestination())
        {
            currentState = State.Wait;
            waitTimer = Random.Range(minWaitTime, maxWaitTime); // Set a random wait time
        }

        if(!isWalking)
            animator.CrossFade(WalkHash, crossFade);

        isWalking = true;
        isRunning = false;
        isIdle = false;
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
        waitTimer -= Time.deltaTime;

        // Transition back to Wander when the timer runs out
        if (waitTimer <= 0)
        {
            currentState = State.Wander;
            SetNewWanderTarget();
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

    private void Flee()
    {
        if (!agent.hasPath)
        {
            Vector3 fleePosition = GetFleePosition();
            if (fleePosition != Vector3.zero)
            {
                agent.speed = fleeSpeed;
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

    public void OnBonked(float value)
    {
        // If already bonked, or getting up play bound animation
        if(currentState == State.Bonked || currentState == State.Getup)
        {
            agent.isStopped = true;
            agent.Warp(transform.position);
            currentBonkedTime = 0;
            currentGetupTime = 0;
            animator.Play(BoundHash, -1, 0);
            OnEnemyBonked?.Invoke();
            isWalking = false;
            isRunning = false;
            isIdle = false;
            return; // To prevent other ifs from firing
        }

        // If value = 1 enemy is super bonked
        if(value >= 1f)
        {
            Debug.Log("SUPER BONKED");
            agent.isStopped = true;
            agent.Warp(transform.position);
            currentState = State.Bonked;
            animator.CrossFade(SuperBonkedHash, crossFade);
            currentBonkedTime = 0;
            isWalking = false;
            isRunning = false;
            isIdle = false;
            OnEnemyBonked?.Invoke();
            return;
        }    

        // If value >= bonkResistance, enemy is bonked.
        // Increase bonk resistance
        if(value >= bonkResistance)
        {
            agent.isStopped = true;
            agent.Warp(transform.position);
            currentState = State.Bonked;
            animator.CrossFade(BonkedHash, crossFade);
            currentBonkedTime = 0;
            bonkResistance += bonkResistGain;
            if (bonkResistance >= bonkResistMax) bonkResistance = bonkResistMax;
            isWalking = false;
            isRunning = false;
            isIdle = false;
            OnEnemyBonked?.Invoke();
        }
    }
}
