using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthPlayer : MonoBehaviour
{
    public float currentHealth;
    private float maxHealth = 100f;

    public Slider healthBar;
    public GameObject hitScreen; 

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.value = currentHealth;
        healthBar.maxValue = maxHealth;

        var color = hitScreen.GetComponent<Image>().color;
        color.a = 0f;
        hitScreen.GetComponent<Image>().color = color;

    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            DoDamage(10);
        }
        if(currentHealth <= 0)
        {
            //Die
        }
        if(hitScreen != null)
        {
            if(hitScreen.GetComponent<Image>().color.a > 0)
            {
                var color = hitScreen.GetComponent<Image>().color;

                color.a -= 0.01f;
                hitScreen.GetComponent<Image>().color = color;
            }
        }
    }
    public void DoDamage(float damage)
    {
        currentHealth -= damage;
        healthBar.value = currentHealth;
        hurtPlayer();
    }
    void hurtPlayer()
    {
        var color = hitScreen.GetComponent<Image>().color;
        color.a = 0.8f;
        hitScreen.GetComponent<Image>().color = color;
    }
}
