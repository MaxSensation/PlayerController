using UnityEngine;

public class PhysicsComponent : MonoBehaviour
{
    private Vector3 _velocity;
    private Vector3 _previousPosition;
    private BoxCollider _boxCollider;
    private PlayerController _player;
    private float _maxDistance;
    private RaycastHit _hit;
    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider>();
        _player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        // _maxDistance = (_player.skinWidth + _player.groundCheckDistance) * 4f;
    }

    private void Update()
    {
        _previousPosition = transform.position;
        CheckCollition();
    }

    private void CheckCollition()
    {
        Physics.BoxCast(transform.position, transform.lossyScale / 2, Vector3.up, out _hit, transform.rotation, _maxDistance);
        if (_hit.collider && _hit.collider.CompareTag("Player"))
        {
            //För att få fram differensen, subtraherar vi spelarens hastighet längs med ytan med plattformens hastighet
            // var playerVelocityOnPlane = Vector3.ProjectOnPlane(_player.velocity, Vector3.up) * _player.velocity.magnitude;
            // var playerSpeedDifference = (playerVelocityOnPlane.magnitude - _velocity.magnitude) * -_velocity.normalized;
            // var normalForce = HelpClass.GetNormalForce((playerSpeedDifference.magnitude - _velocity.magnitude) * -_velocity.normalized,  Vector3.up);
            // _player.Velocity += normalForce;
            //
            // Debug.Log(normalForce);
            //Om differensen av hastigheten är mindre än den statiska friktionen bör karaktären stå still på plattformen (alltså röra sig i samma hastighet som plattformen)
            // if (playerVelocityOnPlane.magnitude < _player.staticFriction)
            {
                // _player.velocity -= playerSpeedDifference;
            }
            // Om spelaren rör sig snabbare än den statiska friktionen 
            // else
            {
                //_player.velocity -= playerSpeedDifference.normalized * (_player.normalForce.magnitude * _player.dynamicFriction);                
            }
        }
    }
    
    // void OnDrawGizmos()
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
    //
    //
    //     Debug.DrawLine(Vector3.zero, _player.Velocity, Color.magenta);
    //     Debug.DrawLine(Vector3.zero, Vector3.ProjectOnPlane(_player.Velocity, Vector3.up) * _player.Velocity.magnitude, Color.yellow);
    // }


    private void LateUpdate()
    {
        _velocity = (_previousPosition - transform.position) / Time.deltaTime;
    }
}
