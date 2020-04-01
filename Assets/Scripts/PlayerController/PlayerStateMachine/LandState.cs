using System;
using UnityEngine;

namespace PlayerStateMachine
{
    [CreateAssetMenu(menuName = "PlayerState/LandState")]
    public class LandState : PlayerBaseState
    {
        public override void Enter()
        {
            Debug.Log("Entered Land State");
            if (Velocity.magnitude > 0f)
                stateMachine.TransitionTo<MoveState>();
            else 
                stateMachine.TransitionTo<StandState>();
        }
    }
}