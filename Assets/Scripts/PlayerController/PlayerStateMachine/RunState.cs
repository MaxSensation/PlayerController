using UnityEngine;

namespace PlayerStateMachine
{
    [CreateAssetMenu(menuName = "PlayerState/RunState")]
    public class RunState : PlayerBaseState
    {
        [SerializeField] private float accelerationSpeed;
        [SerializeField] private float minMoveSpeed;
        // [SerializeField] private float decelerateSpeed;
        // [SerializeField] private float decelerateThreshold;
        
        public override void Enter()
        {
            Debug.Log("Entered Run State");
        }
        
        public override void Run()
        {
            
            // Walk if shift released pressed
            if (Input.GetKeyUp(KeyCode.LeftShift))
                stateMachine.TransitionTo<WalkState>();
            
            // If Player is on Ground and the Player is pressing the jumpKey then change state to JumpState
            if (Player.GetRayCast(Vector3.down, GetGroundCheckDistance + GetSkinWidth).collider && Input.GetKeyDown(KeyCode.Space))
                stateMachine.TransitionTo<JumpState>();

            // If Player is not grounded then change state to In Air State
            if (!Player.GetRayCast(Vector3.down, GetGroundCheckDistance + GetSkinWidth).collider)
                stateMachine.TransitionTo<InAirState>();

            // Get Input from user
            var inputVector = Player.GetInputVector(accelerationSpeed);

            // Add Input force to velocity
            Velocity += inputVector;

            // Check if player is moving and if not change state to standState
            if (Velocity.magnitude <= minMoveSpeed)
                stateMachine.TransitionTo<StandState>();

            // Save the gravityVector as a variable
            var gravityVector = Physic3D.GetGravity();
            
            // Add gravityVector to Velocity
            Velocity += gravityVector;
            
            // Combine forces
            var forces = gravityVector + inputVector;

            // If any directional inputs accelerate with the accelerateSpeed added with turnSpeed 
            if (inputVector.magnitude > 0) 
                Velocity += Physic3D.GetAcceleration(forces, accelerationSpeed + Physic3D.GetTurnVelocity(forces, Velocity.normalized));
            // else
            //     Velocity -= Physic3D.GetDeceleration(Velocity, decelerateSpeed, decelerateThreshold);
        }
    }
}