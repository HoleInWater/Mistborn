using UnityEngine;
using UnityEngine.UIElements;

public class PlayerCollisionHandler3D : MonoBehaviour
{
    public UIDocument uiDocument;
    public string progressBarName = "Health";
    public float damagePerSecondWhileTouching = 10f;


    private ProgressBar _progressBar;

    void OnEnable()
    {
        var root = uiDocument.rootVisualElement;
        _progressBar = root.Q<ProgressBar>(progressBarName);

        if (_progressBar != null)
        {
            // Set the bar to be 100% full at the start
            _progressBar.lowValue = 0;
            _progressBar.highValue = 100;
            _progressBar.value = 100; 
            Debug.Log("Health Bar initialized to 100%");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            DecreaseHealth(10); // Subtracts 10 points (10% of 100)
        }
    }

    void DecreaseHealth(float amount)
    {
        if (_progressBar != null)
        {
            // Subtract the amount and clamp it so it doesn't go below 0
            _progressBar.value = Mathf.Max(_progressBar.value - amount, 0);
            Debug.Log($"Hit! Health is now: {_progressBar.value}%");
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Decrease health continuously while touching the enemy
            DecreaseHealth(damagePerSecondWhileTouching * Time.deltaTime);
        }
    }
}
