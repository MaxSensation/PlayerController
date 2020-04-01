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
            // If moving then change to Move State else Change to Stand State
            if (Velocity.magnitude > 0f)
                stateMachine.TransitionTo<WalkState>();
            else 
                stateMachine.TransitionTo<StandState>();
        }
    }
}