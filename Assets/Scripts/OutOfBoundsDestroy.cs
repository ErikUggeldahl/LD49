using UnityEngine;

public class OutOfBoundsDestroy : MonoBehaviour
{
    void Update()
    {
        if (transform.position.y < -100f)
        {
            Destroy(gameObject);
        }
    }
}
