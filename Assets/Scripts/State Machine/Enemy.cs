using UnityEngine;
using UnityEngine.AI;

namespace StateMachine
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Enemy : MonoBehaviour, IBonkable
    {
        private NavMeshAgent agent;
        private Animator animator;

        FSM stateMachine;

        void OnValidate() => this.OnValidate();

        void Start()
        {
            stateMachine = new FSM();
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
