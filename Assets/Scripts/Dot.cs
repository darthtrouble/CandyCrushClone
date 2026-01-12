using UnityEngine;
using System.Collections;

public class Dot : MonoBehaviour {

    public int column;
    public int row;
    public int targetX;
    public int targetY;
    
    private Board board;
    private GameObject otherDot;
    private float swipeAngle = 0;

    // We will call this FROM the board now
    public void Setup(int x, int y, Board currentBoard) {
        column = x;
        row = y;
        targetX = x;
        targetY = y;
        board = currentBoard;
    }

    void Update () {
        // SMOOTH MOVEMENT (Unchanged)
        targetX = column;
        targetY = row;

        if (Mathf.Abs(targetX - transform.position.x) > .1) {
            Vector2 tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .4f);
        } else {
            Vector2 tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = tempPosition;
        }

        if (Mathf.Abs(targetY - transform.position.y) > .1) {
            Vector2 tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .4f);
        } else {
            Vector2 tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = tempPosition;
        }
    }
    
    // We moved the math here so the Board can call it
    public void CalculateMove(float angle) {
        swipeAngle = angle;
        
        if(swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1) {
            // Right Swipe
            otherDot = board.allDots[column + 1, row];
            otherDot.GetComponent<Dot>().column -= 1;
            column += 1;
        }
        else if(swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1) {
            // Up Swipe
            otherDot = board.allDots[column, row + 1];
            otherDot.GetComponent<Dot>().row -= 1;
            row += 1;
        }
        else if((swipeAngle > 135 || swipeAngle <= -135) && column > 0) {
            // Left Swipe
            otherDot = board.allDots[column - 1, row];
            otherDot.GetComponent<Dot>().column += 1;
            column -= 1;
        }
        else if(swipeAngle < -45 && swipeAngle >= -135 && row > 0) {
            // Down Swipe
            otherDot = board.allDots[column, row - 1];
            otherDot.GetComponent<Dot>().row += 1;
            row -= 1;
        }
        
        StartCoroutine(CheckMoveCo());
    }

    public IEnumerator CheckMoveCo() {
        yield return new WaitForSeconds(.5f);
        board.allDots[column, row] = this.gameObject;
        board.allDots[otherDot.GetComponent<Dot>().column, otherDot.GetComponent<Dot>().row] = otherDot;
    }
}