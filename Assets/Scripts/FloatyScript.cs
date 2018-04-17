using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatyScript : MonoBehaviour {

    public float heliumAmount = 2f;
    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        rb.AddForce(Vector3.up * heliumAmount);
    }
}
