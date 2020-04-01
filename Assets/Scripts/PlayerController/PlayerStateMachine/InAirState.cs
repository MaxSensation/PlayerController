using System;
using UnityEngine;

namespace PlayerStateMachine
{
    [CreateAssetMenu(menuName = "PlayerState/InAirState")]
    public class InAirState : PlayerBaseState
    {
        [SerializeField] private float accelerationSpeed;

        public override void Enter()
        {
            Debug.Log("Entered InAir State");
        }
        
        public override void Run()
        {
            // If grounded then change to land State
            if (Player.GetRayCast(Vector3.down, GetGroundCheckDistance + GetSkinWidth).collider)
                stateMachine.TransitionTo<LandState>();
            
            // Get Input from user
            var inputVector = Player.GetInputVector(accelerationSpeed);
        
            // Add Input force and Gravity force together
            var forces = inputVector + Physic3D.GetGravity();
        
            // Add all forces to velocity
            Velocity += forces;
        
            // If any directional inputs accelerate with the accelerateSpeed added with turnSpeed 
            if (inputVector.magnitude > 0) 
                Velocity += Physic3D.GetAcceleration(forces, accelerationSpeed + Physic3D.GetTurnVelocity(forces, Velocity.normalized));
            // else
            //     Velocity -= Physic3D.GetDeceleration(Velocity, decelerateSpeed, decelerateThreshold);
        }
    }
}