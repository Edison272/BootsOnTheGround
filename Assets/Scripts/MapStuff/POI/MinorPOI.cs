using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinorPOI : MonoBehaviour
{
    public MinorPOI GetNewMinorPOI(Vector2 position)
    {
        return GameObject.Instantiate(gameObject, (Vector2)position, Quaternion.identity).GetComponent<MinorPOI>();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void Destroy()
    {
        Destroy(this.gameObject);
    }
}
