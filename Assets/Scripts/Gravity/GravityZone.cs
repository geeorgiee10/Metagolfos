using UnityEngine;
    using System.Collections.Generic;

public class GravityZone : MonoBehaviour
{
    private List<GravitableObject> affectedObjects = new List<GravitableObject>();
    public DirectionG dir;
    public Renderer[] gravityFieldRenderers;
    public Transform parent;
    public bool mantainGravity;

    public bool isActive = true;

    void Start()
    {
        UpdateMaterials();
    }

    public void ToggleActive()
    {
        isActive = !isActive;

        if (!isActive)
        {
            DisableZoneEffects();
        }
        else
        {
            ApplyGravityToObjectsInside(); // 👈 esto es lo que te falta
        }

        UpdateMaterials();
    }

    private void ApplyGravityToObjectsInside()
    {
        Collider[] colliders = Physics.OverlapBox(
            transform.position,
            transform.localScale / 2f,
            transform.rotation
        );

        foreach (var col in colliders)
        {
            var gravObj = col.GetComponent<GravitableObject>();
            if (gravObj != null)
            {
                gravObj.ChangeGravity(GetVectorFromEnum(dir));
                gravObj.useLocalGravity = true;

                if (!affectedObjects.Contains(gravObj))
                    affectedObjects.Add(gravObj);

                if (parent != null)
                    gravObj.transform.SetParent(parent);
            }
        }
    }

    private void DisableZoneEffects()
{
    foreach (var gravObj in affectedObjects)
    {
        if (gravObj == null) continue;

        if (!mantainGravity)
        {
            gravObj.ResetToWorldGravity();
            gravObj.useLocalGravity = false;
        }

        if (parent != null)
            gravObj.transform.SetParent(null);
    }

    affectedObjects.Clear();
}

    public void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        var gravObj = other.GetComponent<GravitableObject>();
        if (gravObj != null)
        {
            gravObj.ChangeGravity(GetVectorFromEnum(dir));

            if (!affectedObjects.Contains(gravObj))
                affectedObjects.Add(gravObj);

            if(parent != null)
                other.transform.SetParent(parent);
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (!isActive) return;

        var gravObj = other.GetComponent<GravitableObject>();
        if (gravObj != null)
            gravObj.useLocalGravity = true;
    }

    public void OnTriggerExit(Collider other)
    {
        var gravObj = other.GetComponent<GravitableObject>();
        if (gravObj != null)
        {
            affectedObjects.Remove(gravObj);
            if (!mantainGravity)
            {
                gravObj.ResetToWorldGravity();
                gravObj.useLocalGravity = false;
            }

            if(parent != null)
                other.transform.SetParent(null);
        }
    }

    public void UpdateMaterials()
    {
        if (!isActive)
        {
            foreach (Renderer renderer in gravityFieldRenderers)
            {
                renderer.material.SetColor("_BaseColor", Color.gray);
                renderer.material.SetColor("_EmissionColor", Color.gray);
            }
        }
        else
        {
            Color c = GetColorFromEnum(dir);

            foreach (Renderer renderer in gravityFieldRenderers)
            {
                renderer.material.SetColor("_BaseColor", c);
                renderer.material.SetColor("_EmissionColor", c * 2f);
                renderer.material.EnableKeyword("_EMISSION");
            }
        }
    }

    private Vector3 GetVectorFromEnum(DirectionG d) { 
        switch (d)
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

    private Color GetColorFromEnum(DirectionG d) { 
        switch (d)
        {
            case DirectionG.UP:      return Color.yellow;
            case DirectionG.DOWN:    return Color.white;
            case DirectionG.LEFT:    return Color.green;
            case DirectionG.RIGHT:   return Color.blue;
            case DirectionG.FORWARD: return Color.red;
            case DirectionG.BACK:    return new Color(1f, .5f, 0f);
            default:                return Color.white; 
        }
    }
}