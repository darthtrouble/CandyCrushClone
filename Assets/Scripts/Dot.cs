using UnityEngine;
using System.Collections;

public class Dot : MonoBehaviour {

    [Header("Board Variables")]
    public int column;
    public int row;
    public int previousColumn;
    public int previousRow;
    public int targetX;
    public int targetY;
    public bool isMatched = false;

    private Board board;
    private GameObject otherDot;
    private float swipeAngle = 0;

    public void Setup(int x, int y, Board currentBoard) {
        column = x;
        row = y;
        targetX = x;
        targetY = y;
        previousColumn = x;
        previousRow = y;
        board = currentBoard;
    }

    void Update () {
        // TUNED: Increased speed from .4f to .6f for a snappier slide
        targetX = column;
        targetY = row;

        if (Mathf.Abs(targetX - transform.position.x) > .1) {
            Vector2 tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .6f);
        } else {
            Vector2 tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = tempPosition;
        }

        if (Mathf.Abs(targetY - transform.position.y) > .1) {
            Vector2 tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .6f);
        } else {
            Vector2 tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = tempPosition;
        }
    }
    
    public void CalculateMove(float angle) {
        previousColumn = column;
        previousRow = row;
        swipeAngle = angle;
        
        if(swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1) {
            otherDot = board.allDots[column + 1, row];
            otherDot.GetComponent<Dot>().column -= 1;
            column += 1;
        }
        else if(swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1) {
            otherDot = board.allDots[column, row + 1];
            otherDot.GetComponent<Dot>().row -= 1;
            row += 1;
        }
        else if((swipeAngle > 135 || swipeAngle <= -135) && column > 0) {
            otherDot = board.allDots[column - 1, row];
            otherDot.GetComponent<Dot>().column += 1;
            column -= 1;
        }
        else if(swipeAngle < -45 && swipeAngle >= -135 && row > 0) {
            otherDot = board.allDots[column, row - 1];
            otherDot.GetComponent<Dot>().row += 1;
            row -= 1;
        }
        
        StartCoroutine(CheckMoveCo());
    }

    public void FindMatches() {
        isMatched = false;

        // Horizontal
        if (column > 0 && column < board.width - 1) {
            GameObject leftDot1 = board.allDots[column - 1, row];
            GameObject rightDot1 = board.allDots[column + 1, row];
            if(leftDot1 != null && rightDot1 != null) {
                if (leftDot1.tag == this.gameObject.tag && rightDot1.tag == this.gameObject.tag) {
                    leftDot1.GetComponent<Dot>().isMatched = true;
                    rightDot1.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }
        }
        if (column < board.width - 2) {
            GameObject rightDot1 = board.allDots[column + 1, row];
            GameObject rightDot2 = board.allDots[column + 2, row];
            if(rightDot1 != null && rightDot2 != null) {
                if (rightDot1.tag == this.gameObject.tag && rightDot2.tag == this.gameObject.tag) {
                    rightDot1.GetComponent<Dot>().isMatched = true;
                    rightDot2.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }
        }
        if (column > 1) {
            GameObject leftDot1 = board.allDots[column - 1, row];
            GameObject leftDot2 = board.allDots[column - 2, row];
            if(leftDot1 != null && leftDot2 != null) {
                if (leftDot1.tag == this.gameObject.tag && leftDot2.tag == this.gameObject.tag) {
                    leftDot1.GetComponent<Dot>().isMatched = true;
                    leftDot2.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }
        }

        // Vertical
        if (row > 0 && row < board.height - 1) {
            GameObject upDot1 = board.allDots[column, row + 1];
            GameObject downDot1 = board.allDots[column, row - 1];
            if(upDot1 != null && downDot1 != null) {
                if (upDot1.tag == this.gameObject.tag && downDot1.tag == this.gameObject.tag) {
                    upDot1.GetComponent<Dot>().isMatched = true;
                    downDot1.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }
        }
        if (row < board.height - 2) {
            GameObject upDot1 = board.allDots[column, row + 1];
            GameObject upDot2 = board.allDots[column, row + 2];
            if(upDot1 != null && upDot2 != null) {
                if (upDot1.tag == this.gameObject.tag && upDot2.tag == this.gameObject.tag) {
                    upDot1.GetComponent<Dot>().isMatched = true;
                    upDot2.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }
        }
        if (row > 1) {
            GameObject downDot1 = board.allDots[column, row - 1];
            GameObject downDot2 = board.allDots[column, row - 2];
            if(downDot1 != null && downDot2 != null) {
                if (downDot1.tag == this.gameObject.tag && downDot2.tag == this.gameObject.tag) {
                    downDot1.GetComponent<Dot>().isMatched = true;
                    downDot2.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }
        }
    }

    public IEnumerator CheckMoveCo() {
        // TUNED: Reduced wait time to 0.3s
        yield return new WaitForSeconds(.3f);
        
        board.allDots[column, row] = this.gameObject;
        board.allDots[otherDot.GetComponent<Dot>().column, otherDot.GetComponent<Dot>().row] = otherDot;
        
        FindMatches();
        otherDot.GetComponent<Dot>().FindMatches();
        
        if(!isMatched && !otherDot.GetComponent<Dot>().isMatched) {
            otherDot.GetComponent<Dot>().row = row;
            otherDot.GetComponent<Dot>().column = column;
            row = previousRow;
            column = previousColumn;
            
            // TUNED: Reduced wait time to 0.3s
            yield return new WaitForSeconds(.3f);
            
            board.allDots[column, row] = this.gameObject;
            board.allDots[otherDot.GetComponent<Dot>().column, otherDot.GetComponent<Dot>().row] = otherDot;
            board.currentState = GameState.move;
        } else {
            board.DestroyMatches();
        }
    }
}