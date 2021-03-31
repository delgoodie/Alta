using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(IMarch))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Marcher : MonoBehaviour
{
    [HideInInspector]
    public Chip[] chips;
    [HideInInspector]
    public bool updated;
    public int size;
    public bool closed;
    public float scale;
    public Vector3 offset;
    [HideInInspector]
    public Mesh mesh;
    [HideInInspector]
    public MeshCollider meshCollider;
    [HideInInspector]
    private IMarch MainScript;
    private void Awake()
    {
        MainScript = GetComponent<IMarch>();
        mesh = GetComponent<MeshFilter>().mesh;
        meshCollider = GetComponent<MeshCollider>();
        mesh.MarkDynamic();
    }

    private void Update()
    {
        if (updated)
        {
            MainScript.MeshUpdate();
            updated = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (MarchManager.Instance.gizmosEnabled)
        {
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    for (int z = 0; z < size; z++)
                    {
                        float iso = (float)chips[x * size * size + y * size + z].iso;
                        Gizmos.color = new Color(iso > 128 ? 0f : 1f, iso > 128 ? 0f : 1f, iso > 128 ? 0f : 1f, iso / 255f);
                        Gizmos.DrawCube(PosToWorld(new Vector3Int(x, y, z)), Vector3.one * scale);
                    }
        }
    }

    public void Init(bool chipnit)
    {
        updated = false;
        if (chipnit) MainScript.Chipnit();
        March();
    }

    public void March()
    {
        MainScript.Markup();
        MarchManager.Instance.RequestMarch(this);
    }

    public void Add(Vector3 center, float radius, float power)
    {
        bool modified = false;
        for (int i = 0; i < chips.Length; i++)
        {
            Vector3 pos = PosToWorld(IndexToPos(i));
            if ((center - pos).sqrMagnitude < radius * radius)
            {
                chips[i].iso = (byte)Mathf.Clamp(chips[i].iso + Mathf.CeilToInt(power * Time.deltaTime), 0, 0xff);
                modified = true;
            }
        }
        if (modified) March();
    }

    public void Subtract(Vector3 center, float radius, float power)
    {
        bool modified = false;
        for (int i = 0; i < chips.Length; i++)
        {
            Vector3 pos = PosToWorld(IndexToPos(i));
            if ((center - pos).sqrMagnitude < radius * radius)
            {
                chips[i].iso = (byte)Mathf.Clamp(chips[i].iso - Mathf.CeilToInt(power * Time.deltaTime), 0, 0xff);
                modified = true;
            }
        }
        if (modified) March();
    }

    public Vector3Int IndexToPos(int i)
    {
        int s1 = i / size;
        int s2 = s1 / size;
        Vector3Int pos = new Vector3Int(s2, s1 % size, 0);
        pos.z = i - pos.x * size * size - pos.y * size;
        return pos;
    }

    public Vector3 PosToWorld(Vector3Int pos)
    {
        return transform.TransformPoint(new Vector3(pos.x, pos.y, pos.z) * scale + offset);
    }

    public int WorldToIndex(Vector3 world)
    {
        Vector3 trans = transform.InverseTransformPoint(world) / scale - offset;
        return Mathf.RoundToInt(trans.x) * size * size + Mathf.RoundToInt(trans.y) * size + Mathf.RoundToInt(trans.z);
    }

    public Chip WorldToChip(Vector3 world)
    {
        int index = WorldToIndex(world);
        if (index > -1 && index < chips.Length) return chips[index];
        else
        {
            Debug.Log("Invalid WS: " + world.x + ", " + world.y + ", " + world.z);
            return new Chip(0x00, 0x00, 0x0000);
        }
    }
}