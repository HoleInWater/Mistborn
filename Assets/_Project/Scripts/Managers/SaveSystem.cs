// NOTE: Lines 23 and 34 contain Debug.Log which should be removed for production
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }
    public string saveFileName = "ashwalker_save";
    
    void Awake() {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }
    
    public void SaveGame() {
        SaveData data = new SaveData();
        HealthBarTransitions playerHealth = FindObjectOfType<HealthBarTransitions>();
        if (playerHealth != null) data.playerHealth = playerHealth.health;

        // Use Registry for Metallurgist lookup (more reliable than FindObjectOfType)
        if (AshwalkerRegistry.ActiveMetallurgists.Count > 0)
            data.metalReserves = AshwalkerRegistry.ActiveMetallurgists[0].metalReserves;

        PlayerPrefs.SetString(saveFileName, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }
    
    public void LoadGame() {
        if (!PlayerPrefs.HasKey(saveFileName)) return;

        SaveData data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(saveFileName));
        HealthBarTransitions playerHealth = FindObjectOfType<HealthBarTransitions>();
        if (playerHealth != null) playerHealth.health = data.playerHealth;

        // Use Registry for Metallurgist lookup
        if (AshwalkerRegistry.ActiveMetallurgists.Count > 0 && data.metalReserves != null)
        {
            var metallurgist = AshwalkerRegistry.ActiveMetallurgists[0];
            int count = Mathf.Min(data.metalReserves.Length, metallurgist.metalReserves.Length);
            System.Array.Copy(data.metalReserves, metallurgist.metalReserves, count);
        }
    }
}

[System.Serializable]
public class SaveData {
    public float playerHealth;
    public float[] metalReserves = new float[20]; // Matches Metallurgist.metalReserves[20]
}
