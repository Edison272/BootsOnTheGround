using System;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.Utilities;

// handles setting the player target and determining critical hit
[Serializable]
public class TargettingController
{
    RaycastHit2D[] contacts;
    LineRenderer main_line_render;
    LineRenderer vfx_line_render;
    bool target_allies;
    public float alignment = 1;

    public RectTransform canvas_cursor;
    public TargettingController(LineRenderer main_line_render, LineRenderer vfx_line_render)
    {
        this.main_line_render = main_line_render;
        this.vfx_line_render = vfx_line_render;
    }

    // get a physics linecast to determine active character target
    public void UpdateTargetting(Character active_character)
    {
        SetTargettingMode(active_character);
        
        Vector2 target_pos = active_character.GetPosition() + active_character.aim_dir;
        Vector2 source_pos = active_character.GetPosition() + active_character.aim_dir.normalized * (active_character.hitbox_radius+0.1f);
        contacts = Physics2D.LinecastAll(source_pos, target_pos);
        float alpha = 0.5f;
        Color end_color = PlayerController.base_color;
        Color start_color = PlayerController.origin_color;
        float width_mult = 0f;
        if (contacts.Length > 0)
        {
            foreach(RaycastHit2D contact in contacts)
            {
                if (contact.transform.gameObject.tag == "Terrain")
                {
                    target_pos = contact.point;
                    alpha = 0.3f;
                    end_color = PlayerController.wall_color;
                    start_color = PlayerController.wall_color;
                    break;
                }
                Character contact_char = contact.transform.gameObject.GetComponent<Character>();
                if (contact_char && target_allies == (active_character.faction_tag == contact_char.faction_tag))
                {
                    target_pos = contact.point;
                    // Vector2 hit_dir = (contact.transform.GetComponent<Rigidbody2D>().centerOfMass - target_pos).normalized;
                    // Vector2 center_dir = (active_character.GetPosition() - target_pos).normalized;
                    // alignment = Vector2.Dot(hit_dir, center_dir);                
                    
                    alpha = 0.75f * (alignment);
                    width_mult = 0.5f * (alignment);
                    end_color =  alignment > 0.5f ? PlayerController.crit_color : PlayerController.base_color;
                    start_color = alignment > 0.5f ? PlayerController.crit_color : PlayerController.base_color;
                    
                    break;
                }
            }
        }
        
        Vector2 offset_y = new Vector3(0, active_character.main_item.y_offset);
        SetLRPositions(0, source_pos + offset_y, source_pos + offset_y);
        SetLRPositions(1, target_pos + offset_y, target_pos + offset_y);
        SetVFXLRColor(start_color, end_color, vfx_line_render.startColor.a, alpha);
        vfx_line_render.widthMultiplier = width_mult;
    }
    void SetLRPositions(int index, Vector2 main_position, Vector2 vfx_position)
    {
        main_line_render.SetPosition(index, main_position);
        vfx_line_render.SetPosition(index, vfx_position);
    }
    void SetVFXLRColor(Color start_color, Color end_color, float start_alpha, float end_alpha, float vfx_alpha_mod = 0.3f)
    {
        // main line/laser
        main_line_render.startColor = new Color(start_color.r,start_color.g,start_color.b, start_alpha);
        main_line_render.endColor = new Color(end_color.r,end_color.g,end_color.b, end_alpha);

        // extra flash
        vfx_line_render.startColor = new Color(start_color.r,start_color.g,start_color.b, start_alpha * 0.5f);
        vfx_line_render.endColor = new Color(end_color.r,end_color.g,end_color.b, end_alpha * 0.5f);

    }
    private void SetTargettingMode(Character active_character)
    {
        target_allies = (active_character.main_item.base_data.item_type != ItemType.Weapon);
    }
}