using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }
    
    [Header("Save Data")]
    public string saveFileName = "mistborn_save";
    
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
        
        // Swapped "Health" for "HealthBarTransitions"
        HealthBarTransitions playerHealth = FindObjectOfType<HealthBarTransitions>();
        if (playerHealth != null)
        {
            // Assuming your HealthBarTransitions uses a variable like 'health' or 'currentHealth'
            // If the variable name is different in that file, change '.health' below to match it
            data.playerHealth = playerHealth.health; 
        }
        
        Allomancer allomancer = FindObjectOfType<Allomancer>();
        if (allomancer != null)
        {
            data.metalReserves = allomancer.metalReserves;
        }
        
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(saveFileName, json);
        PlayerPrefs.Save();
        
        Debug.Log("Game saved!");
    }
    
    public void LoadGame()
    {
        if (PlayerPrefs.HasKey(saveFileName))
        {
            string json = PlayerPrefs.GetString(saveFileName);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            
            HealthBarTransitions playerHealth = FindObjectOfType<HealthBarTransitions>();
            if (playerHealth != null)
            {
                // We set the health value
                playerHealth.health = data.playerHealth;
                
                // IMPORTANT: If HealthBarTransitions has a function like 'UpdateUI()' or 'SetHealth()', 
                // you should call it here so the red bar actually moves when you load.
            }
            
            Allomancer allomancer = FindObjectOfType<Allomancer>();
            if (allomancer != null)
            {
                allomancer.metalReserves = data.metalReserves;
            }
            
            Debug.Log("Game loaded!");
        }
        else
        {
            Debug.Log("No save file found!");
        }
    }
    
    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey(saveFileName);
        Debug.Log("Save deleted!");
    }
}

[System.Serializable]
public class SaveData
{
    public float playerHealth;
    public float[] metalReserves = new float[16];
}
