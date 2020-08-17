using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder
{
    private class Node
    {
        public Node parent;
        public List<Node> children;
        public Vector2Int value;

        public Node(Node parentIn, Vector2Int valueIn)
        {
            parent = parentIn;
            value = valueIn;
            children = new List<Node>();
        }
    }

    Node root = null;
    Dictionary<Vector2Int, Node> lookup;
    private ushort rows;
    private ushort columns;

    public Pathfinder(ushort rows, ushort columns)
    {
        this.rows = rows;
        this.columns = columns;
        lookup = new Dictionary<Vector2Int, Node>();
    }

    public List<Vector2Int> FindPath(ref Vector2Int src, ref Vector2Int dst)
    {
        // We'll do a BFS for now....
        // Later, we'll want to do a goofy pathfinding that derps on purpose or something,
        // to create interesting paths.

        root = new Node(null, src);
        var leafNode = Traverse(root, ref dst);

        List<Vector2Int> outVal = new List<Vector2Int>();

        var curNode = leafNode;
        while (curNode != null)
        {
            outVal.Add(curNode.value);
            curNode = curNode.parent;
        }

        outVal.Reverse();
        return outVal;
    }

    private Node Traverse(Node node, ref Vector2Int target)
    {
        if (target.x == node.value.x 
            && target.y == node.value.y)
        {
            return node;
        }

        var toUp = new Vector2Int(node.value.x, node.value.y+1);
        bool canGoUp = toUp.y < rows && !lookup.ContainsKey(toUp);

        var toLeft = new Vector2Int(node.value.x-1, node.value.y);
        bool canGoLeft = toLeft.x >= 0 && !lookup.ContainsKey(toLeft);

        var toRight = new Vector2Int(node.value.x+1, node.value.y);
        bool canGoRight = toRight.x < columns && !lookup.ContainsKey(toRight);

        var toDown = new Vector2Int(node.value.x, node.value.y-1);
        bool canGoDown = toDown.y >= 0 && !lookup.ContainsKey(toDown);

        if (canGoUp)
        {
            var newChild = new Node(node, toUp);
            node.children.Add(newChild);
            lookup.Add(toUp, newChild);
        }

        if (canGoLeft)
        {
            var newChild = new Node(node, toLeft);
            node.children.Add(newChild);
            lookup.Add(toLeft, newChild);
        }

        if (canGoDown)
        {
            var newChild = new Node(node, toDown);
            node.children.Add(newChild);
            lookup.Add(toDown, newChild);
        }

        if (canGoRight)
        {
            var newChild = new Node(node, toRight);
            node.children.Add(newChild);
            lookup.Add(toRight, newChild);
        }


        foreach (var child in node.children)
        {
            var result = Traverse(child, ref target);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}
