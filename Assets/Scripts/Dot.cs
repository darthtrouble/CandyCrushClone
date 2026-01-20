using UnityEngine;
using System.Collections;

public class Dot : MonoBehaviour {

    [Header("Board Variables")]
    public int column;
    public int row;
    public bool isMatched = false;
    
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
        transform.localScale = originalScale;
    }

    void Update() {
        if(board == null) return;
        // Smooth Movement
        float targetX = column - board.centerOffset.x;
        float targetY = row - board.centerOffset.y;
        if (Mathf.Abs(targetX - transform.position.x) > .1f) transform.position = Vector2.Lerp(transform.position, new Vector2(targetX, transform.position.y), .6f);
        else transform.position = new Vector2(targetX, transform.position.y);
        if (Mathf.Abs(targetY - transform.position.y) > .1f) transform.position = Vector2.Lerp(transform.position, new Vector2(transform.position.x, targetY), .6f);
        else transform.position = new Vector2(transform.position.x, targetY);
        
        board.allDots[column, row] = this.gameObject;
    }

    public void CalculateMove(float swipeAngle) {
        if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1) MovePieces(Vector2.right);
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1) MovePieces(Vector2.up);
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0) MovePieces(Vector2.left);
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0) MovePieces(Vector2.down);
        else board.currentState = GameState.move;
    }

    void MovePieces(Vector2 direction) {
        otherDot = board.allDots[column + (int)direction.x, row + (int)direction.y];
        if (otherDot != null) {
            board.currentState = GameState.wait;
            
            // Swap
            int tempCol = column; int tempRow = row;
            Dot otherScript = otherDot.GetComponent<Dot>();
            column = otherScript.column; row = otherScript.row;
            otherScript.column = tempCol; otherScript.row = tempRow;
            board.allDots[column, row] = this.gameObject;
            board.allDots[otherScript.column, otherScript.row] = otherDot;
            
            StartCoroutine(CheckMoveCo());
        } else {
            board.currentState = GameState.move;
        }
    }

    public IEnumerator CheckMoveCo() {
        Dot otherScript = otherDot.GetComponent<Dot>();
        
        // --- 1. DOUBLE COLOR BOMB (Destroy Everything) ---
        if (isColorBomb && otherScript.isColorBomb) {
            isMatched = true;
            otherScript.isMatched = true;
            board.NukeBoard();
            board.DestroyMatches(); // Tell board to actually clean up and refill!
            yield break; // Stop here, no need to check anything else
        }

        // --- 2. COLOR BOMB + STRIPE/AREA COMBO ---
        // Case A: I am Color, Other is Stripe/Area
        else if (isColorBomb && (otherScript.isRowBomb || otherScript.isColumnBomb || otherScript.isAreaBomb)) {
            StartCoroutine(ColorBombComboRoutine(otherScript));
            yield break; 
        }
        // Case B: Other is Color, I am Stripe/Area
        else if (otherScript.isColorBomb && (isRowBomb || isColumnBomb || isAreaBomb)) {
            otherScript.StartCoroutine(otherScript.ColorBombComboRoutine(this));
            yield break;
        }

        // --- 3. COLOR BOMB + NORMAL TILE ---
        else if (isColorBomb && otherDot != null) {
            board.DestroyColor(otherDot.tag);
            isMatched = true;
        }
        else if (otherDot != null && otherScript.isColorBomb) {
            board.DestroyColor(this.tag);
            otherScript.isMatched = true;
        }
        
        // --- 4. STANDARD MOVES (Swap back if no match) ---
        else {
            yield return new WaitForSeconds(.2f);
            
            FindMatches();
            if(otherDot != null) otherScript.FindMatches();

            if (!isMatched && !otherScript.isMatched) {
                // Invalid Move -> Swap Back
                int tempCol = column; int tempRow = row;
                column = otherScript.column; row = otherScript.row;
                otherScript.column = tempCol; otherScript.row = tempRow;
                board.allDots[column, row] = this.gameObject;
                board.allDots[otherScript.column, otherScript.row] = otherDot;
                yield return new WaitForSeconds(.2f);
                board.currentState = GameState.move;
            } else {
                board.DestroyMatches();
            }
        }
    }

    // --- NEW: THE TRANSFORMATION SEQUENCE ---
    public IEnumerator ColorBombComboRoutine(Dot bombBeingReplicated) {
        // 1. Get the color we want to transform
        string targetTag = bombBeingReplicated.tag; 

        // Check if we are replicating an Area Bomb (Packet) or a Line Bomb (Stripe)
        bool isSourceArea = bombBeingReplicated.isAreaBomb;

        // Destroy the Color Bomb itself immediately
        isMatched = true; 

        // 2. Loop through board and TRANSFORM matching colors
        for (int i = 0; i < board.width; i++) {
            for (int j = 0; j < board.height; j++) {
                if (board.allDots[i, j] != null) {
                    Dot d = board.allDots[i, j].GetComponent<Dot>();
                    
                    // If it matches the color of the bomb we swiped...
                    if (d.tag == targetTag) {
                        
                        // Reset existing flags just in case
                        d.isRowBomb = false;
                        d.isColumnBomb = false;
                        d.isAreaBomb = false;

                        if (isSourceArea) {
                            // If we swapped with an Area Bomb, make them all Area Bombs
                            d.isAreaBomb = true;
                        } 
                        else {
                            // --- THE FIX: RANDOMIZE DIRECTION ---
                            // If we swapped with a Row OR Column bomb, pick randomly for EACH dot
                            if (Random.value > 0.5f) {
                                d.isRowBomb = true;
                            } else {
                                d.isColumnBomb = true;
                            }
                        }
                        
                        // Show the Visuals (Arrows/Circles) immediately
                        d.ActivateBombVisual(); 
                    }
                }
            }
        }

        // 3. WAIT (Visual Pause)
        yield return new WaitForSeconds(0.3f);

        // 4. POP THEM ALL
        for (int i = 0; i < board.width; i++) {
            for (int j = 0; j < board.height; j++) {
                if (board.allDots[i, j] != null) {
                    Dot d = board.allDots[i, j].GetComponent<Dot>();
                    
                    if (d.tag == targetTag) {
                        d.isMatched = true;
                        d.isBomb = true; // Ensure Board knows to trigger the explosion logic
                    }
                }
            }
        }
        
        // Trigger the explosions
        board.DestroyMatches();
    }

    public void FindMatches() {
        // FIX: If I am a Color Bomb, I NEVER match on my own. 
        // I only explode via Swap (CheckMoveCo) or Explosion (Board.TriggerBomb)
        if (isColorBomb) return; 

        // If I am a standard bomb (Row/Column/Area), I behave like a normal tile 
        // so I CAN be matched to trigger my explosion.
        
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
            
            // PRIORITY CHECK:
            // 1. If I was the one Swapped -> I become the bomb.
            // 2. If nobody was Swapped (falling match) -> Left-most becomes bomb.
            if (otherDot != null || (!HasMatch(column - 1, row) && !isBomb)) {
                 if(!isBomb) isRowBomb = true;
            }
        }
        // Match 4 (Column Bomb - Vertical)
        else if (totalVertical == 4) {
            isMatched = true;
            
            // PRIORITY CHECK:
            // 1. If I was Swapped -> I become bomb.
            // 2. Default -> Bottom-most becomes bomb.
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
            // Important: If we made a bomb, we must STOP neighbors from becoming bombs too.
            // We do this by "Using Up" the `otherDot` variable on neighbors so they fail the priority check.
            
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
            GameObject neighbor = board.allDots[checkCol, checkRow];
            if (neighbor != null) return neighbor.tag == this.tag;
        }
        return false;
    }

    void MarkNeighbor(int checkCol, int checkRow) {
        if (checkCol >= 0 && checkCol < board.width && checkRow >= 0 && checkRow < board.height) {
            GameObject neighbor = board.allDots[checkCol, checkRow];
            if (neighbor != null) {
                Dot d = neighbor.GetComponent<Dot>();
                d.isMatched = true;
                d.otherDot = null; // Reset their swipe memory so they don't try to become bombs!
            }
        }
    }

    public void ActivateBombVisual() {
        isBomb = true;
        
        if (isColorBomb) {
            // FIX: Change Tag so it doesn't match with normal animals anymore
            gameObject.tag = "Untagged"; // Or create a custom tag "ColorBomb"
            
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
}