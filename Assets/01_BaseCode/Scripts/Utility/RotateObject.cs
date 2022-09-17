using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [SerializeField] private float speed = 1;

    private void Update()
    {
        if (Time.timeScale == 0) return;
        transform.Rotate(new Vector3(0, 0, 1) * speed);
    }
}