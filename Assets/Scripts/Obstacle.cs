using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public bool glass;
    public float roll;
    Rigidbody2D rb;
    float startX;

    void Awake() { rb = GetComponent<Rigidbody2D>(); startX = transform.position.x; }

    void FixedUpdate()
    {
        if (roll <= 0) return;
        bool go = transform.position.x - Game.I.player.transform.position.x < 5 && transform.position.x > startX - 1.5f;
        rb.linearVelocity = go ? Vector2.left * roll : Vector2.zero;
    }

    void OnCollisionStay2D(Collision2D c)
    {
        var p = c.collider.GetComponent<Player>();
        if (!glass || p == null || !p.Dashing) return;
        var sr = GetComponent<SpriteRenderer>();
        var b = sr.bounds;
        for (int i = 0; i < 16; i++)
        {
            var s = new GameObject("Shard");
            s.transform.position = new Vector3(Random.Range(b.min.x, b.max.x), Random.Range(b.min.y, b.min.y + 3f));
            s.transform.localScale = Vector3.one * Random.Range(0.08f, 0.3f);
            s.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
            var r = s.AddComponent<SpriteRenderer>();
            r.sprite = sr.sprite;
            r.color = sr.color;
            r.sortingOrder = 9;
            var srb = s.AddComponent<Rigidbody2D>();
            srb.AddForce(new Vector2(Random.Range(3f, 10f), Random.Range(0f, 6f)), ForceMode2D.Impulse);
            srb.angularVelocity = Random.Range(-720f, 720f);
            Destroy(s, 2f);
        }
        Destroy(gameObject);
    }
}
