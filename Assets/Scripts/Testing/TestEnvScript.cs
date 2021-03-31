using UnityEngine;

public class TestEnvScript : ChunkManager
{
    [HideInInspector]
    GameObject target;
    GameObject spawn;
    [HideInInspector]
    GameObject Spider;
    GameObject spider_instance;
    private void Start()
    {
        Init();
    }

    private void Update()
    {
    }

    new public void Init()
    {
        Spider = Resources.Load("Prefabs/Monsters/Spider/Spider") as GameObject;
        target = GameObject.Find("target");
        spawn = GameObject.Find("spawn");

        base.Init();
        ComputeChunkUpdate(Vector3.zero);
    }

    public override void ComputeChunkUpdate(Vector3 pos)
    {
        Vector3Int p = new Vector3Int((int)pos.x / 16, (int)pos.y / 16, (int)pos.z / 16);
        UpdateList.Clear();

        for (int x = 0; x < RenderDistance; x++)
            for (int y = 0; y < RenderDistance; y++)
                for (int z = 0; z < RenderDistance; z++)
                    UpdateList.Add(new Vector3Int(p.x + x, p.y + y, p.z + z));

        CleanChunks(pos);
        ChunkUpdate();
    }

    public override byte PosIso(Vector3 pos)
    {
        return (byte)(0x80 + 20 - (int)pos.y);
    }

    public override byte PosType(Vector3 pos)
    {
        return (byte)(PosIso(pos) > 0x80 ? Chips.Dirt : Chips.Air);
    }
}
