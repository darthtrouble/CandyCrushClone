using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Required for Lists
using UnityEngine.InputSystem;
using TMPro; 
using UnityEngine.SceneManagement; 

public enum GameState {
    wait,
    move,
    win,
    lose,
    pause
}

public class Board : MonoBehaviour {

    [Header("Level Configuration")]
    public LevelData[] levels; 
    private int currentLevelIndex = 0;

    [HideInInspector] public int width;
    [HideInInspector] public int height;
    
    private int movesLeft;
    private int levelGoal;
    
    [Header("Board Styling")]
    public GameObject boardBackground; 
    public float borderPadding = 1f;
    public Vector3 cameraOffset; 
    
    [Header("Prefabs")]
    public GameObject tilePrefab;
    public GameObject[] dots;
    
    [Header("VFX & Audio")]
    public GameObject explosionFX; 
    public AudioClip popSound;     
    public float shakeMagnitude = 0.05f; 
    public float shakeDuration = 0.15f;
    
    private AudioSource audioSource;
    private CameraShake cameraShake;
    private HintManager hintManager; 

    [Header("Score")]
    public ScoreManager scoreManager;
    public int scorePerDot = 20;

    [Header("UI References")]
    public TextMeshProUGUI movesText;
    public GameObject endPanel;
    public TextMeshProUGUI endText;
    public GameObject pausePanel;

    public GameObject[,] allDots;
    public GameState currentState = GameState.move;
    
    private GameControls gameControls; 
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private bool isSwiping = false;
    private Dot currentlySelectedDot;

    private void Awake() {
        gameControls = new GameControls();
        
        currentLevelIndex = PlayerPrefs.GetInt("CurrentLevel", 0);
        if (currentLevelIndex >= levels.Length) currentLevelIndex = 0; 

        if(levels.Length > 0) {
            LevelData data = levels[currentLevelIndex];
            width = data.width;
            height = data.height;
            movesLeft = data.moves;
            levelGoal = data.scoreGoal;
        } else {
            width = 6; height = 8; movesLeft = 20; levelGoal = 1000;
        }

        allDots = new GameObject[width, height];
        
        if (scoreManager == null) scoreManager = FindFirstObjectByType<ScoreManager>();
        
        audioSource = GetComponent<AudioSource>();
        if(audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        
        cameraShake = Camera.main.GetComponent<CameraShake>();
        hintManager = FindFirstObjectByType<HintManager>();
    }
    
    private void OnEnable() { gameControls.Enable(); }
    private void OnDisable() { gameControls.Disable(); }

    void Start () { 
        if(endPanel != null) endPanel.SetActive(false);
        if(pausePanel != null) pausePanel.SetActive(false);
        UpdateMovesText();
        Setup(); 
    }

    void Update() {
        if (currentState == GameState.wait || currentState == GameState.win || currentState == GameState.lose || currentState == GameState.pause) return;

        if (gameControls.Gameplay.Fire.WasPerformedThisFrame()) {
            if(hintManager != null) hintManager.ResetTimer(); // Reset hint on click

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
        Camera.main.transform.position = new Vector3((width - 1) / 2f, (height - 1) / 2f, -10f) + cameraOffset;
        
        if(boardBackground != null) {
            boardBackground.transform.position = new Vector3((width - 1) / 2f, (height - 1) / 2f, -5f);
            SpriteRenderer sr = boardBackground.GetComponent<SpriteRenderer>();
            if(sr != null && sr.drawMode == SpriteDrawMode.Sliced) {
                sr.size = new Vector2(width + borderPadding, height + borderPadding);
            } else {
                boardBackground.transform.localScale = new Vector3(width + borderPadding, height + borderPadding, 1);
            }
        }

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
        
        if (scoreManager.score >= levelGoal) {
            currentState = GameState.win;
            if(endPanel != null) {
                endPanel.SetActive(true);
                endText.text = "YOU WIN!";
            }
            if(currentLevelIndex + 1 > PlayerPrefs.GetInt("UnlockedLevel", 0)) {
                PlayerPrefs.SetInt("UnlockedLevel", currentLevelIndex + 1);
                PlayerPrefs.SetInt("CurrentLevel", currentLevelIndex + 1); 
            }
            CheckHighScore();
        } 
        else if (movesLeft <= 0) {
            currentState = GameState.lose;
             if(endPanel != null) {
                endPanel.SetActive(true);
                endText.text = "TRY AGAIN";
            }
            CheckHighScore();
        } 
        else {
            currentState = GameState.move;
        }
    }

    void CheckHighScore() {
        int currentHighScore = PlayerPrefs.GetInt("HighScore", 0);
        if(scoreManager.score > currentHighScore) {
            PlayerPrefs.SetInt("HighScore", scoreManager.score);
            PlayerPrefs.Save();
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
            
            if(explosionFX != null) {
                Instantiate(explosionFX, allDots[column, row].transform.position, Quaternion.identity);
                if(cameraShake != null) StartCoroutine(cameraShake.Shake(shakeDuration, shakeMagnitude));
            }
            
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
    
    public void RestartGame() {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void PauseGame() {
        currentState = GameState.pause;
        if(pausePanel != null) pausePanel.SetActive(true);
        Time.timeScale = 0f; 
    }
    
    public void ResumeGame() {
        currentState = GameState.move;
        if(pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f; 
    }
    
    public void GoToMenu() {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("MainMenu");
    }

    // ---------------- HINT SYSTEM LOGIC ----------------

    // NOW RETURNS A PAIR (List) OF DOTS
    public List<GameObject> CheckForMatches() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (allDots[x, y] != null) {
                    // Check Right Swap
                    if (x < width - 1) {
                        if (SwitchAndCheck(x, y, Vector2.right)) {
                            // Return the TWO pieces that need to swap
                            return new List<GameObject> { allDots[x, y], allDots[x + 1, y] };
                        }
                    }
                    // Check Up Swap
                    if (y < height - 1) {
                        if (SwitchAndCheck(x, y, Vector2.up)) {
                            // Return the TWO pieces that need to swap
                            return new List<GameObject> { allDots[x, y], allDots[x, y + 1] };
                        }
                    }
                }
            }
        }
        return null;
    }

    private bool SwitchAndCheck(int column, int row, Vector2 direction) {
        SwitchPieces(column, row, direction);
        bool hasMatch = false;
        if (CheckConnection(column, row) || CheckConnection(column + (int)direction.x, row + (int)direction.y)) {
            hasMatch = true;
        }
        SwitchPieces(column, row, direction);
        return hasMatch;
    }

    private void SwitchPieces(int column, int row, Vector2 direction) {
        GameObject holder = allDots[column + (int)direction.x, row + (int)direction.y];
        allDots[column + (int)direction.x, row + (int)direction.y] = allDots[column, row];
        allDots[column, row] = holder;
    }

    private bool CheckConnection(int column, int row) {
        if (allDots[column, row] == null) return false;
        
        if (column > 1 && allDots[column - 1, row].tag == allDots[column, row].tag && allDots[column - 2, row].tag == allDots[column, row].tag) return true;
        if (column < width - 2 && allDots[column + 1, row].tag == allDots[column, row].tag && allDots[column + 2, row].tag == allDots[column, row].tag) return true;
        if (column > 0 && column < width - 1 && allDots[column - 1, row].tag == allDots[column, row].tag && allDots[column + 1, row].tag == allDots[column, row].tag) return true;

        if (row > 1 && allDots[column, row - 1].tag == allDots[column, row].tag && allDots[column, row - 2].tag == allDots[column, row].tag) return true;
        if (row < height - 2 && allDots[column, row + 1].tag == allDots[column, row].tag && allDots[column, row + 2].tag == allDots[column, row].tag) return true;
        if (row > 0 && row < height - 1 && allDots[column, row - 1].tag == allDots[column, row].tag && allDots[column, row + 1].tag == allDots[column, row].tag) return true;

        return false;
    }
}