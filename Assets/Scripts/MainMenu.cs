using UnityEngine;
using UnityEngine.SceneManagement; 

public class MainMenu : MonoBehaviour {

    [Header("Configuration")]
    public LevelData[] levels; // Drag your ScriptableObjects here!
    public GameObject buttonPrefab;
    public Transform gridParent; // The "LevelGrid" panel

    void Start() {
        // How many levels have we unlocked? (Default is 1, which means Index 0)
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        for (int i = 0; i < levels.Length; i++) {
            GameObject btnObj = Instantiate(buttonPrefab, gridParent);
            LevelButton btnScript = btnObj.GetComponent<LevelButton>();
            
            // Check if this specific level is unlocked
            // i + 1 because "UnlockedLevel" stores the COUNT (1, 2, 3), not the index
            bool isUnlocked = (i + 1) <= unlockedLevel;
            
            btnScript.Setup(i, isUnlocked);
        }
    }

    public void QuitGame() {
        Application.Quit();
    }
    
    // Cheat function to reset progress (Attach to a hidden button for testing)
    public void ResetProgress() {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}