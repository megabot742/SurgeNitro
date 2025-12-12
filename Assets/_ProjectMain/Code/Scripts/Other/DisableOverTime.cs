using UnityEngine;

public class DisableOverTime : MonoBehaviour
{
    [SerializeField] float timeToDisable;
    [SerializeField] float counterTime;
    void OnEnable()
    {
        counterTime = timeToDisable;
    }
    void Update()
    {
        counterTime -= Time.deltaTime;
        if (counterTime <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}
