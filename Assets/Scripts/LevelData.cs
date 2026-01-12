using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "Level Data")]
public class LevelData : ScriptableObject {
    public int width = 6;
    public int height = 8;
    public int moves = 20;
    public int scoreGoal = 1000;
    // You can add more things here later, like "Background Image" or "Allowed Animals"
}