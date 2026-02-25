using UnityEngine;

public class PlayerActionController : MonoBehaviour
{
    [Header("Action Seelctor UI")]
    public RectTransform action_selector;
    public RectTransform selector_base;
    public RectTransform action_indicator;
    private Vector2 selection_vector = Vector2.zero;
    public Vector2 converted_selection_vector = Vector2.zero;
    [SerializeField]private float indicator_range = 0;
}