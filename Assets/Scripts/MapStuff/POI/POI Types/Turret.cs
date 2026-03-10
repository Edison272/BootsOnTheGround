using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MinorPOI, IInteractable
{
    [Header("UI")]
    public GameObject InteractPrompt;

    [Header("Functionality")]
    public int characters_in_range;
    public CharacterSO turret;
    public Character active_turret;
    public Character controller; 
    public float curr_build_time = 0;
    public float max_build_time = 10;

    private float og_character_size;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        Character entry_character = collision.gameObject.GetComponent<Character>();
        if(entry_character.faction_tag == GameOverseer.SQUAD_TAG)
        {
            characters_in_range++;
        }
    }
    public void OnTriggerExit2D(Collider2D collision)
    {
        Character entry_character = collision.gameObject.GetComponent<Character>();
        if(entry_character.faction_tag == GameOverseer.SQUAD_TAG)
        {
            characters_in_range--;
        }
    }
    public string GetPromptText()
    {
        return curr_build_time < max_build_time ? "Build Turret" : "Reinforce Turret";
    }

    public void Interact(Character character)
    {
        controller = character;
    }

    public void ToggleInteractPrompt(bool is_enabled)
    {
        InteractPrompt.SetActive(is_enabled);
    }

    // Start is called before the first frame update
    void Start()
    {
        ToggleInteractPrompt(false);
        active_turret = turret.GenerateChar(transform.position);
        curr_build_time = 0;

        float character_size = og_character_size * curr_build_time / max_build_time;
        active_turret.transform.localScale = new Vector3(character_size, character_size, character_size);
    }

    // Update is called once per frame
    void Update()
    {
        if (characters_in_range > 0)
        {
            if (curr_build_time < max_build_time)
            {
                curr_build_time += Time.deltaTime;
                float character_size = og_character_size * curr_build_time / max_build_time;
                active_turret.transform.localScale = new Vector3(character_size, character_size, character_size);
                if (curr_build_time > max_build_time)
                {
                    curr_build_time = max_build_time;
                }
            }
        }
    }
}
