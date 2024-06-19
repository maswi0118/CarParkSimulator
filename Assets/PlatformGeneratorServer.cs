using rest_dtos;
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class GameObjectInfo
{
    public string type;
    public int model;
    public GameObject gObject;

    public GameObjectInfo(string type, int model, GameObject gObject)
    {
        Debug.Log(type + " " + model);
        this.type = type;
        this.model = model;
        this.gObject = gObject;
    }
}

[Serializable]
public class LabelGameObjectPair
{
    public string label;
    public GameObjectInfo gameObjectInfo;
}

[Serializable]
public class LabelGameObjectMap
{
    public List<LabelGameObjectPair> pairs = new List<LabelGameObjectPair>();
}

public class ParkingSpace
{
    public Dictionary<Vector2, string> coordinateToLabelMap;
    public Dictionary<string, GameObjectInfo> labelToGameObjectInfoMap;


    public ParkingSpace()
    {
        coordinateToLabelMap = new Dictionary<Vector2, string>();
        labelToGameObjectInfoMap = new Dictionary<string, GameObjectInfo>();
    }

    public void AddGameObjectInfo(string label, string type, int model, GameObject gObject)
    {
        Debug.Log("Adding game object info");
        Debug.Log(label + " " + type + " " + model);
        if (!labelToGameObjectInfoMap.ContainsKey(label))
        {
            labelToGameObjectInfoMap.Add(label, new GameObjectInfo(type, model, gObject));
        }
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

    public List<LabelGameObjectPair> ConvertDictionaryToList()
    {
        List<LabelGameObjectPair> list = new List<LabelGameObjectPair>();

        foreach (var pair in labelToGameObjectInfoMap)
        {
            list.Add(new LabelGameObjectPair
            {
                label = pair.Key,
                gameObjectInfo = pair.Value
            });
        }

        return list;
    }

}
public class CarAnimator : MonoBehaviour
{
    private Transform target;
    private Vector3 finalTarget;
    private Vector3 middleRowPosition;
    private Vector3 finalAdjustmentPosition;
    private int step = 0;
    public float speed = 5f;

    void Update()
    {
        if (target != null)
        {
            float stepDistance = speed * Time.deltaTime;

            switch (this.step)
            {
                case 0:
                    // Move along Z axis to the middle of the row
                    RotateCar(middleRowPosition - transform.position);
                    transform.position = Vector3.MoveTowards(transform.position, middleRowPosition, stepDistance);
                    if (Vector3.Distance(transform.position, middleRowPosition) < 0.001f)
                    {
                        this.step = 1;
                    }
                    break;
                case 1:
                    // Move along X axis to the target column
                    RotateCar(finalTarget - transform.position);
                    transform.position = Vector3.MoveTowards(transform.position, finalTarget, stepDistance);
                    if (Vector3.Distance(transform.position, finalTarget) < 0.001f)
                    {
                        this.step = 2;
                    }
                    break;
                case 2:
                    // Final adjustment in Z axis
                    RotateCar(finalAdjustmentPosition - transform.position);
                    transform.position = Vector3.MoveTowards(transform.position, finalAdjustmentPosition, stepDistance);
                    if (Vector3.Distance(transform.position, finalAdjustmentPosition) < 0.001f)
                    {
                        target = null;
                    }
                    break;
            }
        }
    }

    private void RotateCar(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            // Calculate the angle between the current forward direction and the target direction
            float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, angle + 180, 0);
        }
    }

    public void SetTarget(Vector3 newTarget, string label)
    {
        target = new GameObject("Target").transform;

        // Determine the row character from the label (first character)
        char row = label[0];

        // Calculate middle of the row position (between B and C, D and E, etc.)
        float middleZ;
        if (row % 2 == 1) // even rows (B, D, F, etc.)
        {
            middleZ = newTarget.z + 4.0f;
        }
        else // odd rows (A, C, E, etc.)
        {
            middleZ = newTarget.z - 5.0f;
        }

        middleRowPosition = new Vector3(transform.position.x, 0.8f, middleZ);

        // Set final target
        finalTarget = new Vector3(newTarget.x, 0.8f, middleZ);

        // Determine the final adjustment position
        finalAdjustmentPosition = new Vector3(newTarget.x, 0.8f, newTarget.z);
    }
}


public class PlatformGeneratorServer : MonoBehaviour
{
    private const int serverPort = 3000;

    public ParkingSpace parkingSpaceMapper = new ParkingSpace();

    public GameObject carPrefab1;
    public GameObject carPrefab2;
    public GameObject carPrefab3;
    public GameObject carPrefab4;

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

        GameObject car;

        if (carData.model == 1)
        {
            car = Instantiate(carPrefab1);
        }
        else if (carData.model == 2)
        {
            car = Instantiate(carPrefab2);
        }
        else if (carData.model == 3)
        {
            car = Instantiate(carPrefab3);
        }
        else
        {
            car = Instantiate(carPrefab4);
        }

        CarAnimator carAnimator = car.AddComponent<CarAnimator>();

        // Set the initial position to A1 (you need to have a mapping for A1 in your coordinateToLabelMap)
        Vector2? initialCoordinates = parkingSpaceMapper.GetCoordinatesByLabel("A1");
        Debug.Log("Initial coordinates: " + initialCoordinates);
        car.transform.position = new Vector3(initialCoordinates.Value.x - 5, 0.8f, initialCoordinates.Value.y + 8);
        car.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f);

        // Set the target position to the specified parking spot
        Vector2? targetCoordinates = parkingSpaceMapper.GetCoordinatesByLabel(carData.placeId);
        if (targetCoordinates.HasValue)
        {
            Vector3 targetPosition = new Vector3(targetCoordinates.Value.x, 0.8f, targetCoordinates.Value.y);
            carAnimator.SetTarget(targetPosition, carData.placeId);
        }

        parkingSpaceMapper.AddGameObjectInfo(carData.placeId, "Car", carData.model, car);
        Debug.Log("Car model:" + carData.model + " is moving to " + carData.placeId);
        Debug.Log(parkingSpaceMapper.labelToGameObjectInfoMap.Count);
        // Debug.Log(parkingSpaceMapper.labelToGameObjectInfoMap[carData.placeId]);
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

        parkingSpaceMapper.AddGameObjectInfo(structure.placeId, structure.type, structure.model, tree);
        Debug.Log("Tree model:" + structure.model + " is moving to " + structure.placeId);
        Debug.Log(parkingSpaceMapper.labelToGameObjectInfoMap.Count);
    }

    void InstantiateBlock(StructureDto structure)
    {
        GameObject block = Instantiate(blockPrefab);
        Vector2? coordinates = parkingSpaceMapper.GetCoordinatesByLabel(structure.placeId);
        block.transform.position = new Vector3(coordinates.Value.x, 0, coordinates.Value.y);

        parkingSpaceMapper.AddGameObjectInfo(structure.placeId, structure.type, structure.model, block);
        Debug.Log("Tree model:" + structure.model + " is moving to " + structure.placeId);
        Debug.Log(parkingSpaceMapper.labelToGameObjectInfoMap.Count);
    }

    void DeleteObject(string data)
    {
        DeleteDto structure = JsonUtility.FromJson<DeleteDto>(data);
        Vector2? coordinates = parkingSpaceMapper.GetCoordinatesByLabel(structure.placeId);


        if (coordinates.HasValue && parkingSpaceMapper.labelToGameObjectInfoMap.ContainsKey(structure.placeId))
        {
            // Find and destroy the GameObject
            GameObjectInfo gameObjectInfo = parkingSpaceMapper.labelToGameObjectInfoMap[structure.placeId];
            GameObject objectToDelete = gameObjectInfo.gObject; // assuming `model` is storing the GameObject reference

            if (objectToDelete != null)
            {
                Destroy(objectToDelete);
            }

            // Remove from the dictionaries
            parkingSpaceMapper.coordinateToLabelMap.Remove(coordinates.Value);
            parkingSpaceMapper.labelToGameObjectInfoMap.Remove(structure.placeId);

            Debug.Log("Object at " + structure.placeId + " deleted.");
        }
        else
        {
            Debug.LogError("Object at " + structure.placeId + " not found.");
        }
    }


    string OnGetRequest(string path)
    {
        // if ("/generate-board".Equals(path))
        // {
        //     GeneratePlatform();
        // }
        if ("/current-state".Equals(path))
        {
            List<LabelGameObjectPair> list = parkingSpaceMapper.ConvertDictionaryToList();
            LabelGameObjectMap map = new LabelGameObjectMap { pairs = list };
            string json = JsonUtility.ToJson(map);
            Debug.Log("Generated JSON: " + json);
            return json;
        }
        return "{}";

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

        if ("/delete-object".Equals(path))
        {
            DeleteObject(data);
        }
        if ("/repark-car".Equals(path))
        {
            ReparkCar(data);
        }
    }

    void ReparkCar(string data)
    {
        
    }

    void generateRow(GameObject parkingSpace, string rowLabel, int amount, float startingX, float startingZ)
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
            Debug.Log(startingZ + " " + verticalLineLength);
            Debug.Log(rowLabel);
            float middleZCoordinate = startingZ;// - verticalLineLength / 2f;
            string label = rowLabel + i;

            parkingSpaceMapper.AddLabel(new Vector2(middleXCoordinate, middleZCoordinate), label);

            GameObject sideLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sideLine.transform.SetParent(parkingSpace.transform);
            sideLine.transform.localScale = new Vector3(0.2f, 0.01f, verticalLineLength);
            sideLine.transform.localPosition = new Vector3((i * horizontalLineLength) + startingX, 0.01f, startingZ);
            Material lineMaterial = new Material(Shader.Find("Standard"));
            lineMaterial.color = Color.white;
            sideLine.GetComponent<Renderer>().material = lineMaterial;

            GameObject textObj = new GameObject("TextLabel");
            textObj.transform.SetParent(parkingSpace.transform);
            TextMeshPro textMeshPro = textObj.AddComponent<TextMeshPro>();
            textMeshPro.text = label;
            textMeshPro.fontSize = 17;
            textMeshPro.alignment = TextAlignmentOptions.Center;
            textMeshPro.color = Color.white;
            textObj.transform.localPosition =
                new Vector3(middleXCoordinate, 0.01f, startingZ - 1.5f);
            textObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            i++;
        }

        GameObject bottomLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bottomLine.transform.SetParent(parkingSpace.transform);
        bottomLine.transform.localScale = new Vector3(amount * 3, 0.01f, 0.2f);
        float bottomLineXPosition = startingX + ((horizontalLineLength * amount) / 2f);
        bottomLine.transform.localPosition = new Vector3(bottomLineXPosition, 0.01f, startingZ + 2.4f);

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
            float middleZCoordinate = startingZ;// - verticalLineLength / 2f;
            string label = rowLabel + i;

            parkingSpaceMapper.AddLabel(new Vector2(middleXCoordinate, middleZCoordinate), label);

            GameObject sideLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sideLine.transform.SetParent(parkingSpace.transform);
            sideLine.transform.localScale = new Vector3(0.2f, 0.01f, verticalLineLength);
            sideLine.transform.localPosition = new Vector3((i * horizontalLineLength) + startingX, 0.01f, startingZ);
            Material lineMaterial = new Material(Shader.Find("Standard"));
            lineMaterial.color = Color.white;
            sideLine.GetComponent<Renderer>().material = lineMaterial;

            GameObject textObj = new GameObject("TextLabel");
            textObj.transform.SetParent(parkingSpace.transform);
            TextMeshPro textMeshPro = textObj.AddComponent<TextMeshPro>();
            textMeshPro.text = label;
            textMeshPro.fontSize = 17;
            textMeshPro.alignment = TextAlignmentOptions.Center;
            textMeshPro.color = Color.white;
            textObj.transform.localPosition =
                new Vector3(middleXCoordinate, 0.01f, startingZ + 1.5f);
            textObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            i++;
        }
    }

    void GeneratePlatform(string data)
    {
        PlatformDto platformData = JsonUtility.FromJson<PlatformDto>(data);

        GameObject terrain = GameObject.CreatePrimitive(PrimitiveType.Plane);
        terrain.transform.position = new Vector3(0, 0, 0);
        terrain.transform.localScale = new Vector3(20, 1, 20);

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
