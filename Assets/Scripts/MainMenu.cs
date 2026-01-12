using UnityEngine;
using UnityEngine.SceneManagement; // Required for changing scenes

public class MainMenu : MonoBehaviour {

    public void PlayGame() {
        // This must match the EXACT name of your game scene
        SceneManager.LoadScene("GameLevel");
    }

    public void QuitGame() {
        Debug.Log("Quit Game!");
        Application.Quit();
    }
}