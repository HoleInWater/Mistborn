using UnityEngine;
using UnityEngine.UIElements;

public class PlayerCollisionHandler3D : MonoBehaviour
{
    public UIDocument uiDocument;
    public string progressBarName = "Health";
    public float damagePerSecondWhileTouching = 10f; // How much health to lose per second while touching the enemy
    // Regen settings
    public float timeNotTouchedEnemy = 0f; // Timer to track how long since last touched enemy
    public float regenDelayAfterTouch = 10f; // seconds to wait before starting regen
    public float regenPerSecond = 5f;
    private bool isTouchingEnemy = false;


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


    void DecreaseHealth(float amount)
    {
        if (_progressBar != null)
        {
            // Subtract the amount and clamp it so it doesn't go below 0
            _progressBar.value = Mathf.Max(_progressBar.value - amount, 0);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Mark that we're touching and decrease health continuously while touching the enemy
            isTouchingEnemy = true;
            timeNotTouchedEnemy = 0f;
            DecreaseHealth(damagePerSecondWhileTouching * Time.deltaTime);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Start counting time since last touch
            isTouchingEnemy = false;
            timeNotTouchedEnemy = 0f;
            Debug.Log("Stopped touching enemy, starting health regeneration timer.");
        }
    }

    void IncreaseHealth(float amount)
    {
        if (_progressBar != null)
        {
            // Add the amount and clamp it so it doesn't go above 100
            _progressBar.value = Mathf.Min(_progressBar.value + amount, 100);
        }

    }

    private void Update()
    {
        // If currently touching, keep timer at zero
        if (isTouchingEnemy)
        {
            // ensure timer stays reset while touching
            timeNotTouchedEnemy = 0f;
            return;
        }

        // Not touching: count up
        timeNotTouchedEnemy += Time.deltaTime;

        // Start regenerating after delay
        if (timeNotTouchedEnemy >= regenDelayAfterTouch)
        {
            IncreaseHealth(regenPerSecond * Time.deltaTime);
        }
    }
}
