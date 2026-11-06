using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    public static Game I;
    public static int checkpoint = 1;

    public Player player;
    public Guard[] guards;
    public Transform[] spawns;
    public float[] lengths;
    public float restartDelay = 0.7f, camHeight = 4.2f;

    int floor = 1;
    bool over, won;
    float restartAt;
    Camera cam;

    void Awake() { I = this; cam = Camera.main; }
    void Start() { SetFloor(checkpoint); }

    public void SetFloor(int f)
    {
        if (f > spawns.Length)
        {
            won = true; checkpoint = 1;
            player.gameObject.SetActive(false);
            foreach (var g in guards) g.gameObject.SetActive(false);
            return;
        }
        floor = checkpoint = f;
        player.transform.position = spawns[f - 1].position;
        player.ResetMove();
        foreach (var g in guards) { g.gameObject.SetActive(f >= 2); g.ResetChase(); }
        cam.backgroundColor = Color.black;
    }

    public void GameOver() { over = true; restartAt = Time.time + restartDelay; }

    void Update()
    {
        var k = Keyboard.current;
        if (!over && !won && player.transform.position.y < spawns[floor - 1].position.y - 8) player.Die();
        if (over && (Time.time > restartAt || (k != null && k.rKey.wasPressedThisFrame))) Restart();
        if (won && k != null && k.rKey.wasPressedThisFrame) Restart();
    }

    void Restart() { SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }

    void LateUpdate()
    {
        float y = spawns[Mathf.Min(floor, spawns.Length) - 1].position.y + camHeight;
        cam.transform.position = new Vector3(player.transform.position.x + 5f, y, -10f);
    }

    void OnGUI()
    {

        var s = new GUIStyle(GUI.skin.label) { fontSize = 26, fontStyle = FontStyle.Bold };
        s.normal.textColor = Color.white;
        var box = Texture2D.whiteTexture;
        if (won)
        {
            GUI.color = Color.black; GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), box); GUI.color = Color.white;
            s.fontSize = 60; s.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "YOU ESCAPED THE OFFICE\n<size=26>press R to run again</size>", s);
            return;
        }
        GUI.Label(new Rect(24, 12, 600, 40), "FLOOR " + floor + " / 10" + (floor >= 6 ? "   - NIGHT SHIFT -" : ""), s);
        s.fontSize = 20;
        GUI.Label(new Rect(Screen.width - 330, 16, 310, 40), "COFFEE x" + player.coffee + "    FOLDERS x" + player.folders, s);


        float t = Mathf.Clamp01((player.transform.position.x - spawns[floor - 1].position.x) / lengths[floor - 1]);
        var bar = new Rect(26, 52, 320, 10);
        GUI.color = Color.white; GUI.DrawTexture(bar, box);
        GUI.color = Color.black; GUI.DrawTexture(new Rect(bar.x + 2, bar.y + 2, bar.width - 4, bar.height - 4), box);
        GUI.color = Color.white; GUI.DrawTexture(new Rect(bar.x + 2, bar.y + 2, (bar.width - 4) * t, bar.height - 4), box);

        if (over)
        {
            GUI.color = new Color(0, 0, 0, 0.85f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), box);
            GUI.color = Color.white;
            s.fontSize = 64; s.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(0, -30, Screen.width, Screen.height), "ORDER FOR 100M DEBT", s);
            s.fontSize = 22; s.fontStyle = FontStyle.Normal;
            GUI.Label(new Rect(0, 50, Screen.width, Screen.height), "you were caught. restarting...", s);
        }
    }
}
