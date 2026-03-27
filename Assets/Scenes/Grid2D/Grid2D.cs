using DStarLite;
using UnityEngine;
using UnityEngine.InputSystem;

public class Grid2D : MonoBehaviour {
    public Vector2 tileSize;
    public Vector2Int gridSize;
    public bool diagonal;
    public float speed = 1;
    
    public GameObject tilePrefab;
    public Transform goalTile;
    public Transform npc;
    
    public Vector2GridGraph graph { get; private set; }
    public Grid2DTile selectedTile;
    public bool showCostAndLookahead { get; private set; }
    
    private Vector3 nextNode;
    
    private InputAction setGoalAction, teleportNpcAction, searchAction;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        setGoalAction = InputSystem.actions["Set Goal"];
        teleportNpcAction = InputSystem.actions["Teleport NPC"];
        searchAction = InputSystem.actions["Jump"];
        
        graph = new Vector2GridGraph(transform.position, tileSize, gridSize, diagonal);
        
        for (var x = 0; x < gridSize.x; x++)
        for (var y = 0; y < gridSize.y; y++) {
            var tileObject = Instantiate(tilePrefab, new Vector3(x, 0, y), Quaternion.identity, transform);
            var tile = tileObject.GetComponent<Grid2DTile>();

            tile.grid = this;
            tile.index = new Vector2Int(x, y);
        }
        
        transform.localScale = new Vector3(tileSize.x, 1, tileSize.y);
    }

    // Update is called once per frame
    void Update()
    {
        if (setGoalAction.triggered && selectedTile && !graph.searching) goalTile.position = selectedTile.transform.position;

        if (teleportNpcAction.triggered && selectedTile && !graph.searching) npc.position = selectedTile.transform.position;

        if (searchAction.triggered) {
            if (graph.searching) StopSearch();
            else StartSearch();

            npc.position = nextNode = graph.ToPosition(graph.currentNode);
        }

        if (graph.searching) {
            if (Mathf.Approximately(Vector3.Distance(npc.position, nextNode), 0)) {
                if (graph.NextPosition(out var node)) nextNode = node;
                else Debug.Log("No path");
            }
            else {
                npc.position = Vector3.MoveTowards(npc.position, nextNode, Time.deltaTime * speed);
            }
        }
    }

    void StartSearch() {
        graph.SearchPathPosition(npc.position, goalTile.position);
        showCostAndLookahead = true;
    }

    void StopSearch() {
        graph.EndSearch();
        showCostAndLookahead = false;
    }
}
