using UnityEngine;

public class PlantTester : MonoBehaviour
{
    public GameObject PlantPrefab;
    private void Start()
    {
        GameObject plant = Instantiate(PlantPrefab, transform);
    }

    // Update is called once per frame
    private void Update()
    {

    }
}
