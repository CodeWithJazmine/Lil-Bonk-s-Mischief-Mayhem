using UnityEngine;
using UnityEngine.AI;

namespace StateMachine
{
    public class PlayerDetector : MonoBehaviour
    {

    }

    public class WanderState : BaseState
    {
        readonly NavMeshAgent agent;
        readonly Vector3 startPoint;
        readonly float wanderRadius;
        readonly float waitTime;

        public WanderState(Enemy enemy, Animator animator, NavMeshAgent agent, float wanderRadius) : base(enemy, animator)
        {
            this.agent = agent;
            this.startPoint = startPoint;
            this.wanderRadius = wanderRadius;
        }

        public override void OnEnter()
        {
            Debug.Log("Wander");
            animator.CrossFade(WalkHash, crossFadeDuration);
        }

        public override void OnUpdate()
        {
            if(HasReachedDestination())
            {
                // Find new destination
                var randomDirection = Random.insideUnitSphere * wanderRadius;
                randomDirection += startPoint;
                NavMeshHit hit;
                NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, 1);
                var finalPosition = hit.position;
                agent.SetDestination(finalPosition);
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
