using UnityEngine;

public class PhysicsComponent : MonoBehaviour
{
    private Rigidbody _rigidBody;
    private Vector3 _previousPosition;
    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        _previousPosition = transform.position;
    }
    

    private void LateUpdate()
    {
        _rigidBody.velocity = (_previousPosition - transform.position) / Time.deltaTime;
    }
}
