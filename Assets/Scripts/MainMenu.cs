using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    // This function is called when you click "Play"
    public void PlayGame() {
        // Load the Level Select screen we just made
        SceneManager.LoadScene("LevelSelect");
    }

    public void GoToSettings() {
    SceneManager.LoadScene("Settings");
    }

    // This function is called when you click "Quit"
    public void QuitGame() {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}