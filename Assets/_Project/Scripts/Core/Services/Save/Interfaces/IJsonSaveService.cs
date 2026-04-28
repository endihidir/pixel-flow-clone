namespace Core.SaveSystem
{
    public interface IJsonSaveService
    {
        public void SaveToTextFile<T>(string key, T data, string path = null);
        public T LoadFromTextFile<T>(string key, T defaultData = default, string path = null, bool autoSaveDefaultData = true);
        public void SaveToPrefs<T>(string key, T data);
        public T LoadFromPrefs<T>(string key, T defaultData = default);
    }
}