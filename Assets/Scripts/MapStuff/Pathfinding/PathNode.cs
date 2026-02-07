using UnityEngine;

public class PathNode
{
    public Vector2 position;
    public int weight;

    public PathNode(Vector2 position)
    {
        this.position = position;
    }
}