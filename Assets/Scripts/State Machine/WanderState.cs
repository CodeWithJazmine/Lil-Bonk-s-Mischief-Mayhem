using UnityEngine;
using UnityEngine.AI;

namespace StateMachine
{
    public class WanderState : BaseState
    {
        readonly NavMeshAgent agent;
        readonly Vector3 startPoint;
        readonly float wanderRadius;
        readonly float waitTimeMin, waitTimeMax;
        private bool idle = false;
        private CountdownTimer timer = new();

        public WanderState(Enemy enemy, Animator animator, NavMeshAgent agent, float wanderRadius, float waitTimeMin = 1f, float waitTimeMax = 2f) : base(enemy, animator)
        {
            this.agent = agent;
            this.startPoint = startPoint;
            this.wanderRadius = wanderRadius;
            this.waitTimeMin = waitTimeMin;
            this.waitTimeMax = waitTimeMax;
        }

        public override void OnEnter()
        {
            Debug.Log("Wander");
            animator.CrossFade(WalkHash, crossFadeDuration);
        }

        public override void OnUpdate()
        {
            if(HasReachedDestination() && !idle)
            {
                animator.CrossFade(IdleHash, crossFadeDuration);
                timer.Reset(Random.Range(waitTimeMin, waitTimeMax));
                idle = true;
            }

            if(idle)
            {
                timer.Tick(Time.deltaTime);

                if(timer.TimeReached())
                {
                    // Find new destination
                    var randomDirection = Random.insideUnitSphere * wanderRadius;
                    randomDirection += startPoint;
                    NavMeshHit hit;
                    NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, 1);
                    var finalPosition = hit.position;
                    agent.SetDestination(finalPosition);
                    animator.CrossFade(WalkHash, crossFadeDuration);
                    idle = false;
                }
            }
        }

        bool HasReachedDestination()
        {
            return !agent.pathPending
                && agent.remainingDistance <= agent.stoppingDistance
                && (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);
        }
    }
}
