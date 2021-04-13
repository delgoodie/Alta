using UnityEngine;

public class Kelp : MonoBehaviour, IEntity
{
    private void Awake()
    {
    }

    void Update()
    {
    }

    void OnDrawGizmosSelected()
    {
    }
    public void Activate()
    {
        gameObject.SetActive(true);
    }
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
