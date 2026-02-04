using UnityEngine;

/// <summary>
/// Centralized manager for saving and loading level progress
/// Handles star ratings, unlocks, and total progress tracking
/// </summary>
public class LevelProgressManager : MonoBehaviour {
    
    private static LevelProgressManager instance;
    
    // PlayerPrefs keys
    private const string UNLOCKED_LEVEL_KEY = "UnlockedLevel";
    private const string STAR_PREFIX = "Level_";
    private const string STAR_SUFFIX = "_Stars";
    
    void Awake() {
        // Singleton pattern
        if (instance != null) {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    // ===== STAR RATINGS =====
    
    /// <summary>
    /// Save star rating for a specific level
    /// </summary>
    public static void SaveStars(int levelIndex, int stars) {
        // Only save if it's better than previous
        int currentStars = GetStarsForLevel(levelIndex);
        if (stars > currentStars) {
            PlayerPrefs.SetInt(STAR_PREFIX + levelIndex + STAR_SUFFIX, stars);
            PlayerPrefs.Save();
        }
    }
    
    /// <summary>
    /// Get star rating for a specific level (0-3)
    /// </summary>
    public static int GetStarsForLevel(int levelIndex) {
        return PlayerPrefs.GetInt(STAR_PREFIX + levelIndex + STAR_SUFFIX, 0);
    }
    
    /// <summary>
    /// Get total stars earned across all levels
    /// </summary>
    public static int GetTotalStars(int maxLevels) {
        int total = 0;
        for (int i = 0; i < maxLevels; i++) {
            total += GetStarsForLevel(i);
        }
        return total;
    }
    
    // ===== LEVEL UNLOCKING =====
    
    /// <summary>
    /// Get the highest level the player has unlocked (1-based)
    /// </summary>
    public static int GetUnlockedLevel() {
        return PlayerPrefs.GetInt(UNLOCKED_LEVEL_KEY, 1);
    }
    
    /// <summary>
    /// Unlock the next level
    /// </summary>
    public static void UnlockNextLevel(int currentLevelIndex) {
        int unlockedLevel = GetUnlockedLevel();
        int nextLevel = currentLevelIndex + 2; // +1 for index, +1 for next
        
        if (nextLevel > unlockedLevel) {
            PlayerPrefs.SetInt(UNLOCKED_LEVEL_KEY, nextLevel);
            PlayerPrefs.Save();
        }
    }
    
    /// <summary>
    /// Check if a specific level is unlocked
    /// </summary>
    public static bool IsLevelUnlocked(int levelIndex) {
        return (levelIndex + 1) <= GetUnlockedLevel();
    }
    
    // ===== PROGRESS RESET =====
    
    /// <summary>
    /// Reset all progress (for testing or player request)
    /// </summary>
    public static void ResetAllProgress() {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Reset only star ratings, keep unlocks
    /// </summary>
    public static void ResetStars(int maxLevels) {
        for (int i = 0; i < maxLevels; i++) {
            PlayerPrefs.DeleteKey(STAR_PREFIX + i + STAR_SUFFIX);
        }
        PlayerPrefs.Save();
    }
}
