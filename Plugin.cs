using System;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;
using BepInEx;

namespace Image_ify
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        private string imageFolder;
        private string selectedTexturePath;
        void Awake()
        {
            imageFolder = Path.Combine(Paths.PluginPath, "Image-ify");
            if (!Directory.Exists(imageFolder))
                Directory.CreateDirectory(imageFolder);
            var validExtensions = new[] { ".png", ".jpg", ".jpeg" };
            var existingImages = Directory.GetFiles(imageFolder).Where(f => validExtensions.Contains(Path.GetExtension(f).ToLower())).ToArray();
            if (existingImages.Length == 0)
            {
                string defaultImageUrl = "https://raw.githubusercontent.com/flamgo124/Image-ify/main/mango.png";
                string defaultImagePath = Path.Combine(imageFolder, "mango.png");
                try
                {
                    using (WebClient client = new WebClient())
                        client.DownloadFile(defaultImageUrl, defaultImagePath);
                    Debug.Log($"Downloaded default image to: {defaultImagePath}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to download default image: {ex.Message}");
                }
            }
            PickRandomImage();
            GorillaTagger.OnPlayerSpawned(Initialized);
        }
        void PickRandomImage()
        {
            var validExtensions = new[] { ".png", ".jpg", ".jpeg" };
            var files = Directory.GetFiles(imageFolder).Where(f => validExtensions.Contains(Path.GetExtension(f).ToLower())).ToArray();
            if (files.Length == 0)
            {
                Debug.LogError("No image files found in 'image-ify all' folder.");
                selectedTexturePath = null;
                return;
            }
            System.Random rng = new System.Random();
            selectedTexturePath = files[rng.Next(files.Length)];
            Debug.Log($"Selected texture: {selectedTexturePath}");
        }
        void Initialized()
        {
            if (string.IsNullOrEmpty(selectedTexturePath))
                return;
            ApplyTextureToAllObjects();
            InvokeRepeating("UpdateTextures", 0f, 2f);
        }
        void UpdateTextures()
        {
            if (string.IsNullOrEmpty(selectedTexturePath) || !File.Exists(selectedTexturePath))
            {
                Debug.LogError($"Texture file not found at: {selectedTexturePath}");
                return;
            }
            Texture2D newTexture = new Texture2D(2, 2);
            newTexture.LoadImage(File.ReadAllBytes(selectedTexturePath));
            ApplyTextureToAllObjects(newTexture);
        }
        void ApplyTextureToAllObjects(Texture2D texture = null)
        {
            if (texture == null)
            {
                if (string.IsNullOrEmpty(selectedTexturePath) || !File.Exists(selectedTexturePath))
                {
                    Debug.LogError($"Texture file not found at: {selectedTexturePath}");
                    return;
                }
                texture = new Texture2D(2, 2);
                texture.LoadImage(File.ReadAllBytes(selectedTexturePath));
            }
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = new Material(renderer.material) { mainTexture = texture };
                    Debug.Log($"Texture applied to {obj.name}");
                }
            }
        }
    }
}
