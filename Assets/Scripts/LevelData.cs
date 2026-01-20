using UnityEngine;
using System.Collections.Generic; // Needed for Lists

[CreateAssetMenu(fileName = "New Level", menuName = "Level Data")]
public class LevelData : ScriptableObject {
    
    [Header("Board Dimensions")]
    public int width = 6;
    public int height = 8;
    
    [Header("Goals")]
    public int moves = 20;
    public int scoreGoal = 1000;

    [Header("Obstacles")]
    // List of coordinates that should start as ICE
    // Example: X=0, Y=0 is bottom left
    public List<Vector2> iceTiles; 
}