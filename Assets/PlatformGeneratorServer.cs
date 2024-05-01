using UnityEngine;

public class PlatformGeneratorServer : MonoBehaviour
{
    private const int serverPort = 3000; // Choose a port number

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

    void GeneratePlatform()
    {
        // Instantiate or manipulate your platform GameObject here
        InstantiatePlatform();
    }

    void InstantiatePlatform()
    {
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.transform.position = new Vector3(0, 1, 0); // Adjust position as needed
    }
}