using System;
using System.IO;
using UnityEngine;
using BepInEx;
using UnityEngine.SceneManagement;

namespace Banana_ify
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        private string texturePath = @"C:\Banana.jpg";

        void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            GorillaTagger.OnPlayerSpawned(Initialized);
        }

        void Initialized()
        {
            ApplyTextureToAllObjects();
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplyTextureToAllObjects();
        }

        void ApplyTextureToAllObjects(Texture2D texture = null)
        {
            if (texture == null)
            {
                if (!File.Exists(texturePath))
                {
                    Debug.LogError($"Texture file not found at: {texturePath}");
                    return;
                }

                texture = new Texture2D(2, 2);
                texture.LoadImage(File.ReadAllBytes(texturePath));
            }

            GameObject[] allObjects = FindObjectsOfType<GameObject>();

            foreach (GameObject obj in allObjects)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = new Material(renderer.material)
                    {
                        mainTexture = texture
                    };
                }
            }
        }
    }
}
