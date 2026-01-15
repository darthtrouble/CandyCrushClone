using UnityEngine;
using System.Collections;

public class Dot : MonoBehaviour {

    [Header("Board Variables")]
    public int column;
    public int row;
    public bool isMatched = false;
    
    private Board board;

    public void Setup(int x, int y, Board boardRef) {
        column = x;
        row = y;
        board = boardRef;
    }

    void Update() {
        if(board == null) return;

        float realTargetX = column - board.centerOffset.x;
        float realTargetY = row - board.centerOffset.y;

        // --- VISUAL MOVEMENT ---
        // Move X
        if (Mathf.Abs(realTargetX - transform.position.x) > .1f) {
            Vector2 tempPosition = new Vector2(realTargetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .6f);
        } else {
            Vector2 tempPosition = new Vector2(realTargetX, transform.position.y);
            transform.position = tempPosition;
        }
        
        // Move Y
        if (Mathf.Abs(realTargetY - transform.position.y) > .1f) {
            Vector2 tempPosition = new Vector2(transform.position.x, realTargetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .6f);
        } else {
            Vector2 tempPosition = new Vector2(transform.position.x, realTargetY);
            transform.position = tempPosition;
        }
    }

    public void CalculateMove(float swipeAngle) {
        if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1) MovePiecesActual(Vector2.right);
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1) MovePiecesActual(Vector2.up);
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0) MovePiecesActual(Vector2.left);
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0) MovePiecesActual(Vector2.down);
        else board.currentState = GameState.move;
    }

    void MovePiecesActual(Vector2 direction) {
        GameObject otherPiece = board.allDots[column + (int)direction.x, row + (int)direction.y];
        
        if (otherPiece != null) {
            Dot otherDot = otherPiece.GetComponent<Dot>();
            
            // 1. Swap Indices
            int tempCol = column;
            int tempRow = row;
            column = otherDot.column;
            row = otherDot.row;
            otherDot.column = tempCol;
            otherDot.row = tempRow;
            
            // 2. CRITICAL: Update Board Array Immediately
            board.allDots[column, row] = this.gameObject;
            board.allDots[otherDot.column, otherDot.row] = otherPiece;

            // 3. Start Check
            StartCoroutine(CheckMoveCo(otherPiece));
        } else {
             board.currentState = GameState.move;
        }
    }

    public IEnumerator CheckMoveCo(GameObject otherPiece) {
        Dot otherDot = otherPiece.GetComponent<Dot>();
        yield return new WaitForSeconds(.2f); // Wait for swap animation
        
        // Check Matches for BOTH pieces
        FindMatches();
        if(otherDot != null) otherDot.FindMatches();

        // --- DECISION TIME ---
        if (!isMatched && !otherDot.isMatched) {
            // INVALID MOVE (Swap Back)
            
            // OPTIONAL: If you want False Moves to count on the counter, uncomment this:
            board.DestroyMatches(); // (This subtracts 1 move even if no match)

            // 1. Swap Indices Back
            int tempCol = column;
            int tempRow = row;
            column = otherDot.column;
            row = otherDot.row;
            otherDot.column = tempCol;
            otherDot.row = tempRow;

            // 2. Swap Board Array Back
            board.allDots[column, row] = this.gameObject;
            board.allDots[otherDot.column, otherDot.row] = otherPiece;

            yield return new WaitForSeconds(.2f); // Wait for slide back
            board.currentState = GameState.move;
        } 
        else {
            // VALID MOVE
            board.DestroyMatches();
        }
    }
    
    // --- NEW ROBUST MATCH FINDER ---
    // Checks Center, Left-Edge, Right-Edge, Top-Edge, Bottom-Edge
    public void FindMatches() {
        // Horizontal Helpers
        GameObject leftDot1 = GetDotAt(column - 1, row);
        GameObject leftDot2 = GetDotAt(column - 2, row);
        GameObject rightDot1 = GetDotAt(column + 1, row);
        GameObject rightDot2 = GetDotAt(column + 2, row);

        // Vertical Helpers
        GameObject upDot1 = GetDotAt(column, row + 1);
        GameObject upDot2 = GetDotAt(column, row + 2);
        GameObject downDot1 = GetDotAt(column, row - 1);
        GameObject downDot2 = GetDotAt(column, row - 2);

        // 1. HORIZONTAL CHECKS
        CheckPattern(leftDot1, leftDot2);   // Match is to the left: [X] [X] X
        CheckPattern(rightDot1, rightDot2); // Match is to the right: X [X] [X]
        CheckPattern(leftDot1, rightDot1);  // Match is in middle: [X] X [X]

        // 2. VERTICAL CHECKS
        CheckPattern(upDot1, upDot2);       // Match is above
        CheckPattern(downDot1, downDot2);   // Match is below
        CheckPattern(upDot1, downDot1);     // Match is in middle
    }

    // Helper to safely get dot without crashing
    GameObject GetDotAt(int c, int r) {
        if(c >= 0 && c < board.width && r >= 0 && r < board.height) {
            return board.allDots[c, r];
        }
        return null;
    }

    // Helper to check if two neighbors match this dot
    void CheckPattern(GameObject dot1, GameObject dot2) {
        if (dot1 != null && dot2 != null) {
            if (dot1.tag == this.gameObject.tag && dot2.tag == this.gameObject.tag) {
                dot1.GetComponent<Dot>().isMatched = true;
                dot2.GetComponent<Dot>().isMatched = true;
                isMatched = true;
            }
        }
    }
}