using UnityEngine;

namespace StateMachine
{
    public class FleeState : BaseState
    {
        public FleeState(Enemy enemy, Animator animator) : base(enemy, animator)
        {
        }
    }
}
