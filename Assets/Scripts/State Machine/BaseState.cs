using UnityEngine;

namespace StateMachine
{
    public abstract class BaseState : IState
    {
        protected readonly Enemy enemy;
        protected readonly Animator animator;

        // Hashed Animations so we can control animations through code
        // Not the tyranny of the Unity Animator 
        protected static readonly int LocomotionHash = Animator.StringToHash("Locomotion");

        protected const float crossFadeDuration = 0.1f;

        protected BaseState(Enemy enemy, Animator animator)
        {
            this.enemy = enemy;
            this.animator = animator;
        }

        public virtual void OnEnter()
        {
            // ...
        }

        public virtual void OnExit()
        {
            // ...
        }

        public virtual void OnFixedUpdate()
        {
            // ...
        }

        public virtual void OnUpdate()
        {
            // ...
        }
    }
}
