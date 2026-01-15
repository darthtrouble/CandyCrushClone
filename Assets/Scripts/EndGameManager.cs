using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EndGameManager : MonoBehaviour {

    [Header("UI Elements")]
    public GameObject endPanel;
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI scoreText;
    
    [Header("Stars")]
    public Image[] starImages;
    public Sprite goldStarSprite;
    public Sprite grayStarSprite;

    [Header("Buttons")]
    public GameObject nextLevelButton;
    public GameObject replayButton;
    public GameObject menuButton;

    public float oneStarPct = 0.5f;
    public float twoStarPct = 0.75f;

    public void ShowWin(int currentScore, int levelGoal) {
        Debug.Log("EndGameManager: ShowWin called!");

        if (endPanel == null) {
            Debug.LogError("ERROR: 'End Panel' slot is empty in EndGameManager!");
            return;
        }

        endPanel.SetActive(true);
        Debug.Log("EndGameManager: Panel set to Active.");

        if (messageText != null) messageText.text = "LEVEL COMPLETE!";
        if (scoreText != null) scoreText.text = "Score: " + currentScore;

        // Reset stars
        if (starImages != null) {
            foreach(Image img in starImages) {
                if(img != null) {
                    img.sprite = grayStarSprite;
                    img.color = Color.white;
                    img.transform.localScale = Vector3.zero;
                }
            }
        }

        // Handle Buttons
        if(nextLevelButton) nextLevelButton.SetActive(true);
        if(replayButton) replayButton.SetActive(true);
        if(menuButton) menuButton.SetActive(true);

        StartCoroutine(AnimateStars(currentScore, levelGoal));
    }

    public void ShowLose(int currentScore) {
        if (endPanel == null) return;
        
        endPanel.SetActive(true);
        if (messageText != null) messageText.text = "OUT OF MOVES";
        if (scoreText != null) scoreText.text = "Score: " + currentScore;
        
        if (starImages != null) {
            foreach(Image img in starImages) {
                if(img) img.gameObject.SetActive(false);
            }
        }
        
        if(nextLevelButton) nextLevelButton.SetActive(false);
        if(replayButton) replayButton.SetActive(true);
        if(menuButton) menuButton.SetActive(true);
    }

    IEnumerator AnimateStars(int score, int goal) {
        int starsEarned = 0;
        if (score >= goal * oneStarPct) starsEarned = 1;
        if (score >= goal * twoStarPct) starsEarned = 2;
        if (score >= goal) starsEarned = 3;

        for (int i = 0; i < starsEarned; i++) {
            yield return new WaitForSeconds(0.5f);
            
            if(starImages != null && i < starImages.Length && starImages[i] != null) {
                starImages[i].gameObject.SetActive(true);
                starImages[i].sprite = goldStarSprite;
                
                float t = 0;
                while(t < 1) {
                    t += Time.deltaTime * 5f;
                    starImages[i].transform.localScale = Vector3.one * Mathf.Lerp(0f, 1.2f, t);
                    yield return null;
                }
                starImages[i].transform.localScale = Vector3.one;
            }
        }
    }
}