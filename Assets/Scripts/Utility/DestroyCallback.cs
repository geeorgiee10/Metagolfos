using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyCallback : MonoBehaviour
{
    //This whole script existing is dumb. X2
    public void Hello()
    {
        Destroy(transform.parent.gameObject);

    }
}
