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

        // Use Registry for Allomancer lookup (more reliable than FindObjectOfType)
        if (MistbornRegistry.ActiveAllomancers.Count > 0)
            data.metalReserves = MistbornRegistry.ActiveAllomancers[0].metalReserves;

        PlayerPrefs.SetString(saveFileName, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }
    
    public void LoadGame() {
        if (!PlayerPrefs.HasKey(saveFileName)) return;

        SaveData data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(saveFileName));
        HealthBarTransitions playerHealth = FindObjectOfType<HealthBarTransitions>();
        if (playerHealth != null) playerHealth.health = data.playerHealth;

        // Use Registry for Allomancer lookup
        if (MistbornRegistry.ActiveAllomancers.Count > 0 && data.metalReserves != null)
        {
            var allomancer = MistbornRegistry.ActiveAllomancers[0];
            int count = Mathf.Min(data.metalReserves.Length, allomancer.metalReserves.Length);
            System.Array.Copy(data.metalReserves, allomancer.metalReserves, count);
        }
    }
}

[System.Serializable]
public class SaveData {
    public float playerHealth;
    public float[] metalReserves = new float[20]; // Matches Allomancer.metalReserves[20]
}
