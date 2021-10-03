using UnityEngine;

public class HelpTextDestroyer : MonoBehaviour
{
    [SerializeField]
    KeyCode keyToDismiss;

    void Update()
    {
        if (Input.GetKeyDown(keyToDismiss))
        {
            Destroy(gameObject);
        }
    }
}
