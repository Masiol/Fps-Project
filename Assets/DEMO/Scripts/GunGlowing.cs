using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunGlowing : MonoBehaviour
{
    public bool startGlowing;
    private float strengthGlowing;
    public Material material;
    void Start()
    {
        strengthGlowing = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        
        if(startGlowing)
        {
            strengthGlowing += 5 * Time.deltaTime * 2;
            if(strengthGlowing > 30)
            {
                strengthGlowing = 30;
            }
        }
        if(!startGlowing)
        {
            strengthGlowing -= 5 * Time.deltaTime * 2;
            if(strengthGlowing < 0)
            {
                strengthGlowing = 0;
            }
        }
        material.SetFloat("_Strength", strengthGlowing);
    }
}
