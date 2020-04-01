using UnityEngine;

namespace PlayerStateMachine
{
    [CreateAssetMenu(menuName = "PlayerState/StandState")]
    public class StandState : PlayerBaseState
    {
        public override void Enter()
        {
            Debug.Log("Entered Stand State");
        }
        public override void Run()
        {
            if (new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).magnitude > 0f)
                stateMachine.TransitionTo<MoveState>();
            
            var grounded = Player.GetRayCast(Vector3.down, GetGroundCheckDistance + GetSkinWidth).collider;
            
            if (!grounded)
                stateMachine.TransitionTo<InAirState>();
            
            if (Input.GetKeyDown(KeyCode.Space))
                stateMachine.TransitionTo<JumpState>();
        }
    }
}