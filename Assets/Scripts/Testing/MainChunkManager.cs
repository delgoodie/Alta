using UnityEngine;

public class MainChunkManager : ChunkManager
{
    private ComputeShader NoiseShader;
    private int NoiseKernel;
    private ComputeBuffer ChipsBuffer;
    private int[] mChips;

    private void Start()
    {
        Init();
    }

    new public void Init()
    {
        base.Init();

        NoiseShader = Resources.Load("Compute Shaders/Noise3D") as ComputeShader;
        NoiseKernel = NoiseShader.FindKernel("Noise3D");
        mChips = new int[Size * Size * Size];
        for (int i = 0; i < mChips.Length; i++) mChips[i] = 0;

        ComputeChunkUpdate(Vector3Int.zero);
    }

    public override void ComputeChunkUpdate(Vector3 pos)
    {
        Vector3Int p = new Vector3Int((int)pos.x / 16, (int)pos.y / 16, (int)pos.z / 16);
        UpdateList.Clear();

        for (int y = p.y - 1; y < p.y + 2; y++)
        {
            for (int x = p.x - RenderDistance; x <= p.x + RenderDistance; x++)
                UpdateList.Add(new Vector3Int(x, y, p.z));

            for (int i = RenderDistance; i > 0; i--)
                for (int x = p.x - (i - 1); x <= p.x + (i - 1); x++)
                {
                    UpdateList.Add(new Vector3Int(x, y, p.z + (RenderDistance - i + 1)));
                    UpdateList.Add(new Vector3Int(x, y, p.z - (RenderDistance - i + 1)));
                }
        }

        CleanChunks(pos);
        ChunkUpdate();
    }

    public override Chip[] Chipnit(Vector3Int p)
    {
        ChipsBuffer = new ComputeBuffer(Size * Size * Size, sizeof(int));

        ChipsBuffer.SetData(mChips);
        NoiseShader.SetBuffer(NoiseKernel, "chips", ChipsBuffer);

        NoiseShader.SetInt("size", Size);
        NoiseShader.SetInt("chunkX", p.x);
        NoiseShader.SetInt("chunkY", p.y);
        NoiseShader.SetInt("chunkZ", p.z);

        NoiseShader.Dispatch(NoiseKernel, Size, Size, Size);

        ChipsBuffer.GetData(mChips);
        ChipsBuffer.Dispose();

        Chip[] chips = new Chip[Size * Size * Size];
        for (int i = 0; i < mChips.Length; i++)
        {
            chips[i].iso = (byte)((mChips[i] >> 24) & 0xff);
            chips[i].type = (byte)((mChips[i] >> 16) & 0xff);
            chips[i].data = (ushort)(mChips[i] & 0xffff);
        }
        return chips;
    }
}