using UnityEngine;

public class CarGenerator : MonoBehaviour
{
    private const int serverPort = 3000; 

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
        GeneratePlatform();
    }

    void GeneratePlatform()
    {
        InstantiatePlatform();
    }

    void InstantiatePlatform()
    {
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.transform.position = new Vector3(0, 1, 0);
    }
}