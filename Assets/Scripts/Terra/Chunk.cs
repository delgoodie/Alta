﻿using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Marcher))]
public class Chunk : MonoBehaviour, IMarch, IEntity
{
    [HideInInspector]
    public Marcher marcher;
    public Vector3Int position;
    public Vector3 center;
    private ComputeShader MarkupShader;
    private ComputeBuffer chipsBuffer;
    private int MarkupKernel;
    private Mesh mesh;
    public List<GameObject> plants;
    public List<GameObject> creatures;

    private void OnDrawGizmos()
    {
        // Gizmos.color = Color.red;
        // Gizmos.DrawWireCube(center, new Vector3(ChunkManager.Instance.Size - 1, ChunkManager.Instance.Size - 1, ChunkManager.Instance.Size - 1));
        // Gizmos.DrawSphere(center, .5f);
    }

    private void OnDrawGizmosSelected()
    {
        if (MarchManager.Instance.gizmosEnabled)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center, new Vector3(ChunkManager.Instance.Size - 1, ChunkManager.Instance.Size - 1, ChunkManager.Instance.Size - 1));
            Gizmos.color = Color.blue;

            for (int i = 0; i < marcher.chips.Length; i++)
            {
                if (marcher.chips[i].type == 4)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(marcher.PosToWorld(marcher.IndexToPos(i)), Vector3.one);
                }
            }
        }
    }

    private void Awake()
    {
        marcher = GetComponent<Marcher>();
        mesh = GetComponent<MeshFilter>().mesh;
        plants = new List<GameObject>();
        creatures = new List<GameObject>();
        MarkupShader = Resources.Load("Compute Shaders/ChipMarkup") as ComputeShader;
        MarkupKernel = MarkupShader.FindKernel("ChipMarkup");
    }

    private void Start()
    {
    }

    private Ray RandomSurfaceRay()
    {
        int tri = Random.Range(0, mesh.triangles.Length / 3);
        Vector3 a = mesh.vertices[mesh.triangles[tri * 3]], b = mesh.vertices[mesh.triangles[tri * 3 + 1]], c = mesh.vertices[mesh.triangles[tri * 3 + 2]];
        return new Ray(transform.position + Vector3.Lerp(a, Vector3.Lerp(b, c, Random.Range(0.0f, 1.0f)), Random.Range(0.0f, 1.0f)), Vector3.Cross(a - b, a - c));
    }

    public void Init(Vector3Int _position)
    {
        position = _position;
        center = transform.position + new Vector3((ChunkManager.Instance.Size - 1) * .5f, (ChunkManager.Instance.Size - 1) * .5f, (ChunkManager.Instance.Size - 1) * .5f);
        Chipnit();
    }

    public void Init(ChunkData data)
    {
        position = new Vector3Int(data.position[0], data.position[1], data.position[2]);
        center = transform.position + new Vector3((ChunkManager.Instance.Size - 1) * .5f, (ChunkManager.Instance.Size - 1) * .5f, (ChunkManager.Instance.Size - 1) * .5f);
        marcher.chips = data.chips;
        marcher.size = data.size;
        Chipnit();
    }

    public void ChipUpdate()
    {
        marcher.Init();
    }

    public void Chipnit()
    {
        ChunkManager.Instance.RequestChipnit(this);
    }

    public void MarchUpdate()
    {
        while (transform.childCount > 0) Destroy(transform.GetChild(0));
        for (int i = 0; i < 30; i++)
        {
            GameObject p = PlantManager.Instance.Kelp(RandomSurfaceRay());
            if (p != null) plants.Add(p);
            // int x = Random.Range(0, 16), y = Random.Range(0, 16), z = Random.Range(0, 16);
            // if (marcher.chips[x * 16 * 16 + y * 16 + z].iso < 128)
            // {
            //     Ray es = new Ray(transform.position + new Vector3(x, y, z), Random.rotation * Vector3.forward);
            //     GameObject c = CreatureManager.Instance.Retrieve(es);
            //     if (c != null) creatures.Add(c);
            // }
        }
    }

    public void Deactivate()
    {
        // for (int i = 0; i < plants.Count; i++) PlantManager.Instance.Release(plants[i]);
        // for (int i = 0; i < creatures.Count; i++) CreatureManager.Instance.Release(creatures[i]);
        plants.Clear();
        creatures.Clear();
        gameObject.SetActive(false);
    }
    public void Activate()
    {
        // for (int i = 0; i < plants.Count; i++) PlantManager.Instance.Release(plants[i]);
        // for (int i = 0; i < creatures.Count; i++) CreatureManager.Instance.Release(creatures[i]);
        gameObject.SetActive(true);
    }
}