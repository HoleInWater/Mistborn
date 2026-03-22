using UnityEngine;
using UnityEngine.SceneManagement;
using System;

[Serializable]
public class SaveData
{
    public float playerHealth;       // Corresponds to Health.currentHealth
    public float playerMaxHealth;    // Corresponds to Health.maxHealth
    public float[] metalReserves;    // Corresponds to MetalReserveManager.reserves
    public Vector3 playerPosition;  // Player transform position
    public Quaternion playerRotation; // Player transform rotation
    public string sceneName;         // Current scene name
    public int currentCheckpoint;    // Last checkpoint ID
}

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }
    
    private const string SAVE_KEY = "MistbornSaveData";
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void SaveGame()
    {
        SaveData data = new SaveData();
        
        Health health = FindObjectOfType<Health>();
        if (health != null)
        {
            data.playerHealth = health.currentHealth;
            data.playerMaxHealth = health.maxHealth;
        }
        
        MetalReserveManager metals = FindObjectOfType<MetalReserveManager>();
        if (metals != null)
        {
            data.metalReserves = metals.reserves;
        }
        
        if (UnityEngine.Camera.main != null)
        {
            data.playerPosition = UnityEngine.Camera.main.transform.position;
            data.playerRotation = UnityEngine.Camera.main.transform.rotation;
        }
        
        data.sceneName = SceneManager.GetActiveScene().name;
        
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
        
        Debug.Log("Game saved!");
    }
    
    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY))
        {
            Debug.Log("No save data found!");
            return;
        }
        
        string json = PlayerPrefs.GetString(SAVE_KEY);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        
        Health health = FindObjectOfType<Health>();
        if (health != null)
        {
            health.SetMaxHealth(data.playerMaxHealth);
            health.SetHealth(data.playerHealth);
        }
        
        MetalReserveManager metals = FindObjectOfType<MetalReserveManager>();
        if (metals != null && data.metalReserves != null)
        {
            for (int i = 0; i < Mathf.Min(metals.reserves.Length, data.metalReserves.Length); i++)
            {
                metals.reserves[i] = data.metalReserves[i];
            }
        }
        
        Debug.Log("Game loaded!");
    }
    
    public bool HasSaveData()
    {
        return PlayerPrefs.HasKey(SAVE_KEY);
    }
    
    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        Debug.Log("Save data deleted!");
    }
}
