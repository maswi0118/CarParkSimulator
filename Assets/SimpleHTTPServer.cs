using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

public class SimpleHTTPServer : MonoBehaviour
{
    public static SimpleHTTPServer Instance;

    private HttpListener listener;

    public delegate string OnGetRequestHandler(string path);
    public event OnGetRequestHandler OnGet;
    
    public delegate void OnPostRequestHandler(string path, string data);
    public event OnPostRequestHandler OnPost;

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
        
        BeginHandleRequests();
    }

    private async void BeginHandleRequests()
    {
        while (listener.IsListening)
        {
            try
            {
                HttpListenerContext context = await listener.GetContextAsync();
                
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
        string data = "";

        if (context.Request.HttpMethod == "GET" && OnGet != null) 
        {
            string responseContent = OnGet(path);
            byte[] responseBytes = Encoding.UTF8.GetBytes(responseContent);
            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.ContentLength64 = responseBytes.Length;
            context.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
        } 
        else if (context.Request.HttpMethod == "POST" && OnPost != null) 
        {
            using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding)) 
            {
                data = reader.ReadToEnd();
            }
            OnPost(path, data);
            byte[] responseBytes = Encoding.UTF8.GetBytes("OK");
            context.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
        }


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