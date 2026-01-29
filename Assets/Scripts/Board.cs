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
    public float extraVerticalPadding = 3f; 
    public int offSet = 10; // Vertical spawn offset
    
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
    public GameObject floatingScorePrefab; // <--- Drag your prefab here in Inspector!

    [Header("Combo Animation")]
    public float basePopDelay = 0.4f; 
    public float popAcceleration = 0.7f; 
    public float minPopDelay = 0.05f;

    // References
    private AudioSource audioSource;
    private CameraShake cameraShake;
    private HintManager hintManager; 

    // State
    public GameObject[,] allDots;
    public GameObject[,] allTiles; 
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
        allTiles = new GameObject[width, height];
        
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
                Dot clickedDot = hit.collider.GetComponent<Dot>();
                
                // --- FIX: CHECK IF STONE ---
                // Only select it if it is NOT a stone
                if (!clickedDot.isStone) { 
                    currentlySelectedDot = clickedDot;
                    isSwiping = true;
                }
                // ---------------------------
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
                
                // --- Background Tiles (Ice) ---
                GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;
                backgroundTile.transform.parent = this.transform;
                backgroundTile.name = $"( {x}, {y} )";
                
                BackgroundTile bgScript = backgroundTile.GetComponent<BackgroundTile>();
                if (bgScript == null) bgScript = backgroundTile.AddComponent<BackgroundTile>();

                int hp = 0;
                if(levels != null && currentLevelIndex < levels.Length && levels[currentLevelIndex].iceTiles != null) {
                    if (levels[currentLevelIndex].iceTiles.Contains(new Vector2(x, y))) {
                        hp = 1; 
                    }
                }
                bgScript.Setup(hp);
                allTiles[x, y] = backgroundTile;

                // --- Dots ---
                int dotToUse = Random.Range(0, dots.Length);
                int maxIterations = 0;
                while(MatchesAt(x, y, dots[dotToUse]) && maxIterations < 100) {
                    dotToUse = Random.Range(0, dots.Length);
                    maxIterations++;
                }

                // Initial Spawn uses offset to fall in (optional) or just appear
                Vector2 spawnPos = new Vector2(x - centerOffset.x, y - centerOffset.y + offSet);
                GameObject dot = Instantiate(dots[dotToUse], spawnPos, Quaternion.identity);
                dot.transform.parent = this.transform;
                dot.name = $"Animal ( {x}, {y} )";
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

    // --- GAME LOOP ---

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
            
            // 1. Destroy and Trigger Bombs
            DestroyMatchesAt();
            yield return new WaitForSeconds(currentDelay);
            
            // 2. Physics & Refill
            DecreaseRow();
            RefillBoard();
            
            yield return new WaitForSeconds(currentDelay);

            // 3. Accelerate the loop for excitement
            currentDelay = Mathf.Max(minPopDelay, currentDelay * popAcceleration);

            // 4. Check for Chain Reactions
            // We scan the whole board. If anyone formed a new match after falling, loop again!
            matchesExist = false;
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    if (allDots[i, j] != null) {
                        Dot d = allDots[i, j].GetComponent<Dot>();
                        d.FindMatches(); // Force check
                        if (d.isMatched) matchesExist = true;
                    }
                }
            }
        }
        
        // 5. DEADLOCK CHECK
        if (IsDeadlocked()) {
            Debug.Log("Deadlock detected! Shuffling...");
            ShuffleBoard();
        }
        
        // --- WIN / LOSE CONDITIONS ---
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
        // DETONATION LOOP
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (allDots[i, j] != null) {
                    Dot dot = allDots[i, j].GetComponent<Dot>();
                    if (dot.isMatched && dot.isBomb) {
                         TriggerBomb(dot);
                    }
                }
            }
        }

        // CREATION & VISUALS LOOP
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (allDots[i, j] != null) {
                    Dot dot = allDots[i, j].GetComponent<Dot>();
                    
                    if (dot.isMatched) {
                        // With immediate stone destruction, this dot should never be a stone.

                        // --- FIX: Damage neighbors BEFORE bomb conversion ---
                        // This ensures adjacent stones are damaged even if this dot becomes a bomb.
                        DamageStone(i + 1, j); // Right
                        DamageStone(i - 1, j); // Left
                        DamageStone(i, j + 1); // Up
                        DamageStone(i, j - 1); // Down
                        // ---------------------------------------------------

                        // Create Powerups
                        if (!dot.isBomb) {
                            if (dot.isColorBomb || dot.isAreaBomb || dot.isRowBomb || dot.isColumnBomb) {
                                dot.isMatched = false;
                                dot.ActivateBombVisual();
                                continue; // Keep this dot, it's a bomb now.
                            }
                        }

                        // Damage Ice
                        if (allTiles[i, j] != null) {
                            BackgroundTile bg = allTiles[i, j].GetComponent<BackgroundTile>();
                            if (bg != null && bg.hitPoints > 0) bg.TakeDamage(1);
                        }

                        // Score & FX for destroying the dot
                        if(scoreManager != null) scoreManager.IncreaseScore(scorePerDot);

                        if (floatingScorePrefab != null) {
                            GameObject floatText = Instantiate(floatingScorePrefab, allDots[i, j].transform.position, Quaternion.identity);
                            if(floatText.GetComponent<FloatingText>() != null) {
                                floatText.GetComponent<FloatingText>().SetScore(scorePerDot);
                            }
                        }

                        if(explosionFX != null) Instantiate(explosionFX, allDots[i, j].transform.position, Quaternion.identity);
                        
                        Destroy(allDots[i, j]);
                        allDots[i, j] = null;
                    }
                }
            }
        }
        
        if(popSound != null) audioSource.PlayOneShot(popSound);
        if(cameraShake != null) StartCoroutine(cameraShake.Shake(0.15f, 0.05f));
    }

    private void DamageStone(int x, int y) {
        // Bounds Check
        if (x >= 0 && x < width && y >= 0 && y < height) {
            if (allDots[x, y] != null) {
                Dot stoneDot = allDots[x, y].GetComponent<Dot>();

                // If it exists, is a stone, and isn't already dying
                if (stoneDot.isStone && !stoneDot.isMatched) {
                    // Check if it died from this hit
                    if (stoneDot.TakeDamage(1)) {

                        // --- FIX 2: IMMEDIATE DESTRUCTION ---
                        // Don't mark isMatched=true. Just kill it NOW.
                        // This ensures it pops visually at the exact same frame as the match.

                        stoneDot.isMatched = true; // Mark logic dead

                        // 1. Spawn Score
                        if(floatingScorePrefab != null) {
                            GameObject floatText = Instantiate(floatingScorePrefab, allDots[x, y].transform.position, Quaternion.identity);
                            if(floatText.GetComponent<FloatingText>() != null) {
                                floatText.GetComponent<FloatingText>().SetScore(scorePerDot);
                            }
                        }

                        // 2. Spawn Explosion
                        if(explosionFX != null) Instantiate(explosionFX, allDots[x, y].transform.position, Quaternion.identity);

                        // 3. Add Score
                        if(scoreManager != null) scoreManager.IncreaseScore(scorePerDot);

                        // 4. Destroy Object
                        Destroy(allDots[x, y]);
                        allDots[x, y] = null; // Clear from board array immediately
                        // ------------------------------------
                    }
                }
            }
        }
    }

    // --- DOUBLE AREA BOMB SEQUENCE (Pop -> Drop -> Wait -> Pop) ---
    public IEnumerator DoubleAreaBombRoutine(int x, int y, Dot activeBomb) {
        // PASS 1: Destroy everything in 3x3 EXCEPT the active bomb
        for (int i = x - 1; i <= x + 1; i++) {
            for (int j = y - 1; j <= y + 1; j++) {
                if (i >= 0 && i < width && j >= 0 && j < height) {
                    // Check if there is a dot and it is NOT our hero bomb
                    if (allDots[i, j] != null && allDots[i, j] != activeBomb.gameObject) {
                        
                        // Destroy visuals manually
                        if(explosionFX != null) Instantiate(explosionFX, allDots[i, j].transform.position, Quaternion.identity);
                        if(scoreManager != null) scoreManager.IncreaseScore(scorePerDot);

                        // --- NEW: FLOATING TEXT ---
                        if (floatingScorePrefab != null) {
                            GameObject floatText = Instantiate(floatingScorePrefab, allDots[i, j].transform.position, Quaternion.identity);
                            floatText.GetComponent<FloatingText>().SetScore(scorePerDot);
                        }
                        // --------------------------
                        
                        Destroy(allDots[i, j]);
                        allDots[i, j] = null;
                    }
                }
            }
        }

        // Apply Gravity (Important! So new pieces fall in for the second pop)
        DecreaseRow();
        RefillBoard();

        // Wait 0.5 Seconds
        yield return new WaitForSeconds(0.5f);

        // PASS 2: Destroy 3x3 again around the bomb's NEW position
        // (It might have fallen, so we use its current column/row)
        int newX = activeBomb.column;
        int newY = activeBomb.row;

        for (int i = newX - 1; i <= newX + 1; i++) {
            for (int j = newY - 1; j <= newY + 1; j++) {
                if (i >= 0 && i < width && j >= 0 && j < height) {
                    if (allDots[i, j] != null) {
                        Dot d = allDots[i, j].GetComponent<Dot>();
                        
                        // Now we destroy everything, including the active bomb
                        if (!d.isMatched) {
                            d.isMatched = true;
                            // Chain Reaction allow
                            if (d.isBomb) TriggerBomb(d); 
                        }
                    }
                }
            }
        }
        
        // Resume normal game loop
        DestroyMatches();
    }

    // --- RECURSIVE BOMB LOGIC ---

    // --- MEGA STRIPES (3-Line Explosions) ---

    public void DestroyRowStrip(int row) {
        // Destroy Center, Above, and Below
        if (row >= 0 && row < height) DestroyRow(row);
        if (row - 1 >= 0) DestroyRow(row - 1);
        if (row + 1 < height) DestroyRow(row + 1);
    }

    public void DestroyColumnStrip(int col) {
        // Destroy Center, Left, and Right
        if (col >= 0 && col < width) DestroyColumn(col);
        if (col - 1 >= 0) DestroyColumn(col - 1);
        if (col + 1 < width) DestroyColumn(col + 1);
    }

    private void TriggerBomb(Dot dot) {
        if (dot.isRowBomb) DestroyRow(dot.row);
        if (dot.isColumnBomb) DestroyColumn(dot.column);
        if (dot.isAreaBomb) DestroyArea(dot.column, dot.row);
        // Color bomb usually manually triggered, but can be added here
    }

    private void DestroyRow(int rowToDestroy) {
        for (int i = 0; i < width; i++) {
            if (allDots[i, rowToDestroy] != null) {
                Dot dot = allDots[i, rowToDestroy].GetComponent<Dot>();
                if (!dot.isMatched) {
                    dot.isMatched = true;
                    if (dot.isBomb) TriggerBomb(dot); // Chain Reaction
                }
            }
        }
    }

    private void DestroyColumn(int colToDestroy) {
        for (int j = 0; j < height; j++) {
            if (allDots[colToDestroy, j] != null) {
                Dot dot = allDots[colToDestroy, j].GetComponent<Dot>();
                if (!dot.isMatched) {
                    dot.isMatched = true;
                    if (dot.isBomb) TriggerBomb(dot);
                }
            }
        }
    }

    private void DestroyArea(int centerCol, int centerRow) {
        for (int i = centerCol - 1; i <= centerCol + 1; i++) {
            for (int j = centerRow - 1; j <= centerRow + 1; j++) {
                if (i >= 0 && i < width && j >= 0 && j < height) {
                    if (allDots[i, j] != null) {
                        Dot dot = allDots[i, j].GetComponent<Dot>();
                        if (!dot.isMatched) {
                            dot.isMatched = true;
                            if (dot.isBomb) TriggerBomb(dot);
                        }
                    }
                }
            }
        }
    }

    public void DestroyColor(string colorTag) {
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (allDots[i, j] != null) {
                    Dot dot = allDots[i, j].GetComponent<Dot>();
                    if (allDots[i, j].tag == colorTag && !dot.isMatched) {
                        dot.isMatched = true;
                        if (dot.isBomb) TriggerBomb(dot);
                    }
                }
            }
        }
    }

    public void NukeBoard() {
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (allDots[i, j] != null) {
                    allDots[i, j].GetComponent<Dot>().isMatched = true;
                }
            }
        }
    }
    
    // --- PHYSICS ---

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
                    Vector2 tempPosition = new Vector2(x - centerOffset.x, y - centerOffset.y + offSet); 
                    
                    // 1. Pick a random dot
                    int dotToUse = Random.Range(0, dots.Length);
                    int maxIterations = 0;

                    // 2. SAFETY CHECK: loop until we find a dot that DOESN'T make a match
                    // We check MatchesAt to look left and down for existing neighbors
                    while(MatchesAt(x, y, dots[dotToUse]) && maxIterations < 100) {
                        dotToUse = Random.Range(0, dots.Length);
                        maxIterations++;
                    }

                    // 3. Create the safe dot
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

    // --- MENUS ---

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
    
    // --- DEADLOCK & HINT SYSTEM ---

    // --- HINT SYSTEM HELPER ---
    // Returns a list of the two dots that can be swapped to make a match
    public List<GameObject> CheckForMatches() {
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (allDots[i, j] != null) {
                    
                    // --- FIX 1: IGNORE STONES ---
                    // If this piece is a stone, skip it completely. 
                    // It cannot be moved, so it can never be part of a hint.
                    if (allDots[i, j].GetComponent<Dot>().isStone) continue;
                    // ---------------------------

                    // 1. Check Swap Right
                    if (i < width - 1) {
                        // Check if neighbor is also NOT a stone
                        if (allDots[i + 1, j] != null && !allDots[i + 1, j].GetComponent<Dot>().isStone) {
                            if (SwitchAndCheck(i, j, Vector2.right)) {
                                return new List<GameObject> { allDots[i, j], allDots[i + 1, j] };
                            }
                        }
                    }
                    
                    // 2. Check Swap Up
                    if (j < height - 1) {
                        // Check if neighbor is also NOT a stone
                        if (allDots[i, j + 1] != null && !allDots[i, j + 1].GetComponent<Dot>().isStone) {
                            if (SwitchAndCheck(i, j, Vector2.up)) {
                                return new List<GameObject> { allDots[i, j], allDots[i, j + 1] };
                            }
                        }
                    }
                }
            }
        }
        return null; 
    }

    // 1. Check if the board has ANY valid move
    public bool IsDeadlocked() {
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (allDots[i, j] != null) {
                    if (i < width - 1) {
                        if (SwitchAndCheck(i, j, Vector2.right)) return false; // Found a move right
                    }
                    if (j < height - 1) {
                        if (SwitchAndCheck(i, j, Vector2.up)) return false; // Found a move up
                    }
                }
            }
        }
        return true; // No moves found anywhere!
    }

    // 2. Virtual Swap to test if a move works (without actually doing it)
    private bool SwitchAndCheck(int column, int row, Vector2 direction) {
        // Swap them in the array
        SwitchPieces(column, row, direction);
        
        // Check if it created a match
        bool hasMatch = false;
        if (CheckConnection(column, row) || CheckConnection(column + (int)direction.x, row + (int)direction.y)) {
            hasMatch = true;
        }
        
        // IMPORTANT: Swap them back immediately! We are just "thinking", not moving.
        SwitchPieces(column, row, direction);
        return hasMatch;
    }

    // Helper for swapping in array
    private void SwitchPieces(int column, int row, Vector2 direction) {
        if (allDots[column + (int)direction.x, row + (int)direction.y] != null) {
            GameObject holder = allDots[column + (int)direction.x, row + (int)direction.y];
            allDots[column + (int)direction.x, row + (int)direction.y] = allDots[column, row];
            allDots[column, row] = holder;
        }
    }

    // Helper to check for standard 3-matches
    private bool CheckConnection(int column, int row) {
        if (allDots[column, row] == null) return false;
        
        // Check Horizontal
        if (column > 1 && allDots[column - 1, row].tag == allDots[column, row].tag && allDots[column - 2, row].tag == allDots[column, row].tag) return true;
        if (column < width - 2 && allDots[column + 1, row].tag == allDots[column, row].tag && allDots[column + 2, row].tag == allDots[column, row].tag) return true;
        if (column > 0 && column < width - 1 && allDots[column - 1, row].tag == allDots[column, row].tag && allDots[column + 1, row].tag == allDots[column, row].tag) return true;
        
        // Check Vertical
        if (row > 1 && allDots[column, row - 1].tag == allDots[column, row].tag && allDots[column, row - 2].tag == allDots[column, row].tag) return true;
        if (row < height - 2 && allDots[column, row + 1].tag == allDots[column, row].tag && allDots[column, row + 2].tag == allDots[column, row].tag) return true;
        if (row > 0 && row < height - 1 && allDots[column, row - 1].tag == allDots[column, row].tag && allDots[column, row + 1].tag == allDots[column, row].tag) return true;
        
        return false;
    }

    public void ShuffleBoard() {
        // 1. Create a list of all current dots
        List<GameObject> currentDots = new List<GameObject>();
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (allDots[i, j] != null) {
                    currentDots.Add(allDots[i, j]);
                }
            }
        }

        // 2. Shuffle the list randomly
        for (int i = 0; i < currentDots.Count; i++) {
            GameObject temp = currentDots[i];
            int randomIndex = Random.Range(i, currentDots.Count);
            currentDots[i] = currentDots[randomIndex];
            currentDots[randomIndex] = temp;
        }

        // 3. Reassign them to the grid
        int dotIndex = 0;
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (dotIndex < currentDots.Count) {
                    // Move the GameObject to the new position
                    GameObject dot = currentDots[dotIndex];
                    dot.transform.position = new Vector2(i - centerOffset.x, j - centerOffset.y); // Snap to grid
                    
                    // Update the Dot Script
                    Dot d = dot.GetComponent<Dot>();
                    d.column = i;
                    d.row = j;
                    
                    // Update Board Array
                    allDots[i, j] = dot;
                    dotIndex++;
                }
            }
        }
        
        // 4. Check if the shuffle failed (still no moves?)
        if (IsDeadlocked()) {
            ShuffleBoard(); // Try again! (Recursion)
        }
    }
    
}