using Game.Level.Data;
using UnityEngine;

namespace Game.Level.EditorTools
{
    public static class LevelJsonConverter
    {
        public static string ToJson(LevelJson levelJson, bool prettyPrint = true) => JsonUtility.ToJson(levelJson, prettyPrint);
    }
}