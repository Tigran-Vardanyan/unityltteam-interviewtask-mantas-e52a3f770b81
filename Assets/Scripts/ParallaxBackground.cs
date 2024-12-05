using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] private Renderer background1; 
    [SerializeField] private Renderer background2; 
    [SerializeField] private float speed1 = 0.1f;  
    [SerializeField] private float speed2 = 0.05f; 

    private Vector2 offset1 = Vector2.zero;
    private Vector2 offset2 = Vector2.zero;

    private void Update()
    {
        offset1.y += speed1 * Time.deltaTime;
        offset2.y += speed2 * Time.deltaTime;
        
        background1.material.mainTextureOffset = offset1;
        background2.material.mainTextureOffset = offset2;
    }
}
