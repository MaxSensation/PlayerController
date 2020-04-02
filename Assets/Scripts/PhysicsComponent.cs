﻿using UnityEngine;

public class PhysicsComponent : MonoBehaviour
{
    private Vector3 _velocity;
    private Rigidbody _rigidbody;
    private PlayerController _player;
    private float _maxDistance;
    private RaycastHit _hit;
    private bool _wasOnPlatform;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        _maxDistance = (_player.GetSkinWidth() + _player.GetGroundCheckDistance()) * 4f;
        _wasOnPlatform = false;
    }

    private void Update()
    {
        CheckCollition();
    }

    private void CheckCollition()
    {
        Physics.BoxCast(transform.position, transform.lossyScale / 2, Vector3.up, out _hit, transform.rotation, _maxDistance);
        if (_hit.collider && _hit.collider.CompareTag("Player"))
        {
            Debug.Log("Found Player");
            _player.transform.parent = transform;
            _wasOnPlatform = true;
        }
        else
        {
            if (_wasOnPlatform)
            {
                _player.AddForce(_rigidbody.velocity);
                _player.transform.parent = null;
                _wasOnPlatform = false;
            }
        }
    }
    
    // public void OnDrawGizmos()
    // {
    //     if (_hit.collider)
    //     {
    //         Gizmos.color = Color.red;
    //         Gizmos.DrawRay(transform.position, Vector3.up * _hit.distance);
    //         Gizmos.DrawWireCube(transform.position + Vector3.up * _hit.distance, transform.lossyScale);
    //     }
    //     else
    //     {
    //         Gizmos.color = Color.green;
    //         Gizmos.DrawRay(transform.position, Vector3.up * _maxDistance);
    //     }
    //     Debug.DrawLine(Vector3.zero, _player.GetVelocity(), Color.magenta);
    //     Debug.DrawLine(Vector3.zero, Vector3.ProjectOnPlane(_player.GetVelocity(), Vector3.up) * _player.GetVelocity().magnitude, Color.yellow);
    // }
}