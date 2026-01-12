using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour {

    // DEFINING VARIABLES
    // 'public' means we can see and change these numbers in the Unity Editor Inspector.
    public int width = 6;
    public int height = 8;
    
    // This holds the blueprint for the background tile we created in Step 5.
    public GameObject tilePrefab;
    
    // We will create a 2D array (a grid) to store our background tiles.
    // Think of this like an Excel sheet: tiles[3, 4]
    private GameObject[,] allTiles;

    // Start is called before the first frame update
    void Start () {
        // Initialize the array with the size we chose.
        allTiles = new GameObject[width, height];
        
        // Run the function to build the board
        Setup();
    }

    private void Setup() {
        // Nested Loop: The outer loop counts the Width (Columns)
        for (int x = 0; x < width; x++) {
            // The inner loop counts the Height (Rows)
            for (int y = 0; y < height; y++) {
                
                // 1. Calculate the position for this tile.
                // We use (x, y) coordinates. 
                // We cast to Vector2 because Unity uses X/Y/Z, but we are in 2D.
                Vector2 tempPosition = new Vector2(x, y);
                
                // 2. Spawn the tile!
                // Instantiate(WhatToSpawn, WhereToSpawn, Rotation)
                // Quaternion.identity means "No rotation".
                GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;
                
                // 3. Keep the Hierarchy clean.
                // Make the new tile a "child" of the Board object so our list isn't messy.
                backgroundTile.transform.parent = this.transform;
                
                // 4. Name the tile (Optional, but helps debugging)
                backgroundTile.name = "( " + x + ", " + y + " )";
                
                // 5. Store it in our array so we can find it later
                allTiles[x, y] = backgroundTile;
            }
        }
    }
}