using UnityEngine;

public class Parallax : MonoBehaviour
{
    public float f = 0.5f;
    Transform cam;
    float x0;

    void Start() { cam = Camera.main.transform; x0 = transform.position.x; }
    void LateUpdate() { transform.position = new Vector3(x0 + cam.position.x * f, transform.position.y, 0); }
}
