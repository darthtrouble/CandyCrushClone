using UnityEngine;
using System.Collections.Generic;

// Objective types for varied gameplay
public enum LevelObjectiveType {
    Score,          // Reach a target score
    CollectAnimals, // Collect X of specific animal colors
    ClearIce,       // Destroy all ice tiles
    TimedChallenge  // Reach score within time limit
}

// Individual objective definition
[System.Serializable]
public class LevelObjective {
    public LevelObjectiveType objectiveType;
    
    [Header("Score Objective")]
    public int targetScore; // For Score and TimedChallenge types
    
    [Header("Collect Animals Objective")]
    public string animalTag; // e.g., "Red", "Blue", "Green"
    public int targetAmount; // How many to collect
    
    [Header("Ice Objective")]
    // No additional fields needed - ClearIce uses iceTiles list from LevelData
    
    [Header("Timed Challenge")]
    public float timeLimit = 60f; // Seconds for timed challenges
    
    // Helper to get description
    public string GetDescription() {
        switch (objectiveType) {
            case LevelObjectiveType.Score:
                return $"Reach {targetScore} points";
            case LevelObjectiveType.CollectAnimals:
                return $"Collect {targetAmount} {animalTag} animals";
            case LevelObjectiveType.ClearIce:
                return $"Clear all {targetAmount} ice blocks";
            case LevelObjectiveType.TimedChallenge:
                // Only show score target if it's required (greater than 0)
                if (targetScore > 0)
                    return $"Reach {targetScore} points in {timeLimit}s";
                else
                    return $"Survive for {timeLimit}s";
            default:
                return "Unknown Objective";
        }
    }
}

[CreateAssetMenu(fileName = "New Level", menuName = "Level Data")]
public class LevelData : ScriptableObject {
    
    [Header("Board Dimensions")]
    public int width = 6;
    public int height = 8;
    
    [Header("Moves")]
    public int moves = 20;
    
    [Header("Level Objectives")]
    [Tooltip("Add one or more objectives for this level")]
    public List<LevelObjective> objectives = new List<LevelObjective>();
    
    [Header("Star Rating Thresholds")]
    [Tooltip("Score needed for 1 star (minimum to win)")]
    public int oneStarScore = 500;
    [Tooltip("Score needed for 2 stars")]
    public int twoStarScore = 1000;
    [Tooltip("Score needed for 3 stars")]
    public int threeStarScore = 1500;

    [Header("Obstacles")]
    [Tooltip("List of coordinates that should start as ICE")]
    public List<Vector2> iceTiles;
    
    [Header("Difficulty (Visual Only)")]
    [Tooltip("For display in level select")]
    public string difficulty = "Easy"; // Easy, Medium, Hard
    
    // Helper method to check if level has specific objective type
    public bool HasObjective(LevelObjectiveType type) {
        foreach (var obj in objectives) {
            if (obj.objectiveType == type) return true;
        }
        return false;
    }
    
    // Get specific objective by type
    public LevelObjective GetObjective(LevelObjectiveType type) {
        foreach (var obj in objectives) {
            if (obj.objectiveType == type) return obj;
        }
        return null;
    }
    
    // Calculate star rating based on score
    public int GetStarRating(int score) {
        if (score >= threeStarScore) return 3;
        if (score >= twoStarScore) return 2;
        if (score >= oneStarScore) return 1;
        return 0;
    }
}