using System;
using PlayerStates;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // [SerializeField] [Range(-1f,-500f)] public float decelerateSpeed;
    // [SerializeField] [Range(0f,1f)] public float decelerateThreshold;
    
    //States
    // private static StandingState _standingState;
    // private static StandingState _walkingState;
    // private static JumpingState _jumpingState;
    
    [SerializeField] [Range(0f,500f)] private float jumpHeight;
    [SerializeField] [Range(0f,500f)] private float gravity;
    [SerializeField] [Range(1f,500f)] private float terminalVelocity; 
    [SerializeField] [Range(1f,500f)] private float accelerationSpeed;
    [SerializeField] [Range(0f,1f)] private float staticFriction;
    [SerializeField] [Range(0f,1f)] private float dynamicFriction;
    [SerializeField] [Range(0f,1f)] private float airResistant;
    [SerializeField] [Range(0f,1f)] private float skinWidth;
    [SerializeField] [Range(0f,1f)] private float groundCheckDistance;
    [SerializeField] [Range(0f,100f)] private float overlayColliderResistant;
    [SerializeField] [Range(0f,500f)] private float mouseSensitivity;
    [SerializeField] private LayerMask collisionLayer; 
    [SerializeField] private bool thirdPersonCamera;
    [SerializeField] private float thirdPersonCameraSize;
    [SerializeField] private float thirdPersonCameraMaxAngle;
    [SerializeField] private float thirdPersonCameraDistance;
    
    private Vector3 _velocity;
    private CapsuleCollider _collider;
    private Transform _camera;
    private Vector2 _cameraRotation;
    private Vector3 _whereCameraWantToMove;
    private RaycastHit _cameraCast;
    private Vector3 _cameraOffset;
    private Vector3 _point1;
    private Vector3 _point2;
    private float _distanceToPoints;
    
    private void Awake()
    {
        // decelerateSpeed = -1f;
        // decelerateThreshold = 0.01f;
        accelerationSpeed = 15f;
        terminalVelocity = 20f;
        jumpHeight = 9f;
        gravity = 14f;
        staticFriction = 0.8f;
        dynamicFriction = 0.6f;
        airResistant = 0.1f;
        skinWidth = 0.05f;
        groundCheckDistance = 0.05f;
        overlayColliderResistant = 20f;
        mouseSensitivity = 100;
        _velocity = Vector3.zero;
        thirdPersonCamera = true;
        thirdPersonCameraDistance = 5f;
        thirdPersonCameraSize = 0.5f;
        thirdPersonCameraMaxAngle = 25f;
        if (Camera.main != null) _camera = Camera.main.transform;
        _collider = GetComponent<CapsuleCollider>();
        _distanceToPoints = _collider.height / 2;
        _cameraRotation = Vector2.zero;
        _cameraOffset = _camera.localPosition;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    private void Update()
    {
        AddForce();
        RotateCamera();
        MoveCamera();
    }
    
    private void AddForce()
    {
        // Get CapsuleInfo
        var capsulePosition = transform.position + _collider.center;
        _point1 = capsulePosition + Vector3.up * _distanceToPoints;
        _point2 = capsulePosition + Vector3.down * _distanceToPoints;
        
        // Get Input from user
        var inputVector = GetInputVector();
        
        // Add Input force, Jump force and Gravity force together
        var forces = inputVector + GetJumpVector() + HelpClass.GetGravity(gravity);
        
        // Add all forces to velocity
        _velocity += forces;
        
        // If any directional inputs accelerate with the accelerateSpeed added with turnSpeed 
        if (inputVector.magnitude > 0) 
            _velocity += HelpClass.GetAcceleration(forces, accelerationSpeed + HelpClass.GetTurnVelocity(forces, _velocity.normalized));
        
        // Add Air resistant to the player
        _velocity *= HelpClass.GetAirResistant(airResistant);

        // Limit the velocity to terminalVelocity
        if (_velocity.magnitude > terminalVelocity) 
            _velocity = _velocity.normalized * terminalVelocity;
        
        // Get MovementPerFrame
        AddOverLayCorrection();
        var movementPerFrame = _velocity * Time.deltaTime;
        
        // Only Move Player as close as possible to the collision
        while (true)
        {
            // Get hit from CapsuleCast in the direction as Velocity
            var hit = GetRayCast(_velocity.normalized, float.PositiveInfinity);
            // If any collision continue 
            if (!hit.collider) break;
            // If AllowedDistance is greater then MovementPerFrame magnitude continue
            if (GetAllowedDistance(hit, movementPerFrame) >= movementPerFrame.magnitude) break;
            // Get NormalForce
            var normalForce = HelpClass.GetNormalForce(_velocity, hit.normal);
            // Add NormalForce To velocity
            _velocity += normalForce;
            // Add Friction to Velocity
            _velocity -= HelpClass.GetFriction(_velocity, normalForce.magnitude, dynamicFriction, staticFriction);
            // Add the new MovementPerFrame
            movementPerFrame = _velocity * Time.deltaTime;
        }
        // Add movementPerFrame to position of Player
        transform.position += movementPerFrame;
    }

    // private void Decelerate()
    // {
    //     var decelerateVector = velocity;
    //     decelerateVector.y = 0;
    //     if (decelerateVector.magnitude > decelerateThreshold) velocity += decelerateVector.normalized * (decelerateSpeed * Time.deltaTime);
    //     else velocity.x = 0;
    // }


    private RaycastHit GetRayCast(Vector3 direction, float magnitude)
    {
        Physics.CapsuleCast(_point1, _point2, _collider.radius, direction.normalized, out var hit, magnitude, collisionLayer);
        return hit;
    }

    private float GetAllowedDistance(RaycastHit hit, Vector3 movement)
    {
        return hit.distance + (skinWidth / Vector3.Dot(movement.normalized, hit.normal));
    }

    private Vector3 GetJumpVector()
    {
        if (!GetRayCast(Vector3.down, groundCheckDistance + skinWidth).collider || !Input.GetKeyDown(KeyCode.Space)) return Vector3.zero;
        return Vector3.up * jumpHeight;
    }

    private Vector3 GetInputVector()
    {
        var direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        direction = CorrectInputVectorFromCamera(direction);
        if (direction.magnitude > 1) return direction.normalized * (accelerationSpeed * Time.deltaTime);
        return direction * (accelerationSpeed * Time.deltaTime);
    }

    private void AddOverLayCorrection()
    {
        var overlapCollides = Physics.OverlapCapsule(_point1, _point2, _collider.radius, collisionLayer);
        foreach (var overlapCollider in overlapCollides)
        {
            var playerClosestPointOnBounds = _collider.ClosestPointOnBounds(overlapCollider.transform.position);
            var colliderOverLapClosestPointOnBounds = overlapCollider.ClosestPointOnBounds(playerClosestPointOnBounds);
            _velocity +=  -_velocity.normalized * ((colliderOverLapClosestPointOnBounds.magnitude + overlayColliderResistant * 100.0f) * Time.deltaTime);
        }
    }

    private void RotateCamera()
    {
        var cameraRotation = new Vector2(Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X")) * (mouseSensitivity * Time.deltaTime);
        _cameraRotation.x += cameraRotation.y;
        _cameraRotation.y -= cameraRotation.x;
        _cameraRotation.y = Mathf.Clamp(_cameraRotation.y, thirdPersonCamera ? -thirdPersonCameraMaxAngle : -89.9f, 89.9f);
        _camera.localRotation = Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0);
    }

    private void MoveCamera()
    {
        if (thirdPersonCamera)
        {
            var playerPosition = transform.position;
            _whereCameraWantToMove = (playerPosition + _cameraOffset) - (_camera.forward * thirdPersonCameraDistance);
            var direction = (_whereCameraWantToMove - playerPosition).normalized;
            var distance = (_whereCameraWantToMove - playerPosition).magnitude;
            _camera.position = Physics.SphereCast(playerPosition + _cameraOffset, thirdPersonCameraSize, direction, out _cameraCast, distance) ? playerPosition + _cameraOffset + direction * _cameraCast.distance : _whereCameraWantToMove;
        } 
        else _camera.localPosition = Vector3.zero;
    }
    
    private Vector3 CorrectInputVectorFromCamera(Vector3 inputVector)
    {
        var hit = GetRayCast(Vector3.down, groundCheckDistance + skinWidth);
        var projectHorizontal = Vector3.ProjectOnPlane(_camera.rotation * inputVector, Vector3.up);
        return hit.collider ? Vector3.ProjectOnPlane(projectHorizontal,  hit.normal).normalized : projectHorizontal.normalized;
    }
}