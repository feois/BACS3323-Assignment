using static System.Single;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Grid2DTile : MonoBehaviour {
    private InputAction pointAction;
    private InputAction clickAction;

    public MeshRenderer cube;
    public GameObject obstacle;
    public TextMeshPro text;
    public Material unselectedMaterial;
    public Material selectedMaterial;
    public Grid2D grid;
    public Vector2Int index;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pointAction = InputSystem.actions.FindAction("Point");
        clickAction = InputSystem.actions.FindAction("Attack");
    }

    // Update is called once per frame
    void Update()
    {
        var ray = Camera.main.ScreenPointToRay(pointAction.ReadValue<Vector2>());

        if (Physics.Raycast(ray, out var hit) && hit.collider.gameObject == cube.gameObject) {
            cube.material = selectedMaterial;
            grid.selectedTile = this;

            if (clickAction.triggered) grid.graph.ToggleObstacle(index);
        }
        else {
            cube.material = unselectedMaterial;
        }
        
        obstacle.SetActive(grid.graph.IsObstacle(index));
        
        var cost = grid.graph.Cost(index);
        var lookahead = grid.graph.Lookahead(index);
        var costString = IsPositiveInfinity(cost) ? "∞" : $"{cost:F2}";
        var lookaheadString = IsPositiveInfinity(lookahead) ? "∞" : $"{lookahead:F2}";

        if (!grid.showCostAndLookahead) costString = lookaheadString = "";
        
        text.text = $"{costString}\n{lookaheadString}\n{index.x}, {index.y}";
    }
}
