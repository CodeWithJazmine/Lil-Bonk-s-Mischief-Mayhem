using UnityEngine;
using UnityEngine.AI;

public class EnemyStateMachine : MonoBehaviour, IBonkable
{
    // States
    private enum State
    {
        Wander,
        Wait,
        Flee
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
    NavMeshAgent agent;
    Animator animator;

    // Animations
    private static readonly int IdleHash = Animator.StringToHash("Idle");
    private static readonly int WalkHash = Animator.StringToHash("Walk");
    private static readonly int RunHash = Animator.StringToHash("Run");
    private static readonly int KnockoutHash = Animator.StringToHash("Knockout");
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
        }

        // Transition logic
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer < fleeDistance)
        {
            currentState = State.Flee;
        }
        else if (currentState == State.Flee && distanceToPlayer >= fleeDistance)
        {
            currentState = State.Wander;
            SetNewWanderTarget();
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

    public void OnBonked()
    {
        throw new System.NotImplementedException();
    }
}
