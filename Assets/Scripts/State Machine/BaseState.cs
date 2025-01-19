using UnityEngine;

namespace StateMachine
{
    public abstract class BaseState : IState
    {
        protected readonly PlayerMovement controller;
        protected readonly Animator animator;

        // Hashed Animations so we can control animations through code
        // Not the tyranny of the Unity Animator 
        protected static readonly int LocomotionHash = Animator.StringToHash("Locomotion");

        protected const float crossFadeDuration = 0.1f;

        protected BaseState(PlayerMovement controller, Animator animator)
        {
            this.controller = controller;
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
