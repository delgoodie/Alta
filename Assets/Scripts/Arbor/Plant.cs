using UnityEngine;

[RequireComponent(typeof(Marcher))]
public class Plant : MonoBehaviour, IMarch
{
    public Marcher marcher;
    private Vector3 digPoint;

    private void Awake()
    {
        marcher = GetComponent<Marcher>();
    }

    void Update()
    {
        if (false)
        {
            // if (Input.GetMouseButton(0))
            // {
            //     Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //     RaycastHit hitInfo;
            //     if (Physics.Raycast(ray, out hitInfo, 50f))
            //     {
            //         //Subtract(hitInfo.point, 1f, 1f);
            //         digPoint = hitInfo.point;
            //     }
            // }
        }
    }

    public void Markup() { }
    public void Chipnit()
    {
        marcher.chips = new Chip[16 * 16 * 16];
        for (int z = 0; z < 16; z++)
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                    marcher.chips[z * 16 * 16 + y * 16 + x] = new Chip(0, 2, 0);
    }

    public void MarchUpdate() { }

    void OnDrawGizmosSelected()
    {
    }
}
