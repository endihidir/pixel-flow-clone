using System.IO;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace Core.SaveSystem
{
    public sealed class JsonSaveService : IJsonSaveService
    {
        private const string DIRECTORY_NAME = "JsonGameData";

#if UNITY_EDITOR
        private static string DirectoryPath => $"{Application.dataPath}/{DIRECTORY_NAME}[EditorOnly]";
#else
        private static string DirectoryPath => $"{Application.persistentDataPath}/{DIRECTORY_NAME}";
#endif

        public void SaveToTextFile<T>(string key, T data, string path = null)
        {
            EnsureDirectoryExists();

            var filePath = string.IsNullOrEmpty(path) ? GetFilePath(key) : path;

            var jsonData = JsonUtility.ToJson(data);
            
            File.WriteAllText(filePath, jsonData);

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        public T LoadFromTextFile<T>(string key, T defaultData = default, string path = null, bool autoSaveDefaultData = true)
        {
            EnsureDirectoryExists();

            var filePath = string.IsNullOrEmpty(path) ? GetFilePath(key) : path;

            if (!File.Exists(filePath))
            {
                if (defaultData is not null && autoSaveDefaultData)
                {
                    SaveToTextFile(key, defaultData);
                }
                
                return defaultData;
            }

            var jsonData = File.ReadAllText(filePath);
                
            var data = JsonUtility.FromJson<T>(jsonData);
            
            return data;
        }

        public void SaveToPrefs<T>(string key, T data)
        {
            var jsonData= JsonUtility.ToJson(data);
            
            PlayerPrefs.SetString(key, jsonData);
        }

        public T LoadFromPrefs<T>(string key, T defaultData = default)
        {
            if (!PlayerPrefs.HasKey(key))
            {
                SaveToPrefs(key, defaultData);
            }
            
            var jsonData= PlayerPrefs.GetString(key, string.Empty);
                
            var data = JsonUtility.FromJson<T>(jsonData);
            
            return data;
        }
        
        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(DirectoryPath)) 
                Directory.CreateDirectory(DirectoryPath);
        }

        public static void ClearJsonData()
        {
            if (!Directory.Exists(DirectoryPath)) return;
            
            var files = Directory.GetFiles(DirectoryPath).Select(Path.GetFileName).ToArray();

            foreach (string key in files)
            {
                var filePath = $"{DirectoryPath}/{key}";
                
                File.Delete(filePath);
            }
            
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }
        
#if UNITY_EDITOR
        [MenuItem("Edit/Clear All Json Data")]
        public static void ClearJson() => ClearJsonData();
        
        [MenuItem("Edit/Clear All")]
        public static void ClearAll()
        {
            ClearJsonData();
            PlayerPrefs.DeleteAll();
        }
#endif

        private string GetFilePath(string key) => Path.Combine(DirectoryPath, $"{key}.json");
    }
}