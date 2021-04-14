using System.Collections.Generic;
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
    private ChunkPlantManager chunkPlantManager;

    private void OnDrawGizmos()
    {
        if (ChunkManager.Instance.EnableGizmos)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center, new Vector3(ChunkManager.Instance.Size - 1, ChunkManager.Instance.Size - 1, ChunkManager.Instance.Size - 1));
            Gizmos.DrawSphere(center, .5f);
        }
    }

    private void Awake()
    {
        marcher = GetComponent<Marcher>();
        mesh = GetComponent<MeshFilter>().mesh;
        chunkPlantManager = GetComponent<ChunkPlantManager>();
        MarkupShader = Resources.Load("Compute Shaders/ChipMarkup") as ComputeShader;
        MarkupKernel = MarkupShader.FindKernel("ChipMarkup");
    }

    private void Start()
    {
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
        ServiceScheduler.Instance.Request("chunk chipnit", gameObject);
    }

    public void MarchUpdate()
    {
        chunkPlantManager.CreatePlants();
    }

    private void OnDisable()
    {
        chunkPlantManager.ReleasePlants();
    }
}