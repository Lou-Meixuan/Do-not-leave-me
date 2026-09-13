#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Creates an independent art pass. The source scenes and the formal route are never saved.
public static class Level04A3Decorator
{
    const string BaseScene = "Assets/DoNotLeaveMe/Levels/Level_04A.unity";
    const string ArtScene = "Assets/DoNotLeaveMe/Levels/Level_04A 1.unity";
    const string OutputScene = "Assets/DoNotLeaveMe/Levels/Level_04A 3.unity";
    const string ReportDirectory = "../_to_delete/A3-review";
    static readonly List<string> Audit = new List<string>();

    [InitializeOnLoadMethod]
    static void CheckRequest()
    {
        EditorApplication.delayCall += () =>
        {
            const string request = "Library/A3-build.request";
            if (!File.Exists(request) || EditorApplication.isPlayingOrWillChangePlaymode) return;
            File.Move(request, request + ".started");
            Build();
        };
    }

    [MenuItem("Tools/DoNotLeaveMe/Level 04A/Create A3 Extended Art Pass")]
    public static void Build()
    {
        Directory.CreateDirectory(ReportDirectory);
        Audit.Clear();
        Scene previous = SceneManager.GetActiveScene();
        Scene art = default, scene = default;
        bool openedArt = false;
        try
        {
            if (File.Exists(OutputScene)) throw new InvalidOperationException("A3 already exists; refusing to overwrite it.");
            if (!AssetDatabase.CopyAsset(BaseScene, OutputScene)) throw new InvalidOperationException("Could not copy A.");
            scene = EditorSceneManager.OpenScene(OutputScene, OpenSceneMode.Additive);
            art = SceneManager.GetSceneByPath(ArtScene);
            if (!art.IsValid() || !art.isLoaded)
            {
                art = EditorSceneManager.OpenScene(ArtScene, OpenSceneMode.Additive);
                openedArt = true;
            }
            SceneManager.SetActiveScene(scene);
            Transform root = Find(scene, "L04A_ParkourRoot");
            var controller = root.GetComponent<Level04BParkourController>();
            var data = new SerializedObject(controller);
            SerializedProperty segments = data.FindProperty("segments");
            var oldPoints = new List<Transform>();
            for (int i = 0; i < segments.arraySize; i++)
                oldPoints.Add((Transform)segments.GetArrayElementAtIndex(i).FindPropertyRelative("start").objectReferenceValue);
            oldPoints.Add((Transform)segments.GetArrayElementAtIndex(segments.arraySize - 1).FindPropertyRelative("end").objectReferenceValue);
            Vector3 forward = (oldPoints[1].position - oldPoints[0].position).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            Transform route = Find(scene, "HumanRoute");
            var points = new List<Transform>
            {
                Marker(route, "A3_AdmissionStart", oldPoints[0].position - forward * 36 + right * 36),
                Marker(route, "A3_AdmissionTurn", oldPoints[0].position - forward * 36)
            };
            points.AddRange(oldPoints);
            // Insert before the four existing entries, retaining their tuning and references.
            segments.InsertArrayElementAtIndex(0);
            segments.InsertArrayElementAtIndex(0);
            for (int i = 0; i < 2; i++)
            {
                var entry = segments.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("name").stringValue = i == 0 ? "A3_Admission" : "A3_TransferHall";
                entry.FindPropertyRelative("start").objectReferenceValue = points[i];
                entry.FindPropertyRelative("end").objectReferenceValue = points[i + 1];
                entry.FindPropertyRelative("laneWidth").floatValue = 1.8f;
                entry.FindPropertyRelative("runSpeed").floatValue = i == 0 ? 7.5f : 8f;
                entry.FindPropertyRelative("decisionWindow").floatValue = 4.5f;
                Vector3 f = (points[i + 1].position - points[i].position).normalized;
                Vector3 next = (points[i + 2].position - points[i + 1].position).normalized;
                float turn = Vector3.SignedAngle(f, next, Vector3.up);
                entry.FindPropertyRelative("requiredTurn").enumValueIndex = Mathf.Abs(turn) < 1 ? 0 : turn < 0 ? 1 : 2;
                entry.FindPropertyRelative("finalJunction").boolValue = false;
            }
            data.ApplyModifiedPropertiesWithoutUndo();
            var dog = new SerializedObject(root.GetComponent<Level04BParkourDogRunner>());
            var chase = dog.FindProperty("chasePath");
            chase.InsertArrayElementAtIndex(0);
            chase.InsertArrayElementAtIndex(0);
            Vector3 initialForward = (points[1].position - points[0].position).normalized;
            for (int i = 0; i < 2; i++)
            {
                Vector3 f = (points[i + 1].position - points[i].position).normalized;
                chase.GetArrayElementAtIndex(i).objectReferenceValue = Marker(Find(scene, "DogRoute"),
                    "A3_Dog_" + i, points[i].position - Vector3.Cross(Vector3.up, f) * 2.5f);
            }
            dog.ApplyModifiedPropertiesWithoutUndo();
            Find(scene, "HumanRespawnAnchor").SetPositionAndRotation(points[0].position, Quaternion.LookRotation(initialForward));
            Find(scene, "DogRespawnAnchor").SetPositionAndRotation(points[0].position - Vector3.Cross(Vector3.up, initialForward) * 2.5f, Quaternion.LookRotation(initialForward));
            Find(scene, "ParkourCameraPose").SetPositionAndRotation(points[0].position + Vector3.up * 3.2f - initialForward * 6, Quaternion.LookRotation(initialForward));

            Transform artRoot = Child(root, "A3_Art_RebuiltAlongRoute");
            // Keep A's collider and trigger setup, replacing only its primitive/repeated visuals.
            foreach (Transform child in root)
                if (child.name.StartsWith("L04B_CorridorModel_") || child.name == "Floor" || child.name.StartsWith("Wall"))
                    foreach (Renderer renderer in child.GetComponentsInChildren<Renderer>(true)) renderer.enabled = false;

            Transform models = Find(art, "Models");
            Transform surfaces = Find(art, "Walls_and_Floors");
            GameObject floor = Sample(surfaces, "floor_tile_blue");
            GameObject wall = Sample(surfaces, "wall_tile");
            string[] propTerms = { "hospital bed", "medical_trolley", "wheelchair", "medical cabinet", "restraint_chair", "drug_storage_shelf", "cabinet", "stretcher" };
            var props = propTerms.Select(term => Sample(models, term, false)).Where(p => p != null).Distinct().ToArray();
            if (props.Length < 3) throw new InvalidOperationException("Not enough A1 art samples.");
            GameObject pipe = Sample(models, "pipe", false);
            GameObject sign = Sample(models, "sign", false);
            GameObject lamp = Sample(models, "wall_lamp", true);
            Light sourceLight = Find(art, "Corridor_Wall_Lights").GetComponentsInChildren<Light>(true).First();
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector3 start = points[i].position, end = points[i + 1].position;
                Vector3 f = (end - start).normalized, r = Vector3.Cross(Vector3.up, f);
                float length = Vector3.Distance(start, end);
                Quaternion rotation = Quaternion.LookRotation(f);
                Transform section = Child(artRoot, "Hall_" + (i + 1));
                if (i < 2)
                {
                    Cube(section, "ExtensionFloorCollision", (start + end) * .5f - Vector3.up * .15f,
                        new Vector3(6.6f, .3f, length), rotation, true);
                    for (int side = -1; side <= 1; side += 2)
                        Cube(section, "ExtensionSideCollision", (start + end) * .5f + r * side * 3.5f + Vector3.up * 1.5f,
                            new Vector3(.3f, 3, length - 10), rotation, true);
                }
                int count = Mathf.CeilToInt(length / 4);
                for (int t = 0; t < count; t++)
                {
                    float distance = (t + .5f) * length / count;
                    Vector3 center = start + f * distance;
                    Fit(floor, section, "TiledFloor", center - Vector3.up * .045f, rotation, new Vector3(10, .08f, length / count + .025f));
                    if (distance < 5 || distance > length - 5) continue;
                    for (int side = -1; side <= 1; side += 2)
                        Fit(wall, section, "TiledWall", center + r * side * 5 + Vector3.up * 2,
                            rotation, new Vector3(.18f, 4, length / count + .025f));
                }
                // Bed/cart alcoves stay beyond the dog track and all three human lanes.
                for (float distance = 7; distance < length - 6; distance += 7)
                {
                    int index = Mathf.RoundToInt(distance / 7) + i;
                    int side = index % 2 == 0 ? 1 : -1;
                    Vector3 center = start + f * distance;
                    GameObject prop = Fit(props[index % props.Length], section, "A1_Prop_" + index,
                        center + r * side * 4.05f + Vector3.up * .9f, rotation * Quaternion.Euler(0, side * 90, 0),
                        new Vector3(1.5f, 1.8f, 2.2f), true);
                    var propRenderers = prop.GetComponentsInChildren<Renderer>(true);
                    float bottom = propRenderers.Min(renderer => renderer.bounds.min.y);
                    prop.transform.position += Vector3.up * (start.y - bottom);
                    Fit(lamp, section, "A1_WallLamp", center - r * side * 4.7f + Vector3.up * 3.2f,
                        rotation * Quaternion.Euler(0, side * 90, 0), new Vector3(.45f, .7f, .35f), true);
                    if (sign != null && index % 2 == 0)
                        Fit(sign, section, "A1_HospitalSign", center + r * side * 4.85f + Vector3.up * 2.8f,
                            rotation, new Vector3(.08f, .6f, 1.1f), true);
                }
                if (pipe != null)
                    Fit(pipe, section, "A1_ServicePipe", (start + end) * .5f + r * 4.8f + Vector3.up * 3.65f,
                        rotation, new Vector3(.16f, .16f, length - 10));
                // Soft fill supports the original dark chase camera; fixtures remain A1 assets.
                for (float d = 4; d < length; d += 9)
                {
                    var lightObject = Object.Instantiate(sourceLight.gameObject, section);
                    lightObject.name = "A1_CorridorFill";
                    lightObject.SetActive(true);
                    var light = lightObject.GetComponent<Light>();
                    light.transform.localScale = Vector3.one;
                    light.type = LightType.Point;
                    light.transform.position = start + f * d + Vector3.up * 3.4f;
                    light.range = 9;
                    light.intensity = 1.4f;
                    light.color = i % 2 == 0 ? new Color(.72f, .84f, .87f) : new Color(1, .83f, .65f);
                    light.shadows = LightShadows.None;
                }
            }
            for (int i = 1; i < points.Count - 1; i++)
            {
                Fit(floor, artRoot, "JunctionFloor_" + i, points[i].position - Vector3.up * .048f,
                    Quaternion.identity, new Vector3(10, .08f, 10));
                Vector3 incoming = (points[i - 1].position - points[i].position).normalized;
                Vector3 outgoing = (points[i + 1].position - points[i].position).normalized;
                foreach (Vector3 side in new[] { Vector3.forward, Vector3.back, Vector3.left, Vector3.right })
                    if (Vector3.Dot(side, incoming) < .9f && Vector3.Dot(side, outgoing) < .9f)
                        Fit(wall, artRoot, "JunctionWall_" + i, points[i].position + side * 5 + Vector3.up * 2,
                            Quaternion.LookRotation(Vector3.Cross(Vector3.up, side)), new Vector3(.18f, 4, 10));
            }
            foreach (string branch in new[] { "Left", "Right" })
            {
                Vector3 branchEnd = Find(scene, branch + "Branch_02").position;
                Vector3 last = points[points.Count - 1].position;
                Vector3 direction = (branchEnd - last).normalized;
                Fit(floor, artRoot, branch + "BranchFloor", (last + branchEnd) * .5f - Vector3.up * .045f,
                    Quaternion.LookRotation(direction), new Vector3(10, .08f, Vector3.Distance(last, branchEnd) + 5));
            }
            Vector3 merge = Find(scene, "LongCorridorEntry").position;
            Vector3 walkEnd = Find(scene, "Walkout_02").position;
            Fit(floor, artRoot, "WalkoutFloor", (merge + walkEnd) * .5f - Vector3.up * .045f,
                Quaternion.LookRotation(walkEnd - merge), new Vector3(6.55f, .08f, Vector3.Distance(merge, walkEnd)));

            // Duplicate the existing proven obstacle assemblies for the two new halls.
            AddObstacle(Find(scene, "LaneBlock_01"), points[0].position, points[1].position, .45f, -1, "A3_LaneBlock");
            AddObstacle(Find(scene, "JumpBlock_01"), points[1].position, points[2].position, .45f, 0, "A3_JumpBlock");
            var obstacles = root.GetComponentsInChildren<Level04BParkourObstacle>(true);
            foreach (var obstacle in obstacles)
            {
                Transform visual = obstacle.transform.Find("PlaceholderVisual");
                if (visual == null) continue;
                var renderer = visual.GetComponent<Renderer>();
                if (renderer == null) continue;
                Bounds bounds = renderer.bounds;
                renderer.enabled = false;
                Fit(props[obstacle.name.Contains("Jump") ? 1 % props.Length : 0], obstacle.transform,
                    "A1_ObstacleVisual", bounds.center, obstacle.transform.rotation, visual.localScale);
            }
            if (!controller.ValidateRoute(out string error)) throw new InvalidOperationException(error);
            Validate(controller, artRoot, points, obstacles.Length);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Render(scene, points[0].position + Vector3.up * 3.2f - initialForward * 5,
                points[0].position + initialForward * 18 + Vector3.up, "A3-start.png");
            Vector3 middle = Vector3.Lerp(points[3].position, points[4].position, .18f);
            Vector3 middleForward = (points[4].position - points[3].position).normalized;
            Render(scene, middle + Vector3.up * 3.2f - middleForward * 4,
                middle + middleForward * 17 + Vector3.up, "A3-hospital-hall.png");
            Vector3 centerAll = points.Aggregate(Vector3.zero, (sum, p) => sum + p.position) / points.Count;
            Render(scene, centerAll + new Vector3(95, 120, -95), centerAll, "A3-overview.png", true);
            File.WriteAllLines(ReportDirectory + "/build-result.txt", Audit.Concat(new[] { "SUCCESS: " + OutputScene }));
            Debug.Log("[A3] Created extended course with A1 art. Review: " + ReportDirectory);
        }
        catch (Exception exception)
        {
            File.WriteAllText(ReportDirectory + "/build-result.txt", "FAILED\n" + exception);
            Debug.LogException(exception);
        }
        finally
        {
            if (openedArt && art.IsValid()) EditorSceneManager.CloseScene(art, true);
            if (scene.IsValid()) EditorSceneManager.CloseScene(scene, true);
            if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
        }
    }

    static Transform Find(Scene scene, string name) => scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).First(t => t.name.Trim() == name);
    static Transform Child(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }
    static Transform Marker(Transform parent, string name, Vector3 point)
    {
        var t = Child(parent, name); t.position = point; return t;
    }
    static GameObject Sample(Transform group, string term, bool required = true)
    {
        foreach (Transform child in group)
        {
            string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(child.gameObject) ?? "";
            if ((path + " " + child.name).IndexOf(term, StringComparison.OrdinalIgnoreCase) < 0) continue;
            if (child.GetComponentsInChildren<Renderer>(true).Length == 0) continue;
            Audit.Add("A1 sample: " + child.name + " | " + path);
            return child.gameObject;
        }
        if (required) throw new InvalidOperationException("No A1 sample for " + group.name + "/" + term);
        return null;
    }
    static GameObject Fit(GameObject sample, Transform parent, string name, Vector3 center, Quaternion rotation, Vector3 size, bool uniform = false)
    {
        Transform wrapper = Child(parent, name);
        wrapper.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        GameObject model = Object.Instantiate(sample, wrapper, true);
        model.SetActive(true);
        // Retain A1's mesh/material overrides and authored orientation inside a fitted wrapper.
        foreach (Collider c in model.GetComponentsInChildren<Collider>(true)) c.enabled = false;
        foreach (Rigidbody body in model.GetComponentsInChildren<Rigidbody>(true)) { body.isKinematic = true; body.detectCollisions = false; }
        foreach (MonoBehaviour behaviour in model.GetComponentsInChildren<MonoBehaviour>(true))
            if (behaviour != null && behaviour is GateController) behaviour.enabled = false;
        var renderers = model.GetComponentsInChildren<Renderer>(true);
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
        model.transform.position -= bounds.center;
        Vector3 scale = new Vector3(size.x / Mathf.Max(.001f, bounds.size.x), size.y / Mathf.Max(.001f, bounds.size.y), size.z / Mathf.Max(.001f, bounds.size.z));
        if (uniform) scale = Vector3.one * Mathf.Min(scale.x, Mathf.Min(scale.y, scale.z));
        wrapper.SetPositionAndRotation(center, rotation);
        wrapper.localScale = scale;
        return wrapper.gameObject;
    }
    static void Cube(Transform parent, string name, Vector3 center, Vector3 size, Quaternion rotation, bool collision)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name; cube.transform.SetParent(parent, false);
        cube.transform.SetPositionAndRotation(center, rotation); cube.transform.localScale = size;
        cube.GetComponent<Renderer>().enabled = false;
        cube.GetComponent<Collider>().enabled = collision;
    }
    static void AddObstacle(Transform source, Vector3 start, Vector3 end, float fraction, int lane, string name)
    {
        GameObject copy = Object.Instantiate(source.gameObject, source.parent);
        copy.name = name;
        Vector3 f = (end - start).normalized;
        copy.transform.SetPositionAndRotation(Vector3.Lerp(start, end, fraction) + Vector3.Cross(Vector3.up, f) * lane * 1.8f, Quaternion.LookRotation(f));
    }
    static void Validate(Level04BParkourController controller, Transform art, List<Transform> points, int obstacleCount)
    {
        var data = new SerializedObject(controller);
        var segments = data.FindProperty("segments");
        float length = 0;
        for (int i = 0; i < segments.arraySize; i++)
        {
            var segment = segments.GetArrayElementAtIndex(i);
            if (Mathf.Abs(segment.FindPropertyRelative("laneWidth").floatValue - 1.8f) > .001f) throw new Exception("Lane width changed.");
            length += Vector3.Distance(points[i].position, points[i + 1].position);
        }
        foreach (Collider c in art.GetComponentsInChildren<Collider>(true))
            if (c.enabled && !c.name.StartsWith("Extension")) throw new Exception("Art collider still enabled: " + c.name);
        foreach (var transform in art.GetComponentsInChildren<Transform>(true))
            if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject) > 0) throw new Exception("Missing art script: " + transform.name);
        Audit.Add("Validated: " + segments.arraySize + " contiguous segments; " + length.ToString("F1") + " metres; 1.8 m lane spacing; " + obstacleCount + " obstacle assemblies.");
        Audit.Add("Original four segments, branches, merge, walkout, exit and control scripts retained.");
    }
    static void Render(Scene scene, Vector3 position, Vector3 target, string filename, bool overview = false)
    {
        GameObject cameraObject = new GameObject("A3_ReviewCamera");
        SceneManager.MoveGameObjectToScene(cameraObject, scene);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.enabled = false;
        camera.overrideSceneCullingMask = EditorSceneManager.GetSceneCullingMask(scene);
        camera.transform.SetPositionAndRotation(position, Quaternion.LookRotation(target - position));
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.045f, .055f, .065f);
        camera.nearClipPlane = .1f; camera.farClipPlane = 500; camera.fieldOfView = 65;
        camera.orthographic = overview; camera.orthographicSize = 68;
        var rt = new RenderTexture(1440, 900, 24);
        RenderTexture old = RenderTexture.active;
        try
        {
            camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
            var image = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0); image.Apply();
            File.WriteAllBytes(ReportDirectory + "/" + filename, image.EncodeToPNG());
            Object.DestroyImmediate(image);
        }
        finally
        {
            camera.targetTexture = null; RenderTexture.active = old;
            rt.Release(); Object.DestroyImmediate(rt); Object.DestroyImmediate(cameraObject);
        }
    }
}
#endif
