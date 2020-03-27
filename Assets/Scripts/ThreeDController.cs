﻿using System;
using UnityEngine;

public class ThreeDController : MonoBehaviour
{
    [SerializeField]
    [Range(0f,500f)]
    public float jumpHeight;
    [SerializeField]
    [Range(0f,500f)]
    public float gravity;
    [SerializeField]
    [Range(1f,500f)]
    public float terminalVelocity;
    [SerializeField]
    [Range(1f,500f)]
    public float accelerationSpeed;
    [SerializeField]
    [Range(-1f,-500f)]
    public float decelerateSpeed;
    [SerializeField]
    [Range(0f,1f)]
    public float decelerateThreshold;
    [SerializeField]
    [Range(0f,1f)]
    public float staticFriction;
    [SerializeField]
    [Range(0f,1f)]
    public float dynamicFriction;
    [SerializeField]
    [Range(0f,1f)]
    public float airResistant;
    [SerializeField]
    [Range(0f,1f)]
    public float skinWidth;
    [SerializeField]
    [Range(0f,1f)]
    public float groundCheckDistance;
    [SerializeField]
    [Range(1f,500f)]
    public float mouseSensitivity;
    public LayerMask geometriLayer;
    private CapsuleCollider _collider;
    private Transform _firstPersonCamera;
    private Vector3 _velocity;
    private Vector2 _cameraRotation;
    private void Awake()
    {
        accelerationSpeed = 15f;
        decelerateSpeed = -1f;
        decelerateThreshold = 0.01f;
        terminalVelocity = 30f;
        jumpHeight = 9f;
        gravity = 14f;
        staticFriction = 0.6f;
        dynamicFriction = 0.4f;
        airResistant = 0.1f;
        skinWidth = 0.05f;
        groundCheckDistance = 0.05f;
        mouseSensitivity = 100;
        _collider = GetComponent<CapsuleCollider>();
        _firstPersonCamera = transform.GetChild(0);
        _velocity = Vector3.zero;
        _cameraRotation = Vector2.zero;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private Vector3 GetAllowedDistance(Vector3 movement)
    {
        var distanceToPoints = _collider.height / 2;
        var position = transform.position;
        var center = _collider.center;
        var point1 = position + center + Vector3.up * distanceToPoints;
        var point2 = position + center + Vector3.down * distanceToPoints;
        Physics.CapsuleCast(point1, point2, _collider.radius, movement.normalized, out var hit, float.PositiveInfinity,
            geometriLayer);
        if (!hit.collider) return movement;
        var allowedDistance = hit.distance + (skinWidth / Vector3.Dot(movement.normalized, hit.normal));
        if (allowedDistance >= movement.magnitude) return movement;
        var normalForce = HelpClass.GetNormalForce(_velocity, hit.normal);
        _velocity += normalForce;
        AddFriction(normalForce.magnitude);
        return GetAllowedDistance(_velocity * Time.deltaTime);
    }

    private Vector3 GetJumpVector()
    {
        if (!GetGroundNormal().collider || !Input.GetKeyDown(KeyCode.Space)) return Vector3.zero;
        return Vector3.up * jumpHeight;
    }

    private Vector3 GetInputVector()
    {
        var direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
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

    private void AddForce()
    {
        var inputVector = GetInputVector();
        inputVector = CorrectInputVectorFromCamera(inputVector);
        var forces = (inputVector + GetJumpVector() + GetGravityVector());
        if (inputVector.magnitude > 0)
            Accelerate(forces);
        else
            Decelerate();
        _velocity += forces;
        _velocity *= GetAirResistance();
        transform.position += GetAllowedDistance(_velocity * Time.deltaTime);
    }

    private Vector3 CorrectInputVectorFromCamera(Vector3 inputVector)
    {
        var hit = GetGroundNormal();
        return Vector3.ProjectOnPlane(_firstPersonCamera.rotation.normalized * inputVector, hit.collider ? hit.normal : Vector3.up);
    }

    private RaycastHit GetGroundNormal()
    {
        var distanceToPoints = _collider.height / 2;
        var position = transform.position;
        var center = _collider.center;
        var point1 = position + center + Vector3.up * distanceToPoints;
        var point2 = position + center + Vector3.down * distanceToPoints;
        Physics.CapsuleCast(point1, point2, _collider.radius, Vector3.down, out var hit, groundCheckDistance + skinWidth, geometriLayer);
        return hit;
    }

    private float GetAirResistance()
    {
        return Mathf.Pow(1 - airResistant, Time.deltaTime);
    }

    private void AddFriction(float normalForceMagnitude)
    {
        if (_velocity.magnitude < (normalForceMagnitude * staticFriction)) _velocity = Vector3.zero;
        _velocity -= (_velocity.normalized * (normalForceMagnitude * dynamicFriction));
    }

    private void Accelerate(Vector3 forces)
    {
        var turnSpeed = Mathf.Lerp(0.2f, 0.4f, Vector3.Dot(forces.normalized, _velocity.normalized));
        _velocity += forces * ((accelerationSpeed + turnSpeed) * Time.deltaTime);
        if (_velocity.magnitude > terminalVelocity) _velocity = _velocity.normalized * (terminalVelocity);
    }

    private void Decelerate()
    {
        var decelerateVector = _velocity;
        decelerateVector.y = 0;
        if (decelerateVector.magnitude > decelerateThreshold) _velocity += decelerateVector.normalized * (decelerateSpeed * Time.deltaTime);
        else _velocity.x = 0;
    }

    private void Update()
    {
        RotateCamera();
        AddForce();
    }

    private void RotateCamera()
    {
        var cameraRotation = new Vector2(Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X")) * (mouseSensitivity * Time.deltaTime);
        _cameraRotation.x += cameraRotation.y;
        _cameraRotation.y -= cameraRotation.x;
        _cameraRotation.y = Mathf.Clamp(_cameraRotation.y, -90f, 90f);
        _firstPersonCamera.localRotation = Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0);
    }
}