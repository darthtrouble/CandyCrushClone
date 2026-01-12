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
                
                // 1. Create Background Tile (Same as before)
                Vector2 tempPosition = new Vector2(x, y);
                GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;
                backgroundTile.transform.parent = this.transform;
                backgroundTile.name = "( " + x + ", " + y + " )";
                allTiles[x, y] = backgroundTile;
                
                // 2. NEW: Pick a random animal to spawn
                // Random.Range(0, dots.Length) picks a number between 0 and how many animals you have.
                int dotToUse = Random.Range(0, dots.Length);
                
                // 3. NEW: Spawn the Animal
                GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                dot.transform.parent = this.transform;
                dot.name = "Animal ( " + x + ", " + y + " )";
                
                // 4. Add it to our logic grid
                allDots[x, y] = dot;
            }
        }
    }
}