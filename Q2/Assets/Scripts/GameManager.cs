using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Rigidbody player;
    public GameObject powerUp;
    public int totalCubes = 6;
    public bool started;
    public bool powerActive;
    public bool won;

    int collected;

    public void CollectCube()
    {
        collected++;
        if (collected == totalCubes)
            powerUp.SetActive(true);
    }

    void OnGUI()
    {
        GUI.skin.label.fontSize = 22;
        GUI.skin.button.fontSize = 48;

        GUI.Label(new Rect(20, 20, 300, 35), "Cubes: " + collected + " / " + totalCubes);

        if (powerActive)
            GUI.Label(new Rect(20, 60, 300, 35), "Power-up active!");

        if (!started)
        {
            if (GUI.Button(new Rect(Screen.width / 2f - 160, Screen.height / 2f - 55, 320, 110), "Start"))
            {
                started = true;
                player.useGravity = true;
            }
        }

        if (won)
        {
            GUIStyle winStyle = new GUIStyle(GUI.skin.label);
            winStyle.fontSize = 64;
            winStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(Screen.width / 2f - 200, Screen.height / 2f - 60, 400, 120), "You win!", winStyle);
        }
    }
}
