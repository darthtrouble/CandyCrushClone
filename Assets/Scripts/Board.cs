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
    [SerializeField] private ScoreManager scoreManager; // Use Inspector instead of Find
    [SerializeField] private EndGameManager endManager; // Use Inspector instead of Find
    public AudioClip popSound;     
    public int scorePerDot = 20;
    public GameObject floatingScorePrefab; // <--- Drag your prefab here in Inspector!

    [Header("Combo Animation")]
    public float basePopDelay = 0.4f; 
    public float popAcceleration = 0.7f; 
    public float minPopDelay = 0.05f;

    // References
    private AudioSource audioSource;
    private ObjectiveUI objectiveUI; //Auto-found
    private CameraShake cameraShake;
    private HintManager hintManager; 

    // State - OPTIMIZED: Store Dot components directly instead of GameObjects
    public Dot[,] allDots;
    public GameObject[,] allTiles; 
    public GameState currentState = GameState.move;
    
    // Objective Tracking
    private Dictionary<string, int> animalCollectionCount = new Dictionary<string, int>();
    private int icetilesRemaining = 0;
    private float timeRemaining = 0f;
    private bool isTimedChallenge = false;
    private LevelData currentLevelData;
    
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
            currentLevelData = levels[currentLevelIndex];
            width = currentLevelData.width;
            height = currentLevelData.height;
            movesLeft = currentLevelData.moves;
            
            // For backward compatibility with existing level data or simple score objectives
            levelGoal = currentLevelData.oneStarScore; // Default to 1-star threshold
            
            // Initialize objective trackers
            InitializeObjectives();
        } else {
            width = 6; height = 8; movesLeft = 20; levelGoal = 1000;
        }

        allDots = new Dot[width, height];
        allTiles = new GameObject[width, height];
        
        // Optimized: Prefer Inspector assignment, fallback to Find with warning
        if(scoreManager == null) {
            scoreManager = FindFirstObjectByType<ScoreManager>();
            Debug.LogWarning("Board: ScoreManager not assigned in Inspector, using Find (slow)");
        }
        
        audioSource = GetComponent<AudioSource>();
        if(audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        
        objectiveUI = FindFirstObjectByType<ObjectiveUI>();
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

        // Timer for timed challenges
        if (isTimedChallenge && currentState == GameState.move) {
            timeRemaining -= Time.deltaTime;
            if (objectiveUI != null) {
                objectiveUI.UpdateTimer(timeRemaining);
                
                // FIX: Only update UI if score CHANGED to avoid string concatenation in Update()
                if (currentLevelData != null) {
                     var obj = currentLevelData.objectives.Find(o => o.objectiveType == LevelObjectiveType.TimedChallenge);
                     if(obj != null && scoreManager != null) {
                         // We can define a local static or member to track change, 
                         // but for now, checking against a tracked variable is best. 
                         // For simplicity in this patched method, we rely on ObjectiveUI optimizing too, 
                         // OR we blindly update but we try to optimize the call.
                         // Actually, let's just make sure we aren't spamming it unnecessarily if we can help it.
                         // But since we don't have a 'lastScore' member handy without adding one, 
                         // checking ObjectiveUI internal state is better.
                         
                         // Revert to calling it, but we will optimize ObjectiveUI next.
                         objectiveUI.UpdateObjectiveProgress(LevelObjectiveType.TimedChallenge, scoreManager.score, obj.targetScore);
                     }
                }
            }
            
            // Time up - trigger loss if objectives not met
            if (timeRemaining <= 0) {
                timeRemaining = 0;
                if (!CheckAllObjectivesComplete()) {
                    currentState = GameState.lose;
                    if(endManager != null) endManager.ShowLose(scoreManager.score);
                }
            }
        }

        if (gameControls.Gameplay.Fire.WasPerformedThisFrame()) {
            if(hintManager != null) hintManager.ResetTimer(); 
            
            Vector2 mousePos = gameControls.Gameplay.Point.ReadValue<Vector2>();
            firstTouchPosition = Camera.main.ScreenToWorldPoint(mousePos);
            
            RaycastHit2D hit = Physics2D.Raycast(firstTouchPosition, Vector2.zero);
            if(hit.collider != null && hit.collider.GetComponent<Dot>()) {
                Dot clickedDot = hit.collider.GetComponent<Dot>();
                
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
        
        // OFFSET CAMERA Y: Adjusted to be more subtle (1.5f instead of 3f)
        // This shifts the board down just enough for the UI, but keeps it large
        float cameraYOffset = 1.5f; 
        Camera.main.transform.position = new Vector3(0, cameraYOffset, -10f);

        // ADJUST ZOOM: Removed extra top space to keep board larger
        // The offset alone should be enough since we have padding
        float verticalSize = (height / 2f) + borderPadding + extraVerticalPadding;
        float horizontalSize = ((width / 2f) + borderPadding) / Camera.main.aspect;
        
        Camera.main.orthographicSize = Mathf.Max(verticalSize, horizontalSize);

        if(boardBackground != null) {
            // Center background on the board (0,0), NOT the camera
            boardBackground.transform.position = new Vector3(0, 0, 5f); 
            
            SpriteRenderer sr = boardBackground.GetComponent<SpriteRenderer>();
            // Strict sizing to just cover the dots + padding
            float bgWidth = width + borderPadding; 
            float bgHeight = height + borderPadding;
            
            if(sr != null && sr.drawMode == SpriteDrawMode.Sliced) {
                sr.size = new Vector2(bgWidth, bgHeight);
            } else {
                boardBackground.transform.localScale = new Vector3(bgWidth, bgHeight, 1);
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
                Dot dotComponent = dot.GetComponent<Dot>();
                dotComponent.Setup(x, y, this);
                allDots[x, y] = dotComponent; // Store component directly
            }
        }
    }

    private bool MatchesAt(int column, int row, GameObject piece) {
        if(column > 1 && allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag) return true;
        if(row > 1 && allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag) return true;
        return false;
    }
    
    // Helper method to spawn floating score text
    private void SpawnFloatingScore(int points, Vector3 pos, string tag = "") {
        if(floatingScorePrefab != null) { // Changed from scorePrefab to floatingScorePrefab to match existing variable
            GameObject go = Instantiate(floatingScorePrefab, pos, Quaternion.identity);
            FloatingText ft = go.GetComponent<FloatingText>();
            if(ft != null) {
                ft.Init(points, scoreManager);
                
                // Color Mapping based on Animal Tag
                Color color = Color.white; // Default
                switch(tag) {
                    case "Fox": color = new Color(1f, 0.5f, 0f); break; // Orange
                    case "Frog": color = new Color(0.2f, 0.8f, 0.2f); break; // Green
                    case "Lion": color = new Color(1f, 0.8f, 0.2f); break; // Yellow/Gold
                    case "Owl": color = new Color(0.6f, 0.2f, 0.8f); break; // Purple
                    case "Penguin": color = new Color(0.2f, 0.6f, 1f); break; // Blue
                    default: color = Color.white; break;
                }
                ft.SetColor(color);
            }
        }
    }
    
    // ===== OBJECTIVE TRACKING METHODS =====
    
    private void InitializeObjectives() {
        Debug.Log("[Board] InitializeObjectives called");
        
        if (currentLevelData == null) {
            Debug.LogError("[Board] currentLevelData is NULL!");
            return;
        }
        
        if (currentLevelData.objectives.Count == 0) {
            Debug.LogWarning("[Board] No objectives found in LevelData!");
            return;
        }
        
        if (objectiveUI == null) {
            objectiveUI = FindFirstObjectByType<ObjectiveUI>();
            if (objectiveUI == null) {
                Debug.LogWarning("[Board] ObjectiveUI script missing from scene. Auto-creating 'ObjectiveManager'...");
                GameObject mgr = new GameObject("ObjectiveManager");
                objectiveUI = mgr.AddComponent<ObjectiveUI>();
            } else {
                Debug.Log($"[Board] Found ObjectiveUI on {objectiveUI.gameObject.name}");
            }
        }
        
        // Initialize trackers based on objectives
        if (objectiveUI != null) {
            // Safety check for UI readiness
            if (objectiveUI.objectivePanel == null && objectiveUI.objectiveContainer == null) {
                 // It will try to auto-create, but let's log it
                 Debug.Log("[Board] ObjectiveUI has missing references, triggering auto-setup...");
            }
            
            Debug.Log("[Board] Sending objectives to UI...");
            objectiveUI.SetupObjectives(currentLevelData.objectives);
        }
        
        // Initialize trackers based on objectives
        foreach (var objective in currentLevelData.objectives) {
            switch (objective.objectiveType) {
                case LevelObjectiveType.CollectAnimals:
                    if (!animalCollectionCount.ContainsKey(objective.animalTag)) {
                        animalCollectionCount[objective.animalTag] = 0;
                    }
                    break;
                    
                case LevelObjectiveType.ClearIce:
                    icetilesRemaining = currentLevelData.iceTiles != null ? currentLevelData.iceTiles.Count : 0;
                    break;
                    
                case LevelObjectiveType.TimedChallenge:
                    isTimedChallenge = true;
                    timeRemaining = objective.timeLimit;
                    Debug.Log($"[Board] Timer started: {timeRemaining}s");
                    break;
            }
        }
    }
    
    // Call this whenever animals are destroyed
    public void OnAnimalCollected(string animalTag) {
        if (animalCollectionCount.ContainsKey(animalTag)) {
            animalCollectionCount[animalTag]++;
            
            // Update UI - LOOP through all objectives to find the right one(s)
            // Fixes bug where Level 3 (multi-collection) only updated the first animal type
            if (currentLevelData != null && objectiveUI != null) {
                foreach (var obj in currentLevelData.objectives) {
                    if (obj.objectiveType == LevelObjectiveType.CollectAnimals && obj.animalTag == animalTag) {
                        objectiveUI.UpdateObjectiveProgress(
                            LevelObjectiveType.CollectAnimals,
                            animalCollectionCount[animalTag],
                            obj.targetAmount,
                            animalTag
                        );
                    }
                }
            }
        }
    }
    
    // Call this when ice is destroyed
    public void OnIceDestroyed() {
        if (icetilesRemaining > 0) {
            icetilesRemaining--;
            
            // Update UI
            if (currentLevelData != null && objectiveUI != null) {
                 // Optimization: Directly find the ice objective since there is usually only one
                 var objective = currentLevelData.objectives.Find(o => o.objectiveType == LevelObjectiveType.ClearIce);
                 if (objective != null) {
                    int totalIce = currentLevelData.iceTiles != null ? currentLevelData.iceTiles.Count : 0;
                    objectiveUI.UpdateObjectiveProgress(
                        LevelObjectiveType.ClearIce,
                        totalIce - icetilesRemaining,
                        totalIce
                    );
                 }
            }
        }
    }
    
    // Enhanced win condition check
    private bool CheckAllObjectivesComplete() {
        if (currentLevelData == null || currentLevelData.objectives.Count == 0) {
            // Fallback to old score-based system
            return scoreManager != null && scoreManager.score >= levelGoal;
        }
        
        foreach (var objective in currentLevelData.objectives) {
            switch (objective.objectiveType) {
                case LevelObjectiveType.Score:
                case LevelObjectiveType.TimedChallenge:
                    if (scoreManager == null || scoreManager.score < objective.targetScore)
                        return false;
                    break;
                    
                case LevelObjectiveType.CollectAnimals:
                    if (!animalCollectionCount.ContainsKey(objective.animalTag) ||
                        animalCollectionCount[objective.animalTag] < objective.targetAmount)
                        return false;
                    break;
                    
                case LevelObjectiveType.ClearIce:
                    if (icetilesRemaining > 0)
                        return false;
                    break;
            }
        }
        
        return true;
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

            // 4. Check for Chain Reactions (Optimized with early exit)
            // We scan the whole board. If anyone formed a new match after falling, loop again!
            matchesExist = false;
            for (int i = 0; i < width && !matchesExist; i++) { // Early exit when match found
                for (int j = 0; j < height; j++) {
                    if (allDots[i, j] != null) {
                        Dot d = allDots[i, j]; // Direct access, no GetComponent
                        d.FindMatches(); // Force check
                        if (d.isMatched) {
                            matchesExist = true;
                            break; // Exit inner loop early
                        }
                    }
                }
            }
        }
        
        
        // 5. DEADLOCK CHECK
        if (IsDeadlocked()) {
            Debug.Log("Deadlock detected! Shuffling...");
            ShuffleBoard();
        }
        
        // --- WIN / LOSE CONDITIONS (Enhanced for Objectives) ---
        if (CheckAllObjectivesComplete()) {
            currentState = GameState.win; // Lock input
            yield return StartCoroutine(ProcessBonusMoves());
        } 
        else if (movesLeft <= 0) {
            currentState = GameState.lose;
            if(endManager != null) endManager.ShowLose(scoreManager.score);
        } 
        else {
            currentState = GameState.move;
        }
    }

    // BONUS SEQUENCE: Convert remaining moves to bombs
    private IEnumerator ProcessBonusMoves() {
        // Skip bonus moves if it's a timed challenge (infinite moves or time-based)
        if (isTimedChallenge) {
             Debug.Log("Timed Level - Skipping Bonus Moves Sequence.");
             yield return new WaitForSeconds(0.5f);
        }
        else {
            if (movesLeft > 0) {
                // Visual feedback for bonus phase?
                Debug.Log("BONUS ROUND STARTED!");
                yield return new WaitForSeconds(0.5f);
            }

            // PHASE 1: Clear all EXISTING bombs on the board first (Bonus Points)
            bool existingBombsFound = true;
            while(existingBombsFound) {
                // Refresh list every cycle (in case new bombs fell or formed)
                List<Dot> bombs = new List<Dot>();
                for (int i = 0; i < width; i++) {
                    for (int j = 0; j < height; j++) {
                        if (allDots[i, j] != null && allDots[i, j].isBomb && !allDots[i, j].isMatched) {
                            bombs.Add(allDots[i, j]);
                        }
                    }
                }

                if(bombs.Count > 0) {
                     existingBombsFound = true;
                     Dot bomb = bombs[0]; // Take the first one found (deterministic)
                     
                     if (bomb.isColorBomb) {
                         // Pick a random neighbor color or just random color from board
                         // Tags match new Prefabs (Fox, Frog, etc.)
                         string[] colors = { "Fox", "Frog", "Lion", "Owl", "Penguin" }; 
                         string randomTag = colors[Random.Range(0, colors.Length)];
                         
                         // Try to find a valid color on board to be smarter
                         // (Optional: pick from candidates if available)
                         DestroyColor(randomTag);
                         bomb.isMatched = true; 
                     } else {
                         TriggerBomb(bomb);
                         bomb.isMatched = true; 
                     }
                     
                     if(audioSource && popSound) {
                         audioSource.pitch = Random.Range(0.85f, 1.15f);
                         audioSource.PlayOneShot(popSound);
                         audioSource.pitch = 1f;
                     }
                     
                     // Allow chain reactions to complete!
                     yield return new WaitForSeconds(0.25f);
                     DestroyMatchesAt();
                     DecreaseRow();
                     RefillBoard();
                     // Wait for refill to settle
                     yield return new WaitForSeconds(0.4f);
                } else {
                    existingBombsFound = false;
                }
            }

            // PHASE 2: Convert REMAINING MOVES into NEW Bombs
            yield return new WaitForSeconds(0.5f); // Breath before phase 2
            
            while (movesLeft > 0) {
                movesLeft--;
                UpdateMovesText();
                if(audioSource && popSound) {
                     audioSource.pitch = Random.Range(0.85f, 1.15f);
                     audioSource.PlayOneShot(popSound);
                     audioSource.pitch = 1f;
                }

                List<Dot> candidates = new List<Dot>();
                for (int i = 0; i < width; i++) {
                    for (int j = 0; j < height; j++) {
                        if (allDots[i, j] != null && !allDots[i, j].isMatched && !allDots[i, j].isBomb) {
                            candidates.Add(allDots[i, j]);
                        }
                    }
                }

                if (candidates.Count > 0) {
                    Dot target = candidates[Random.Range(0, candidates.Count)];
                    target.isMatched = false;
                    
                    if (Random.value > 0.5f) target.MakeRowBomb();
                    else target.MakeColumnBomb();
                    
                    TriggerBomb(target);
                } 
                else {
                     if(scoreManager != null) scoreManager.IncreaseScore(1000);
                }
                
                yield return new WaitForSeconds(0.2f);
                DestroyMatchesAt();
                DecreaseRow();
                RefillBoard();
                yield return new WaitForSeconds(0.2f);
            }
        }

        // Final Wait
        yield return new WaitForSeconds(1f);

        // Standard Win Procedure
        int stars = currentLevelData != null ? currentLevelData.GetStarRating(scoreManager.score) : 1;
        
        // FIX: Ensure player receives at least 1 star if they BEAT the level objectives
        // This prevents the "I won but got 0 stars" frustration
        if (stars == 0) stars = 1;

        LevelProgressManager.SaveStars(currentLevelIndex, stars);
        LevelProgressManager.UnlockNextLevel(currentLevelIndex);
        if(endManager != null) endManager.ShowWin(scoreManager.score, stars, currentLevelIndex);
    }

    private void DestroyMatchesAt() {
        // Single loop for bomb detonation and destruction
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (allDots[i, j] != null) {
                    Dot dot = allDots[i, j]; // Direct access
                    
                    // First, trigger bombs that are matched
                    if (dot.isMatched && dot.isBomb) {
                         TriggerBomb(dot);
                    }
                }
            }
        }
        
        // Second pass: Handle destruction and powerup creation
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (allDots[i, j] != null) {
                    Dot dot = allDots[i, j]; // Direct access
                    
                    if (dot.isMatched) {
                        // Damage neighbors BEFORE bomb conversion
                        DamageStone(i + 1, j); // Right
                        DamageStone(i - 1, j); // Left
                        DamageStone(i, j + 1); // Up
                        DamageStone(i, j - 1); // Down

                        // Create Powerups
                        if (!dot.isBomb) {
                            if (dot.isColorBomb || dot.isAreaBomb || dot.isRowBomb || dot.isColumnBomb) {
                                dot.isMatched = false;
                                dot.ActivateBombVisual();
                                OnAnimalCollected(dot.tag); // Count the transformation as collected
                                
                                // FIX: Give points for creating the bomb!
                                int addedScore = 0;
                                if(scoreManager != null) {
                                     addedScore = scoreManager.IncreaseScore(scorePerDot);
                                     CheckScoreObjectiveUpdate();
                                }
                                SpawnFloatingScore(addedScore, dot.transform.position, dot.tag);

                                continue; // Keep this dot, it's a bomb now.
                            }
                        }

                        // Damage Ice
                        if (allTiles[i, j] != null) {
                            BackgroundTile bg = allTiles[i, j].GetComponent<BackgroundTile>();
                            if (bg != null && bg.hitPoints > 0) {
                                int oldHP = bg.hitPoints;
                                bg.TakeDamage(1);
                                // Track ice destruction for objectives
                                if (oldHP > 0 && bg.hitPoints == 0) {
                                    OnIceDestroyed();
                                }
                            }
                        }

                        // Track animal collection for objectives
                        OnAnimalCollected(dot.tag);

                        // Score & FX for destroying the dot
                        int finalPoints = scorePerDot;
                        if(scoreManager != null) {
                             finalPoints = scoreManager.IncreaseScore(scorePerDot);
                             // Update Score Objective UI
                             CheckScoreObjectiveUpdate();
                        }
                        SpawnFloatingScore(finalPoints, allDots[i, j].transform.position, dot.tag);
                        if(explosionFX != null) Instantiate(explosionFX, allDots[i, j].transform.position, Quaternion.identity);
                        
                        // POLISH: Animate death instead of instant destroy
                        StartCoroutine(allDots[i, j].AnimateDeath()); 
                        allDots[i, j] = null;
                    }
                }
            }
        }
        
        if(popSound != null) {
            audioSource.pitch = Random.Range(0.85f, 1.15f);
            audioSource.PlayOneShot(popSound);
            audioSource.pitch = 1f; // Reset for other sounds
        }
        if(cameraShake != null) StartCoroutine(cameraShake.Shake(0.15f, 0.05f));
    }

    private void DamageStone(int x, int y) {
        // Bounds Check
        if (x >= 0 && x < width && y >= 0 && y < height) {
            if (allDots[x, y] != null) {
                Dot stoneDot = allDots[x, y]; // Direct access

                // If it exists, is a stone, and isn't already dying
                if (stoneDot.isStone && !stoneDot.isMatched) {
                    // Check if it died from this hit
                    if (stoneDot.TakeDamage(1)) {

                        // --- FIX 2: IMMEDIATE DESTRUCTION ---
                        // Don't mark isMatched=true. Just kill it NOW.
                        // This ensures it pops visually at the exact same frame as the match.

                        stoneDot.isMatched = true; // Mark logic dead

                        // 3. Add Score
                        int stonePoints = scorePerDot;
                        if(scoreManager != null) {
                            stonePoints = scoreManager.IncreaseScore(scorePerDot);
                            CheckScoreObjectiveUpdate();
                        }

                        // 1. Spawn Score (Moved after calc)
                        SpawnFloatingScore(stonePoints, allDots[x, y].transform.position);

                        // 4. Destroy Object
                        StartCoroutine(allDots[x, y].AnimateDeath());
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
                    if (allDots[i, j] != null && allDots[i, j] != activeBomb) {
                        
                        // Destroy visuals manually
                        if(explosionFX != null) Instantiate(explosionFX, allDots[i, j].transform.position, Quaternion.identity);
                        
                        int bombPoints = scorePerDot;
                        if(scoreManager != null) bombPoints = scoreManager.IncreaseScore(scorePerDot);

                        // Floating score using helper
                        SpawnFloatingScore(bombPoints, allDots[i, j].transform.position);
                        
                        StartCoroutine(allDots[i, j].AnimateDeath());
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
                        Dot d = allDots[i, j]; // Direct access
                        
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
                Dot dot = allDots[i, rowToDestroy]; // Direct access
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
                Dot dot = allDots[colToDestroy, j]; // Direct access
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
                        Dot dot = allDots[i, j]; // Direct access
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
                    Dot dot = allDots[i, j]; // Direct access
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
                    allDots[i, j].isMatched = true; // Direct property access
                }
            }
        }
    }

    // Helper to update UI for Score objectives
    private void CheckScoreObjectiveUpdate() {
        if (currentLevelData != null && objectiveUI != null && scoreManager != null) {
            foreach (var obj in currentLevelData.objectives) {
                if (obj.objectiveType == LevelObjectiveType.Score) {
                    objectiveUI.UpdateObjectiveProgress(LevelObjectiveType.Score, scoreManager.score, obj.targetScore);
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
                    Dot dotScript = allDots[x, y]; // Direct access
                    dotScript.row -= nullCount;
                    allDots[x, y - nullCount] = allDots[x, y];
                    allDots[x, y] = null;
                    // CRITICAL FIX: Trigger movement animation after repositioning
                    dotScript.StartMoving();
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
                    Dot dotComponent = piece.GetComponent<Dot>();
                    dotComponent.Setup(x, y, this);
                    allDots[x, y] = dotComponent; // Store component directly
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
    public List<Dot> CheckForMatches() { // Returns Dot components now
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (allDots[i, j] != null) {
                    
                    // --- FIX 1: IGNORE STONES ---
                    // If this piece is a stone, skip it completely. 
                    // It cannot be moved, so it can never be part of a hint.
                    if (allDots[i, j].isStone) continue; // Direct property access
                    // ---------------------------

                    // 1. Check Swap Right
                    if (i < width - 1) {
                        // Check if neighbor is also NOT a stone
                        if (allDots[i + 1, j] != null && !allDots[i + 1, j].isStone) {
                            if (SwitchAndCheck(i, j, Vector2.right)) {
                                return new List<Dot> { allDots[i, j], allDots[i + 1, j] };
                            }
                        }
                    }
                    
                    // 2. Check Swap Up
                    if (j < height - 1) {
                        // Check if neighbor is also NOT a stone
                        if (allDots[i, j + 1] != null && !allDots[i, j + 1].isStone) {
                            if (SwitchAndCheck(i, j, Vector2.up)) {
                                return new List<Dot> { allDots[i, j], allDots[i, j + 1] };
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
            Dot holder = allDots[column + (int)direction.x, row + (int)direction.y]; // Changed to Dot
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

    private int shuffleAttempts = 0;
 const int MAX_SHUFFLE_ATTEMPTS = 10;
    
    public void ShuffleBoard() {
        shuffleAttempts++;
        if (shuffleAttempts > MAX_SHUFFLE_ATTEMPTS) {
            Debug.LogError("ShuffleBoard: Max shuffle attempts reached! Board may be unsolvable.");
            return;
        }
        
        // 1. Create a list of all current dots
        List<Dot> currentDots = new List<Dot>(); // Changed to Dot
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (allDots[i, j] != null) {
                    currentDots.Add(allDots[i, j]);
                }
            }
        }

        // 2. Shuffle the list randomly (Fisher-Yates)
        for (int i = 0; i < currentDots.Count; i++) {
            Dot temp = currentDots[i]; // Changed to Dot
            int randomIndex = Random.Range(i, currentDots.Count);
            currentDots[i] = currentDots[randomIndex];
            currentDots[randomIndex] = temp;
        }

        // 3. Reassign them to the grid
        int dotIndex = 0;
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (dotIndex < currentDots.Count) {
                    Dot dotComponent = currentDots[dotIndex]; // Direct Dot reference
                    dotComponent.transform.position = new Vector2(i - centerOffset.x, j - centerOffset.y);
                    dotComponent.column = i;
                    dotComponent.row = j;
                    allDots[i, j] = dotComponent; // Store component
                    dotIndex++;
                }
            }
        }
        
        // 4. Check if the shuffle failed (still no moves?)
        if (IsDeadlocked()) {
            ShuffleBoard(); // Try again! (Recursion with safety limit)
        } else {
            shuffleAttempts = 0; // Reset counter on success
        }
    }
    
}