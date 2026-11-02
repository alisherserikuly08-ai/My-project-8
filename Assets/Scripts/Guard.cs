using System.Collections.Generic;
using UnityEngine;

public class Guard : MonoBehaviour
{
    public Transform player;
    public float delay = 0.6f, minDelay = 0.38f, catchUp = 0.01f, push = 0.4f, speed = 7f;

    List<Vector3> trail = new List<Vector3>();
    float start;

    void Awake() { start = delay; }

    public void ResetChase()
    {
        trail.Clear();
        delay = start;
        transform.position = player.position + Vector3.left * delay * speed;
    }

    public void Push() { delay = Mathf.Min(delay + push, 3f); }

    void FixedUpdate()
    {
        trail.Add(player.position);
        if (trail.Count > 400) trail.RemoveAt(0);
        delay = Mathf.Max(minDelay, delay - catchUp * Time.fixedDeltaTime);
        int i = trail.Count - 1 - Mathf.RoundToInt(delay / Time.fixedDeltaTime);
        transform.position = i < 0 ? player.position + Vector3.left * delay * speed : trail[i];
    }
}
