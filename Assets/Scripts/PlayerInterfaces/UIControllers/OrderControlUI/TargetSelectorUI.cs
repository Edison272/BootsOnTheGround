using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TargetSelectorUI : MonoBehaviour
{
    public Transform prompt;
    public TextMeshPro prompt_text;
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

    public void SetScale(float scale)
    {
        target_marker.localScale = new Vector3(scale, scale, scale);
    }
    public void SetPosition(Vector2 position)
    {
        this.transform.position = position;
    }

    public void SetUIAlpha(float alpha)
    {
        foreach(SpriteRenderer set_alpha in color_components)
        {
            Color sa_c = set_alpha.color;
            set_alpha.color = new Color(sa_c.r, sa_c.b, sa_c.g, alpha);
        }
        Color pt_c = prompt_text.color;
        prompt_text.color = new Color(pt_c.r, pt_c.b, pt_c.g, alpha > 0.01f ? 1: 0);
    }

    public void SetText(string new_prompt)
    {
        prompt_text.text = new_prompt;
    }
}
