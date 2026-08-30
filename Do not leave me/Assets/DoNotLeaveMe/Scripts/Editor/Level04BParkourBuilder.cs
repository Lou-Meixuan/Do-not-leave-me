#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Level04AParkourBuilder
{
    const string ScenePath = "Assets/DoNotLeaveMe/Levels/Level_04A.unity";
    const string OldScenePath = "Assets/DoNotLeaveMe/Levels/Level_04B.unity";
    const string RootName = "L04A_ParkourRoot";
    const string OldRootName = "L04B_ParkourRoot";

    [MenuItem("Tools/DoNotLeaveMe/Level 04A/Migrate and Build Parkour")]
    public static void Build()
    {
        Scene scene = OpenOrCreateLevel04A();
        Transform handoff = FindInScene(scene, "HumanRespawnAnchor");
        if (handoff == null)
            throw new System.InvalidOperationException("Level_04A/HumanRespawnAnchor was not found.");

        GameObject existing = FindRoot(scene, RootName);
        if (existing != null)
            Object.DestroyImmediate(existing);

        GameObject root = new GameObject(RootName);
        SceneManager.MoveGameObjectToScene(root, scene);
        Transform routeRoot = Child(root.transform, "HumanRoute");
        Transform dogRoot = Child(root.transform, "DogRoute");
        Transform obstacleRoot = Child(root.transform, "PlaceholderObstacles");

        Vector3 forward = Flat(handoff.forward);
        if (forward.sqrMagnitude < 0.1f)
            forward = Vector3.forward;
        forward.Normalize();
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        Vector3 mergePosition = handoff.position - forward * 1.5f;

        Vector3[] points =
        {
            mergePosition - forward * 42f - right * 36f,
            mergePosition - forward * 42f - right * 18f,
            mergePosition - forward * 24f - right * 18f,
            mergePosition - forward * 24f,
            mergePosition - forward * 10f,
        };

        List<Transform> humanMarkers = new List<Transform>();
        for (int i = 0; i < points.Length; i++)
            humanMarkers.Add(Marker(routeRoot, "SegmentPoint_" + i, points[i], forward));

        Level04BParkourController.RouteSegment[] segments = new Level04BParkourController.RouteSegment[points.Length - 1];
        for (int i = 0; i < segments.Length; i++)
        {
            Vector3 segmentForward = (points[i + 1] - points[i]).normalized;
            Vector3 nextForward = i + 2 < points.Length ? (points[i + 2] - points[i + 1]).normalized : segmentForward;
            float signedTurn = Vector3.SignedAngle(segmentForward, nextForward, Vector3.up);
            bool final = i == segments.Length - 1;
            segments[i] = new Level04BParkourController.RouteSegment
            {
                name = "Corridor_" + (i + 1),
                start = humanMarkers[i],
                end = humanMarkers[i + 1],
                laneWidth = 1.8f,
                runSpeed = 8f + i * 0.25f,
                decisionWindow = 4.5f,
                requiredTurn = final
                    ? Level04BParkourController.TurnDirection.Either
                    : signedTurn < 0f
                        ? Level04BParkourController.TurnDirection.Left
                        : Level04BParkourController.TurnDirection.Right,
                finalJunction = final
            };
            BuildCorridor(root.transform, points[i], points[i + 1], 6.6f);
        }

        Transform merge = Marker(routeRoot, "LongCorridorEntry", mergePosition, forward);
        Vector3 junctionForward = (points[points.Length - 1] - points[points.Length - 2]).normalized;
        Vector3 junctionRight = Vector3.Cross(Vector3.up, junctionForward).normalized;
        Vector3 leftDirection = -junctionRight;
        Vector3 rightDirection = junctionRight;
        Transform leftA = Marker(routeRoot, "LeftBranch_01", points[points.Length - 1] + leftDirection * 4f, leftDirection);
        Transform leftB = Marker(routeRoot, "LeftBranch_02", points[points.Length - 1] + leftDirection * 10f, leftDirection);
        Transform rightA = Marker(routeRoot, "RightBranch_01", points[points.Length - 1] + rightDirection * 4f, rightDirection);
        Transform rightB = Marker(routeRoot, "RightBranch_02", points[points.Length - 1] + rightDirection * 10f, rightDirection);
        Transform walkA = Marker(routeRoot, "Walkout_01", mergePosition + forward * 3f, forward);
        Transform walkB = Marker(routeRoot, "Walkout_02", mergePosition + forward * 7f, forward);

        BuildCorridor(root.transform, points[points.Length - 1], leftA.position, 6.6f, false);
        BuildCorridor(root.transform, leftA.position, leftB.position, 6.6f, false);
        BuildCorridor(root.transform, points[points.Length - 1], rightA.position, 6.6f, false);
        BuildCorridor(root.transform, rightA.position, rightB.position, 6.6f, false);
        BuildCorridor(root.transform, merge.position, walkB.position, 6.6f, false);
        BuildDarkExit(root.transform, mergePosition, forward, right);

        List<Transform> dogChase = new List<Transform>();
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 direction = i + 1 < points.Length ? (points[i + 1] - points[i]).normalized : forward;
            Vector3 dogRight = Vector3.Cross(Vector3.up, direction).normalized;
            dogChase.Add(Marker(dogRoot, "DogChase_" + i, points[i] - dogRight * 2.5f, direction));
        }
        Transform dogLeftA = Marker(dogRoot, "DogLeft_01", leftA.position - right * 0.8f, forward);
        Transform dogLeftB = Marker(dogRoot, "DogLeft_02", leftB.position - right * 0.8f, forward);
        Transform dogRightA = Marker(dogRoot, "DogRight_01", rightA.position + right * 0.8f, forward);
        Transform dogRightB = Marker(dogRoot, "DogRight_02", rightB.position + right * 0.8f, forward);
        Transform dogWalkA = Marker(dogRoot, "DogWalkout_01", walkA.position - right * 1.4f, forward);
        Transform dogWalkB = Marker(dogRoot, "DogWalkout_02", walkB.position - right * 1.4f, forward);

        BuildObstacle(obstacleRoot, "LaneBlock_01", Vector3.Lerp(points[0], points[1], 0.45f), 0, (points[1] - points[0]).normalized, false);
        BuildObstacle(obstacleRoot, "JumpBlock_01", Vector3.Lerp(points[1], points[2], 0.45f), 0, (points[2] - points[1]).normalized, true);
        BuildObstacle(obstacleRoot, "LaneBlock_02", Vector3.Lerp(points[2], points[3], 0.4f), 1, (points[3] - points[2]).normalized, false);
        BuildObstacle(obstacleRoot, "JumpBlock_02", Vector3.Lerp(points[3], points[4], 0.5f), 1, (points[4] - points[3]).normalized, true);

        GameObject cameraPoseObject = new GameObject("ParkourCameraPose");
        cameraPoseObject.transform.SetParent(root.transform);
        cameraPoseObject.transform.SetPositionAndRotation(points[0] + Vector3.up * 3f - forward * 6f, Quaternion.LookRotation(forward));
        GameObject monster = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        monster.name = "MonsterPresentationPlaceholder";
        monster.transform.SetParent(root.transform);
        monster.transform.localScale = new Vector3(1.2f, 1.4f, 1.2f);

        Level04BParkourDogRunner dogRunner = root.AddComponent<Level04BParkourDogRunner>();
        Level04BParkourController controller = root.AddComponent<Level04BParkourController>();

        SerializedObject dogSerialized = new SerializedObject(dogRunner);
        SetArray(dogSerialized, "chasePath", dogChase.ToArray());
        SetArray(dogSerialized, "leftBranchPath", new[] { dogLeftA, dogLeftB });
        SetArray(dogSerialized, "rightBranchPath", new[] { dogRightA, dogRightB });
        SetArray(dogSerialized, "walkoutPath", new[] { dogWalkA, dogWalkB });
        dogSerialized.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject serialized = new SerializedObject(controller);
        SetRouteSegments(serialized.FindProperty("segments"), segments);
        SetArray(serialized, "leftBranchPath", new[] { leftA, leftB });
        SetArray(serialized, "rightBranchPath", new[] { rightA, rightB });
        SetArray(serialized, "walkoutPath", new[] { walkA, walkB });
        serialized.FindProperty("mergeAnchor").objectReferenceValue = merge;
        serialized.FindProperty("cameraPose").objectReferenceValue = cameraPoseObject.transform;
        serialized.FindProperty("monsterPresentation").objectReferenceValue = monster.transform;
        serialized.FindProperty("dogRunner").objectReferenceValue = dogRunner;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        ConfigureLevel(scene, points[0], points[0] - right * 1.4f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        MigrateFormalRoute();
        RemoveOldGeneratedRoot();
        Selection.activeGameObject = root;
        Debug.Log("[Level04AParkourBuilder] Built Level 04A parkour, migrated the route, and preserved Level 04B content.");
    }

    public static void BuildAndValidateForBatch()
    {
        try
        {
            Build();
            Validate();
            EditorApplication.Exit(0);
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }

    [MenuItem("Tools/DoNotLeaveMe/Level 04A/Validate Parkour Migration")]
    public static void Validate()
    {
        Level04BParkourController controller = Object.FindObjectOfType<Level04BParkourController>();
        if (controller == null)
            throw new System.InvalidOperationException("L04B parkour controller was not found in the open scene.");
        if (!controller.ValidateRoute(out string error))
            throw new System.InvalidOperationException(error);
        if (controller.GetComponentsInChildren<Level04BParkourObstacle>(true).Length == 0)
            throw new System.InvalidOperationException("No placeholder parkour obstacles are present.");
        if (controller.gameObject.scene.name != "Level_04A")
            throw new System.InvalidOperationException("Parkour controller is not in Level_04A.");
        Debug.Log("[Level04AParkourBuilder] Route validation passed.", controller);
    }

    static Scene OpenOrCreateLevel04A()
    {
        Scene scene;
        if (System.IO.File.Exists(System.IO.Path.GetFullPath(ScenePath)))
            return EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject levelObject = new GameObject("Level04A");
        SceneManager.MoveGameObjectToScene(levelObject, scene);
        levelObject.AddComponent<LevelController>();

        Transform humanAnchor = Marker(levelObject.transform, "HumanRespawnAnchor", Vector3.zero, Vector3.forward);
        Marker(levelObject.transform, "DogRespawnAnchor", new Vector3(-1.4f, 0f, 0f), Vector3.forward);
        CreateCube(levelObject.transform, "AnchorGround", humanAnchor.position - Vector3.up * 0.15f,
            new Vector3(8f, 0.3f, 8f), Quaternion.identity, true);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.ImportAsset(ScenePath);
        return scene;
    }

    static void ConfigureLevel(Scene scene, Vector3 humanSpawn, Vector3 dogSpawn)
    {
        LevelController level = Object.FindObjectOfType<LevelController>();
        Transform humanAnchor = FindInScene(scene, "HumanRespawnAnchor");
        Transform dogAnchor = FindInScene(scene, "DogRespawnAnchor");
        humanAnchor.SetPositionAndRotation(humanSpawn, Quaternion.LookRotation(Vector3.left));
        dogAnchor.SetPositionAndRotation(dogSpawn, Quaternion.LookRotation(Vector3.left));
        SerializedObject serialized = new SerializedObject(level);
        serialized.FindProperty("levelId").stringValue = "Level04A";
        serialized.FindProperty("humanRespawnAnchor").objectReferenceValue = humanAnchor;
        serialized.FindProperty("dogRespawnAnchor").objectReferenceValue = dogAnchor;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    static void BuildDarkExit(Transform parent, Vector3 start, Vector3 forward, Vector3 right)
    {
        Transform dark = Child(parent, "DarkExitCorridor");
        Vector3 end = start + forward * 18f;
        BuildCorridor(dark, start, end, 6.6f);
        CreateCube(dark, "BlackCeiling", (start + end) * 0.5f + Vector3.up * 3.25f,
            new Vector3(6.9f, 0.3f, 18f), Quaternion.LookRotation(forward), true);

        GameObject doorRoot = new GameObject("ToLevel04B_ExitDoor");
        doorRoot.transform.SetParent(dark);
        doorRoot.transform.SetPositionAndRotation(end, Quaternion.LookRotation(forward));
        GameObject pivot = CreateCube(doorRoot.transform, "DoorVisual", end + Vector3.up * 1.5f,
            new Vector3(3.2f, 3f, 0.25f), Quaternion.LookRotation(forward), true);
        pivot.transform.localPosition = new Vector3(-1.6f, 1.5f, 0f);
        BoxCollider blocking = pivot.GetComponent<BoxCollider>();
        Door door = doorRoot.AddComponent<Door>();
        SerializedObject doorData = new SerializedObject(door);
        doorData.FindProperty("blockingCollider").objectReferenceValue = blocking;
        doorData.FindProperty("visualPivot").objectReferenceValue = pivot.transform;
        doorData.ApplyModifiedPropertiesWithoutUndo();

        GameObject opener = new GameObject("ExitDoorOpener");
        opener.transform.SetParent(dark);
        opener.transform.position = end - forward * 2.5f + Vector3.up;
        BoxCollider openerCollider = opener.AddComponent<BoxCollider>();
        openerCollider.isTrigger = true;
        openerCollider.size = new Vector3(5f, 2.5f, 2f);
        ActuatorTrigger actuator = opener.AddComponent<ActuatorTrigger>();
        SerializedObject actuatorData = new SerializedObject(actuator);
        SerializedProperty actuators = actuatorData.FindProperty("actuators");
        actuators.arraySize = 1;
        actuators.GetArrayElementAtIndex(0).objectReferenceValue = door;
        actuatorData.ApplyModifiedPropertiesWithoutUndo();

        GameObject exit = new GameObject("Level04A_RouteExit");
        exit.transform.SetParent(dark);
        exit.transform.position = end + forward * 2f + Vector3.up;
        BoxCollider exitCollider = exit.AddComponent<BoxCollider>();
        exitCollider.isTrigger = true;
        exitCollider.size = new Vector3(5f, 2.5f, 2f);
        RouteAdvanceTrigger routeExit = exit.AddComponent<RouteAdvanceTrigger>();
        SerializedObject exitData = new SerializedObject(routeExit);
        exitData.FindProperty("requiredOpenDoor").objectReferenceValue = door;
        exitData.ApplyModifiedPropertiesWithoutUndo();
    }

    static void MigrateFormalRoute()
    {
        EditorBuildSettingsScene[] oldScenes = EditorBuildSettings.scenes;
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(oldScenes);
        if (!scenes.Exists(item => item.path == ScenePath))
        {
            int level04Index = scenes.FindIndex(item => item.path.EndsWith("/Level_04.unity"));
            scenes.Insert(level04Index >= 0 ? level04Index + 1 : scenes.Count,
                new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        Scene level04 = EditorSceneManager.OpenScene("Assets/DoNotLeaveMe/Levels/Level_04.unity", OpenSceneMode.Single);
        foreach (GameObject sceneRoot in level04.GetRootGameObjects())
        {
            foreach (ActuatorTrigger trigger in sceneRoot.GetComponentsInChildren<ActuatorTrigger>(true))
            {
                SerializedObject data = new SerializedObject(trigger);
                SerializedProperty successor = data.FindProperty("successorScene");
                if (successor.stringValue == "Level_04B")
                {
                    successor.stringValue = "Level_04A";
                    data.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            foreach (PhysicalDoorExitBinding binding in sceneRoot.GetComponentsInChildren<PhysicalDoorExitBinding>(true))
                binding.enabled = false;
        }
        EditorSceneManager.SaveScene(level04);

        Scene persistent = EditorSceneManager.OpenScene("Assets/DoNotLeaveMe/Levels/Persistent.unity", OpenSceneMode.Single);
        GameFlowController flow = Object.FindObjectOfType<GameFlowController>();
        SerializedObject flowData = new SerializedObject(flow);
        SerializedProperty catalog = flowData.FindProperty("routeCatalog");
        bool hasLevel04A = false;
        int level04IndexInCatalog = -1;
        for (int i = 0; i < catalog.arraySize; i++)
        {
            string sceneName = catalog.GetArrayElementAtIndex(i).FindPropertyRelative("sceneName").stringValue;
            hasLevel04A |= sceneName == "Level_04A";
            if (sceneName == "Level_04") level04IndexInCatalog = i;
        }
        if (!hasLevel04A)
        {
            int insertIndex = level04IndexInCatalog >= 0 ? level04IndexInCatalog + 1 : catalog.arraySize;
            catalog.InsertArrayElementAtIndex(insertIndex);
            SerializedProperty entry = catalog.GetArrayElementAtIndex(insertIndex);
            entry.FindPropertyRelative("levelId").stringValue = "Level04A";
            entry.FindPropertyRelative("sceneName").stringValue = "Level_04A";
            entry.FindPropertyRelative("arrivalTransitionDoorName").stringValue = "ToLevel045";
            entry.FindPropertyRelative("sharedArtScenes").arraySize = 0;
            flowData.ApplyModifiedPropertiesWithoutUndo();
        }
        EditorSceneManager.SaveScene(persistent);
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
    }

    static void RemoveOldGeneratedRoot()
    {
        Scene oldScene = EditorSceneManager.OpenScene(OldScenePath, OpenSceneMode.Additive);
        GameObject oldRoot = FindRoot(oldScene, OldRootName);
        if (oldRoot != null)
        {
            Object.DestroyImmediate(oldRoot);
            EditorSceneManager.SaveScene(oldScene);
        }
        EditorSceneManager.CloseScene(oldScene, true);
    }

    static void BuildCorridor(Transform parent, Vector3 start, Vector3 end, float width, bool buildWalls = true)
    {
        Vector3 center = (start + end) * 0.5f;
        Vector3 direction = (end - start).normalized;
        float length = Vector3.Distance(start, end);
        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
        CreateCube(parent, "Floor", center - Vector3.up * 0.15f, new Vector3(width, 0.3f, length), rotation, true);
        if (!buildWalls)
            return;
        Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
        CreateCube(parent, "WallLeft", center - right * (width * 0.5f + 0.15f) + Vector3.up * 1.6f,
            new Vector3(0.3f, 3.5f, length), rotation, true);
        CreateCube(parent, "WallRight", center + right * (width * 0.5f + 0.15f) + Vector3.up * 1.6f,
            new Vector3(0.3f, 3.5f, length), rotation, true);
    }

    static void BuildObstacle(Transform parent, string name, Vector3 center, int lane, Vector3 forward, bool jumpable)
    {
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        center += right * (lane * 1.8f);
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent);
        root.transform.position = center;
        root.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        root.AddComponent<Level04BParkourObstacle>();

        Vector3 visualScale = jumpable ? new Vector3(1.45f, 0.75f, 0.8f) : new Vector3(1.45f, 2.4f, 0.8f);
        GameObject visual = CreateCube(root.transform, "PlaceholderVisual", center + Vector3.up * (visualScale.y * 0.5f),
            visualScale, root.transform.rotation, true);
        visual.transform.localPosition = new Vector3(0f, visualScale.y * 0.5f, 0f);
        visual.transform.localRotation = Quaternion.identity;

        GameObject evaluation = new GameObject("NearMissVolume");
        evaluation.transform.SetParent(root.transform, false);
        evaluation.transform.localPosition = new Vector3(0f, 1.1f, -1.1f);
        BoxCollider evaluationCollider = evaluation.AddComponent<BoxCollider>();
        evaluationCollider.isTrigger = true;
        evaluationCollider.size = new Vector3(2.2f, 2.8f, 2.6f);
        evaluation.AddComponent<Level04BParkourNearMissTrigger>();

        GameObject lethal = new GameObject("LethalVolume");
        lethal.transform.SetParent(root.transform, false);
        lethal.transform.localPosition = new Vector3(0f, visualScale.y * 0.5f, 0f);
        BoxCollider lethalCollider = lethal.AddComponent<BoxCollider>();
        lethalCollider.isTrigger = true;
        lethalCollider.size = visualScale + new Vector3(0.08f, 0.08f, 0.08f);
        lethal.AddComponent<Level04BParkourLethalTrigger>();
    }

    static GameObject CreateCube(Transform parent, string name, Vector3 position, Vector3 scale, Quaternion rotation, bool collider)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent);
        cube.transform.SetPositionAndRotation(position, rotation);
        cube.transform.localScale = scale;
        if (!collider)
            Object.DestroyImmediate(cube.GetComponent<Collider>());
        return cube;
    }

    static Transform Marker(Transform parent, string name, Vector3 position, Vector3 forward)
    {
        Transform marker = Child(parent, name);
        marker.position = position;
        if (forward.sqrMagnitude > 0.01f)
            marker.rotation = Quaternion.LookRotation(forward, Vector3.up);
        return marker;
    }

    static Transform Child(Transform parent, string name)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent, false);
        return child.transform;
    }

    static Transform FindInScene(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                if (child.name == name)
                    return child;
        }
        return null;
    }

    static GameObject FindRoot(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            if (root.name == name)
                return root;
        return null;
    }

    static Vector3 Flat(Vector3 value)
    {
        value.y = 0f;
        return value;
    }

    static void SetArray(SerializedObject owner, string propertyName, Transform[] values)
    {
        SerializedProperty property = owner.FindProperty(propertyName);
        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
    }

    static void SetRouteSegments(SerializedProperty property, Level04BParkourController.RouteSegment[] values)
    {
        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
        {
            SerializedProperty item = property.GetArrayElementAtIndex(i);
            item.FindPropertyRelative("name").stringValue = values[i].name;
            item.FindPropertyRelative("start").objectReferenceValue = values[i].start;
            item.FindPropertyRelative("end").objectReferenceValue = values[i].end;
            item.FindPropertyRelative("laneWidth").floatValue = values[i].laneWidth;
            item.FindPropertyRelative("runSpeed").floatValue = values[i].runSpeed;
            item.FindPropertyRelative("decisionWindow").floatValue = values[i].decisionWindow;
            item.FindPropertyRelative("requiredTurn").enumValueIndex = (int)values[i].requiredTurn;
            item.FindPropertyRelative("finalJunction").boolValue = values[i].finalJunction;
        }
    }
}
#endif
