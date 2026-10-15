using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Construye la escena jugable del Escape Room sobre MainScene.
/// Es idempotente: reutiliza los objetos que ya existen (buscados por nombre) y solo crea los que faltan.
/// Menú: EscapeRoom > Construir escena de juego. Batch: -executeMethod EscapeRoomSceneBuilder.BuildAndVerify
/// </summary>
public static class EscapeRoomSceneBuilder
{
    private const string ScenePath = "Assets/00_Scenes/MainScene.unity";
    private const string PlayerPrefabPath = "Assets/02_Prefabs/Player.prefab";
    private const string MaterialsFolder = "Assets/03_Materials";

    private const float RoomHalfSize = 5f;
    private const float WallHeight = 3f;
    private const float WallThickness = 0.2f;

    [MenuItem("EscapeRoom/Construir escena de juego")]
    public static void BuildFromMenu()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        Build();
        bool ok = VerifyReferences() && SimulateEscape();
        EditorUtility.DisplayDialog("Escape Room",
            ok ? "Escena construida, verificada y guardada en " + ScenePath + ".\nLa simulación confirma que el jugador puede escapar."
               : "La escena se construyó, pero la verificación falló. Revisa la Console.",
            "OK");
    }

    [MenuItem("EscapeRoom/Probar flujo completo (simulación)")]
    public static void SimulateFromMenu()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        bool ok = SimulateEscape();
        EditorUtility.DisplayDialog("Escape Room", ok ? "Simulación correcta: el jugador puede escapar." : "La simulación falló. Revisa la Console.", "OK");
    }

    /// <summary>Punto de entrada para -batchmode: construye, verifica referencias y simula la partida.</summary>
    public static void BuildAndVerify()
    {
        Build();
        bool ok = VerifyReferences() && SimulateEscape();
        if (!ok)
            throw new System.Exception("[EscapeRoomSceneBuilder] La verificación falló.");
        Debug.Log("[EscapeRoomSceneBuilder] Construcción y verificación OK.");
    }

    public static void Build()
    {
        SetupPlayerPrefab();

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        Material wallMat = GetOrCreateMaterial("Wall", new Color(0.78f, 0.74f, 0.66f));
        Material floorMat = GetOrCreateMaterial("Floor", new Color(0.36f, 0.27f, 0.2f));
        Material chestMat = GetOrCreateMaterial("Chest", new Color(0.55f, 0.33f, 0.14f));
        Material keyMat = GetOrCreateMaterial("Key", new Color(1f, 0.82f, 0.1f));
        Material doorMat = GetOrCreateMaterial("Door", new Color(0.3f, 0.18f, 0.1f));
        Material keypadMat = GetOrCreateMaterial("Keypad", new Color(0.15f, 0.35f, 0.6f));
        Material noteMat = GetOrCreateMaterial("Note", new Color(0.95f, 0.95f, 0.88f));
        Material tableMat = GetOrCreateMaterial("Table", new Color(0.45f, 0.3f, 0.18f));

        // Piso
        GameObject floor = Require(scene, "Floor");
        floor.GetComponent<Renderer>().sharedMaterial = floorMat;

        // Paredes: la pared norte deja un hueco para la puerta (x entre -1 y 1).
        Transform room = GetOrCreateEmpty(scene, "Room").transform;
        float y = WallHeight / 2f;
        float s = RoomHalfSize;
        CreateBox(scene, "Wall_North_Left", room, new Vector3(-3f, y, s), new Vector3(4f, WallHeight, WallThickness), wallMat);
        CreateBox(scene, "Wall_North_Right", room, new Vector3(3f, y, s), new Vector3(4f, WallHeight, WallThickness), wallMat);
        CreateBox(scene, "Wall_South", room, new Vector3(0f, y, -s), new Vector3(2 * s, WallHeight, WallThickness), wallMat);
        CreateBox(scene, "Wall_East", room, new Vector3(s, y, 0f), new Vector3(WallThickness, WallHeight, 2 * s), wallMat);
        CreateBox(scene, "Wall_West", room, new Vector3(-s, y, 0f), new Vector3(WallThickness, WallHeight, 2 * s), wallMat);

        // Jugador con cámara en primera persona.
        GameObject player = FindInScene(scene, "Player");
        if (player == null)
        {
            player = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath), scene);
            player.name = "Player";
        }
        player.transform.SetPositionAndRotation(new Vector3(0f, 1f, -3f), Quaternion.identity);

        GameObject cameraGo = Require(scene, "Main Camera");
        cameraGo.transform.SetParent(player.transform, false);
        cameraGo.transform.localPosition = new Vector3(0f, 0.6f, 0f);
        cameraGo.transform.localRotation = Quaternion.identity;
        Camera camera = cameraGo.GetComponent<Camera>();
        camera.nearClipPlane = 0.05f;

        SetField(player.GetComponent<PlayerController>(), "cameraPivot", cameraGo.transform);
        SetField(player.GetComponent<PlayerInteraction>(), "playerCamera", camera);

        // Cofre con tapa y llave dentro.
        GameObject chest = Require(scene, "Chest");
        chest.transform.SetPositionAndRotation(new Vector3(-3f, 0.4f, 2f), Quaternion.identity);
        chest.transform.localScale = new Vector3(1.5f, 0.8f, 1f);
        chest.GetComponent<Renderer>().sharedMaterial = chestMat;

        GameObject lid = CreateBox(scene, "Lid", chest.transform, Vector3.zero, Vector3.one, chestMat);
        lid.transform.localPosition = new Vector3(0f, 0.6f, 0f);
        lid.transform.localRotation = Quaternion.identity;
        lid.transform.localScale = new Vector3(1.02f, 0.2f, 1.02f);

        GameObject key = CreateBox(scene, "Key", null, new Vector3(-3f, 0.95f, 1.85f), new Vector3(0.45f, 0.08f, 0.15f), keyMat);
        key.GetComponent<BoxCollider>().size = new Vector3(1.5f, 3f, 3f); // más fácil de apuntar
        GetOrAdd<KeyItem>(key);
        key.SetActive(false);

        ChestController chestController = GetOrAdd<ChestController>(chest);
        SetField(chestController, "lid", lid.transform);
        SetField(chestController, "keyItem", key);

        // Mesa con la nota (pista).
        CreateBox(scene, "Table", null, new Vector3(3f, 0.4f, -3f), new Vector3(1.2f, 0.8f, 0.8f), tableMat);
        GameObject note = CreateBox(scene, "Note", null, new Vector3(3f, 0.81f, -3f), new Vector3(0.3f, 0.02f, 0.4f), noteMat);
        note.GetComponent<BoxCollider>().size = new Vector3(1f, 10f, 1f);
        GetOrAdd<ClueNote>(note);

        // Teclado numérico (PuzzleManager) en la pared este, cerca del cofre.
        GameObject keypad = Require(scene, "PuzzleManager");
        keypad.transform.SetPositionAndRotation(new Vector3(RoomHalfSize - WallThickness / 2f - 0.05f, 1.3f, 2f), Quaternion.identity);
        keypad.transform.localScale = new Vector3(0.1f, 0.5f, 0.4f);
        GetOrAddCubeVisual(keypad, keypadMat);
        SetField(keypad.GetComponent<PuzzleManager>(), "chest", chestController);
        SetField(keypad.GetComponent<PuzzleManager>(), "correctCode", "1234");

        // Puerta de salida.
        GameObject door = Require(scene, "Door");
        door.transform.SetPositionAndRotation(new Vector3(0f, 1.5f, RoomHalfSize), Quaternion.identity);
        door.transform.localScale = new Vector3(2f, WallHeight, 0.15f);
        door.GetComponent<Renderer>().sharedMaterial = doorMat;
        SetField(door.GetComponent<DoorController>(), "openAngle", -90f);

        // GameManager + UI.
        GameObject manager = Require(scene, "GameManager");
        manager.transform.position = Vector3.zero;
        GetOrAdd<GameUI>(manager);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("[EscapeRoomSceneBuilder] Escena construida: " + ScenePath);
    }

    private static void SetupPlayerPrefab()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
        try
        {
            // CharacterController reemplaza al CapsuleCollider para que el jugador choque con paredes.
            CapsuleCollider capsule = root.GetComponent<CapsuleCollider>();
            if (capsule != null)
                Object.DestroyImmediate(capsule);

            CharacterController cc = GetOrAdd<CharacterController>(root);
            cc.height = 2f;
            cc.radius = 0.4f;
            cc.center = Vector3.zero;

            GetOrAdd<PlayerInteraction>(root);

            PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    public static bool VerifyReferences()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var errors = new List<string>();

        void Check(bool condition, string error)
        {
            if (!condition) errors.Add(error);
        }

        GameObject player = FindInScene(scene, "Player");
        Check(player != null, "Falta Player");
        if (player != null)
        {
            Check(player.GetComponent<PlayerController>() != null, "Player sin PlayerController");
            Check(player.GetComponent<PlayerInteraction>() != null, "Player sin PlayerInteraction");
            Check(player.GetComponent<CharacterController>() != null, "Player sin CharacterController");
            Check(player.GetComponentInChildren<Camera>() != null, "Player sin cámara");
            Check(GetField(player.GetComponent<PlayerInteraction>(), "playerCamera") != null, "PlayerInteraction.playerCamera vacío");
        }

        ChestController chest = Object.FindObjectsByType<ChestController>(FindObjectsInactive.Include).FirstOrDefault();
        Check(chest != null, "Falta ChestController");
        if (chest != null)
        {
            Check(GetField(chest, "lid") != null, "Cofre sin tapa");
            Check(GetField(chest, "keyItem") != null, "Cofre sin llave asignada");
        }

        PuzzleManager puzzle = Object.FindObjectsByType<PuzzleManager>(FindObjectsInactive.Include).FirstOrDefault();
        Check(puzzle != null && GetField(puzzle, "chest") != null, "PuzzleManager sin cofre asignado");
        Check(Object.FindObjectsByType<KeyItem>(FindObjectsInactive.Include).Length == 1, "Debe existir exactamente una llave");
        Check(Object.FindObjectsByType<ClueNote>(FindObjectsInactive.Include).Length >= 1, "Falta la pista");
        Check(Object.FindObjectsByType<DoorController>(FindObjectsInactive.Include).Length == 1, "Debe existir exactamente una puerta");
        Check(Object.FindObjectsByType<GameManager>(FindObjectsInactive.Include).Length == 1, "Debe existir exactamente un GameManager");
        Check(Object.FindObjectsByType<GameUI>(FindObjectsInactive.Include).Length == 1, "Debe existir exactamente un GameUI");
        Check(EditorBuildSettings.scenes.Length > 0 && EditorBuildSettings.scenes[0].path == ScenePath, "MainScene no es la primera escena del Build");

        foreach (string error in errors)
            Debug.LogError("[Verificación] " + error);
        if (errors.Count == 0)
            Debug.Log("[Verificación] Todas las referencias están correctas.");
        return errors.Count == 0;
    }

    /// <summary>
    /// Recorre la partida completa llamando a la lógica de juego (sin guardar la escena):
    /// pista → código incorrecto → código correcto → cofre → llave → puerta.
    /// </summary>
    public static bool SimulateEscape()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var errors = new List<string>();

        void Check(bool condition, string error)
        {
            if (!condition) errors.Add(error);
        }

        try
        {
            GameManager gm = GameManager.Instance;
            DoorController door = Object.FindAnyObjectByType<DoorController>();
            ChestController chest = Object.FindAnyObjectByType<ChestController>();
            PuzzleManager puzzle = Object.FindAnyObjectByType<PuzzleManager>();
            ClueNote note = Object.FindAnyObjectByType<ClueNote>();
            KeyItem key = Object.FindObjectsByType<KeyItem>(FindObjectsInactive.Include).FirstOrDefault();

            Check(gm != null && door != null && chest != null && puzzle != null && note != null && key != null, "Faltan objetos del juego");
            if (errors.Count == 0)
            {
                Vector3 doorStart = door.transform.position;

                note.Interact();

                door.Interact();
                Check(!gm.EscapeCompleted, "La puerta se abrió sin llave");

                chest.Interact();
                Check(!key.gameObject.activeSelf, "El cofre se abrió sin resolver el código");

                Check(!puzzle.CheckCode("0000"), "El código incorrecto fue aceptado");
                Check(puzzle.CheckCode("1234"), "El código correcto fue rechazado");

                chest.Interact();
                Check(key.gameObject.activeSelf, "La llave no apareció al abrir el cofre");

                key.Interact();
                Check(gm.HasKey, "No se obtuvo la llave");
                Check(!key.gameObject.activeSelf, "La llave sigue visible después de tomarla");

                door.Interact();
                Check(gm.EscapeCompleted, "No se completó el escape al abrir la puerta con llave");
                Check(door.transform.position != doorStart, "La puerta no se movió al abrirse");
            }
        }
        finally
        {
            // Descarta los cambios hechos por la simulación.
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        foreach (string error in errors)
            Debug.LogError("[Simulación] " + error);
        if (errors.Count == 0)
            Debug.Log("[Simulación] Flujo completo OK: pista → código → cofre → llave → puerta → escape.");
        return errors.Count == 0;
    }

    private static GameObject FindInScene(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == name)
                    return t.gameObject;
            }
        }
        return null;
    }

    private static GameObject Require(Scene scene, string name)
    {
        GameObject go = FindInScene(scene, name);
        if (go == null)
            throw new System.Exception($"[EscapeRoomSceneBuilder] No se encontró '{name}' en {ScenePath}");
        return go;
    }

    private static GameObject GetOrCreateEmpty(Scene scene, string name)
    {
        GameObject go = FindInScene(scene, name);
        if (go == null)
        {
            go = new GameObject(name);
            SceneManager.MoveGameObjectToScene(go, scene);
        }
        return go;
    }

    private static GameObject CreateBox(Scene scene, string name, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        GameObject go = FindInScene(scene, name);
        if (go == null)
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
        }

        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = material;
        return go;
    }

    private static void GetOrAddCubeVisual(GameObject go, Material material)
    {
        MeshFilter filter = GetOrAdd<MeshFilter>(go);
        if (filter.sharedMesh == null)
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            filter.sharedMesh = temp.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(temp);
        }

        GetOrAdd<MeshRenderer>(go).sharedMaterial = material;
        GetOrAdd<BoxCollider>(go);
    }

    private static Material GetOrCreateMaterial(string name, Color color)
    {
        string path = $"{MaterialsFolder}/{name}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }

        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);
        mat.color = color;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static T GetOrAdd<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        return component != null ? component : go.AddComponent<T>();
    }

    private static void SetField(Object target, string field, object value)
    {
        var so = new SerializedObject(target);
        SerializedProperty prop = so.FindProperty(field);
        if (prop == null)
            throw new System.Exception($"[EscapeRoomSceneBuilder] {target.GetType().Name} no tiene el campo '{field}'");

        switch (value)
        {
            case Object obj: prop.objectReferenceValue = obj; break;
            case float f: prop.floatValue = f; break;
            case string str: prop.stringValue = str; break;
            default: throw new System.Exception($"Tipo no soportado para '{field}'");
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Object GetField(Object target, string field)
    {
        if (target == null) return null;
        return new SerializedObject(target).FindProperty(field)?.objectReferenceValue;
    }
}
