using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectRotation : MonoBehaviour
{
    void Update()
    {
        gameObject.transform.Rotate(1, 1, 1);
    }
}