using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using TMPro; // Needed for UI
using UnityEngine.SceneManagement; // Needed for Restarting

public enum GameState {
    wait,
    move,
    win,
    lose
}

public class Board : MonoBehaviour {

    [Header("Board Dimensions")]
    public int width = 6;
    public int height = 8;
    
    [Header("Prefabs")]
    public GameObject tilePrefab;
    public GameObject[] dots;
    
    [Header("VFX & Audio")]
    public GameObject explosionFX; 
    public AudioClip popSound;     
    private AudioSource audioSource;

    [Header("Score & Goals")]
    public ScoreManager scoreManager;
    public int scorePerDot = 20;
    public int levelGoal = 1000;
    public int movesLeft = 20;

    [Header("UI References")]
    public TextMeshProUGUI movesText;
    public GameObject endPanel;
    public TextMeshProUGUI endText;

    public GameObject[,] allDots;
    public GameState currentState = GameState.move;
    
    private GameControls gameControls; 
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private bool isSwiping = false;
    private Dot currentlySelectedDot;

    private void Awake() {
        gameControls = new GameControls();
        allDots = new GameObject[width, height];
        
        if (scoreManager == null) scoreManager = FindFirstObjectByType<ScoreManager>();
        
        audioSource = GetComponent<AudioSource>();
        if(audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }
    
    private void OnEnable() { gameControls.Enable(); }
    private void OnDisable() { gameControls.Disable(); }

    void Start () { 
        // Ensure UI is set up correctly at start
        if(endPanel != null) endPanel.SetActive(false);
        UpdateMovesText();
        Setup(); 
    }

    void Update() {
        // Stop input if game is over or waiting
        if (currentState == GameState.wait || currentState == GameState.win || currentState == GameState.lose) return;

        if (gameControls.Gameplay.Fire.WasPerformedThisFrame()) {
            Vector2 mousePos = gameControls.Gameplay.Point.ReadValue<Vector2>();
            firstTouchPosition = Camera.main.ScreenToWorldPoint(mousePos);
            
            RaycastHit2D hit = Physics2D.Raycast(firstTouchPosition, Vector2.zero);
            if(hit.collider != null && hit.collider.GetComponent<Dot>()) {
                currentlySelectedDot = hit.collider.GetComponent<Dot>();
                isSwiping = true;
            }
        }
        
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
            currentState = GameState.wait;
            currentlySelectedDot.CalculateMove(swipeAngle);
            currentlySelectedDot = null;
        } else {
            currentState = GameState.move;
        }
    }

    private void Setup() {
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
                dot.GetComponent<Dot>().Setup(x, y, this);
                allDots[x, y] = dot;
            }
        }
    }
    
    private bool MatchesAt(int column, int row, GameObject piece) {
        if(column > 1 && allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag) return true;
        if(row > 1 && allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag) return true;
        return false;
    }

    public void DestroyMatches() {
        // A match happened! Deduct a move.
        movesLeft--;
        UpdateMovesText();
        
        StartCoroutine(DestroyMatchesCo());
    }
    
    private void UpdateMovesText() {
        if(movesText != null) movesText.text = "Moves: " + movesLeft;
    }

    private IEnumerator DestroyMatchesCo() {
        yield return new WaitForSeconds(.25f);
        
        bool matchesExist = true;
        while (matchesExist) {
            DestroyMatchesAt();
            yield return new WaitForSeconds(.25f);

            DecreaseRow();
            RefillBoard();
            yield return new WaitForSeconds(.25f);

            matchesExist = false;
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    if (allDots[i, j] != null) {
                        Dot d = allDots[i, j].GetComponent<Dot>();
                        d.FindMatches(); 
                        if (d.isMatched) {
                            matchesExist = true;
                        }
                    }
                }
            }
        }
        
        // CHECK WIN/LOSE CONDITION
        if (scoreManager.score >= levelGoal) {
            currentState = GameState.win;
            if(endPanel != null) {
                endPanel.SetActive(true);
                endText.text = "YOU WIN!";
            }
        } 
        else if (movesLeft <= 0) {
            currentState = GameState.lose;
             if(endPanel != null) {
                endPanel.SetActive(true);
                endText.text = "TRY AGAIN";
            }
        } 
        else {
            currentState = GameState.move;
        }
    }

    private void DestroyMatchesAt() {
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (allDots[i, j] != null) {
                    DestroyMatchesAt(i, j);
                }
            }
        }
        if(popSound != null) audioSource.PlayOneShot(popSound);
    }

    private void DestroyMatchesAt(int column, int row) {
        if (allDots[column, row].GetComponent<Dot>().isMatched) {
            if(scoreManager != null) scoreManager.IncreaseScore(scorePerDot);
            if(explosionFX != null) Instantiate(explosionFX, allDots[column, row].transform.position, Quaternion.identity);
            
            Destroy(allDots[column, row]);
            allDots[column, row] = null;
        }
    }

    private void DecreaseRow() {
        for (int x = 0; x < width; x++) {
            int nullCount = 0;
            for (int y = 0; y < height; y++) {
                if (allDots[x, y] == null) {
                    nullCount++;
                } else if (nullCount > 0) {
                    allDots[x, y].GetComponent<Dot>().row -= nullCount;
                    allDots[x, y - nullCount] = allDots[x, y];
                    allDots[x, y] = null;
                }
            }
        }
    }

    private void RefillBoard() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (allDots[x, y] == null) {
                    Vector2 tempPosition = new Vector2(x, y + 2); 
                    int dotToUse = Random.Range(0, dots.Length);
                    GameObject piece = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    piece.transform.parent = this.transform;
                    piece.name = "Animal ( " + x + ", " + y + " )";
                    piece.GetComponent<Dot>().Setup(x, y, this);
                    allDots[x, y] = piece;
                }
            }
        }
    }
    
    // PUBLIC FUNCTION TO RESTART GAME
    public void RestartGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}