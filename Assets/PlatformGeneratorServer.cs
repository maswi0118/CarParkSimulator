using rest_dtos;
using UnityEngine;
using System.Collections.Generic;

public class ParkingSpace
{
    public Dictionary<Vector2, string> coordinateToLabelMap;

    public ParkingSpace()
    {
        coordinateToLabelMap = new Dictionary<Vector2, string>();
    }

    public void AddLabel(Vector2 coordinate, string label)
    {
        if (!coordinateToLabelMap.ContainsKey(coordinate))
        {
            coordinateToLabelMap.Add(coordinate, label);
        }
    }
    
    public Vector2? GetCoordinatesByLabel(string label)
    {
        foreach (KeyValuePair<Vector2, string> entry in coordinateToLabelMap)
        {
            if (entry.Value == label)
            {
                return entry.Key;
            }
        }
        return null; 
    }
}

public class CarAnimator : MonoBehaviour
{
    public Transform target;
    public float speed = 5f;

    void Update()
    {
        if (target != null)
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target.position, step);

            if (Vector3.Distance(transform.position, target.position) < 0.001f)
            {
                target = null; // Stop moving when the target is reached
            }
        }
    }

    public void SetTarget(Vector3 newTarget)
    {
        target = new GameObject("Target").transform;
        target.position = newTarget;
    }
}

public class PlatformGeneratorServer : MonoBehaviour
{
    private const int serverPort = 3000;

    public ParkingSpace parkingSpaceMapper = new ParkingSpace();

    public GameObject carPrefab;
    public GameObject treePrefabOne;
    public GameObject treePrefabTwo;
    public GameObject blockPrefab;

    void Start()
    {
        StartServer();
    }

    void StartServer()
    {
        SimpleHTTPServer.Instance.Setup(serverPort);
        SimpleHTTPServer.Instance.OnGet += OnGetRequest;
        SimpleHTTPServer.Instance.OnPost += OnPostRequest;
        Debug.Log("Server started on port " + serverPort);
    }

    void GenerateCar(string data)
    {
        InstantiateCar(data);
    }

    void GenerateStructure(string data)
    {
        StructureDto structure = JsonUtility.FromJson<StructureDto>(data);
        if ("tree".Equals(structure.type))
        {
            InstantiateTree(structure);
        }
        else if ("block".Equals(structure.type))
        {
            InstantiateBlock(structure);
        }
    }

    void InstantiatePlatform()
    {
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.transform.position = new Vector3(0, 1, 0);
    }

    void InstantiateCar(string data)
    {
        CarDto carData = JsonUtility.FromJson<CarDto>(data);
        GameObject car = Instantiate(carPrefab);
        car.name = carData.name;

        CarAnimator carAnimator = car.AddComponent<CarAnimator>();

        // Set the initial position to A1 (you need to have a mapping for A1 in your coordinateToLabelMap)
        Vector2? initialCoordinates = parkingSpaceMapper.GetCoordinatesByLabel("A1");
        car.transform.position = new Vector3(initialCoordinates.Value.x, 0, initialCoordinates.Value.y);

        // Set the target position to the specified parking spot
        Vector2? targetCoordinates = parkingSpaceMapper.GetCoordinatesByLabel(carData.placeId);
        if (targetCoordinates.HasValue)
        {
            Vector3 targetPosition = new Vector3(targetCoordinates.Value.x, 0, targetCoordinates.Value.y);
            carAnimator.SetTarget(targetPosition);
        }
    }

    void InstantiateTree(StructureDto structure)
    {
        GameObject tree;
        if (structure.model == 1)
        {
            tree = Instantiate(treePrefabOne);
        }
        else
        {
            tree = Instantiate(treePrefabTwo);
        }
        Vector2? coordinates = parkingSpaceMapper.GetCoordinatesByLabel(structure.placeId);
        tree.transform.position = new Vector3(coordinates.Value.x, 0, coordinates.Value.y);
    }

    void InstantiateBlock(StructureDto structure)
    {
        GameObject block = Instantiate(blockPrefab);
        Vector2? coordinates = parkingSpaceMapper.GetCoordinatesByLabel(structure.placeId);
        block.transform.position = new Vector3(coordinates.Value.x, 0, coordinates.Value.y);
    }

    void OnGetRequest(string path)
    {
        // if ("/generate-board".Equals(path))
        // {
        //     GeneratePlatform();
        // }
    }

    void OnPostRequest(string path, string data)
    {
        if ("/new-car".Equals(path))
        {
            GenerateCar(data);
        }

        if ("/generate-board".Equals(path))
        {
            GeneratePlatform(data);
        }

        if ("/new-structure".Equals(path))
        {
            GenerateStructure(data);
        }
    }

    void generateRow(GameObject parkingSpace, string rowLabel, int amount, float startingX, float startingZ)
    {
        int horizontalLineLength = 3;
        int verticalLineLength = 5;

        GameObject lastSideLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lastSideLine.transform.SetParent(parkingSpace.transform);
        lastSideLine.transform.localScale = new Vector3(0.2f, 0.01f, 5); // Adjust scale as needed
        lastSideLine.transform.localPosition = new Vector3(startingX, 0.01f, startingZ); // Adjust position as needed
        Material lastLineMaterial = new Material(Shader.Find("Standard"));
        lastLineMaterial.color = Color.white;
        lastSideLine.GetComponent<Renderer>().material = lastLineMaterial;

        int i = 1;
        while (i <= amount)
        {
            float middleXCoordinate = (i * horizontalLineLength) + startingX - horizontalLineLength / 2f;
            float middleZCoordinate = startingZ - verticalLineLength / 2f;
            string label = rowLabel + i;

            parkingSpaceMapper.AddLabel(new Vector2(middleXCoordinate, middleZCoordinate), label);

            GameObject sideLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sideLine.transform.SetParent(parkingSpace.transform);
            sideLine.transform.localScale = new Vector3(0.2f, 0.01f, verticalLineLength);
            sideLine.transform.localPosition =
                new Vector3((i * horizontalLineLength) + startingX, 0.01f, startingZ);
            Material lineMaterial = new Material(Shader.Find("Standard"));
            lineMaterial.color = Color.white;
            sideLine.GetComponent<Renderer>().material = lineMaterial;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(parkingSpace.transform);
            textObj.transform.localPosition =
                new Vector3(middleXCoordinate, 0.01f, startingZ - 1.5f);
            textObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            TextMesh textMesh = textObj.AddComponent<TextMesh>();
            textMesh.text = label;
            textMesh.fontSize = 17;
            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.color = Color.white;

            i++;
        }

        GameObject bottomLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bottomLine.transform.SetParent(parkingSpace.transform);
        bottomLine.transform.localScale = new Vector3(amount * 3, 0.01f, 0.2f);
        float bottomLineXPosition = startingX + ((horizontalLineLength * amount) / 2f);
        bottomLine.transform.localPosition =
            new Vector3(bottomLineXPosition, 0.01f, startingZ + 2.4f);

        Material bottomMaterial = new Material(Shader.Find("Standard"));
        bottomMaterial.color = Color.white;
        bottomLine.GetComponent<Renderer>().material = bottomMaterial;
    }

    void generateMirrorRow(GameObject parkingSpace, string rowLabel, int amount, float startingX, float startingZ)
    {
        int horizontalLineLength = 3;
        int verticalLineLength = 5;

        GameObject lastSideLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lastSideLine.transform.SetParent(parkingSpace.transform);
        lastSideLine.transform.localScale = new Vector3(0.2f, 0.01f, 5);
        lastSideLine.transform.localPosition = new Vector3(startingX, 0.01f, startingZ);
        Material lastLineMaterial = new Material(Shader.Find("Standard"));
        lastLineMaterial.color = Color.white;
        lastSideLine.GetComponent<Renderer>().material = lastLineMaterial;

        int i = 1;
        while (i <= amount)
        {
            float middleXCoordinate = (i * horizontalLineLength) + startingX - horizontalLineLength / 2f;
            float middleZCoordinate = startingZ - verticalLineLength / 2f;
            string label = rowLabel + i;

            parkingSpaceMapper.AddLabel(new Vector2(middleXCoordinate, middleZCoordinate), label);

            GameObject sideLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sideLine.transform.SetParent(parkingSpace.transform);
            sideLine.transform.localScale = new Vector3(0.2f, 0.01f, verticalLineLength);
            sideLine.transform.localPosition =
                new Vector3((i * horizontalLineLength) + startingX, 0.01f, startingZ);
            Material lineMaterial = new Material(Shader.Find("Standard"));
            lineMaterial.color = Color.white;
            sideLine.GetComponent<Renderer>().material = lineMaterial;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(parkingSpace.transform);
            textObj.transform.localPosition =
                new Vector3(middleXCoordinate, 0.01f, startingZ - 1.5f);
            textObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            TextMesh textMesh = textObj.AddComponent<TextMesh>();
            textMesh.text = label;
            textMesh.fontSize = 17;
            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.color = Color.white;

            i++;
        }
    }

    void GeneratePlatform(string data)
    {
        PlatformDto platformData = JsonUtility.FromJson<PlatformDto>(data);

        GameObject terrain = GameObject.CreatePrimitive(PrimitiveType.Plane);
        terrain.transform.position = new Vector3(0, 0, 0);
        terrain.transform.localScale = new Vector3(6, 1, 6);

        Material asphaltMaterial = Resources.Load<Material>("Asphalt_material_11");
        Vector2 tiling = new Vector2(8, 8);
        asphaltMaterial.mainTextureScale = tiling;
        terrain.GetComponent<Renderer>().material = asphaltMaterial;

        GameObject parkingSpace = new GameObject("ParkingSpace");
        parkingSpace.transform.SetParent(terrain.transform);

        float startingX = -26.0f;
        float startingZ = 19.0f;

        string labels = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        int labelIdx = 0;

        for (int i = 1; i <= platformData.rows; i++)
        {
            generateMirrorRow(parkingSpace, labels[labelIdx++].ToString(), platformData.columns, startingX, startingZ + 5);
            generateRow(parkingSpace, labels[labelIdx++].ToString(), platformData.columns, startingX, startingZ);
            startingZ -= 14;
        }

        foreach (KeyValuePair<Vector2, string> entry in parkingSpaceMapper.coordinateToLabelMap)
        {
            Debug.Log("Coordinate: " + entry.Key + ", Label: " + entry.Value);
        }
    }
}
