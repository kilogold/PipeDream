using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class NodePortComparer : IEqualityComparer<List<Vector2Int>>
{
    public bool Equals(List<Vector2Int> x, List<Vector2Int> y)
    {
        if (x.Count != y.Count)
            return false;

        for (int i = 0; i < x.Count; i++)
        {
            if (x[i] != y[i])
                return false;
        }

        return true;
    }

    public int GetHashCode(List<Vector2Int> obj)
    {
        int code = 1;

        foreach (Vector2Int item in obj)
        {
            code <<= 2;
            code |= item.GetHashCode();
        }

        return code;
    }
}

public class GridManager : MonoBehaviour
{
    public GameObject pipePrefabHorizontal;
    public GameObject pipePrefabL;
    public GameObject pipePrefabStart;
    public GameObject pipePrefabEnd;

    /// <summary>
    /// Mapping between port configurations and associated pipe prefabs.
    /// This could be abstracted a bit more if need be.
    /// </summary>
    private Dictionary<List<Vector2Int>, GameObject> lookup;
    private Dictionary<GameObject, GridData.Node> pipeGridInstances;
    private Dictionary<Vector2Int, GameObject> pipeInstanceMap;

    [SerializeField]
    private GridData gridData;

    void Awake()
    {
        gridData.OnInitializedEvent += GenerateGrid;

        lookup = new Dictionary<List<Vector2Int>, GameObject>(new NodePortComparer()) 
        {
            { new List<Vector2Int>(){ new Vector2Int(1,0), new Vector2Int(-1,0) }, pipePrefabHorizontal },
            { new List<Vector2Int>(){ new Vector2Int(0,1), new Vector2Int(1,0) }, pipePrefabL },
            { new List<Vector2Int>(){ new Vector2Int(1,0) }, pipePrefabStart },
            { new List<Vector2Int>(){ new Vector2Int(-1,0) }, pipePrefabEnd }
        };
    }

    public void GenerateGrid()
    {
        int gridRows = gridData.data.GetLength(1);
        int gridColumns = gridData.data.GetLength(0);

        pipeGridInstances = new Dictionary<GameObject, GridData.Node>(gridData.data.Length);
        pipeInstanceMap = new Dictionary<Vector2Int, GameObject>(gridData.data.Length);

        // Can choose any sprite for sizing. They should all be the same size dimensions.
        SpriteRenderer pipeSprite = pipePrefabStart.GetComponent<SpriteRenderer>();
        float spriteWidth = pipeSprite.bounds.size.x;
        float spriteHeight = pipeSprite.bounds.size.y;
        for (int iCol = 0; iCol < gridColumns; iCol++)
        {
            for (int iRow = 0; iRow < gridRows; iRow++)
            {
                var node = gridData.data[iCol, iRow];
                if (node == null)
                    continue;

                var instance = InstantiateFromNode(spriteWidth, spriteHeight, iCol, iRow, node);

                pipeGridInstances.Add(instance, node);
                pipeInstanceMap.Add(new Vector2Int(iCol, iRow), instance);
            }
        }

        // Offset root of grid to bottom-left corner of screen.
        float gridHalfWidth = (gridColumns * spriteWidth) / 2.0f;
        float gridHalfHeight = (gridRows * spriteHeight) / 2.0f;
        transform.position -= new Vector3(gridHalfWidth, gridHalfHeight, 0);
        transform.position = Vector3Int.CeilToInt(transform.position); // Rounding hack.
    }

    private GameObject InstantiateFromNode(float spriteWidth, float spriteHeight, int iCol, int iRow, GridData.Node node)
    {
        var newPos = new Vector2(spriteWidth * iCol, spriteHeight * iRow);
        var rot = Quaternion.AngleAxis(node.orientation, Vector3.forward);

        GameObject prefab = null;
        if(node.hackOriginalPortOrientation == null)
        {
            prefab = lookup[node.ports];
        }
        else
        {
            prefab = lookup[node.hackOriginalPortOrientation];
        }

        var instance = Instantiate(prefab, newPos, rot, transform);

        var userInput = instance.GetComponent<UserInput>();
        if (userInput)
        {
            userInput.OnNodeInteraction += OnNodeInteraction;
            userInput.OnAnimationCompleted += OnPipeAnimFilled;
        }

        return instance;
    }

    private void OnNodeInteraction(GameObject obj)
    {
        if (!enabled)
            return;

        var node = pipeGridInstances[obj];
        node.Rotate(true);
        obj.transform.rotation =  Quaternion.AngleAxis(node.orientation, Vector3.forward);

        var path = gridData.EvaluateStartToEndPath();
        if (path == null)
            return;

        enabled = false;

        StartCoroutine(FillPipes(path));

        // TODO:
        // Signal a toast manager.
    }


    private bool isFillingPipes = false;
    IEnumerator FillPipes(List<Vector2Int> path)
    {
        //Start case:
        {
            GameObject pipe = pipeInstanceMap[path[0]];
            pipe.GetComponent<Animator>().enabled = true;
            isFillingPipes = true;

            yield return new WaitUntil(() => !isFillingPipes);
        }

        // Rest case:
        for (int i = 1; i < path.Count - 2; ++i)
        {
            GameObject pipe = pipeInstanceMap[path[i]];
            GameObject fillObj = pipe.transform.GetChild(0).gameObject;

            
            bool shouldInvert = false;



            fillObj.GetComponent<AnimationCallback>().OnPlayAnimation(shouldInvert);
            isFillingPipes = true;

            yield return new WaitUntil(() => !isFillingPipes);
        }
    }

    private void OnPipeAnimFilled(GameObject go)
    {
        isFillingPipes = false;
    }

}
