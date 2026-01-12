using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour {

    public int width = 6;
    public int height = 8;
    
    public GameObject tilePrefab;
    
    // NEW: We need an array to hold the different types of animal prefabs (Blue, Red, Green, etc.)
    public GameObject[] dots;
    
    private GameObject[,] allTiles;
    
    // NEW: We need a second array to track the actual GAME PIECES (the animals), not just the background tiles.
    public GameObject[,] allDots;

    void Start () {
        allTiles = new GameObject[width, height];
        // Initialize the dots array
        allDots = new GameObject[width, height];
        
        Setup();
    }

    private void Setup() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                
                // 1. Create Background Tile
                Vector2 tempPosition = new Vector2(x, y);
                GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;
                backgroundTile.transform.parent = this.transform;
                backgroundTile.name = "( " + x + ", " + y + " )";
                allTiles[x, y] = backgroundTile;
                
                // 2. PREVENT MATCHES (NEW CODE START) ----------------
                int dotToUse = Random.Range(0, dots.Length);
                
                int maxIterations = 0; // Safety break to prevent infinite loops
                while(MatchesAt(x, y, dots[dotToUse]) && maxIterations < 100)
                {
                    dotToUse = Random.Range(0, dots.Length);
                    maxIterations++;
                }
                // (NEW CODE END) -------------------------------------

                // 3. Spawn the Animal
                GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                dot.transform.parent = this.transform;
                dot.name = "Animal ( " + x + ", " + y + " )";
                
                allDots[x, y] = dot;
            }
        }
    }
    
    // NEW HELPER FUNCTION
    // This checks the animal to the left (x-1) and below (y-1)
    private bool MatchesAt(int column, int row, GameObject piece) {
        
        // Check Horizontal (Left)
        if(column > 1) {
            // If the animal 1 spot left AND 2 spots left match the new piece...
            if(allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag) {
                return true;
            }
        }
        
        // Check Vertical (Down)
        if(row > 1) {
            // If the animal 1 spot down AND 2 spots down match the new piece...
            if(allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag) {
                return true;
            }
        }
        
        return false;
    }
}