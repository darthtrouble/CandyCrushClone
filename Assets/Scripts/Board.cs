using UnityEngine;
using UnityEngine.InputSystem; // REQUIRED for New Input

public class Board : MonoBehaviour {

    public int width = 6;
    public int height = 8;
    public GameObject tilePrefab;
    public GameObject[] dots;
    public GameObject[,] allDots;
    
    // INPUT VARIABLES
    private GameControls gameControls; // Reference to the C# class we generated
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private bool isSwiping = false;
    private Dot currentlySelectedDot;

    private void Awake() {
        gameControls = new GameControls();
        allDots = new GameObject[width, height];
    }
    
    private void OnEnable() {
        gameControls.Enable();
    }
    
    private void OnDisable() {
        gameControls.Disable();
    }

    void Start () {
        Setup();
    }

    // LISTENING FOR INPUT IN UPDATE
    void Update() {
        // Did we just press the button/screen?
        if (gameControls.Gameplay.Fire.WasPerformedThisFrame()) {
            // Read the position
            Vector2 mousePos = gameControls.Gameplay.Point.ReadValue<Vector2>();
            firstTouchPosition = Camera.main.ScreenToWorldPoint(mousePos);
            
            // RAYCAST: Shoot a laser at that position to see what we hit
            RaycastHit2D hit = Physics2D.Raycast(firstTouchPosition, Vector2.zero);
            
            if(hit.collider != null && hit.collider.GetComponent<Dot>()) {
                // We hit a dot! Remember it.
                currentlySelectedDot = hit.collider.GetComponent<Dot>();
                isSwiping = true;
            }
        }
        
        // Did we just let go?
        if (gameControls.Gameplay.Fire.WasReleasedThisFrame() && isSwiping) {
            isSwiping = false;
            if(currentlySelectedDot != null) {
                Vector2 mousePos = gameControls.Gameplay.Point.ReadValue<Vector2>();
                finalTouchPosition = Camera.main.ScreenToWorldPoint(mousePos);
                CalculateAngle();
            }
        }
    }

    void CalculateAngle() {
        if(Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > .5f || Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > .5f) {
            float swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
            
            // Tell the DOT to move
            currentlySelectedDot.CalculateMove(swipeAngle);
            currentlySelectedDot = null; // Forget the dot so we don't move it again by accident
        }
    }

    private void Setup() {
        // (This code remains exactly the same as Phase 3)
        // ... include your existing Setup() code here ...
        // One tiny change:
        // Inside the loop, add: dot.GetComponent<Dot>().Setup(x, y, this);
        // ...
        
        // Copy your existing Setup function here, but make sure to 
        // initialize the Dot script properly when you spawn it.
         for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Vector2 tempPosition = new Vector2(x, y);
                GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;
                backgroundTile.transform.parent = this.transform;
                backgroundTile.name = "( " + x + ", " + y + " )";
                
                int dotToUse = Random.Range(0, dots.Length);
                int maxIterations = 0;
                while(MatchesAt(x, y, dots[dotToUse]) && maxIterations < 100) {
                    dotToUse = Random.Range(0, dots.Length);
                    maxIterations++;
                }

                GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                dot.transform.parent = this.transform;
                dot.name = "Animal ( " + x + ", " + y + " )";
                
                // INITIALIZE THE DOT
                dot.GetComponent<Dot>().Setup(x, y, this);
                
                allDots[x, y] = dot;
            }
        }
    }
    
    private bool MatchesAt(int column, int row, GameObject piece) {
        // (Keep this the same as before)
        if(column > 1 && allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag) return true;
        if(row > 1 && allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag) return true;
        return false;
    }
}