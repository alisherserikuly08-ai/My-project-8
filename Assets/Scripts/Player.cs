using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float speed = 7f, dashSpeed = 15f, jumpForce = 13f, wallSlide = 2f, slipTime = 0.6f;
    public int coffee, folders;
    public Sprite folderSprite;
    public LayerMask ground;
    public bool dead;
    public bool Dashing => dash > 0;

    Rigidbody2D rb;
    BoxCollider2D box;
    int dir = 1;
    float dash, slip;

    void Awake() { rb = GetComponent<Rigidbody2D>(); box = GetComponent<BoxCollider2D>(); }

    public void ResetMove()
    {
        rb.linearVelocity = Vector2.zero;
        dir = 1;
        dash = slip = 0;
        transform.localScale = Vector3.one;
    }

    void Update()
    {
        var k = Keyboard.current;
        if (dead || k == null) return;
        var v = rb.linearVelocity;
        bool grounded = Cast(Vector2.down);
        bool wall = Cast(Vector2.right * dir);
        if (grounded && v.y <= 0.01f) dir = 1;

        if (slip <= 0 && (k.spaceKey.wasPressedThisFrame || k.wKey.wasPressedThisFrame))
        {
            if (grounded) v.y = jumpForce;
            else if (wall) { dir = -dir; v.y = jumpForce; }
        }
        if (wall && !grounded && v.y < -wallSlide) v.y = -wallSlide;

        float h = box.size.y;
        Vector2 feet = transform.position;
        bool roof = Physics2D.OverlapBox(feet + Vector2.up * h * 0.75f, new Vector2(box.size.x * 0.9f, h * 0.45f), 0, ground);
        bool low = k.sKey.isPressed || (transform.localScale.y < 1 && roof);
        transform.localScale = new Vector3(1, low ? 0.5f : 1, 1);

        if (k.dKey.wasPressedThisFrame && coffee > 0 && dash <= 0) { coffee--; dash = 0.5f; }
        var m = Mouse.current;
        if (m != null && m.leftButton.wasPressedThisFrame && folders > 0) { folders--; Throw(); }

        v.x = slip > 0 ? dir * speed * 0.35f : dir * (dash > 0 ? dashSpeed : speed);
        rb.linearVelocity = v;
        dash -= Time.deltaTime;
        slip -= Time.deltaTime;
    }

    bool Cast(Vector2 d)
    {
        var b = box.bounds;
        return Physics2D.BoxCast(b.center, b.size * 0.9f, 0, d, 0.1f, ground);
    }

    void Throw()
    {
        var f = new GameObject("Folder");
        f.transform.position = transform.position + Vector3.up * 0.6f * transform.localScale.y;
        f.transform.localScale = new Vector3(0.35f, 0.25f, 1);
        var r = f.AddComponent<SpriteRenderer>();
        r.sprite = folderSprite;
        r.color = Color.black;
        r.sortingOrder = 8;
        f.AddComponent<Rigidbody2D>().gravityScale = 0;
        f.AddComponent<BoxCollider2D>().isTrigger = true;
        f.AddComponent<Folder>();
    }

    void OnTriggerEnter2D(Collider2D c)
    {
        if (c.CompareTag("Lethal") || c.CompareTag("Guard")) Die();
        else if (c.CompareTag("Puddle") && dash <= 0) slip = slipTime;
    }

    public void Die()
    {
        if (dead) return;
        dead = true;
        rb.simulated = false;
        Game.I.GameOver();
    }
}
