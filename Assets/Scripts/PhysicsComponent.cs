// using UnityEngine;
//
// public class PhysicsComponent : MonoBehaviour
// {
//     private Vector3 _velocity;
//     private Vector3 _previousPosition;
//     private BoxCollider _boxCollider;
//     private PlayerController _player;
//     private float _maxDistance;
//     private RaycastHit _hit;
//     
//     [SerializeField] [Range(0f,1f)] private float staticFriction;
//     [SerializeField] [Range(0f,1f)] private float dynamicFriction;
//     
//     private void Awake()
//     {
//         staticFriction = 0.8f;
//         dynamicFriction = 0.6f;
//         _boxCollider = GetComponent<BoxCollider>();
//         _player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
//         _maxDistance = (_player.GetSkinWidth() + _player.GetGroundCheckDistance()) * 4f;
//     }
//
//     private void Update()
//     {
//         _previousPosition = transform.position;
//         CheckCollition();
//     }
//
//     private void CheckCollition()
//     {
//         Physics.BoxCast(transform.position, transform.lossyScale / 2, Vector3.up, out _hit, transform.rotation, _maxDistance);
//         if (_hit.collider && _hit.collider.CompareTag("Player"))
//         {
//              // För att få fram differensen, subtraherar vi spelarens hastighet längs med ytan med plattformens hastighet
//              var playerVelocityOnPlane = Vector3.ProjectOnPlane(_player.GetVelocity(), Vector3.up) * _player.GetVelocity().magnitude;
//              var speedDifference = (_velocity - playerVelocityOnPlane).magnitude;
//              var playerSpeedDifferenceVector = speedDifference * _velocity.normalized;
//              
//              var normalForce = Physic3D.GetNormalForce(playerSpeedDifferenceVector + _player.GetVelocity(),  Vector3.up);
//              // _player.AddForce(normalForce);
//              
//              // Om differensen av hastigheten är mindre än den statiska friktionen bör karaktären stå still på plattformen (alltså röra sig i samma hastighet som plattformen)
//              if (playerVelocityOnPlane.magnitude < staticFriction)
//              {
//                  _player.AddForce(-playerSpeedDifferenceVector);
//              }
//              // Om spelaren rör sig snabbare än den statiska friktionen 
//              else
//              {                
//                 _player.AddForce(-Physic3D.GetFriction(_player.GetVelocity(), normalForce.magnitude, dynamicFriction, staticFriction));
//              }
//         }
//     }
//     
//     void OnDrawGizmos()
//     {
//         if (_hit.collider)
//         {
//             Gizmos.color = Color.red;
//             Gizmos.DrawRay(transform.position, Vector3.up * _hit.distance);
//             Gizmos.DrawWireCube(transform.position + Vector3.up * _hit.distance, transform.lossyScale);
//         }
//         else
//         {
//             Gizmos.color = Color.green;
//             Gizmos.DrawRay(transform.position, Vector3.up * _maxDistance);
//         }
//         Debug.DrawLine(Vector3.zero, _player.GetVelocity(), Color.magenta);
//         Debug.DrawLine(Vector3.zero, Vector3.ProjectOnPlane(_player.GetVelocity(), Vector3.up) * _player.GetVelocity().magnitude, Color.yellow);
//     }
//
//
//     private void LateUpdate()
//     {
//         _velocity = (_previousPosition - transform.position) / Time.deltaTime;
//     }
// }