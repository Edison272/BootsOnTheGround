using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FollowPlayerZoneUI : MonoBehaviour
{
    public SpriteRenderer[] color_components;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetUIAlpha(float alpha)
    {
        foreach(SpriteRenderer set_alpha in color_components)
        {
            Color sa_c = set_alpha.color;
            set_alpha.color = new Color(sa_c.r, sa_c.b, sa_c.g, alpha);
        }
    }
    public float GetAlpha()
    {
        return color_components[0].color.a;
    }
}
