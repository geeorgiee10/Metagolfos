using UnityEngine;

public static class GravityManager
{
    public static Vector3 worldGravityDir = Vector3.down;
    public static float gravityForce = 9.81f*2;

    public static Vector3 GetGravity()
    {
        return worldGravityDir * gravityForce;
    }

    public static void ChangeWorldGravity(Vector3 direction)
    {
        worldGravityDir = direction.normalized;
    }

    public static void InvertGravity() => worldGravityDir = worldGravityDir*(-1);

    
    public static Color GetColorFromGravity(Vector3 d)
    {
        if (d == Vector3.up) return Color.yellow;
        if (d == Vector3.down) return Color.white;
        if (d == Vector3.left) return Color.green;
        if (d == Vector3.right) return Color.blue;
        if (d == Vector3.forward) return Color.red;
        if (d == Vector3.back) return new Color(1f, .5f, 0f);

        return Color.white;
    }
}
