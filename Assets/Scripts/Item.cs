using UnityEngine;

// Coffee = +1 dash, otherwise Folder = +1 throw.
public class Item : MonoBehaviour
{
    public bool coffee;

    void OnTriggerEnter2D(Collider2D c)
    {
        var p = c.GetComponent<Player>();
        if (p == null) return;
        if (coffee) p.coffee++; else p.folders++;
        Destroy(gameObject);
    }
}
