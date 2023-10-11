using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    //Destroy itself after 5 seconds
    void Start()
    {
        Destroy(gameObject, 5);
    }
}
