using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
    [Tooltip("Extra space for UI on iPads (Try 3 or 4)")]
    public float extraVerticalPadding = 3f; 
    
    // PUBLIC so Dots can read it and stay centered
    public Vector2 centerOffset; 

    [Header("Prefabs")]
    public GameObject tilePrefab;
    public GameObject[] dots;
    public GameObject explosionFX; 
    
    [Header("UI & Audio")]
    public TextMeshProUGUI movesText;
    public GameObject pausePanel; 
    public ScoreManager scoreManager;
    public EndGameManager endManager; 
    public AudioClip popSound;     
    public int scorePerDot = 20;

    [Header("Combo Animation")]
    [Tooltip("Starting speed for the first pop (0.4s)")]
    public float basePopDelay = 0.4f; 
    [Tooltip("How much faster it gets per combo (0.7 = 30% faster)")]
    public float popAcceleration = 0.7f; 
    [Tooltip("The fastest it can possibly go")]
    public float minPopDelay = 0.05f;

    // References
    private AudioSource audioSource;
    private CameraShake cameraShake;
    private HintManager hintManager; 

    // State
    public GameObject[,] allDots;
    public GameObject[,] allTiles; // NEW: Track background tiles for Ice
    public GameState currentState = GameState.move;
    
    // Input
    private GameControls gameControls; 
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private bool isSwiping = false;
    private Dot currentlySelectedDot;

    private void Awake() {
        gameControls = new GameControls();
        currentLevelIndex = PlayerPrefs.GetInt("CurrentLevel", 0);

        if(levels != null && currentLevelIndex < levels.Length) {
            LevelData data = levels[currentLevelIndex];
            width = data.width;
            height = data.height;
            movesLeft = data.moves;
            levelGoal = data.scoreGoal;
        } else {
            width = 6; height = 8; movesLeft = 20; levelGoal = 1000;
        }

        allDots = new GameObject[width, height];
        allTiles = new GameObject[width, height]; // Initialize Tile Array
        
        if(scoreManager == null) scoreManager = FindFirstObjectByType<ScoreManager>();
        
        audioSource = GetComponent<AudioSource>();
        if(audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        
        cameraShake = Camera.main.GetComponent<CameraShake>();
        hintManager = FindFirstObjectByType<HintManager>();
    }
    
    private void OnEnable() { gameControls.Enable(); }
    private void OnDisable() { gameControls.Disable(); }

    void Start () { 
        if(pausePanel != null) pausePanel.SetActive(false);
        UpdateMovesText();
        Setup(); 
    }

    void Update() {
        if (currentState == GameState.pause || currentState == GameState.win || currentState == GameState.lose || currentState == GameState.wait) return;

        if (gameControls.Gameplay.Fire.WasPerformedThisFrame()) {
            if(hintManager != null) hintManager.ResetTimer(); 
            
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
        }
    }

    private void Setup() {
        centerOffset = new Vector2((width - 1) / 2f, (height - 1) / 2f);
        Camera.main.transform.position = new Vector3(0, 0, -10f);

        float verticalSize = (height / 2f) + borderPadding + extraVerticalPadding;
        float horizontalSize = ((width / 2f) + borderPadding) / Camera.main.aspect;
        Camera.main.orthographicSize = Mathf.Max(verticalSize, horizontalSize);

        if(boardBackground != null) {
            boardBackground.transform.position = new Vector3(0, 0, 5f); 
            SpriteRenderer sr = boardBackground.GetComponent<SpriteRenderer>();
            if(sr != null && sr.drawMode == SpriteDrawMode.Sliced) {
                sr.size = new Vector2(width + borderPadding, height + borderPadding);
            } else {
                boardBackground.transform.localScale = new Vector3(width + borderPadding, height + borderPadding, 1);
            }
            if(sr) sr.sortingLayerName = "Board"; 
        }

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Vector2 tempPosition = new Vector2(x - centerOffset.x, y - centerOffset.y);
                
                // --- NEW ICE LOGIC ---
                GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;
                backgroundTile.transform.parent = this.transform;
                backgroundTile.name = $"( {x}, {y} )";
                backgroundTile.GetComponent<SpriteRenderer>().sortingLayerName = "Board";
                
                // Add the script dynamically (so you don't have to edit the prefab)
                BackgroundTile bgScript = backgroundTile.GetComponent<BackgroundTile>();
                if (bgScript == null) bgScript = backgroundTile.AddComponent<BackgroundTile>();

                int hp = 0;
                // Check level data for ice
                if(levels != null && currentLevelIndex < levels.Length && levels[currentLevelIndex].iceTiles != null) {
                    if (levels[currentLevelIndex].iceTiles.Contains(new Vector2(x, y))) {
                        hp = 1; 
                    }
                }
                bgScript.Setup(hp);
                allTiles[x, y] = backgroundTile;
                // ---------------------

                int dotToUse = Random.Range(0, dots.Length);
                int maxIterations = 0;
                while(MatchesAt(x, y, dots[dotToUse]) && maxIterations < 100) {
                    dotToUse = Random.Range(0, dots.Length);
                    maxIterations++;
                }

                GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                dot.transform.parent = this.transform;
                dot.name = $"Animal ( {x}, {y} )";
                dot.GetComponent<Dot>().Setup(x, y, this);
                allDots[x, y] = dot;
                dot.GetComponent<SpriteRenderer>().sortingLayerName = "Units";
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
    
    private void UpdateMovesText() { if(movesText != null) movesText.text = "Moves: " + movesLeft; }

    private IEnumerator DestroyMatchesCo() {
        float currentDelay = basePopDelay;

        yield return new WaitForSeconds(0.1f);
        
        bool matchesExist = true;
        while (matchesExist) {
            
            DestroyMatchesAt();
            yield return new WaitForSeconds(currentDelay);
            
            DecreaseRow();
            RefillBoard();
            
            yield return new WaitForSeconds(currentDelay);

            // ACCELERATE
            currentDelay = Mathf.Max(minPopDelay, currentDelay * popAcceleration);

            matchesExist = false;
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    if (allDots[i, j] != null) {
                        Dot d = allDots[i, j].GetComponent<Dot>();
                        d.FindMatches(); 
                        if (d.isMatched) matchesExist = true;
                    }
                }
            }
        }
        
        if (scoreManager.score >= levelGoal) {
            currentState = GameState.win;
            if(endManager != null) endManager.ShowWin(scoreManager.score, levelGoal);
            
            int unlockedLevels = PlayerPrefs.GetInt("UnlockedLevel", 1);
            if (currentLevelIndex + 1 >= unlockedLevels) {
                PlayerPrefs.SetInt("UnlockedLevel", currentLevelIndex + 2);
                PlayerPrefs.Save();
            }
        } 
        else if (movesLeft <= 0) {
            currentState = GameState.lose;
            if(endManager != null) endManager.ShowLose(scoreManager.score);
        } 
        else {
            currentState = GameState.move;
        }
    }

    private void DestroyMatchesAt() {
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (allDots[i, j] != null && allDots[i, j].GetComponent<Dot>().isMatched) {
                    
                    // --- BREAK ICE ---
                    if (allTiles[i, j] != null) {
                        BackgroundTile bg = allTiles[i, j].GetComponent<BackgroundTile>();
                        if (bg != null && bg.hitPoints > 0) {
                            bg.TakeDamage(1);
                        }
                    }
                    // -----------------

                    if(scoreManager != null) scoreManager.IncreaseScore(scorePerDot);
                    if(explosionFX != null) Instantiate(explosionFX, allDots[i, j].transform.position, Quaternion.identity);
                    if(cameraShake != null) StartCoroutine(cameraShake.Shake(0.15f, 0.05f));
                    
                    Destroy(allDots[i, j]);
                    allDots[i, j] = null;
                }
            }
        }
        if(popSound != null) audioSource.PlayOneShot(popSound);
    }
    
    private void DecreaseRow() {
        for (int x = 0; x < width; x++) {
            int nullCount = 0;
            for (int y = 0; y < height; y++) {
                if (allDots[x, y] == null) nullCount++;
                else if (nullCount > 0) {
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
                    Vector2 tempPosition = new Vector2(x - centerOffset.x, y + 2 - centerOffset.y); 
                    int dotToUse = Random.Range(0, dots.Length);
                    GameObject piece = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    piece.transform.parent = this.transform;
                    piece.name = $"Animal ( {x}, {y} )";
                    piece.GetComponent<Dot>().Setup(x, y, this);
                    allDots[x, y] = piece;
                    piece.GetComponent<SpriteRenderer>().sortingLayerName = "Units";
                }
            }
        }
    }

    public void PauseGame() {
        if(currentState == GameState.move) {
            currentState = GameState.pause;
            if(pausePanel != null) pausePanel.SetActive(true);
            Time.timeScale = 0f; 
        }
    }

    public void ResumeGame() {
        if(currentState == GameState.pause) {
            currentState = GameState.move;
            if(pausePanel != null) pausePanel.SetActive(false);
            Time.timeScale = 1f; 
        }
    }

    public void RestartGame() { 
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
    }

    public void GoToMenu() { 
        Time.timeScale = 1f; 
        SceneManager.LoadScene("MainMenu"); 
    }

    public void LoadNextLevel() {
        Time.timeScale = 1f;
        int nextIndex = currentLevelIndex + 1;
        if (levels != null && nextIndex >= levels.Length) nextIndex = 0; 
        
        PlayerPrefs.SetInt("CurrentLevel", nextIndex);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public List<GameObject> CheckForMatches() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (allDots[x, y] != null) {
                    if (x < width - 1) if (SwitchAndCheck(x, y, Vector2.right)) return new List<GameObject> { allDots[x, y], allDots[x + 1, y] };
                    if (y < height - 1) if (SwitchAndCheck(x, y, Vector2.up)) return new List<GameObject> { allDots[x, y], allDots[x, y + 1] };
                }
            }
        }
        return null;
    }
    private bool SwitchAndCheck(int column, int row, Vector2 direction) {
        SwitchPieces(column, row, direction);
        bool hasMatch = false;
        if (CheckConnection(column, row) || CheckConnection(column + (int)direction.x, row + (int)direction.y)) hasMatch = true;
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