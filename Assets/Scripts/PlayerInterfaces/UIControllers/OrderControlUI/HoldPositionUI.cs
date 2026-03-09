using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldPositionUI : MonoBehaviour
{
    [SerializeField] LineRenderer line_render;
    public Transform character_marker;
    public Transform target_marker;
    [SerializeField] SpriteRenderer[] color_components;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPositions(Vector2 start_position, Vector2 end_position)
    {
        // line
        Vector2 end_vec_offset = (start_position - end_position).normalized * (target_marker.localScale.x + 0.1f);

        line_render.SetPosition(0, start_position);
        line_render.SetPosition(1, end_position + end_vec_offset);

        character_marker.position = start_position;
        target_marker.position = end_position;
    }

    public void SetUIAlpha(float alpha)
    {
        foreach(SpriteRenderer set_alpha in color_components)
        {
            Color sa_c = set_alpha.color;
            set_alpha.color = new Color(sa_c.r, sa_c.b, sa_c.g, alpha);
        }

        Color lr_sc = line_render.startColor, lr_ec = line_render.endColor;
        line_render.startColor = new Color(lr_sc.r,lr_sc.g,lr_sc.b, alpha);
        line_render.endColor = new Color(lr_ec.r,lr_ec.g,lr_ec.b, alpha);
    }
}
