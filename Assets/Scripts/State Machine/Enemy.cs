using UnityEngine;
using UnityEngine.AI;

namespace StateMachine
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Enemy : MonoBehaviour, IBonkable
    {
        [SerializeField] NavMeshAgent agent;
        [SerializeField] Animator animator;

        [SerializeField] float wanderRadius = 5f;
        [SerializeField] float waitTimeMin = 1.5f;
        [SerializeField] float waitTimeMax = 5f;

        FSM stateMachine;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
        }

        void Start()
        {
            stateMachine = new FSM();

            var wanderState = new WanderState(this, animator, agent, 5f, waitTimeMin, waitTimeMax);

            Any(wanderState, new FuncPredicate(() => true)); // Always true for testing

            stateMachine.SetState(wanderState);
        }

        void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
        void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);

        void Update()
        {
            stateMachine.Update();
        }

        void FixedUpdate()
        {
            stateMachine.FixedUpdate();
        }

        public void OnBonked()
        {

        }
    }
}
