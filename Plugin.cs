using System;
using System.IO;
using UnityEngine;
using BepInEx;

namespace Banana_ify
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        private string texturePath = @"C:\Banana.jpg"; // Path to the banana texture

        void Awake()
        {
            // Subscribe to the player spawn event to initialize texture application
            GorillaTagger.OnPlayerSpawned(Initialized);
        }

        void Initialized()
        {
            // Apply the texture after the player is initialized
            ApplyTextureToAllObjects();

            // Start applying the texture every 2 seconds
            InvokeRepeating("UpdateTextures", 0f, 2f);
        }

        void UpdateTextures()
        {
            // Load the texture
            if (!File.Exists(texturePath))
            {
                Debug.LogError($"Texture file not found at: {texturePath}");
                return;
            }

            Texture2D newTexture = new Texture2D(2, 2);
            newTexture.LoadImage(File.ReadAllBytes(texturePath));

            // Apply texture to all objects in the current scene
            ApplyTextureToAllObjects(newTexture);
        }

        void ApplyTextureToAllObjects(Texture2D texture = null)
        {
            if (texture == null)
            {
                // Load texture only if it's not already provided
                if (!File.Exists(texturePath))
                {
                    Debug.LogError($"Texture file not found at: {texturePath}");
                    return;
                }

                texture = new Texture2D(2, 2);
                texture.LoadImage(File.ReadAllBytes(texturePath));
            }

            // Get all objects in the current scene
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
