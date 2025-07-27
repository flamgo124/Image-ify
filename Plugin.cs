using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Collections;
using UnityEngine;
using BepInEx;

namespace Image_ify
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        private string imageFolder;
        private string selectedTexturePath;
        private Texture2D currentTexture;
        private GameObject closestObject;
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

            LoadTexture();
            StartCoroutine(UpdateTextureClosestToPlayer());
        }
        void LoadTexture()
        {
            if (string.IsNullOrEmpty(selectedTexturePath) || !File.Exists(selectedTexturePath))
            {
                Debug.LogError($"Texture file not found at: {selectedTexturePath}");
                return;
            }
            currentTexture = new Texture2D(2, 2);
            currentTexture.LoadImage(File.ReadAllBytes(selectedTexturePath));
        }
        IEnumerator UpdateTextureClosestToPlayer()
        {
            while (true)
            {
                if (currentTexture == null)
                {
                    yield return new WaitForSeconds(0.05f);
                    continue;
                }

                GameObject player = GorillaTagger.Instance?.gameObject;
                if (player == null)
                {
                    yield return new WaitForSeconds(0.05f);
                    continue;
                }

                var allObjects = FindObjectsOfType<GameObject>();
                float closestDist = float.MaxValue;
                GameObject closest = null;
                Vector3 playerPos = player.transform.position;

                foreach (var obj in allObjects)
                {
                    Renderer rend = obj.GetComponent<Renderer>();
                    if (rend == null) continue;

                    float dist = Vector3.Distance(playerPos, obj.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = obj;
                    }
                }

                if (closest != null && closest != closestObject)
                {
                    closestObject = closest;
                    Renderer rend = closestObject.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        rend.material = new Material(rend.material) { mainTexture = currentTexture };
                        Debug.Log($"Applied texture to closest object: {closestObject.name}");
                    }
                }

                yield return new WaitForSeconds(0.05f);
            }
        }
    }
}
