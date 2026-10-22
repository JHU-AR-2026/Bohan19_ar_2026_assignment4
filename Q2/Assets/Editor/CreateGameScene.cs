using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

[InitializeOnLoad]
public static class CreateGameScene
{
    const string scenePath = "Assets/Scenes/RollABall.unity";

    static CreateGameScene()
    {
        EditorApplication.delayCall += CreateOnFirstOpen;
    }

    static void CreateOnFirstOpen()
    {
        if (!File.Exists(scenePath))
            CreateScene();
    }

    [MenuItem("Q2/Open Game Scene")]
    public static void OpenScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        if (File.Exists(scenePath))
            EditorSceneManager.OpenScene(scenePath);
        else
            CreateScene();
    }

    public static void CreateScene()
    {
        Directory.CreateDirectory("Assets/Scenes");
        Directory.CreateDirectory("Assets/Materials");
        AssetDatabase.Refresh();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Material floor = MakeMaterial("Floor", new Color(0.3f, 0.4f, 0.5f));
        Material wall = MakeMaterial("Wall", new Color(0.65f, 0.7f, 0.75f));
        Material blue = MakeMaterial("Player", new Color(0.1f, 0.55f, 1f));
        Material yellow = MakeMaterial("Collectible", Color.yellow);
        Material orange = MakeMaterial("PowerUp", new Color(1f, 0.45f, 0f));
        Material red = MakeMaterial("BreakableWall", new Color(0.85f, 0.15f, 0.15f));
        Material green = MakeMaterial("Goal", new Color(0.1f, 1f, 0.4f, 0.3f));

        green.SetFloat("_Mode", 2);
        green.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        green.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        green.SetInt("_ZWrite", 0);
        green.EnableKeyword("_ALPHABLEND_ON");
        green.renderQueue = 3000;
        EditorUtility.SetDirty(green);

        MakeFloor("Main Floor", new Vector3(0, -0.5f, 0), new Vector3(20, 1, 20), floor);
        MakeObject("Left Wall", PrimitiveType.Cube, new Vector3(-10, 2.5f, 0), new Vector3(1, 5, 21), wall);
        MakeObject("Right Wall", PrimitiveType.Cube, new Vector3(10, 2.5f, 0), new Vector3(1, 5, 21), wall);
        MakeObject("Back Wall", PrimitiveType.Cube, new Vector3(0, 2.5f, -10), new Vector3(20, 5, 1), wall);
        MakeObject("Front Left Wall", PrimitiveType.Cube, new Vector3(-6, 2.5f, 10), new Vector3(8, 5, 1), wall);
        MakeObject("Front Right Wall", PrimitiveType.Cube, new Vector3(6, 2.5f, 10), new Vector3(8, 5, 1), wall);

        MakeFloor("Runway", new Vector3(0, -0.5f, 12), new Vector3(4, 1, 4), floor);
        MakeFloor("Landing", new Vector3(0, -0.5f, 20), new Vector3(4, 1, 6), floor);
        MakeObject("Runway Left Wall", PrimitiveType.Cube, new Vector3(-2.25f, 2.5f, 12), new Vector3(0.5f, 5, 4), wall);
        MakeObject("Runway Right Wall", PrimitiveType.Cube, new Vector3(2.25f, 2.5f, 12), new Vector3(0.5f, 5, 4), wall);
        GameObject barrier = MakeObject("Breakable Wall", PrimitiveType.Cube,
            new Vector3(0, 2.5f, 11), new Vector3(4, 5, 0.5f), red);
        barrier.tag = "BreakableWall";

        GameManager game = new GameObject("Game Manager").AddComponent<GameManager>();
        GameObject ball = MakeObject("Player", PrimitiveType.Sphere,
            new Vector3(0, 0.5f, -6), Vector3.one, blue);
        Rigidbody body = ball.AddComponent<Rigidbody>();
        body.useGravity = false;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        PlayerController controller = ball.AddComponent<PlayerController>();
        controller.game = game;
        game.player = body;

        Vector3[] positions = {
            new Vector3(-6, 0.6f, -5),
            new Vector3(6, 0.6f, -5),
            new Vector3(0, 0.6f, 0),
            new Vector3(-6, 2.3f, 3),
            new Vector3(6, 2.3f, 3),
            new Vector3(0, 2.3f, 6)
        };
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject cube = MakeObject("Collectible " + (i + 1), PrimitiveType.Cube,
                positions[i], Vector3.one * 0.6f, yellow);
            cube.tag = "Collectible";
            cube.GetComponent<Collider>().isTrigger = true;
        }
        game.totalCubes = positions.Length;

        GameObject capsule = MakeObject("Power Up", PrimitiveType.Capsule,
            new Vector3(0, 1, 8), new Vector3(0.7f, 0.8f, 0.7f), orange);
        capsule.tag = "PowerUp";
        capsule.GetComponent<Collider>().isTrigger = true;
        capsule.SetActive(false);
        game.powerUp = capsule;

        GameObject goal = MakeObject("Goal Zone", PrimitiveType.Cube,
            new Vector3(0, 1.5f, 20), new Vector3(3.8f, 3, 2), green);
        goal.tag = "Goal";
        goal.GetComponent<Collider>().isTrigger = true;

        Camera camera = new GameObject("Main Camera").AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.transform.position = ball.transform.position + new Vector3(0, 12, -10);
        camera.transform.LookAt(ball.transform.position);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.12f, 0.17f, 0.23f);
        camera.fieldOfView = 60f;
        camera.gameObject.AddComponent<AudioListener>();
        camera.gameObject.AddComponent<CameraFollow>().player = ball.transform;

        Light light = new GameObject("Directional Light").AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.transform.rotation = Quaternion.Euler(50, -30, 0);
        RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.55f);

        EditorSceneManager.SaveScene(scene, scenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };
        AssetDatabase.SaveAssets();
    }

    static Material MakeMaterial(string name, Color color)
    {
        Material material = new Material(Shader.Find("Standard"));
        material.color = color;
        AssetDatabase.CreateAsset(material, "Assets/Materials/" + name + ".mat");
        return material;
    }

    static GameObject MakeObject(string name, PrimitiveType shape, Vector3 position,
        Vector3 scale, Material material)
    {
        GameObject obj = GameObject.CreatePrimitive(shape);
        obj.name = name;
        obj.transform.position = position;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = material;
        return obj;
    }

    static void MakeFloor(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject obj = MakeObject(name, PrimitiveType.Cube, position, scale, material);
        obj.layer = 6;
    }
}
