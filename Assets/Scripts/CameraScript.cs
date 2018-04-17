using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {

    private const int SEED = 42;
    public float magnitude = 0.1f; // The amount the trauma is increased by each incident.
    public float dampening = 0.001f;  // The amount the trauma is decreased by on each frame.
    public float power = 3f;  // The exponent that we raise the trauma value to, to get the shake amount.

    public float maxYaw = 10f;
    public float maxPitch = 10f;
    public float maxRoll = 10f;

    private float trauma = 0f; // The linear sliding value of shakeyness
    private float shake = 0f;  // the polynomial actual shake amount

    private void Start()
    {
        //InduceTrauma(3);
    }

    private void Update()
    {
        if (trauma > 0)
        {
            ScreenShake();
            trauma -= dampening;
            if (trauma < 0)
                trauma = 0;
        }
        else
        {
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }

    /// <summary>
    /// Induces trauma on the camera causing or increasing the shaking amount.
    /// </summary>
    /// <param name="amt">The amt is multiplied by the magnitude which is then added to the current trauma.</param>
    public void InduceTrauma(float amt)
    {
        trauma += amt*magnitude;
    }

    public void ScreenShake()
    {
        
        // Determine the shake amount by squaring the trauma value
        shake = Mathf.Pow(trauma, power);
        // Then find the amount we're moving in for pitch, yall, roll
        float pitch = maxPitch * shake * Random.Range(-1, 2);
        float yaw = maxYaw * shake * Random.Range(-1, 2);
        float roll = maxRoll * shake * Random.Range(-1, 2);

        transform.localRotation = Quaternion.Euler(pitch, yaw, roll);
    }
}
