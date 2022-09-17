using UnityEngine;

public class DisableMe : MonoBehaviour
{
    [SerializeField] private bool isPool;

    [SerializeField] private float time;

    // Start is called before the first frame update
    private void OnEnable()
    {
        StartCoroutine(Helper.StartAction(() => { DisableHandle(); }, time));
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private void DisableHandle()
    {
        if (isPool)
            SimplePool.Despawn(gameObject);
        else
            gameObject.SetActive(false);
    }
}