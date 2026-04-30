using UnityEngine;

[System.Serializable]
public class GravityDirection
{
    // Esto aparecerá como un desplegable en el Inspector de Unity
    public DirectionG dir;

    public GravityDirection(DirectionG dir)
    {
        this.dir = dir;
    }

    // Esta función devuelve el Vector3 correspondiente
    public Vector3 GetVector()
    {
        switch (dir)
        {
            case DirectionG.UP:      return Vector3.up;
            case DirectionG.DOWN:    return Vector3.down;
            case DirectionG.LEFT:    return Vector3.left;
            case DirectionG.RIGHT:   return Vector3.right;
            case DirectionG.FORWARD: return Vector3.forward;
            case DirectionG.BACK:    return Vector3.back;
            default:                return Vector3.down; 
        }
    }
}

public enum DirectionG
{
    UP,
    DOWN,
    RIGHT,
    LEFT,
    FORWARD,
    BACK
}
