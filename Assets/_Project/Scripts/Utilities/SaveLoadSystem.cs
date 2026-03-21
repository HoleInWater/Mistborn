using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Mistborn.Utilities
{
    public static class SaveLoadSystem
    {
        private static readonly string SAVE_FOLDER = Application.persistentDataPath + "/Saves/";
        private static readonly string EXTENSION = ".sav";

        public static void Save(string saveName, object data)
        {
            if (!Directory.Exists(SAVE_FOLDER))
            {
                Directory.CreateDirectory(SAVE_FOLDER);
            }

            string path = GetPath(saveName);
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = File.Create(path);

            try
            {
                formatter.Serialize(stream, data);
            }
            catch (Exception e)
            {
                Debug.LogError($"Save failed: {e.Message}");
            }
            finally
            {
                stream.Close();
            }
        }

        public static object Load(string saveName)
        {
            string path = GetPath(saveName);

            if (!File.Exists(path))
            {
                Debug.LogWarning($"Save file not found: {path}");
                return null;
            }

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = File.Open(path, FileMode.Open);

            try
            {
                object data = formatter.Deserialize(stream);
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"Load failed: {e.Message}");
                return null;
            }
            finally
            {
                stream.Close();
            }
        }

        public static void SaveObject<T>(string saveName, T obj) where T : class
        {
            Save(saveName, obj);
        }

        public static T LoadObject<T>(string saveName) where T : class
        {
            object data = Load(saveName);
            return data as T;
        }

        public static bool SaveExists(string saveName)
        {
            return File.Exists(GetPath(saveName));
        }

        public static void DeleteSave(string saveName)
        {
            string path = GetPath(saveName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public static string[] GetAllSaveNames()
        {
            if (!Directory.Exists(SAVE_FOLDER))
            {
                return new string[0];
            }

            string[] files = Directory.GetFiles(SAVE_FOLDER, "*" + EXTENSION);
            string[] names = new string[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                names[i] = Path.GetFileNameWithoutExtension(files[i]);
            }

            return names;
        }

        public static void DeleteAllSaves()
        {
            if (!Directory.Exists(SAVE_FOLDER)) return;

            string[] files = Directory.GetFiles(SAVE_FOLDER, "*" + EXTENSION);
            foreach (string file in files)
            {
                File.Delete(file);
            }
        }

        private static string GetPath(string saveName)
        {
            return SAVE_FOLDER + saveName + EXTENSION;
        }
    }

    public class Saveable : MonoBehaviour
    {
        [SerializeField] private string m_saveId;

        public string SaveId => m_saveId;

        public virtual object GetSaveData()
        {
            return null;
        }

        public virtual void LoadSaveData(object data)
        {
        }

        public void Save()
        {
            SaveLoadSystem.Save(m_saveId, GetSaveData());
        }

        public void Load()
        {
            if (SaveLoadSystem.SaveExists(m_saveId))
            {
                object data = SaveLoadSystem.Load(m_saveId);
                LoadSaveData(data);
            }
        }
    }

    public class GameSaveData
    {
        public string saveName;
        public DateTime saveTime;
        public string sceneName;
        public Vector3 playerPosition;
        public Quaternion playerRotation;
        public float playTime;
        public int playerHealth;
        public int gold;
        public SerializableDictionary<string, object> flags = new SerializableDictionary<string, object>();
        public string[] unlockedAchievements;
    }

    public class SerializableDictionary<TKey, TValue> : System.Collections.Generic.Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private TKey[] m_keys;
        [SerializeField] private TValue[] m_values;

        public void OnBeforeSerialize()
        {
            m_keys = new TKey[Count];
            m_values = new TValue[Count];

            int i = 0;
            foreach (var kvp in this)
            {
                m_keys[i] = kvp.Key;
                m_values[i] = kvp.Value;
                i++;
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();
            if (m_keys == null || m_values == null) return;

            for (int i = 0; i < Mathf.Min(m_keys.Length, m_values.Length); i++)
            {
                Add(m_keys[i], m_values[i]);
            }
        }
    }
}
