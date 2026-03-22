using UnityEngine;
using UnityEngine.UIElements;

public class HealthBarTransition : MonoBehaviour
{
    public UIDocument uiDocument;
    public string progressBarName = "Health";
    public float damagePerSecondWhileTouching = 10f;
    public float timeNotTouchedEnemy = 0f;
    public float regenDelayAfterTouch = 10f;
    public float regenPerSecond = 5f;
    
    private bool isTouchingEnemy = false;
    private ProgressBar _progressBar;

    void OnEnable()
    {
        var root = uiDocument.rootVisualElement;
        _progressBar = root.Q<ProgressBar>(progressBarName);

        if (_progressBar != null)
        {
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
            _progressBar.value = Mathf.Max(_progressBar.value - amount, 0);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            isTouchingEnemy = true;
            timeNotTouchedEnemy = 0f;
            DecreaseHealth(damagePerSecondWhileTouching * Time.deltaTime);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            isTouchingEnemy = false;
            timeNotTouchedEnemy = 0f;
            Debug.Log("Stopped touching enemy, starting health regeneration timer.");
        }
    }

    void IncreaseHealth(float amount)
    {
        if (_progressBar != null)
        {
            _progressBar.value = Mathf.Min(_progressBar.value + amount, 100);
        }
    }

    private void Update()
    {
        if (isTouchingEnemy)
        {
            timeNotTouchedEnemy = 0f;
            return;
        }

        timeNotTouchedEnemy += Time.deltaTime;

        if (timeNotTouchedEnemy >= regenDelayAfterTouch)
        {
            IncreaseHealth(regenPerSecond * Time.deltaTime);
        }
    }
}
