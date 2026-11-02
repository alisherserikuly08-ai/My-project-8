using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class GameBuilder
{
    const float H = 30f, Top = 9f;
    static readonly Color K = new Color(0.02f, 0.02f, 0.025f);
    static Sprite sq, dot, glow;
    static Transform root;
    static bool night;
    static int ground;
    static Color wallC, farC, screenC, glassC, ledC, textC, trimC;

    [MenuItem("Tools/Build Dynamic Vector Office")]
    static void Build()
    {
        if (EditorApplication.isCompiling || EditorApplication.isPlaying) return;
        Setup();
        Sprites();
        foreach (var n in new[] { "World", "Player", "Guard 1", "Guard 2", "Game" })
        {
            var o = GameObject.Find(n);
            if (o) Object.DestroyImmediate(o);
        }

        var p = Man("Player", false);
        p.tag = "Player";
        var rb = p.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var mat = new PhysicsMaterial2D("Slick") { friction = 0, bounciness = 0 };
        AssetDatabase.CreateAsset(mat, "Assets/Generated/Slick.physicsMaterial2D");
        p.GetComponent<BoxCollider2D>().sharedMaterial = mat;
        var player = p.AddComponent<Player>();
        player.ground = 1 << ground;
        player.folderSprite = sq;

        var guards = new Guard[2];
        for (int i = 0; i < 2; i++)
        {
            var g = Man("Guard " + (i + 1), true);
            g.tag = "Guard";
            g.GetComponent<BoxCollider2D>().isTrigger = true;
            g.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            guards[i] = g.AddComponent<Guard>();
            guards[i].player = p.transform;
            guards[i].delay = 0.6f + i * 0.08f;
            guards[i].minDelay = 0.38f + i * 0.08f;
        }

        root = new GameObject("World").transform;
        var spawns = new Transform[10];
        var lengths = new float[10];
        for (int f = 1; f <= 10; f++) lengths[f - 1] = Floor(f, out spawns[f - 1]);

        var game = new GameObject("Game").AddComponent<Game>();
        game.player = player;
        game.guards = guards;
        game.spawns = spawns;
        game.lengths = lengths;
        var cam = Camera.main;
        cam.orthographic = true;
        cam.orthographicSize = 5.625f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.transform.position = new Vector3(5, game.camHeight, -10);
        p.transform.position = spawns[0].position;

        AssetDatabase.SaveAssets();
        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!list.Exists(s => s.path == scene.path)) list.Insert(0, new EditorBuildSettingsScene(scene.path, true));
        EditorBuildSettings.scenes = list.ToArray();
    }

    static GameObject Man(string name, bool guard)
    {
        var m = new GameObject(name);
        var rig = new GameObject("Rig").transform;
        rig.SetParent(m.transform, false);
        var run = m.AddComponent<ProceduralRun>();
        run.rig = rig;
        int o = guard ? 4 : 7;

        run.thighR = Joint(rig, "ThighR", 0.03f, 0.58f, 0.14f, 0.3f, o - 1, out Transform kneeR);
        run.calfR = Joint(kneeR, "CalfR", 0, 0, 0.12f, 0.26f, o - 1, out _);
        run.armR = Joint(rig, "ArmR", 0.03f, 0.88f, 0.09f, 0.24f, o - 1, out Transform elbowR);
        run.foreR = Joint(elbowR, "ForeR", 0, 0, 0.08f, 0.22f, o - 1, out _);

        Part(rig, "Torso", -0.17f, 0.5f, 0.34f, 0.42f, K, o);
        Part(rig, "Neck", -0.05f, 0.9f, 0.1f, 0.05f, K, o);
        Part(rig, "Head", -0.11f, 0.93f, 0.22f, 0.22f, K, o, dot);

        run.thighL = Joint(rig, "ThighL", -0.03f, 0.58f, 0.14f, 0.3f, o + 1, out Transform kneeL);
        run.calfL = Joint(kneeL, "CalfL", 0, 0, 0.12f, 0.26f, o + 1, out _);
        run.armL = Joint(rig, "ArmL", -0.03f, 0.88f, 0.09f, 0.24f, o + 1, out Transform elbowL);
        run.foreL = Joint(elbowL, "ForeL", 0, 0, 0.08f, 0.22f, o + 1, out _);

        if (guard)
        {
            Part(rig, "Cap", -0.13f, 1.12f, 0.26f, 0.08f, K, o + 1);
            Part(rig, "Brim", 0.04f, 1.12f, 0.16f, 0.03f, K, o + 1);
            Part(run.foreL, "Baton", -0.03f, -0.5f, 0.06f, 0.28f, K, o + 1);
            Part(rig, "Belt", -0.17f, 0.5f, 0.34f, 0.06f, K, o + 1);
        }
        else
        {
            var tie = new GameObject("TiePivot").transform;
            tie.SetParent(rig, false);
            tie.localPosition = new Vector3(0.09f, 0.88f, 0);
            Part(tie, "Tie", -0.03f, -0.34f, 0.06f, 0.34f, K, o + 2);
            run.tie = tie;
            Part(rig, "Hair", -0.12f, 1.09f, 0.2f, 0.07f, K, o + 1);
        }
        Fit(m);
        return m;
    }

    static Transform Joint(Transform parent, string n, float x, float y, float w, float len, int o, out Transform end)
    {
        var pivot = new GameObject(n).transform;
        pivot.SetParent(parent, false);
        pivot.localPosition = new Vector3(x, y, 0);
        Part(pivot, n + "M", -w / 2, -len, w, len, K, o);
        if (n.StartsWith("Calf")) Part(pivot, "Shoe", -w / 2, -len, w + 0.06f, 0.05f, K, o);
        end = new GameObject(n + "End").transform;
        end.SetParent(pivot, false);
        end.localPosition = new Vector3(0, -len, 0);
        return pivot;
    }

    static float Floor(int f, out Transform spawn)
    {
        night = f >= 6;
        wallC = night ? new Color(0.07f, 0.08f, 0.1f) : new Color(0.8f, 0.82f, 0.85f);
        farC = night ? new Color(0.045f, 0.05f, 0.065f) : new Color(0.62f, 0.67f, 0.74f);
        trimC = night ? new Color(0.12f, 0.14f, 0.17f) : new Color(0.45f, 0.48f, 0.53f);
        screenC = night ? new Color(0.55f, 1f, 1f) : new Color(0.86f, 0.9f, 0.94f);
        glassC = night ? new Color(0f, 0.9f, 1f, 0.55f) : new Color(0.35f, 0.85f, 1f, 0.45f);
        ledC = night ? new Color(0.3f, 1f, 0.9f) : new Color(0.32f, 0.35f, 0.4f);
        textC = night ? new Color(0.25f, 0.3f, 0.36f) : new Color(0.42f, 0.46f, 0.52f);

        var world = root;
        root = new GameObject("Floor " + f).transform;
        root.SetParent(world);
        root.position = new Vector3(0, (f - 1) * H, 0);
        spawn = new GameObject("Spawn").transform;
        spawn.SetParent(root, false);
        spawn.localPosition = new Vector3(0, 0.05f, 0);

        Random.InitState(f * 977);
        float x = 8, gs = -6;
        if (f == 1)
        {
            Text(3, 4.5f, "THE COLLECTORS ARE COMING. RUN.", 0.1f);
            Text(x + 2, 3, "W / SPACE  -  JUMP", 0.09f); x += Chunk(1, x, ref gs) + 5;
            Text(x + 2, 3, "HOLD S  -  SLIDE", 0.09f); x += Chunk(2, x, ref gs) + 5;
            Text(x + 5, 6.5f, "JUMP AT A WALL, THEN JUMP AGAIN", 0.08f); x += Chunk(7, x, ref gs) + 5;
            Text(x + 3, 3, "SPILLED COFFEE MAKES YOU STUMBLE", 0.08f); x += Chunk(4, x, ref gs) + 5;
            Text(x + 3, 3, "D  -  DASH THROUGH GLASS", 0.09f); x += Chunk(5, x, ref gs) + 5;
            Text(x + 3, 3, "LMB  -  THROW FOLDERS AT GUARDS", 0.09f); Item(x + 3, false); x += 8;
        }
        else
        {
            int[] set = night ? new[] { 3, 4, 6, 6, 7, 8, 8, 9, 5 } : new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            int n = night ? 8 + f : 4 + f;
            for (int i = 0; i < n; i++)
            {
                if (i % 3 == 1) Item(x + 0.5f, false);
                x += Chunk(set[Random.Range(0, set.Length)], x, ref gs);
                x += night ? 3.5f : Random.Range(4.5f, 6f);
            }
        }
        float end = x + 3;

        Solid(Part(root, "Floor", gs, -6, end + 8 - gs, 6, K, 2));
        Solid(Part(root, "WallL", -7, 0, 1, Top, K, 2));
        Solid(Part(root, "WallR", end + 7, 0, 1, Top, K, 2));
        Solid(Part(root, "Ceiling", -7, Top, end + 15, 6, K, 2));
        Part(root, "BackWall", -7, -6, end + 15, Top + 6, wallC, -40);
        Part(root, "Baseboard", -7, 0, end + 15, 0.12f, trimC, -39);
        Part(root, "Crown", -7, Top - 0.15f, end + 15, 0.15f, trimC, -39);
        Background(end);
        Elevator(end, f);
        Text(3.5f, 7.6f, "FLOOR " + f + (night ? "  -  NIGHT SHIFT" : ""), 0.14f);

        root = world;
        return end;
    }

    static float Chunk(int t, float x, ref float gs)
    {
        switch (t)
        {
            case 1: Cooler(x + 1); return 3;
            case 2: Desk(x + 1); return 5;
            case 3: Cooler(x + 1); Desk(x + 6); return 10;
            case 4: Puddle(x + 1, 2); Desk(x + 6.5f); return 11;
            case 5: Item(x + 1, true); Glass(x + 6); return 8;
            case 6: Desk(x + 1); Item(x + 6, true); Glass(x + 10); return 12;
            case 7: Climb(x, ref gs); return 18;
            case 8: RackRun(x, ref gs); return 13;
            case 9: Pit(x + 1, 3.5f, ref gs); Cooler(x + 8); return 10;
        }
        return 0;
    }

    static void Climb(float x, ref float gs)
    {
        Vent(x + 1, 1.7f, 1);
        Rack(x + 4, 0, 2, 4.5f);
        Pit(x + 6, 9, ref gs);
        Rack(x + 7.5f, -6, 2, 10.5f);
        Rack(x + 11, -6, 2, 10.5f);
    }

    static void RackRun(float x, ref float gs)
    {
        Pit(x + 1.5f, 10.5f, ref gs);
        Rack(x + 2.5f, -6, 1.5f, 8f);
        Rack(x + 6, -6, 1.5f, 8.8f);
        Rack(x + 9.5f, -6, 1.5f, 8f);
    }

    static void Pit(float x, float w, ref float gs)
    {
        Solid(Part(root, "Floor", gs, -6, x - gs, 6, K, 2));
        gs = x + w;
    }

    static void Rack(float x, float y, float w, float h)
    {
        var r = Group("ServerRack", x, y);
        Solid(Part(r, "Body", 0, 0, w, h, K, 1));
        for (float ly = Mathf.Max(0, -y) + 0.3f; ly < h - 0.2f; ly += 0.32f)
        {
            Part(r, "Slot", 0.12f, ly, w - 0.24f, 0.02f, trimC, 2);
            Part(r, "Led", w - 0.24f, ly + 0.09f, 0.06f, 0.06f, Random.value < 0.7f ? ledC : trimC, 2);
        }
    }

    static void Vent(float x, float bottom, float w)
    {
        var v = Group("Vent", x, bottom);
        Solid(Part(v, "Duct", 0, 0, w, Top - bottom, K, 1));
        for (float y = 0.25f; y < 1.4f; y += 0.2f) Part(v, "Grille", 0.15f, y, w - 0.3f, 0.04f, trimC, 2);
    }

    static void Desk(float x)
    {
        var d = Group("Desk", x, 0);
        Solid(Part(d, "Top", 0, 0.78f, 2.2f, 0.1f, K, 1));
        Solid(Part(d, "Panel", 0.15f, 0.62f, 1.9f, 0.16f, K, 1));
        Part(d, "LegA", 0.05f, 0, 0.08f, 0.78f, K, 0);
        Part(d, "LegB", 2.07f, 0, 0.08f, 0.78f, K, 0);
        Part(d, "Mug", 0.3f, 0.88f, 0.1f, 0.12f, K, 1);
        Part(d, "Stand", 1.36f, 0.88f, 0.08f, 0.1f, K, 1);
        Solid(Part(d, "Monitor", 1.1f, 0.98f, 0.6f, 0.4f, K, 1));
        Part(d, "Screen", 1.14f, 1.02f, 0.52f, 0.32f, screenC, 2);
        if (night) Part(d, "Glow", 0.5f, 0.3f, 1.8f, 1.8f, new Color(0.4f, 1f, 1f, 0.25f), -1, glow);
    }

    static void Cooler(float x)
    {
        var c = Group("WaterCooler", x, 0);
        Solid(Part(c, "Cabinet", 0, 0, 0.5f, 0.8f, K, 1));
        Part(c, "TapA", 0.08f, 0.55f, 0.08f, 0.06f, trimC, 2);
        Part(c, "TapB", 0.32f, 0.55f, 0.08f, 0.06f, trimC, 2);
        Solid(Part(c, "Bottle", 0.06f, 0.8f, 0.38f, 0.42f, K, 1));
        Part(c, "Neck", 0.17f, 1.22f, 0.16f, 0.06f, K, 1);
        Part(c, "Water", 0.1f, 0.84f, 0.3f, 0.25f, night ? new Color(0.3f, 0.9f, 1f, 0.35f) : new Color(0.5f, 0.6f, 0.7f, 0.5f), 2);
    }

    static void Glass(float x)
    {
        var g = Solid(Part(root, "Glass", x, 0, 0.35f, Top, glassC, 3));
        Part(g.transform, "Glare", -0.3f, -0.5f, 0.12f, 1f, new Color(1, 1, 1, 0.45f), 4);
        Part(g.transform, "Glare", 0.1f, -0.5f, 0.05f, 1f, new Color(1, 1, 1, 0.3f), 4);
        g.AddComponent<Obstacle>().glass = true;
    }

    static void Puddle(float x, float w)
    {
        var p = Part(root, "Puddle", x, 0, w, 0.05f, K, 3);
        p.tag = "Puddle";
        p.AddComponent<BoxCollider2D>().isTrigger = true;
        Part(root, "Gloss", x + 0.2f, 0.035f, w * 0.4f, 0.015f, night ? screenC : Color.white, 4);
        Part(root, "Gloss", x + w * 0.7f, 0.035f, w * 0.15f, 0.015f, night ? screenC : Color.white, 4);
    }

    static void Item(float x, bool coffee)
    {
        var i = Group(coffee ? "Coffee" : "FolderPickup", x, 0);
        if (coffee)
        {
            Part(i, "Cup", -0.1f, 0, 0.2f, 0.24f, K, 5);
            Part(i, "Lid", -0.12f, 0.24f, 0.24f, 0.04f, K, 5);
            Part(i, "Handle", 0.1f, 0.06f, 0.06f, 0.1f, K, 5);
            Part(i, "Sleeve", -0.1f, 0.07f, 0.2f, 0.06f, screenC, 6);
        }
        else
        {
            Part(i, "Body", -0.18f, 0, 0.36f, 0.26f, K, 5);
            Part(i, "Tab", -0.18f, 0.26f, 0.14f, 0.05f, K, 5);
            Part(i, "Label", -0.1f, 0.1f, 0.2f, 0.05f, screenC, 6);
        }
        Fit(i.gameObject).isTrigger = true;
        i.gameObject.AddComponent<Item>().coffee = coffee;
    }

    static void Elevator(float x, int f)
    {
        var e = Group("Elevator", x, 0);
        Part(e, "Frame", 0, 0, 1.6f, 2.6f, K, -10);
        Part(e, "DoorA", 0.08f, 0, 0.7f, 2.5f, trimC, -9);
        Part(e, "DoorB", 0.82f, 0, 0.7f, 2.5f, trimC, -9);
        Part(e, "Display", 0.55f, 2.68f, 0.5f, 0.16f, screenC, -9);
        Fit(e.gameObject).isTrigger = true;
        e.gameObject.AddComponent<Level>().nextFloor = f + 1;
        Text(x + 0.8f, 3.2f, f == 10 ? "EXIT" : "ELEVATOR  " + (f + 1), 0.07f);
    }

    static void Background(float end)
    {
        var far = new GameObject("Far").transform;
        far.SetParent(root, false);
        far.gameObject.AddComponent<Parallax>().f = 0.5f;
        float max = (end + 6) * 0.5f + 12;
        for (float x = -12; x < max; x += Random.Range(3f, 4.5f))
        {
            float h = Random.Range(1.3f, 2f);
            Part(far, "Cubicle", x, 0, 2.6f, h * 0.55f, farC, -35);
            Part(far, "Divider", x, 0, 0.08f, h, farC, -35);
            if (Random.value < 0.7f)
            {
                Part(far, "Monitor", x + 0.9f, h * 0.55f, 0.55f, 0.36f, farC, -35);
                Part(far, "Screen", x + 0.94f, h * 0.55f + 0.04f, 0.47f, 0.28f, night ? screenC * 0.8f : screenC * 0.9f, -34);
                if (night) Part(far, "Glow", x + 0.4f, h * 0.55f - 0.6f, 1.6f, 1.6f, new Color(0.4f, 1f, 1f, 0.18f), -36, glow);
            }
        }
    }

    static Transform Group(string n, float x, float y)
    {
        var t = new GameObject(n).transform;
        t.SetParent(root, false);
        t.localPosition = new Vector3(x, y, 0);
        return t;
    }

    static GameObject Part(Transform p, string n, float x, float y, float w, float h, Color c, int o, Sprite s = null)
    {
        var go = new GameObject(n);
        go.transform.SetParent(p, false);
        go.transform.localPosition = new Vector3(x + w / 2, y + h / 2, 0);
        go.transform.localScale = new Vector3(w, h, 1);
        var r = go.AddComponent<SpriteRenderer>();
        r.sprite = s ? s : sq;
        r.color = c;
        r.sortingOrder = o;
        return go;
    }

    static GameObject Solid(GameObject g)
    {
        g.layer = ground;
        g.AddComponent<BoxCollider2D>();
        return g;
    }

    static BoxCollider2D Fit(GameObject g)
    {
        Bounds b = new Bounds();
        bool first = true;
        foreach (var r in g.GetComponentsInChildren<SpriteRenderer>())
        {
            if (r.name == "Brim" || r.name == "Tie" || r.name == "Baton") continue;
            if (first) b = r.bounds; else b.Encapsulate(r.bounds);
            first = false;
        }
        var c = g.AddComponent<BoxCollider2D>();
        c.size = b.size;
        c.offset = b.center - g.transform.position;
        return c;
    }

    static void Text(float x, float y, string t, float size)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(root, false);
        go.transform.localPosition = new Vector3(x, y, 0);
        var tm = go.AddComponent<TextMesh>();
        tm.text = t;
        tm.fontSize = 64;
        tm.characterSize = size;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.color = textC;
        tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var mr = go.GetComponent<MeshRenderer>();
        mr.sharedMaterial = tm.font.material;
        mr.sortingOrder = -21;
    }

    static void Sprites()
    {
        AssetDatabase.DeleteAsset("Assets/Generated");
        AssetDatabase.CreateFolder("Assets", "Generated");
        sq = Make("Square", 4, 0);
        dot = Make("Dot", 16, 1);
        glow = Make("Glow", 32, 2);
    }

    static Sprite Make(string n, int s, int kind)
    {
        var t = new Texture2D(s, s, TextureFormat.RGBA32, false) { name = n, wrapMode = TextureWrapMode.Clamp };
        t.filterMode = kind == 2 ? FilterMode.Bilinear : FilterMode.Point;
        var px = new Color[s * s];
        for (int i = 0; i < px.Length; i++)
        {
            float d = Vector2.Distance(new Vector2(i % s + 0.5f, i / s + 0.5f), Vector2.one * s / 2f) / (s / 2f);
            float a = kind == 0 ? 1 : kind == 1 ? (d <= 1 ? 1 : 0) : Mathf.Pow(Mathf.Clamp01(1 - d), 2);
            px[i] = new Color(1, 1, 1, a);
        }
        t.SetPixels(px);
        t.Apply();
        AssetDatabase.CreateAsset(t, "Assets/Generated/" + n + ".asset");
        var sp = Sprite.Create(t, new Rect(0, 0, s, s), Vector2.one * 0.5f, s);
        sp.name = n;
        AssetDatabase.AddObjectToAsset(sp, t);
        return sp;
    }

    static void Setup()
    {
        var tm = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tags = tm.FindProperty("tags");
        foreach (var t in new[] { "Lethal", "Guard", "Puddle" })
        {
            bool has = false;
            for (int i = 0; i < tags.arraySize; i++) has |= tags.GetArrayElementAtIndex(i).stringValue == t;
            if (has) continue;
            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = t;
        }
        var layers = tm.FindProperty("layers");
        ground = -1;
        for (int i = 8; i < 32 && ground < 0; i++) if (layers.GetArrayElementAtIndex(i).stringValue == "Ground") ground = i;
        for (int i = 8; i < 32 && ground < 0; i++)
            if (layers.GetArrayElementAtIndex(i).stringValue == "") { layers.GetArrayElementAtIndex(i).stringValue = "Ground"; ground = i; }
        tm.ApplyModifiedProperties();
    }
}
