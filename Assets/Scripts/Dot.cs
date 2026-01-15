using UnityEngine;
using System.Collections;

public class Dot : MonoBehaviour {

    [Header("Board Variables")]
    public int column;
    public int row;
    public int targetX;
    public int targetY;
    public bool isMatched = false;
    
    private Board board;

    // Call this immediately after instantiating
    public void Setup(int x, int y, Board boardRef) {
        column = x;
        row = y;
        board = boardRef;
    }

    // Update is called once per frame
    void Update() {
        if(board == null) return;

        // Use the Board's offset to calculate the REAL position in the world
        float realTargetX = column - board.centerOffset.x;
        float realTargetY = row - board.centerOffset.y;

        // --- Move X ---
        if (Mathf.Abs(realTargetX - transform.position.x) > .1f) {
            // Smoothly slide towards target
            Vector2 tempPosition = new Vector2(realTargetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .6f);
            
            if (board.allDots[column, row] != this.gameObject) {
                board.allDots[column, row] = this.gameObject;
            }
        } else {
            // Snap exactly to position if close enough
            Vector2 tempPosition = new Vector2(realTargetX, transform.position.y);
            transform.position = tempPosition;
        }
        
        // --- Move Y ---
        if (Mathf.Abs(realTargetY - transform.position.y) > .1f) {
            // Smoothly slide towards target
            Vector2 tempPosition = new Vector2(transform.position.x, realTargetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .6f);
            
            if (board.allDots[column, row] != this.gameObject) {
                board.allDots[column, row] = this.gameObject;
            }
        } else {
            // Snap exactly to position if close enough
            Vector2 tempPosition = new Vector2(transform.position.x, realTargetY);
            transform.position = tempPosition;
        }
    }

    public void CalculateMove(float swipeAngle) {
        // We receive the angle from Board.cs, so we just decide the direction here
        if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1) {
            // Right Swipe
            MovePiecesActual(Vector2.right);
        } else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1) {
            // Up Swipe
            MovePiecesActual(Vector2.up);
        } else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0) {
            // Left Swipe
            MovePiecesActual(Vector2.left);
        } else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0) {
            // Down Swipe
            MovePiecesActual(Vector2.down);
        }
        
        StartCoroutine(CheckMoveCo());
    }

    void MovePiecesActual(Vector2 direction) {
        GameObject otherDot = board.allDots[column + (int)direction.x, row + (int)direction.y];
        if (otherDot != null) {
            // Swap logic: We update the column/row variables.
            // The Update() loop above handles the actual visual movement automatically.
            Dot otherDotScript = otherDot.GetComponent<Dot>();
            
            int tempCol = column;
            int tempRow = row;
            
            column = otherDotScript.column;
            row = otherDotScript.row;
            
            otherDotScript.column = tempCol;
            otherDotScript.row = tempRow;
        }
    }

    public IEnumerator CheckMoveCo() {
        yield return new WaitForSeconds(.5f);
        if (board != null) {
            // If neither piece matched, we should swap back (Logic simplified for now)
            if (!isMatched && !board.allDots[column, row].GetComponent<Dot>().isMatched) {
               // In a full game, you would call a "SwapBack" function here.
               // For now, we just let the board process matches.
            } 
            
            board.DestroyMatches();
        }
    }
    
    public void FindMatches() {
        if (column > 0 && column < board.width - 1) {
            GameObject leftDot1 = board.allDots[column - 1, row];
            GameObject rightDot1 = board.allDots[column + 1, row];
            if (leftDot1 != null && rightDot1 != null) {
                if (leftDot1.tag == this.gameObject.tag && rightDot1.tag == this.gameObject.tag) {
                    leftDot1.GetComponent<Dot>().isMatched = true;
                    rightDot1.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }
        }
        if (row > 0 && row < board.height - 1) {
            GameObject upDot1 = board.allDots[column, row + 1];
            GameObject downDot1 = board.allDots[column, row - 1];
            if (upDot1 != null && downDot1 != null) {
                if (upDot1.tag == this.gameObject.tag && downDot1.tag == this.gameObject.tag) {
                    upDot1.GetComponent<Dot>().isMatched = true;
                    downDot1.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }
        }
    }
}