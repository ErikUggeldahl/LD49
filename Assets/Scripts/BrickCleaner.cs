using UnityEngine;

public class BrickCleaner : MonoBehaviour
{
    private void OnMouseEnter()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.F))
        {
            Destroy(gameObject);
        }
    }
}
