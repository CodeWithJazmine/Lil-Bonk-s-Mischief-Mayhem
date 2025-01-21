using UnityEngine;

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
    [SerializeField] private float wanderSpeed = 2f;
    [SerializeField] private float fleeSpeed = 4f;
    [SerializeField] private float minWaitTime = 2f; // Minimum wait time
    [SerializeField] private float maxWaitTime = 5f; // Maximum wait time
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
        // Move toward the wander target
        transform.position = Vector3.MoveTowards(transform.position, wanderTarget, wanderSpeed * Time.deltaTime);

        // Check if the enemy has reached the wander target
        if (Vector3.Distance(transform.position, wanderTarget) < 0.5f)
        {
            currentState = State.Wait;
            waitTimer = Random.Range(minWaitTime, maxWaitTime); // Set a random wait time
        }

        // Face the wander target
        Vector3 direction = wanderTarget - transform.position;
        direction.y = 0; // Lock rotation to the Y-axis
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        if(!isWalking)
            animator.CrossFade(WalkHash, crossFade);

        isWalking = true;
        isRunning = false;
        isIdle = false;
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
        Vector3 randomDirection = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
        wanderTarget = transform.position + randomDirection;
    }

    private void Flee()
    {
        // Calculate direction away from the player
        Vector3 fleeDirection = (transform.position - player.position).normalized;

        // Move in the flee direction
        transform.position += fleeDirection * fleeSpeed * Time.deltaTime;

        // Face the flee direction
        if (fleeDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(fleeDirection);
        }

        if (!isRunning)
            animator.CrossFade(RunHash, crossFade);

        isWalking = false;
        isRunning = true;
        isIdle = false;
    }

    public void OnBonked()
    {
        throw new System.NotImplementedException();
    }
}
