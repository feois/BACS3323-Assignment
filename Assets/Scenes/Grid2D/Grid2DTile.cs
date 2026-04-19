using static System.Single;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Grid2DTile : MonoBehaviour {
    private InputAction pointAction;
    private InputAction clickAction;

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
        clickAction = InputSystem.actions.FindAction("Attack");
    }

    // Update is called once per frame
    void Update()
    {
        var ray = Camera.main!.ScreenPointToRay(pointAction.ReadValue<Vector2>());

        if (Physics.Raycast(ray, out var hit) && hit.collider.gameObject == cube.gameObject) {
            cube.material = selectedMaterial;
            grid.selectedTile = this;

            if (clickAction.triggered) {
                grid.graph.ToggleObstacle(index);
                grid.seen.Remove((index.x, index.y));
            }
        }
        else {
            cube.material = unselectedMaterial;
        }

        obstacle.SetActive(grid.graph.IsObstacle(index));

        if (grid.npcGraph != null) {
            var cost = grid.npcGraph.Cost(index);
            var lookahead = grid.npcGraph.Lookahead(index);
            var costString = IsPositiveInfinity(cost) ? "∞" : $"{cost:F2}";
            var lookaheadString = IsPositiveInfinity(lookahead) ? "∞" : $"{lookahead:F2}";

            text.text = $"{costString}\n{lookaheadString}";

            visionMask.SetActive(grid.vision && !grid.seen.Contains((index.x, index.y)));
        }
        else {
            text.text = "";
            visionMask.SetActive(false);
        }
    }
}
