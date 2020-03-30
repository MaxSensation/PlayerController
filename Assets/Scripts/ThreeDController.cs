using System;
using UnityEngine;
using UnityEngine.Internal;

public class ThreeDController : MonoBehaviour
{
    [SerializeField] [Range(0f,500f)] public float jumpHeight;
    [SerializeField] [Range(0f,500f)] public float gravity;
    [SerializeField] [Range(1f,500f)] public float terminalVelocity; 
    [SerializeField] [Range(1f,500f)] public float accelerationSpeed;
    // [SerializeField]
    // [Range(-1f,-500f)]
    // public float decelerateSpeed;
    // [SerializeField]
    // [Range(0f,1f)]
    // public float decelerateThreshold;
    [SerializeField] [Range(0f,1f)] public float staticFriction;
    [SerializeField] [Range(0f,1f)] public float dynamicFriction;
    [SerializeField] [Range(0f,1f)] public float airResistant;
    [SerializeField] [Range(0f,1f)] public float skinWidth;
    [SerializeField] [Range(0f,1f)] public float groundCheckDistance;
    [SerializeField] [Range(0f,10f)] public float overlayColliderResistant;
    [SerializeField] [Range(0f,500f)] public float mouseSensitivity;
    [SerializeField] public LayerMask collisionLayer; 
    [SerializeField] public bool thirdPersonCamera;
    [SerializeField] public float thirdPersonCameraSize;
    [SerializeField] public float thirdPersonCameraMaxAngle;
    [SerializeField] public float thirdPersonCameraDistance;
    [NonSerialized] public Vector3 Velocity;
    [NonSerialized] public Vector3 normalForce;
    private CapsuleCollider _collider;
    private Transform _camera;
    private Vector2 _cameraRotation;
    private Vector3 _whereCameraWantToMove;
    private RaycastHit _cameraCast;
    
    public enum State
    {
        STATE_STANDING,
        STATE_JUMPING
    }
    
    private void Awake()
    {
        accelerationSpeed = 15f;
        // decelerateSpeed = -1f;
        // decelerateThreshold = 0.01f;
        terminalVelocity = 20f;
        jumpHeight = 9f;
        gravity = 14f;
        staticFriction = 0.8f;
        dynamicFriction = 0.6f;
        airResistant = 0.1f;
        skinWidth = 0.05f;
        groundCheckDistance = 0.05f;
        overlayColliderResistant = 4f;
        mouseSensitivity = 100;
        _collider = GetComponent<CapsuleCollider>();
        if (Camera.main != null) _camera = Camera.main.transform;
        Velocity = Vector3.zero;
        _cameraRotation = Vector2.zero;
        Cursor.lockState = CursorLockMode.Locked;
        thirdPersonCamera = true;
        thirdPersonCameraDistance = 10f;
        thirdPersonCameraSize = 0.5f;
        thirdPersonCameraMaxAngle = 25f;
    }
    
    private void AddForce()
    {
        var inputVector = GetInputVector();
        var forces = (inputVector + GetJumpVector() + GetGravityVector());
        if (inputVector.magnitude > 0) Accelerate(forces);
        // else Decelerate();
        Velocity += forces;
        AddAirResistance();
        var correctedVector = GetAllowedDistance(Velocity * Time.deltaTime);
        transform.position += correctedVector;
        //transform.position += Velocity * Time.deltaTime;
    }
    
    private void Accelerate(Vector3 forces)
    {
        var turnSpeed = Mathf.Lerp(0.1f, 0.4f, Vector3.Dot(forces.normalized, Velocity.normalized));
        Velocity += forces * ((accelerationSpeed + turnSpeed) * Time.deltaTime);
        if (Velocity.magnitude > terminalVelocity) Velocity = Velocity.normalized * (terminalVelocity);
    }

    // private void Decelerate()
    // {
    //     var decelerateVector = velocity;
    //     decelerateVector.y = 0;
    //     if (decelerateVector.magnitude > decelerateThreshold) velocity += decelerateVector.normalized * (decelerateSpeed * Time.deltaTime);
    //     else velocity.x = 0;
    // }
    
    private Vector3 GetAllowedDistance(Vector3 movement)
    {
        while (true)
        {
            var distanceToPoints = _collider.height / 2;
            var position = transform.position;
            var center = _collider.center;
            var point1 = position + center + Vector3.up * distanceToPoints;
            var point2 = position + center + Vector3.down * distanceToPoints;
            Physics.CapsuleCast(point1, point2, _collider.radius, movement.normalized, out var hit, float.PositiveInfinity, collisionLayer);
            if (!hit.collider) return movement;
            var allowedDistance = hit.distance + (skinWidth / Vector3.Dot(movement.normalized, hit.normal));
            if (allowedDistance < 0) allowedDistance = 0;
            if (allowedDistance >= movement.magnitude) return movement;
            normalForce = HelpClass.GetNormalForce(Velocity, hit.normal);
            Velocity += normalForce;
            AddFriction(normalForce.magnitude);
            AddOverLayCorrection(movement, point1, point2);
            movement = Velocity * Time.deltaTime;
        }
    }

    private Vector3 GetJumpVector()
    {
        if (!GetGroundNormal().collider || !Input.GetKeyDown(KeyCode.Space)) return Vector3.zero;
        return Vector3.up * jumpHeight;
    }

    private Vector3 GetInputVector()
    {
        var direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        direction = CorrectInputVectorFromCamera(direction);
        if (direction.magnitude > 1)
        {
            return direction.normalized * (accelerationSpeed * Time.deltaTime);
        }
        return direction * (accelerationSpeed * Time.deltaTime);
    }

    private Vector3 GetGravityVector()
    {
        return Vector3.down * (gravity * Time.deltaTime);
    }
    
    private RaycastHit GetGroundNormal()
    {
        var distanceToPoints = _collider.height / 2;
        var position = transform.position;
        var center = _collider.center;
        var point1 = position + center + Vector3.up * distanceToPoints;
        var point2 = position + center + Vector3.down * distanceToPoints;
        Physics.CapsuleCast(point1, point2, _collider.radius, Vector3.down, out var hit, groundCheckDistance + skinWidth, collisionLayer);
        return hit;
    }
    
    private void AddAirResistance()
    {
        Velocity *= Mathf.Pow(1 - airResistant, Time.deltaTime);
    }

    private void AddFriction(float normalForceMagnitude)
    {
        if (Velocity.magnitude < (normalForceMagnitude * staticFriction)) Velocity = Vector3.zero;
        Velocity -= (Velocity.normalized * (normalForceMagnitude * dynamicFriction));
    }
    
    private void AddOverLayCorrection(Vector3 movement, Vector3 point1, Vector3 point2)
    {
        var overlapCollides = Physics.OverlapCapsule(point1, point2, _collider.radius, collisionLayer);
        foreach (var overlapCollider in overlapCollides)
        {
            var playerClosestPointOnBounds = _collider.ClosestPointOnBounds(overlapCollider.transform.position);
            var colliderOverLapClosestPointOnBounds = overlapCollider.ClosestPointOnBounds(playerClosestPointOnBounds);
            Velocity +=  -movement.normalized * ((colliderOverLapClosestPointOnBounds.magnitude + overlayColliderResistant*100.0f) * Time.deltaTime);
        }
    }

    private void Update()
    {
        RotateCamera();
        AddForce();
        MoveCamera();
    }

    private void MoveCamera()
    {
        if (thirdPersonCamera)
        {
            _whereCameraWantToMove = transform.position - _camera.forward * thirdPersonCameraDistance;
            Physics.SphereCast(transform.position, thirdPersonCameraSize,  (_whereCameraWantToMove - transform.position).normalized, out _cameraCast, (_whereCameraWantToMove - transform.position).magnitude);
            if (_cameraCast.collider)
                _camera.position = transform.position + (_whereCameraWantToMove - transform.position).normalized * _cameraCast.distance;
            else
                _camera.position = _whereCameraWantToMove;
        }
        else 
            _camera.localPosition = Vector3.zero;
        
    }
    
    private void RotateCamera()
    {
        var cameraRotation = new Vector2(Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X")) * (mouseSensitivity * Time.deltaTime);
        _cameraRotation.x += cameraRotation.y;
        _cameraRotation.y -= cameraRotation.x;
        _cameraRotation.y = Mathf.Clamp(_cameraRotation.y, thirdPersonCamera ? -thirdPersonCameraMaxAngle : -89.9f, 89.9f);
        _camera.localRotation = Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0);
    }
    
    private Vector3 CorrectInputVectorFromCamera(Vector3 inputVector)
    {
        var hit = GetGroundNormal();
        var projectHorizontal = Vector3.ProjectOnPlane(_camera.rotation * inputVector, Vector3.up);
        return hit.collider ? Vector3.ProjectOnPlane(projectHorizontal,  hit.normal).normalized : projectHorizontal.normalized;
    }
    
    
    void OnDrawGizmos()
    {
        if (thirdPersonCamera)
        {
            if (_cameraCast.collider)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(transform.position, (_whereCameraWantToMove - transform.position).normalized * (_whereCameraWantToMove - transform.position).magnitude);
                Gizmos.DrawSphere(transform.position + (_whereCameraWantToMove - transform.position).normalized * _cameraCast.distance, thirdPersonCameraSize);
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, (_whereCameraWantToMove - transform.position).normalized * (_whereCameraWantToMove - transform.position).magnitude);
            }   
        }
    }
}