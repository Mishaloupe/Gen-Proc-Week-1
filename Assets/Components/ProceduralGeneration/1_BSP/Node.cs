using UnityEngine;

public class Node
{
    public RectInt rect;
    public Node? child1;
    public Node? child2;

    public Node(RectInt rect)
    {
        this.rect = rect;
    }
    public Node(RectInt rect, Node child1, Node child2)
    {
        this.rect = rect;
        this.child1 = child1;
        this.child2 = child2;
    }
}
