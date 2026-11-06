using UnityEngine;

public class Level : MonoBehaviour
{
    public int nextFloor = 2;

    void OnTriggerEnter2D(Collider2D c)
    {
        if (c.CompareTag("Player")) Game.I.SetFloor(nextFloor);
    }
}
