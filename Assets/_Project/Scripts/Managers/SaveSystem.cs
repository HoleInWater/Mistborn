// NOTE: Lines 23 and 34 contain Debug.Log which should be removed for production
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }
    public string saveFileName = "mistborn_save";
    
    void Awake() {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }
    
    public void SaveGame() {
        SaveData data = new SaveData();
        HealthBarTransitions playerHealth = FindObjectOfType<HealthBarTransitions>();
        if (playerHealth != null) data.playerHealth = playerHealth.health;
        
        Allomancer allomancer = FindObjectOfType<Allomancer>();
        if (allomancer != null) data.metalReserves = allomancer.metalReserves;
        
        PlayerPrefs.SetString(saveFileName, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
        Debug.Log("Game saved!");
    }
    
    public void LoadGame() {
        if (PlayerPrefs.HasKey(saveFileName)) {
            SaveData data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(saveFileName));
            HealthBarTransitions playerHealth = FindObjectOfType<HealthBarTransitions>();
            if (playerHealth != null) playerHealth.health = data.playerHealth;
            
            Allomancer allomancer = FindObjectOfType<Allomancer>();
            if (allomancer != null) allomancer.metalReserves = data.metalReserves;
            Debug.Log("Game loaded!");
        }
    }
}

[System.Serializable]
public class SaveData {
    public float playerHealth;
    public float[] metalReserves = new float[16];
}
