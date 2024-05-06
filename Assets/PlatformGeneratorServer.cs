using rest_dtos;
using UnityEngine;

public class PlatformGeneratorServer : MonoBehaviour
{
    public GameObject carPrefab;
    
    private const int serverPort = 3000;

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

    void OnGetRequest(string path)
    {
        if ("/generate-board".Equals(path))
        {
            GeneratePlatform();
        }
    }
    
    void OnPostRequest(string path, string data)
    {
        if ("/new-car".Equals(path))
        {
            GenerateCar(data);
        }
    }

    void GeneratePlatform()
    {
        InstantiatePlatform();
    }

    void GenerateCar(string data)
    {
        InstantiateCar(data);
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
        car.transform.position = new Vector3(carData.x, carData.y, carData.z);
    }
}