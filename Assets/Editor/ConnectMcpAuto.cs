using UnityEditor;
using UnityEngine;
using MCPForUnity.Editor.Services;
using MCPForUnity.Editor.Services.Transport;

namespace SatwaLangka.EditorScripts
{
    [InitializeOnLoad]
    public static class ConnectMcpAuto
    {
        static ConnectMcpAuto()
        {
            EditorApplication.delayCall += async () =>
            {
                try
                {
                    Debug.Log("[MCP AutoConnect] Connecting Unity to MCP Hub on port 8080...");
                    var tm = MCPServiceLocator.TransportManager;
                    if (tm != null)
                    {
                        await tm.StartAsync(TransportMode.Http);
                        Debug.Log("[MCP AutoConnect] Unity successfully connected to MCP HTTP Hub!");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("[MCP AutoConnect] Note: " + ex.Message);
                }
            };
        }
    }
}
