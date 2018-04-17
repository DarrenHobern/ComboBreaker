using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HealthDisplayScript : MonoBehaviour {

    [SerializeField]
    private Sprite fullHeartImage;
    [SerializeField]
    private Sprite halfHeartImage;

    private int health = 10;
    private Image[] healthImages;

    // Functions -------------------------
    private void Awake()
    {
        healthImages = GetComponentsInChildren<Image>();
        if(healthImages == null)
        {
            Debug.LogError("Health Images not found");
            return;
        }
    }

    public void SetHealth(int hp)
    {
        health = Mathf.Clamp(hp, 0, healthImages.Length*2);
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {  

        int healthCutoff = Mathf.FloorToInt(health / 2);
        for (int i = 0; i < healthCutoff; i++)
        {
            healthImages[i].sprite = fullHeartImage;
            healthImages[i].color = Color.white;
        }
        for (int i = healthCutoff; i < healthImages.Length; i++)
        {
            healthImages[i].color = Color.clear;
        }
        // If it is an odd number of health, show a half heart at the cutoff
        if (health % 2 != 0)
        {
            healthImages[healthCutoff].sprite = halfHeartImage;
            healthImages[healthCutoff].color = Color.white;
        }

    }
}
