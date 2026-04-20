using static System.Single;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Grid2DTile : MonoBehaviour {
    private InputAction pointAction;
    private InputAction obstacleAction;

    public MeshRenderer cube;
    public GameObject obstacle;
    public TextMeshPro text, xText, yText;
    public Material unselectedMaterial;
    public Material selectedMaterial;
    public GameObject visionMask;
    
    public Grid2D grid;
    public Vector2Int index;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pointAction = InputSystem.actions.FindAction("Point");
        obstacleAction = InputSystem.actions.FindAction("Set Obstacle");
    }

    // Update is called once per frame
    void Update()
    {
        var ray = Camera.main!.ScreenPointToRay(pointAction.ReadValue<Vector2>());

        if (Physics.Raycast(ray, out var hit) && hit.collider.gameObject == cube.gameObject) {
            cube.material = selectedMaterial;
            grid.selectedTile = this;

            if (obstacleAction.triggered) {
                if (grid.graph.IsObstacle(index)) grid.obstacles.Remove((index.x, index.y));
                else grid.obstacles.Add((index.x, index.y));
                grid.graph.ToggleObstacle(index);
                grid.npcGraph?.ToggleObstacle(index);
                grid.seen.Remove((index.x, index.y));
            }
        }
        else {
            cube.material = unselectedMaterial;
        }

        obstacle.SetActive(grid.graph.IsObstacle(index));
        visionMask.SetActive(grid.searching && grid.vision && !grid.seen.Contains((index.x, index.y)));

        if (grid.searching && !grid.astar) {
            var cost = grid.npcGraph!.Cost(index);
            var lookahead = grid.npcGraph.Lookahead(index);
            
            text.text = $"{(IsPositiveInfinity(cost) ? "∞" : $"{cost:F2}")}\n{(IsPositiveInfinity(lookahead) ? "∞" : $"{lookahead:F2}")}";
        }
        else text.text = "";
    }
}
