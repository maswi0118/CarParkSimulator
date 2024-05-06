using UnityEngine;
using System.Collections.Generic;
    
public class ParkingSpace
{
    public Dictionary<Vector2, string> coordinateToLabelMap;

    // Constructor to initialize the dictionary
    public ParkingSpace()
    {
        coordinateToLabelMap = new Dictionary<Vector2, string>();
    }

    // Method to add a coordinate-label pair to the dictionary
    public void AddLabel(Vector2Int coordinate, string label)
    {
        if (!coordinateToLabelMap.ContainsKey(coordinate))
        {
            coordinateToLabelMap.Add(coordinate, label);
        }
    }
}

public class PlatformGeneratorServer : MonoBehaviour
{
    private const int serverPort = 3000; // Choose a port number
    
    public ParkingSpace parkingSpace = new ParkingSpace();
    
    void Start()
    {
        StartServer();
    }

    void StartServer()
    {
        SimpleHTTPServer.Instance.Setup(serverPort);
        SimpleHTTPServer.Instance.OnGet += OnGetRequest;
        Debug.Log("Server started on port " + serverPort);
    }
    

    

    void OnGetRequest(string path)
    {
        // Handle the incoming request and generate the platform
        GeneratePlatform();
    }
    
    void generateRow(GameObject parkingSpace, int amount, float startingX, float startingZ)
    {
        int horizontalLineLength = 3;
        int verticaltLineLength = 5;
        
        GameObject lastSideLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lastSideLine.transform.SetParent(parkingSpace.transform);
        lastSideLine.transform.localScale = new Vector3(0.2f, 0.01f, 5); // Adjust scale as needed
        lastSideLine.transform.localPosition = new Vector3(startingX, 0.01f, startingZ); // Adjust position as needed
        Material lastLineMaterial = new Material(Shader.Find("Standard"));
        lastLineMaterial.color = Color.white;
        lastSideLine.GetComponent<Renderer>().material = lastLineMaterial;     
        
        int i = 1;
        while(i <= amount)
        {
            float middleXCoordinate = (i * horizontalLineLength) + startingX - horizontalLineLength / 2f;
            float middleZCoordinate = startingZ - verticaltLineLength / 2f;
            string label = "A" + 1;
            
            parkingSpace.AddLabel(new Vector2(middleXCoordinate, middleZCoordinate), label);
            
            GameObject sideLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sideLine.transform.SetParent(parkingSpace.transform);
            sideLine.transform.localScale = new Vector3(0.2f, 0.01f, verticaltLineLength); // Adjust scale as needed
            sideLine.transform.localPosition = new Vector3((i*horizontalLineLength) + startingX, 0.01f, startingZ); // Adjust position as needed
            Material lineMaterial = new Material(Shader.Find("Standard"));
            lineMaterial.color = Color.white;
            sideLine.GetComponent<Renderer>().material = lineMaterial;
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(parkingSpace.transform);
            textObj.transform.localPosition = new Vector3(middleXCoordinate, 0.01f, startingZ - 1.5f); // Adjust position as needed
            textObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // Rotate the text to be flat
            TextMesh textMesh = textObj.AddComponent<TextMesh>();
            textMesh.text = label; // Set the text
            textMesh.fontSize = 17; // Adjust font size as needed
            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.MiddleCenter;
            //textMesh.transform.localScale = new Vector3(0.2f, 0.01f, 0.2f); // Adjust scale to match the side lines
            textMesh.color = Color.white;
            
            i++;
        }
        
        GameObject bottomLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bottomLine.transform.SetParent(parkingSpace.transform);
        bottomLine.transform.localScale = new Vector3(amount * 3, 0.01f, 0.2f); // Adjust scale as needed
        // bottomLine.transform.localPosition = new Vector3(startingX + ((horizontalLineLength * (amount-1))/2), 0.01f, startingZ + 2.4f); // Adjust position as needed
        float bottomLineXPosition = startingX + ((horizontalLineLength * amount) / 2f);
        bottomLine.transform.localPosition = new Vector3(bottomLineXPosition, 0.01f, startingZ + 2.4f); // Adjust position as needed

        Material bottomMaterial = new Material(Shader.Find("Standard"));
        bottomMaterial.color = Color.white;
        bottomLine.GetComponent<Renderer>().material = bottomMaterial;
        
    }
        
    
    void GeneratePlatform()
    {
        GameObject terrain = GameObject.CreatePrimitive(PrimitiveType.Plane);
        terrain.transform.position = new Vector3(0, 0, 0); // Adjust position as needed
        terrain.transform.localScale = new Vector3(4, 1, 4); // Adjust scale as needed
        // terrain.
        
        Material asphaltMaterial = Resources.Load<Material>("Asphalt_material_11");
        Vector2 tiling = new Vector2(8, 8); // Adjust tiling as needed
        asphaltMaterial.mainTextureScale = tiling;
        terrain.GetComponent<Renderer>().material = asphaltMaterial;
        
        
        GameObject parkingSpace = new GameObject("ParkingSpace");
        parkingSpace.transform.SetParent(terrain.transform); // Set parent to the plane
        
        float startingX = -15.0f;
        float startingZ = 7.0f;

        generateRow(parkingSpace, 9, startingX, startingZ);

        foreach (KeyValuePair<Vector2, string> entry in parkingSpace.coordinateToLabelMap)
        {
            Debug.Log("Coordinate: " + entry.Key + ", Label: " + entry.Value);
        }

    }
}