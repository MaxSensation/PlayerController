using UnityEngine;

public static class HelpClass
{
    public static Vector3 GetNormalForce(Vector3 velocity, Vector3 normal)
    {
        var dotProduct = Vector3.Dot(velocity, normal);
        var projection = (dotProduct > 0 ? 0 : dotProduct) * normal;
        return -projection;
    }

    public static Vector3 GetFriction(Vector3 velocity, float normalForceMagnitude, float dynamicFrictionCoefficient, float staticFrictionCoefficient)
    { 
        if (velocity.magnitude < normalForceMagnitude * staticFrictionCoefficient) return Vector3.zero;
        return velocity.normalized * (normalForceMagnitude * dynamicFrictionCoefficient);
    }

    public static float GetAirResistant(float airResistant)
    {
        return Mathf.Pow(1 - airResistant, Time.deltaTime);
    }

    public static Vector3 GetAcceleration(Vector3 velocity, float accelerationSpeedCoefficient)
    {
        return velocity * (accelerationSpeedCoefficient * Time.deltaTime);
    }

    public static float GetTurnVelocity(Vector3 forces, Vector3 velocity)
    {
        return Mathf.Lerp(0.1f, 0.4f, Vector3.Dot(forces.normalized, velocity.normalized));
    }

    public static Vector3 GetGravity(float gravityCoefficient)
    {
        return Vector3.down * (gravityCoefficient * Time.deltaTime);
    }
}
