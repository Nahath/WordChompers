using System;
using System.Threading.Tasks;
using MCPForUnity.Editor.Services;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class McpAutoStart
{
    static McpAutoStart()
    {
        if (Application.isBatchMode) return;
        EditorApplication.delayCall += StartMcpBridge;
    }

    private static async void StartMcpBridge()
    {
        try
        {
            if (MCPServiceLocator.Bridge.IsRunning) return;

            if (!MCPServiceLocator.Server.IsLocalHttpServerReachable())
            {
                bool started = MCPServiceLocator.Server.StartLocalHttpServer(quiet: true);
                if (!started) return;

                for (int i = 0; i < 20; i++)
                {
                    await Task.Delay(500);
                    if (MCPServiceLocator.Server.IsLocalHttpServerReachable()) break;
                }
            }

            await MCPServiceLocator.Bridge.StartAsync();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[McpAutoStart] Failed to start MCP bridge: {ex.Message}");
        }
    }
}
