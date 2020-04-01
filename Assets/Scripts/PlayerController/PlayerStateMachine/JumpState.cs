using UnityEngine;

namespace PlayerStateMachine
{
    [CreateAssetMenu(menuName = "PlayerState/JumpState")]
    public class JumpState : PlayerBaseState
    {
        [SerializeField] private float jumpHeight;
        
        public override void Enter()
        {
            Debug.Log("Entered Jump State");
            Velocity += Vector3.up * jumpHeight + Physic3D.GetGravity();
            stateMachine.TransitionTo<InAirState>();
        }
    }
}