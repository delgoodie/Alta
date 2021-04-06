using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MC_UVs : MonoBehaviour, IMarch
{
    public bool c000;
    public bool c001;
    public bool c010;
    public bool c011;
    public bool c100;
    public bool c101;
    public bool c110;
    public bool c111;
    private Marcher marcher;
    private void Awake()
    {
        marcher = GetComponent<Marcher>();
    }

    private void Start()
    {
        Chipnit();
        marcher.March();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + Vector3.one * .5f, Vector3.one);
        Gizmos.color = Color.black;
        if (c000) Gizmos.DrawSphere(new Vector3(0f, 0f, 0f) + transform.position, .1f);
        if (c100) Gizmos.DrawSphere(new Vector3(1f, 0f, 0f) + transform.position, .1f);
        if (c010) Gizmos.DrawSphere(new Vector3(0f, 1f, 0f) + transform.position, .1f);
        if (c001) Gizmos.DrawSphere(new Vector3(0f, 0f, 1f) + transform.position, .1f);
        if (c110) Gizmos.DrawSphere(new Vector3(1f, 1f, 0f) + transform.position, .1f);
        if (c101) Gizmos.DrawSphere(new Vector3(1f, 0f, 1f) + transform.position, .1f);
        if (c011) Gizmos.DrawSphere(new Vector3(0f, 1f, 1f) + transform.position, .1f);
        if (c111) Gizmos.DrawSphere(new Vector3(1f, 1f, 1f) + transform.position, .1f);
    }

    /*
    how long is recovery and do i need urinary cathader after surgery, if so for how long
    */

    public void Chipnit()
    {
        marcher.chips = new Chip[8] {
            new Chip((byte)(c000 ? 255 : 0), 0, 0),
            new Chip((byte)(c001 ? 255 : 0), 0, 0),
            new Chip((byte)(c010 ? 255 : 0), 0, 0),
            new Chip((byte)(c011 ? 255 : 0), 0, 0),
            new Chip((byte)(c100 ? 255 : 0), 0, 0),
            new Chip((byte)(c101 ? 255 : 0), 0, 0),
            new Chip((byte)(c110 ? 255 : 0), 0, 0),
            new Chip((byte)(c111 ? 255 : 0), 0, 0),
        };
    }

    public void MarchUpdate() { }
}
