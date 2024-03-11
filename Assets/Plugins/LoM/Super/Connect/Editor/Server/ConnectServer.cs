using System.Net;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LoM.Super.Connect.Editor
{
    /// <summary>
    /// Internal class which manages the ConnectServer.<br/>
    /// Use ConnectRouter to register routes for the ConnectServer.
    /// </summary>
    [InitializeOnLoad]
    internal class ConnectServer
    {
        // Constants
        private static string SERVER_URL = "http://localhost:{0}/";
        
        // Member Variables
        private static HttpListener listener;
        private static Thread listenerThread;

        // Static Constructor
        static ConnectServer()
        {
            StartServer();
        }

        // Start the server
        private static void StartServer()
        {
            listener = new HttpListener(); 
            listener.Prefixes.Add(string.Format(SERVER_URL, EditorPrefs.GetString("SuperBehaviour.ConnectServer.Port", "20635")));
            listener.Start();

            listenerThread = new Thread(StartListener);
            listenerThread.Start();
        }

        // Start the listener
        private static void StartListener() 
        {
            try 
            {
                while (listener.IsListening)
                {
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;

                    // Check for terminate command
                    if (request.Url.AbsolutePath == "/terminate") 
                    {
                        ShutDownServer();
                        return;
                    }
                    
                    // Return Alive message
                    if (request.Url.AbsolutePath == "/")
                    {
                        string responseString = "{\"status\": \"alive\"}";
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                        response.ContentLength64 = buffer.Length;
                        System.IO.Stream output = response.OutputStream;
                        output.Write(buffer, 0, buffer.Length);
                        output.Close();
                        continue;
                    }
                    
                    // Check if Route exists
                    string route = request.Url.AbsolutePath;
                    if (ConnectRouter.Exists(route))
                    {
                        ThreadedEditorUtility.ExecuteInMainThread(() => {
                            ConnectResponse connectResponse = ConnectRouter.Route(route, request);
                            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(connectResponse.ReturnJSON);
                            response.StatusCode = connectResponse.StatusCode;
                            response.ContentLength64 = buffer.Length;
                            System.IO.Stream output = response.OutputStream;
                            output.Write(buffer, 0, buffer.Length);
                            output.Close();
                        });
                    }
                    else {
                        string responseString = "{\"status\": \"not found\"}";
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        response.ContentLength64 = buffer.Length;
                        System.IO.Stream output = response.OutputStream;
                        output.Write(buffer, 0, buffer.Length);
                        output.Close();
                    }
                }
            }
            catch (System.Net.HttpListenerException)
            {
                // Ignore this exception, it happens when the server is shut down
            }
            catch (System.Threading.ThreadAbortException) 
            {
                // Ignore this exception, it happens when the server is shut down
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        // Shut down the server
        private static void ShutDownServer()
        {
            if (listener != null)
            {
                listener.Stop();
                listener.Close();
            }

            if (listenerThread != null)
            {
                listenerThread.Join();
                listenerThread = null;
            }
        }
    }
}