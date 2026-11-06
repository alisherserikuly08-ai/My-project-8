using UnityEngine;

public class Folder : MonoBehaviour
{
    public float speed = 15f;

    void Start()
    {
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.left * speed;
        Destroy(gameObject, 2f);
    }

    void OnTriggerEnter2D(Collider2D c)
    {
        var g = c.GetComponent<Guard>();
        if (g != null) { g.Push(); Destroy(gameObject); }
        else if (c.gameObject.layer == LayerMask.NameToLayer("Ground")) Destroy(gameObject);
    }
}
