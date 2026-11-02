using UnityEngine;

public class ProceduralRun : MonoBehaviour
{
    public Transform rig, thighL, thighR, calfL, calfR, armL, armR, foreL, foreR, tie;
    public float rate = 13f;
    Vector3 last;

    void LateUpdate()
    {
        float dt = Time.deltaTime;
        Vector3 v = (transform.position - last) / Mathf.Max(dt, 0.001f);
        last = transform.position;
        if (Mathf.Abs(v.x) > 0.5f) rig.localScale = new Vector3(Mathf.Sign(v.x), 1, 1);

        bool air = Mathf.Abs(v.y) > 1f;
        float p = Time.time * rate;
        float op = p + Mathf.PI;

        Rot(thighL, air ? 35 : Mathf.Sin(p) * 40, dt);
        Rot(thighR, air ? -20 : Mathf.Sin(op) * 40, dt);
        Rot(calfL, air ? -60 : -Mathf.Max(0, Mathf.Sin(p - 1.2f)) * 55, dt);
        Rot(calfR, air ? -40 : -Mathf.Max(0, Mathf.Sin(op - 1.2f)) * 55, dt);
        Rot(armL, air ? 150 : Mathf.Sin(op) * 35, dt);
        Rot(armR, air ? 120 : Mathf.Sin(p) * 35, dt);
        Rot(foreL, air ? -40 : -50 - Mathf.Max(0, Mathf.Sin(op - 1f)) * 30, dt);
        Rot(foreR, air ? -30 : -50 - Mathf.Max(0, Mathf.Sin(p - 1f)) * 30, dt);

        Vector3 target = new Vector3(0, air ? 0.08f : Mathf.Abs(Mathf.Cos(p)) * 0.05f, 0);
        rig.localPosition = Vector3.Lerp(rig.localPosition, target, dt * 12);
        if (tie) tie.localRotation = Quaternion.Euler(0, 0, -70 - Mathf.Min(Mathf.Abs(v.x) * 4, 30) + Mathf.Sin(p * 2) * 8);
    }

    void Rot(Transform t, float angle, float dt)
    {
        t.localRotation = Quaternion.Slerp(t.localRotation, Quaternion.Euler(0, 0, angle), dt * 14);
    }
}
