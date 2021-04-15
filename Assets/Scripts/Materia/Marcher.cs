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
            MainScript.MarchUpdate();
            updated = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
    }

    public void Init()
    {
        updated = false;
        March();
    }

    public void March()
    {
        mesh.Clear();
        ServiceScheduler.Instance.Request("march", gameObject);
    }

    public void Add(Vector3 center, float radius, float power)
    {
        bool modified = false;
        for (int i = 0; i < chips.Length; i++)
        {
            Vector3 world = LocalToWorld(IndexToLocal(i));
            if ((center - world).sqrMagnitude < radius * radius)
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
            Vector3 world = LocalToWorld(IndexToLocal(i));
            if ((center - world).sqrMagnitude < radius * radius)
            {
                chips[i].iso = (byte)Mathf.Clamp(chips[i].iso - Mathf.CeilToInt(power * Time.deltaTime), 0, 0xff);
                modified = true;
            }
        }
        if (modified) March();
    }

    public Vector3Int IndexToLocal(int i)
    {
        int s1 = i / size;
        int s2 = s1 / size;
        Vector3Int local = new Vector3Int(s2, s1 % size, 0);
        local.z = i - local.x * size * size - local.y * size;
        return local;
    }

    public Vector3 LocalToWorld(Vector3Int local)
    {
        return transform.TransformPoint(new Vector3(local.x, local.y, local.z) * scale + offset);
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