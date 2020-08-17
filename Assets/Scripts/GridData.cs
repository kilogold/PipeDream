using System;
using System.Collections.Generic;
using UnityEngine;

public class GridData : MonoBehaviour
{
    public class Node
    {
        /// <summary>
        /// Connections available.
        /// </summary>
        public List<Vector2Int> ports = new List<Vector2Int>();

        /// <summary>
        /// Rotation in degrees. Should change in 90-degree increments.
        /// </summary>
        public int orientation { get; private set; }

        public List<Vector2Int> hackOriginalPortOrientation { get; private set; } = null;

        public void Rotate(bool clockwise, byte iterations = 1)
        {
            if(orientation == 0 && hackOriginalPortOrientation == null)
            {
                hackOriginalPortOrientation = new List<Vector2Int>(ports);
            }

            const short max_iter = 360;

            int rotAmount = (clockwise ? -1 : 1) * (90 * iterations);
            orientation += rotAmount;

            
            for (int i = 0; i < ports.Count; i++)
            {
                var result = 
                    Quaternion.AngleAxis(rotAmount, Vector3.forward) * 
                    new Vector2(ports[i].x, ports[i].y);
                ports[i] = new Vector2Int(Mathf.RoundToInt(result.x), Mathf.RoundToInt(result.y));
            }

            //if(ports.Count > 1) Debug.Log($"O: {orientation} | A: <{ports[0].x},{ports[0].y}> B:| <{ports[1].x},{ports[1].y}>");
            orientation %= max_iter;
            //Debug.Log(orientation);
        }

        public static Vector2 GetRotationAlignedPort(Node node, Vector2Int port)
        {
            var result = Quaternion.AngleAxis(node.orientation, Vector3.forward) * new Vector2(port.x, port.y);
            return result;
        }
    };

    public ushort rows;
    public ushort columns;
    public Node[,] data;
    public Pathfinder pathfinder;
    public event System.Action OnInitializedEvent;

    [SerializeField]
    private Vector2Int startCoords;
    [SerializeField]
    private Vector2Int endCoords;

    private void Start()
    {
        Initialize(columns, rows);
    }

    public void Initialize(ushort columns, ushort rows)
    {
        this.columns = columns;
        this.rows = rows;

        data = new Node[columns,rows];
        pathfinder = new Pathfinder((ushort)data.GetLength(1), (ushort)data.GetLength(0));

        List<Vector2Int> guaranteedPath = GenerateStartToEndPath();

        GenerateNodesAlongPath(guaranteedPath);

        ShufflePathNodes();

        FillGridWithRandomNodes();

        OnInitializedEvent?.Invoke();
    }

    private Node GetNode(Vector2Int coords)
    {
        return data[coords.x, coords.y];
    }

    private bool IsWithinBounds(Vector2Int coords)
    {
        if (coords.x < 0) return false;
        if (coords.x >= columns) return false;
        if (coords.y < 0) return false;
        if (coords.y >= rows) return false;

        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="aOutPort"></param>
    /// <param name="b"></param>
    /// <param name="connectingPort"> Returns the port not facing the connection.</param>
    /// <returns></returns>
    private bool DoPortsConnect(Node a, Vector2Int aOutPort, Node b, out Vector2Int connectingPort)
    {
        if(aOutPort == -b.ports[0])
        {
            connectingPort = b.ports[0];
            return true;
        }

        if(aOutPort == -b.ports[1])
        {
            connectingPort = b.ports[1];
            return true;
        }

        connectingPort = Vector2Int.zero;
        return false;
    }

    /// <summary>
    /// Checks to see if there is a viable path between start and end.
    /// This is the indicator that the puzzle is solved.
    /// The logic executed pathfinding to identify any routes leading to the end.
    /// </summary>
    /// <returns></returns>
    public List<Vector2Int> EvaluateStartToEndPath()
    {
        Node endNode = GetNode(endCoords);
        Node curNode = GetNode(startCoords);
        Vector2Int curTvsl = startCoords;
        Vector2Int curOutPort = curNode.ports[0]; // Start/End Node only has one port.

        List<Vector2Int> path = new List<Vector2Int>(1) { startCoords }; //Todo: make member to avoid re-alloc

        int debugLoop = 0;
        while (curNode != endNode)
        {
            if (++debugLoop >= 100)
                return null;

            curNode = GetNode(curTvsl);

            // Traverse to next node.
            curTvsl += curOutPort;
            path.Add(curTvsl);

            // Check for grid bounds
            if (!IsWithinBounds(curTvsl))
                return null;

            // Get node found at current output port.
            Node nextNode = GetNode(curTvsl);

            // Check if current node's out-port connects to any of the adjacent's node's ports.
            if (!DoPortsConnect(curNode, curOutPort, nextNode, out var connectingPort))
            {
                return null;
            }

            // Designate the next node ports as our current.
            // We will update the 'curNode' reference at the start of the next
            // loop iteration.
            // Port may be facing where we came from (backwards).
            // If so, flip 
            curOutPort = GetOutputPort(nextNode, connectingPort);
        }
        return path;
    }

    /// <summary>
    /// Identifies the corresponding output for the node, provided an input node.
    /// Logic assumes 1:1 input/output ports on nodes.
    /// </summary>
    /// <param name="curNode"></param>
    /// <param name="curInPort"></param>
    /// <returns></returns>
    private Vector2Int GetOutputPort(Node curNode, Vector2Int curInPort)
    {
        // Special case: Start/End
        if (curNode.ports.Count == 1)
            return curNode.ports[0];

        return (curNode.ports[0] == curInPort) ? curNode.ports[1] : curNode.ports[0];
    }

    /// <summary>
    /// Attempts to fill the grid with random nodes where missing.
    /// Existing nodes are skipped; only missing nodes are generated.
    /// </summary>
    private void FillGridWithRandomNodes()
    {
        for (int i = 0; i < data.GetLength(0); i++)
        {
            for (int j = 0; j < data.GetLength(1); j++)
            {
                if(data[i,j] == null)
                {
                    data[i, j] = GenerateRandomNode();
                }
            }
        }
    }

    /// <summary>
    /// Rotates nodes in an arbitrary fashion in an attempt to conceal the guaranteed winning path.
    /// </summary>
    private void ShufflePathNodes()
    {

    }

    private void GenerateNodesAlongPath(List<Vector2Int> path)
    {
        {
            Node startNode = new Node();
            startNode.ports.Add(new Vector2Int(1, 0));
            var startCoords = path[0];
            data[startCoords.x, startCoords.y] = startNode;
        }
        {
            Node endNode = new Node();
            endNode.ports.Add(new Vector2Int(-1, 0));
            var endCoords = path[path.Count-1];
            data[endCoords.x, endCoords.y] = endNode;
        }

        for (int i = 0; i < path.Count-1; i++)
        {
            //BUG: bounds check.
            GenerateSegmentNode(path[i], path[i + 1]);
        }
    }

    /// <summary>
    /// Generates a source node with ports connecting to the destination node.
    /// </summary>
    /// <param name="src"></param>
    /// <param name="dst"></param>
    /// <returns></returns>
    private Node GenerateSegmentNode(Vector2Int src, Vector2Int dst)
    {
        return GenerateRandomNode();// BUG/HACK
    }

    private Node GenerateRandomNode()
    {
        var outNode = new Node();
        GenerateRandomPortConfig(ref outNode.ports);

        byte iter = (byte)UnityEngine.Random.Range(0, 3);
        outNode.Rotate(true, iter);
        return outNode;
    }

    private void GenerateRandomPortConfig(ref List<Vector2Int> ports)
    {
        // TODO: Have data-driven definition of different kind of nodes.
        // HACK: Hard-code for now.
        if (ports.Count > 0)
            ports.Clear();

        var rand = UnityEngine.Random.Range(0, 2);

        if(rand == 0) // Horizontal Straight
        {
            ports.Add(new Vector2Int(1, 0));
            ports.Add(new Vector2Int(-1, 0));
        }
        else if(rand == 1) // L-Corner
        {
            ports.Add(new Vector2Int(0, 1));
            ports.Add(new Vector2Int(1, 0));
        }
    }

    public List<Vector2Int> GenerateStartToEndPath()
    {
        //startCoords = new Vector2Int() { x = 0, y = 0 }; //Vector2Int.Random();
        //endCoords = new Vector2Int(2, 0); //Vector2Int.Random();
        if (startCoords == endCoords)
        {
            throw new System.NotImplementedException();
        }

        return pathfinder.FindPath(ref startCoords, ref endCoords); ;
    }
}