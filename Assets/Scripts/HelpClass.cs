using UnityEngine;

public static class HelpClass
{
    public static Vector3 GetNormalForce(Vector3 velocity, Vector3 normal)
    {
        var dotProduct = Vector3.Dot(velocity, normal);
        var projection = (dotProduct > 0 ? 0 : dotProduct) * normal;
        return -projection;
    }
}
