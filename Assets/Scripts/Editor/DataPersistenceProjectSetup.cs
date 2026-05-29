using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class DataPersistenceProjectSetup
{
    private const string MainMenuScene = "Assets/Scenes/MainMenu.unity";
    private const string GameScene = "Assets/Scenes/Game.unity";

    public static void CreateProject()
    {
        EnsureFolders();
        CreateMaterials();
        CreateBrickPrefab();
        CreateMainMenuScene();
        CreateGameScene();

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MainMenuScene, true),
            new EditorBuildSettingsScene(GameScene, true)
        };

        AssetDatabase.SaveAssets();
    }

    private static void CreateMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "MainMenu";
        CreateCamera(new Color(0.93f, 0.95f, 0.98f));
        new GameObject("Data Manager").AddComponent<DataManager>();

        var canvas = CreateCanvas();
        var title = CreateText(canvas.transform, "Title", "Data Persistence Breakout", new Vector2(0f, 170f), TextAnchor.MiddleCenter, 42, new Vector2(620f, 70f));
        title.color = new Color(0.1f, 0.16f, 0.22f);
        CreateText(canvas.transform, "Name Label", "Player name", new Vector2(0f, 80f), TextAnchor.MiddleCenter, 22, new Vector2(260f, 40f));
        var input = CreateInputField(canvas.transform, "Player Name Input", new Vector2(0f, 32f));
        var highScore = CreateText(canvas.transform, "High Score Text", "Best Score: none yet", new Vector2(0f, -35f), TextAnchor.MiddleCenter, 24, new Vector2(520f, 50f));
        var startButton = CreateButton(canvas.transform, "Start Button", "Start", new Vector2(-110f, -115f), new Vector2(180f, 54f));
        var quitButton = CreateButton(canvas.transform, "Quit Button", "Quit", new Vector2(110f, -115f), new Vector2(180f, 54f));

        var ui = new GameObject("Main Menu UI").AddComponent<MainMenuUI>();
        var serialized = new SerializedObject(ui);
        serialized.FindProperty("playerNameInput").objectReferenceValue = input;
        serialized.FindProperty("highScoreText").objectReferenceValue = highScore;
        serialized.FindProperty("startButton").objectReferenceValue = startButton;
        serialized.FindProperty("quitButton").objectReferenceValue = quitButton;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        CreateEventSystem();
        EditorSceneManager.SaveScene(scene, MainMenuScene);
    }

    private static void CreateGameScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Game";
        CreateCamera(new Color(0.94f, 0.96f, 0.92f));
        new GameObject("Data Manager").AddComponent<DataManager>();

        var bounceMaterial = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Materials/Bouncy.physicsMaterial2D");
        var paddle = CreateBox("Paddle", new Vector3(0f, -3.8f, 0f), new Vector3(2.2f, 0.28f, 0.2f), LoadMaterial("PaddleBlue"));
        Object.DestroyImmediate(paddle.GetComponent<BoxCollider>());
        var paddleCollider = paddle.AddComponent<BoxCollider2D>();
        paddleCollider.sharedMaterial = bounceMaterial;

        var ball = CreateBox("Ball", new Vector3(0f, -3.25f, 0f), new Vector3(0.36f, 0.36f, 0.36f), LoadMaterial("BallWhite"));
        Object.DestroyImmediate(ball.GetComponent<BoxCollider>());
        var ballCollider = ball.AddComponent<CircleCollider2D>();
        ballCollider.sharedMaterial = bounceMaterial;
        var rigidbody = ball.AddComponent<Rigidbody2D>();
        rigidbody.gravityScale = 0f;
        rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rigidbody.freezeRotation = true;

        CreateWall("Left Wall", new Vector3(-7f, 0f, 0f), new Vector3(0.25f, 8.5f, 0.2f), bounceMaterial);
        CreateWall("Right Wall", new Vector3(7f, 0f, 0f), new Vector3(0.25f, 8.5f, 0.2f), bounceMaterial);
        CreateWall("Top Wall", new Vector3(0f, 4.4f, 0f), new Vector3(14f, 0.25f, 0.2f), bounceMaterial);
        CreateDeathZone();

        var canvas = CreateCanvas();
        var scoreText = CreateText(canvas.transform, "Score Text", "Score: 0", new Vector2(24f, -24f), TextAnchor.UpperLeft, 26, new Vector2(260f, 44f));
        var highScoreText = CreateText(canvas.transform, "High Score Text", "Best: none", new Vector2(-24f, -24f), TextAnchor.UpperRight, 26, new Vector2(420f, 44f));
        var messageText = CreateText(canvas.transform, "Message Text", "Press Space to launch", new Vector2(0f, -24f), TextAnchor.UpperCenter, 28, new Vector2(520f, 50f));
        var restartButton = CreateButton(canvas.transform, "Restart Button", "Restart", new Vector2(-110f, -95f), new Vector2(180f, 52f));
        var menuButton = CreateButton(canvas.transform, "Menu Button", "Menu", new Vector2(110f, -95f), new Vector2(180f, 52f));

        var controller = new GameObject("Game Controller").AddComponent<GameController>();
        var serialized = new SerializedObject(controller);
        serialized.FindProperty("scoreText").objectReferenceValue = scoreText;
        serialized.FindProperty("highScoreText").objectReferenceValue = highScoreText;
        serialized.FindProperty("messageText").objectReferenceValue = messageText;
        serialized.FindProperty("restartButton").objectReferenceValue = restartButton;
        serialized.FindProperty("menuButton").objectReferenceValue = menuButton;
        serialized.FindProperty("paddle").objectReferenceValue = paddle.transform;
        serialized.FindProperty("ball").objectReferenceValue = rigidbody;
        serialized.FindProperty("brickPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Brick.prefab");
        serialized.ApplyModifiedPropertiesWithoutUndo();

        CreateEventSystem();
        EditorSceneManager.SaveScene(scene, GameScene);
    }

    private static void CreateBrickPrefab()
    {
        string path = "Assets/Prefabs/Brick.prefab";
        var brick = CreateBox("Brick", Vector3.zero, new Vector3(1.18f, 0.36f, 0.2f), LoadMaterial("BrickOrange"));
        Object.DestroyImmediate(brick.GetComponent<BoxCollider>());
        brick.AddComponent<BoxCollider2D>().sharedMaterial = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Materials/Bouncy.physicsMaterial2D");
        brick.AddComponent<Brick>();
        PrefabUtility.SaveAsPrefabAsset(brick, path);
        Object.DestroyImmediate(brick);
    }

    private static void CreateMaterials()
    {
        CreateMaterial("PaddleBlue", new Color(0.1f, 0.32f, 0.62f));
        CreateMaterial("BallWhite", new Color(0.96f, 0.96f, 0.94f));
        CreateMaterial("BrickOrange", new Color(0.95f, 0.43f, 0.18f));
        CreateMaterial("WallGray", new Color(0.22f, 0.24f, 0.27f));

        string path = "Assets/Materials/Bouncy.physicsMaterial2D";
        if (AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path) == null)
        {
            var material = new PhysicsMaterial2D("Bouncy") { friction = 0f, bounciness = 1f };
            AssetDatabase.CreateAsset(material, path);
        }
    }

    private static GameObject CreateBox(string name, Vector3 position, Vector3 scale, Material material)
    {
        var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.position = position;
        box.transform.localScale = scale;
        box.GetComponent<Renderer>().sharedMaterial = material;
        return box;
    }

    private static void CreateWall(string name, Vector3 position, Vector3 scale, PhysicsMaterial2D material)
    {
        var wall = CreateBox(name, position, scale, LoadMaterial("WallGray"));
        Object.DestroyImmediate(wall.GetComponent<BoxCollider>());
        wall.AddComponent<BoxCollider2D>().sharedMaterial = material;
    }

    private static void CreateDeathZone()
    {
        var deathZone = new GameObject("Death Zone");
        deathZone.transform.position = new Vector3(0f, -4.75f, 0f);
        var collider = deathZone.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(15f, 0.5f);
        deathZone.AddComponent<DeathZone>();
    }

    private static void CreateCamera(Color background)
    {
        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        var camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.backgroundColor = background;
        camera.clearFlags = CameraClearFlags.SolidColor;
    }

    private static Canvas CreateCanvas()
    {
        var canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        return canvas;
    }

    private static Text CreateText(Transform parent, string name, string label, Vector2 position, TextAnchor anchor, int size, Vector2 dimensions)
    {
        var textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        var rect = textObject.GetComponent<RectTransform>();
        rect.sizeDelta = dimensions;
        rect.anchoredPosition = position;
        SetAnchor(rect, anchor);

        var text = textObject.GetComponent<Text>();
        text.text = label;
        text.font = BuiltInFont();
        text.fontSize = size;
        text.alignment = anchor;
        text.color = Color.black;
        return text;
    }

    private static InputField CreateInputField(Transform parent, string name, Vector2 position)
    {
        var inputObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(InputField));
        inputObject.transform.SetParent(parent, false);
        var rect = inputObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(360f, 48f);
        rect.anchoredPosition = position;
        SetAnchor(rect, TextAnchor.MiddleCenter);
        inputObject.GetComponent<Image>().color = Color.white;

        var text = CreateText(inputObject.transform, "Text", "", Vector2.zero, TextAnchor.MiddleLeft, 22, new Vector2(330f, 42f));
        Stretch(text.GetComponent<RectTransform>(), new Vector2(16f, 4f), new Vector2(-16f, -4f));
        var placeholder = CreateText(inputObject.transform, "Placeholder", "Enter name", Vector2.zero, TextAnchor.MiddleLeft, 22, new Vector2(330f, 42f));
        placeholder.color = new Color(0.5f, 0.5f, 0.5f);
        Stretch(placeholder.GetComponent<RectTransform>(), new Vector2(16f, 4f), new Vector2(-16f, -4f));

        var input = inputObject.GetComponent<InputField>();
        input.textComponent = text;
        input.placeholder = placeholder;
        return input;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 position, Vector2 size)
    {
        var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        var rect = buttonObject.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        SetAnchor(rect, TextAnchor.MiddleCenter);

        var image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.1f, 0.36f, 0.62f);

        var button = buttonObject.GetComponent<Button>();
        var labelText = CreateText(buttonObject.transform, "Text", label, Vector2.zero, TextAnchor.MiddleCenter, 23, size);
        labelText.color = Color.white;
        Stretch(labelText.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
        return button;
    }

    private static Material CreateMaterial(string name, Color color)
    {
        string path = "Assets/Materials/" + name + ".mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
        {
            return existing;
        }

        var material = new Material(Shader.Find("Standard"));
        material.color = color;
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static Material LoadMaterial(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/" + name + ".mat");
    }

    private static void CreateEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }

    private static void EnsureFolders()
    {
        CreateFolder("Assets", "Scenes");
        CreateFolder("Assets", "Prefabs");
        CreateFolder("Assets", "Materials");
    }

    private static void CreateFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }

    private static Font BuiltInFont()
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    private static void SetAnchor(RectTransform rect, TextAnchor anchor)
    {
        Vector2 anchorPoint = anchor switch
        {
            TextAnchor.UpperLeft => new Vector2(0f, 1f),
            TextAnchor.UpperRight => new Vector2(1f, 1f),
            TextAnchor.UpperCenter => new Vector2(0.5f, 1f),
            TextAnchor.MiddleLeft => new Vector2(0f, 0.5f),
            TextAnchor.MiddleCenter => new Vector2(0.5f, 0.5f),
            _ => new Vector2(0.5f, 0.5f)
        };

        rect.anchorMin = anchorPoint;
        rect.anchorMax = anchorPoint;
        rect.pivot = anchorPoint;
    }
}
