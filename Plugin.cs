using System;
using System.IO;
using UnityEngine;
using BepInEx;
using UnityEngine.SceneManagement;  // Add this for scene management

namespace Banana_ify
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        private string texturePath = @"C:\Banana.jpg";

        void Awake()
        {
            // Subscribe to the scene loaded event
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Also listen for player spawn event to initialize texture application
            GorillaTagger.OnPlayerSpawned(Initialized);
        }

        void Initialized()
        {
            // Apply the texture after the player is initialized
            ApplyTextureToAllObjects();
        }

        // This method is called when a scene is loaded
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Call the texture application after every scene load
            ApplyTextureToAllObjects();
        }

        void ApplyTextureToAllObjects(Texture2D texture = null)
        {
            if (texture == null)
            {
                // Load the texture if it's not provided
                if (!File.Exists(texturePath))
                {
                    Debug.LogError($"Texture file not found at: {texturePath}");
                    return;
                }

                texture = new Texture2D(2, 2);
                texture.LoadImage(File.ReadAllBytes(texturePath));
            }

            // Apply texture to all objects in the current scene
            GameObject[] allObjects = FindObjectsOfType<GameObject>();

            foreach (GameObject obj in allObjects)
            {
                // Apply texture to the object if it has a renderer
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    // Create a new material to avoid changing shared materials
                    renderer.material = new Material(renderer.material)
                    {
                        mainTexture = texture
                    };
                    Debug.Log($"Texture applied to {obj.name}");
                }
            }
        }
    }
}
