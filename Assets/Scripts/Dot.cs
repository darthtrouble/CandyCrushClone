using UnityEngine;
using System.Collections;

public class Dot : MonoBehaviour {

    [Header("Board Variables")]
    public int column;
    public int row;
    public bool isMatched = false;

    [Header("Attributes")]
    public bool isStone = false; // Is this an obstacle?
    public int health = 1;       // How many hits to destroy it
    public Sprite[] stoneSprites; // Optional: Cracks visuals
    
    [Header("Power-Up Flags")]
    public bool isColorBomb = false;
    public bool isAreaBomb = false;
    public bool isColumnBomb = false;
    public bool isRowBomb = false;
    public bool isBomb = false;       

    [Header("Visual References")]
    public GameObject rowArrow;
    public GameObject columnArrow;
    public GameObject colorBombSprite;
    public GameObject areaBombSprite;

    private Vector3 originalScale; 
    private Board board;
    private GameObject otherDot; // The tile I swapped with

    void Awake() {
        originalScale = transform.localScale;
    }

    public void Setup(int x, int y, Board boardRef) {
        column = x;
        row = y;
        board = boardRef;
        transform.localScale = Vector3.zero; // Start small for pop-in
        StartCoroutine(PopInAnimation());
        StartMoving(); // Trigger movement animation
    }

    private IEnumerator PopInAnimation() {
        float elapsed = 0;
        float duration = 0.3f;
        while(elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // EaseOutBack
            float back = 1 + 2.70158f * Mathf.Pow(t - 1, 3) + 1.70158f * Mathf.Pow(t - 1, 2);
            // Manual Lerp Unclamped
            transform.localScale = Vector3.zero + (originalScale - Vector3.zero) * back;
            yield return null;
        }
        transform.localScale = originalScale;
    }

    private Coroutine pulseCoroutine;

    // Update OnMouseDown to PREVENT moving/swapping stones
    private void OnMouseDown() {
        if (isStone || board.currentState != GameState.move) return;
        
        // VISUAL PULSE
        if(pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        pulseCoroutine = StartCoroutine(PulseSelection(1.2f));
    }

    private void OnMouseUp() {
        // Return to normal size
        if(pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        if(this != null) transform.localScale = originalScale;
    }

    private IEnumerator PulseSelection(float targetMult) {
        Vector3 target = originalScale * targetMult;
        while(Vector3.Distance(transform.localScale, target) > 0.05f) {
            transform.localScale = Vector3.Lerp(transform.localScale, target, Time.deltaTime * 10f);
            yield return null;
        }
        transform.localScale = target;
    }

    // Optimized: Only move when position changes instead of checking every frame
    private Coroutine moveCoroutine;
    
    public void StartMoving() {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(SmoothMove());
    }
    
    private IEnumerator SmoothMove() {
        if(board == null) yield break;
        
        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(column - board.centerOffset.x, row - board.centerOffset.y, 0);
        
        float dist = Vector3.Distance(startPos, targetPos);
        if(dist < 0.01f) {
           transform.position = targetPos;
           yield break;
        }

        float duration = 0.2f; // Snappier!
        // If falling far (refill), make it faster per unit, but clamp
        if(dist > 1f) duration = 0.25f;

        float elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // SmoothStep for nice landing
            t = t * t * (3f - 2f * t);
            
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        
        // Snap to final position
        transform.position = targetPos;
    }

    public void CalculateMove(float swipeAngle) {
        // Reset scale first
        OnMouseUp();

        if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1) MovePieces(Vector2.right);
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1) MovePieces(Vector2.up);
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0) MovePieces(Vector2.left);
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0) MovePieces(Vector2.down);
        else board.currentState = GameState.move;
    }

    void MovePieces(Vector2 direction) {
        otherDot = board.allDots[column + (int)direction.x, row + (int)direction.y]?.gameObject; // Get GameObject from Dot
        if (otherDot != null) {
            Dot otherScript = otherDot.GetComponent<Dot>();

            // Don't allow swapping with stones
            if (otherScript.isStone) {
                board.currentState = GameState.move;
                return;
            }

            board.currentState = GameState.wait;
            
            // Swap
            int tempCol = column; int tempRow = row;
            column = otherScript.column; row = otherScript.row;
            otherScript.column = tempCol; otherScript.row = tempRow;
            board.allDots[column, row] = this; // Store components
            board.allDots[otherScript.column, otherScript.row] = otherScript;
            
            // Trigger movement animations
            StartMoving();
            otherScript.StartMoving();
            
            StartCoroutine(CheckMoveCo());
        } else {
            board.currentState = GameState.move;
        }
    }

    public IEnumerator CheckMoveCo() {
        if (otherDot == null) yield break;

        Dot otherScript = otherDot.GetComponent<Dot>();
        
        // --- 1. DOUBLE COLOR BOMB (Nuke) ---
        if (isColorBomb && otherScript.isColorBomb) {
            isMatched = true; otherScript.isMatched = true;
            gameObject.tag = "Untagged";
            otherScript.gameObject.tag = "Untagged";
            board.NukeBoard();
            board.DestroyMatches();
            yield break;
        }

        // --- 2. COLOR BOMB + ANY BOMB (Transform) ---
        else if (isColorBomb && (otherScript.isRowBomb || otherScript.isColumnBomb || otherScript.isAreaBomb)) {
            gameObject.tag = "Untagged";
            StartCoroutine(ColorBombComboRoutine(otherScript));
            yield break; 
        }
        else if (otherScript.isColorBomb && (isRowBomb || isColumnBomb || isAreaBomb)) {
            otherScript.gameObject.tag = "Untagged";
            otherScript.StartCoroutine(otherScript.ColorBombComboRoutine(this));
            yield break;
        }

        // --- 3. STRIPE + AREA (Mega Stripe) ---
        else if ((isRowBomb || isColumnBomb) && otherScript.isAreaBomb) {
            isMatched = true; otherScript.isMatched = true;
            gameObject.tag = "Untagged";
            otherScript.gameObject.tag = "Untagged";
            if (isRowBomb) board.DestroyRowStrip(row);
            else board.DestroyColumnStrip(column);
            board.DestroyMatches();
            yield break;
        }
        else if (isAreaBomb && (otherScript.isRowBomb || otherScript.isColumnBomb)) {
            isMatched = true; otherScript.isMatched = true;
            gameObject.tag = "Untagged";
            otherScript.gameObject.tag = "Untagged";

            if (otherScript.isRowBomb) board.DestroyRowStrip(otherScript.row);
            else board.DestroyColumnStrip(otherScript.column);
            
            board.DestroyMatches();
            yield break;
        }

        // --- 4. STRIPE + STRIPE (Cross Blast) ---
        else if ((isRowBomb || isColumnBomb) && (otherScript.isRowBomb || otherScript.isColumnBomb)) {
            isMatched = true; otherScript.isMatched = true;
            gameObject.tag = "Untagged";
            otherScript.gameObject.tag = "Untagged";
            board.DestroyMatches();
            yield break; 
        }

        // --- 5. AREA + AREA (Double Pop Sequence) ---
        else if (isAreaBomb && otherScript.isAreaBomb) {
            otherScript.isMatched = true; 
            gameObject.tag = "Untagged";
            otherScript.gameObject.tag = "Untagged";
            StartCoroutine(board.DoubleAreaBombRoutine(otherScript.column, otherScript.row, this));
            yield break;
        }

        // --- 6. COLOR BOMB + NORMAL ---
        else if (isColorBomb) {
            gameObject.tag = "Untagged"; // Untag bomb
            board.DestroyColor(otherDot.tag);
            isMatched = true; 
            board.DestroyMatches();
        }
        else if (otherScript.isColorBomb) {
            otherScript.gameObject.tag = "Untagged"; // Untag bomb
            board.DestroyColor(this.tag);
            otherScript.isMatched = true;
            board.DestroyMatches();
        }
        
        // --- 7. STANDARD MOVES ---
        else {
            yield return new WaitForSeconds(.3f); // Wait for move to finish
            FindMatches();
            if(otherDot != null) otherScript.FindMatches();

            if (!isMatched && !otherScript.isMatched) {
                // Swap Back
                int tempCol = column; int tempRow = row;
                column = otherScript.column; row = otherScript.row;
                otherScript.column = tempCol; otherScript.row = tempRow;
                board.allDots[column, row] = this; // Store components
                board.allDots[otherScript.column, otherScript.row] = otherScript;
                
                // Trigger movement animations for swap back
                StartMoving();
                otherScript.StartMoving();
                
                yield return new WaitForSeconds(.3f); 
                board.currentState = GameState.move;
            } else {
                board.DestroyMatches();
            }
        }
    }

    public IEnumerator ColorBombComboRoutine(Dot bombBeingReplicated) {
        // 1. Get the color we want to transform BEFORE untagging
        string targetTag = bombBeingReplicated.tag; 
        bool isSourceArea = bombBeingReplicated.isAreaBomb;

        // FIX: Now that we have the data, UNTAG the source bomb so it doesn't match neighbors
        bombBeingReplicated.gameObject.tag = "Untagged";

        // Destroy the Color Bomb itself immediately
        isMatched = true; 

        // 2. Loop through board and TRANSFORM matching colors
        for (int i = 0; i < board.width; i++) {
            for (int j = 0; j < board.height; j++) {
                if (board.allDots[i, j] != null) {
                    Dot d = board.allDots[i, j]; // Direct access, no GetComponent
                    
                    if (d.tag == targetTag) {
                        d.isRowBomb = false;
                        d.isColumnBomb = false;
                        d.isAreaBomb = false;

                        if (isSourceArea) {
                            d.isAreaBomb = true;
                        } 
                        else {
                            if (Random.value > 0.5f) d.isRowBomb = true; 
                            else d.isColumnBomb = true;
                        }
                        d.ActivateBombVisual(); 
                    }
                }
            }
        }

        yield return new WaitForSeconds(0.3f);

        for (int i = 0; i < board.width; i++) {
            for (int j = 0; j < board.height; j++) {
                if (board.allDots[i, j] != null) {
                    Dot d = board.allDots[i, j]; // Direct access
                    
                    if (d.tag == targetTag) {
                        d.isMatched = true;
                        d.isBomb = true; 
                    }
                }
            }
        }
        
        board.DestroyMatches();
    }

    public void FindMatches() {
        if (isStone) return; // Stones never match!
        if (isColorBomb) return; 

        // 1. Count Horizontal
        int leftCount = 0; int rightCount = 0;
        while (HasMatch(column - (leftCount + 1), row)) leftCount++;
        while (HasMatch(column + (rightCount + 1), row)) rightCount++;
        int totalHorizontal = 1 + leftCount + rightCount;

        // 2. Count Vertical
        int downCount = 0; int upCount = 0;
        while (HasMatch(column, row - (downCount + 1))) downCount++;
        while (HasMatch(column, row + (upCount + 1))) upCount++;
        int totalVertical = 1 + downCount + upCount;

        // --- BOMB LOGIC ---
        
        // Match 5 (Color Bomb)
        if (totalHorizontal >= 5 || totalVertical >= 5) {
            isMatched = true;
            if (!isBomb) isColorBomb = true;
        }
        // L or T Shape (Area Bomb)
        else if (totalHorizontal >= 3 && totalVertical >= 3) {
            isMatched = true;
            if (!isBomb) isAreaBomb = true;
        }
        // Match 4 (Row Bomb - Horizontal)
        else if (totalHorizontal == 4) {
            isMatched = true;
            if (otherDot != null || (!HasMatch(column - 1, row) && !isBomb)) {
                 if(!isBomb) isRowBomb = true;
            }
        }
        // Match 4 (Column Bomb - Vertical)
        else if (totalVertical == 4) {
            isMatched = true;
            if (otherDot != null || (!HasMatch(column, row - 1) && !isBomb)) {
                if(!isBomb) isColumnBomb = true;
            }
        }
        // Match 3 (Standard)
        else if (totalHorizontal >= 3 || totalVertical >= 3) {
            isMatched = true;
        }

        // --- MARK NEIGHBORS (Death Loop) ---
        if (isMatched) {
            if (totalHorizontal >= 3) {
                for (int i = 1; i <= leftCount; i++) MarkNeighbor(column - i, row);
                for (int i = 1; i <= rightCount; i++) MarkNeighbor(column + i, row);
            }
            if (totalVertical >= 3) {
                for (int i = 1; i <= downCount; i++) MarkNeighbor(column, row - i);
                for (int i = 1; i <= upCount; i++) MarkNeighbor(column, row + i);
            }
        }
    }

    bool HasMatch(int checkCol, int checkRow) {
        if (checkCol >= 0 && checkCol < board.width && checkRow >= 0 && checkRow < board.height) {
            Dot neighbor = board.allDots[checkCol, checkRow]; // Direct Dot access
            if (neighbor != null) return neighbor.tag == this.tag;
        }
        return false;
    }

    void MarkNeighbor(int checkCol, int checkRow) {
        if (checkCol >= 0 && checkCol < board.width && checkRow >= 0 && checkRow < board.height) {
            Dot neighbor = board.allDots[checkCol, checkRow]; // Direct Dot access
            if (neighbor != null) {
                neighbor.isMatched = true; // Direct property access
                neighbor.otherDot = null; // Reset their swipe memory
            }
        }
    }

    /// <summary>
    /// Plays a shrink animation then destroys this object.
    /// Call this instead of Destroy(gameObject).
    /// </summary>
    public IEnumerator AnimateDeath() {
        // Disablecollider to prevent accidental clicks
        Collider2D col = GetComponent<Collider2D>();
        if(col) col.enabled = false;

        float elapsed = 0;
        float duration = 0.2f;
        Vector3 startScale = transform.localScale;

        while(elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // BackIn curve for "suck in" effect
            float val = 1 - (t * t * (2.70158f * t - 1.70158f)); 
            transform.localScale = Vector3.zero + (startScale - Vector3.zero) * val;
            yield return null;
        }
        
        Destroy(gameObject);
    }

    public void ActivateBombVisual() {
        isBomb = true;
        
        if (isColorBomb) {
            gameObject.tag = "ColorBomb"; 
            if(colorBombSprite != null) colorBombSprite.SetActive(true);
        }
        else if (isAreaBomb && areaBombSprite != null) {
            areaBombSprite.SetActive(true);
        }
        else {
            if (isRowBomb && rowArrow != null) rowArrow.SetActive(true);
            if (isColumnBomb && columnArrow != null) columnArrow.SetActive(true);
        }
    }

    public void MakeRowBomb() {
        isRowBomb = true; isColumnBomb = false; isAreaBomb = false; isColorBomb = false;
        ActivateBombVisual();
    }

    public void MakeColumnBomb() {
        isRowBomb = false; isColumnBomb = true; isAreaBomb = false; isColorBomb = false;
        ActivateBombVisual();
    }

    public void MakeAreaBomb() {
        isRowBomb = false; isColumnBomb = false; isAreaBomb = true; isColorBomb = false;
        ActivateBombVisual();
    }

    public bool TakeDamage(int damage) {
        health -= damage;
        if (health <= 0) return true;
        return false;
    }
}