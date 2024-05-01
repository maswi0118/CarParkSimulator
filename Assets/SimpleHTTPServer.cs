using System;
using System.Net;
using System.Text;
using UnityEngine;

public class SimpleHTTPServer : MonoBehaviour
{
    public static SimpleHTTPServer Instance;

    private HttpListener listener;

    public delegate void OnGetRequestHandler(string path);
    public event OnGetRequestHandler OnGet;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Setup(int port)
    {
        listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:" + port + "/");
        listener.Start();

        // Start handling requests in the background
        BeginHandleRequests();
    }

    private async void BeginHandleRequests()
    {
        while (listener.IsListening)
        {
            try
            {
                // Wait for a context asynchronously
                HttpListenerContext context = await listener.GetContextAsync();

                // Handle the context in the main thread
                HandleRequest(context);
            }
            catch (Exception e)
            {
                Debug.LogError("Error handling request: " + e.Message);
            }
        }
    }

    private void HandleRequest(HttpListenerContext context)
    {
        string path = context.Request.Url.LocalPath;

        if (context.Request.HttpMethod == "GET" && OnGet != null)
        {
            OnGet(path);
        }

        byte[] responseBytes = Encoding.UTF8.GetBytes("OK");
        context.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
        context.Response.Close();
    }

    void OnDestroy()
    {
        if (listener != null && listener.IsListening)
        {
            listener.Stop();
        }
    }
}