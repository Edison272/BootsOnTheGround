using System.Collections;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    
    public void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.transform.gameObject);
    }
    public void OnTriggerExit2D(Collider2D collision)
    {
        
    }
}
