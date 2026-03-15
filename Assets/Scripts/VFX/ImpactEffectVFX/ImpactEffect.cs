using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class ImpactEffect : MonoBehaviour
{
    public GameObject main_body;
    public GameObject vfx_body;
    public ImpactEffect AbsolutePrefab;
    [SerializeField] float max_impact_time = 0.3f;
    float curr_impact_time;
    void Awake()
    {
        Reset();
    }

    public void Reset()
    {
        main_body.SetActive(false);
    }

    public void Update()
    {
        if (curr_impact_time > 0)
        {
            curr_impact_time -= Time.deltaTime;
            if (curr_impact_time <= 0)
            {
                ImpactEffect.AddToPool(this);
            }
        }
    }

    public void SetVFX(Vector2 vfx_position, Vector2 direction, float scale_modifier)
    {
        curr_impact_time = max_impact_time;
        main_body.transform.localScale = AbsolutePrefab.main_body.transform.localScale * scale_modifier;

        vfx_body.transform.position = vfx_position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        vfx_body.transform.rotation = Quaternion.Euler(0, 0, angle);

        
        main_body.SetActive(true);
    }

    #region Singleton Stuff
    private static Dictionary<ImpactEffect, List<GameObject>> impact_effect_pool = new Dictionary<ImpactEffect, List<GameObject>>();
    
    public static void ResetPool()
    {
        impact_effect_pool.Clear();
    }
    public static void AddToPool(ImpactEffect discard_effect)
    {
        discard_effect.main_body.SetActive(false);
        impact_effect_pool[discard_effect.AbsolutePrefab].Add(discard_effect.gameObject);
    }
    public static void StartImpact(
        ImpactEffect instance_type, 
        Vector2 impact_main_position, 
        Vector2 impact_vfx_position, 
        Vector2 impact_direction, 
        float scale_modifier = 1)
    {
        if (impact_effect_pool.ContainsKey(instance_type) && impact_effect_pool[instance_type].Count > 0)
        {
            List<GameObject> object_pool = impact_effect_pool[instance_type];
            int last_index = object_pool.Count-1;
            ImpactEffect impact_instance = object_pool[last_index].GetComponent<ImpactEffect>();
            object_pool.RemoveAt(last_index);
            impact_instance.transform.position = impact_main_position;
            impact_instance.SetVFX(impact_main_position + impact_vfx_position, impact_direction, scale_modifier);
        }
        else
        {
            ImpactEffect impact_instance = GameObject.Instantiate(instance_type, impact_main_position, Quaternion.identity).GetComponent<ImpactEffect>();
            impact_instance.AbsolutePrefab = instance_type;
            impact_instance.SetVFX(impact_main_position + impact_vfx_position, impact_direction, scale_modifier);
            impact_effect_pool[instance_type] = new List<GameObject>();
        }

    }

    #endregion
}
