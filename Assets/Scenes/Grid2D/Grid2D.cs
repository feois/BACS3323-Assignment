using System.Collections.Generic;
using System.Linq;
using DStarLite;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Grid2D : MonoBehaviour {
    public Vector2 tileSize;
    public Vector2Int gridSize;
    public float speed = 1;
    
    public GameObject tilePrefab;
    public Transform goalTile;
    public Animator npc;
    public TextMeshProUGUI status, visionText, diagonalText;
    
    public Vector2GridGraph graph { get; private set; }
    public Vector2GridGraph npcGraph { get; private set; }
    public Grid2DTile selectedTile { get; set; }
    public bool vision { get; private set; }
    private bool diagonal;
    public readonly HashSet<(int, int)> obstacles = new();
    public readonly HashSet<(int, int)> seen = new();
    
    private Vector3 target;
    private bool blocked;
    
    private InputAction goalAction, npcAction, searchAction, visionAction, diagonalAction;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        goalAction = InputSystem.actions["Set Goal"];
        npcAction = InputSystem.actions["Teleport NPC"];
        searchAction = InputSystem.actions["Search"];
        visionAction = InputSystem.actions["Toggle Vision"];
        diagonalAction = InputSystem.actions["Toggle Diagonal"];
        
        graph = new Vector2GridGraph(transform.position, tileSize, gridSize, diagonal);
        
        for (var x = 0; x < gridSize.x; x++)
        for (var y = 0; y < gridSize.y; y++) {
            var tileObject = Instantiate(tilePrefab, new Vector3(x, 0, y), Quaternion.identity, transform);
            var tile = tileObject.GetComponent<Grid2DTile>();

            tile.grid = this;
            tile.index = new Vector2Int(x, y);

            if (x == 0) {
                tile.xText.text = $"{y}";
                tile.xText.gameObject.SetActive(true);
            }

            if (y == 0) {
                tile.yText.text = $"{x}";
                tile.yText.gameObject.SetActive(true);
            }
        }
        
        transform.localScale = new Vector3(tileSize.x, 1, tileSize.y);
    }

    // Update is called once per frame
    void Update()
    {
        if (goalAction.triggered && selectedTile && npcGraph == null) goalTile.position = selectedTile.transform.position;

        if (npcAction.triggered && selectedTile && npcGraph == null) npc.transform.position = selectedTile.transform.position;

        if (searchAction.triggered) {
            if (npcGraph == null) StartSearch();
            else StopSearch();
        }

        if (visionAction.triggered && npcGraph == null) {
            vision = !vision;
            visionText.text = $"Vision [V]: {(vision ? "On" : "Off")}";
        }

        if (diagonalAction.triggered && npcGraph == null) {
            diagonal = !diagonal;
            diagonalText.text = $"Diagonal [D]: {(diagonal ? "On" : "Off")}";
        }

        if (npcGraph != null) {
            if (!blocked && npcGraph.IsObstacle(npcGraph.currentNode)) {
                blocked = true;
                npcGraph.Undo(true);
                target = npcGraph.ToPosition(npcGraph.currentNode);
            }
            
            if (Mathf.Approximately(Vector3.Distance(npc.transform.position, target), 0)) {
                blocked = false;

                if (vision) {
                    for (var x = -2; x <= 2; x++)
                    for (var y = -2; y <= 2; y++) {
                        var n = npcGraph.currentNode + new Vector3Int(x, 0, y);

                        if (graph.Validate(n)) {
                            if (graph.IsObstacle(n)) npcGraph.SetObstacle(n);
                            else npcGraph.UnsetObstacle(n);
                            seen.Add((n.x, n.z));
                        }
                    }
                }

                if (npcGraph.NextPosition(out target)) {
                    status.text =
                        $@"
                        Start: {npcGraph.startNode.x} {npcGraph.startNode.z}
                        Goal: {npcGraph.goalNode.x} {npcGraph.goalNode.z}
                        Current: {npcGraph.History().Last().x} {npcGraph.History().Last().z}
                        Target: {npcGraph.currentNode.x} {npcGraph.currentNode.z}
                        ";
                }
                else {
                    status.text = "No path";
                    StopSearch();
                }
                
                npc.SetBool("walking", false);
            }
            else {
                npc.transform.LookAt(target);
                npc.transform.position = Vector3.MoveTowards(npc.transform.position, target, Time.deltaTime * speed);
                npc.SetBool("walking", true);
            }
        }
    }

    void StartSearch() {
        npcGraph = new Vector2GridGraph(transform.position, tileSize, gridSize, diagonal);
        foreach (var (x, y) in obstacles) npcGraph.SetObstacle(new Vector3Int(x, 0, y));
        npcGraph.SearchPathPosition(npc.transform.position, goalTile.position);
        blocked = false;
        npc.transform.position = target = npcGraph.ToPosition(npcGraph.currentNode);
        seen.Clear();
    }

    void StopSearch() {
        if (npcGraph != null) {
            npc.transform.position = npcGraph.ToPosition(npcGraph.currentNode);
            npcGraph.EndSearch();
            seen.Clear();
            npcGraph = null;
        }
    }
}
