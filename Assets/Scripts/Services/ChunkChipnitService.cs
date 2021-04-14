using UnityEngine;
using System.Collections.Generic;

public class ChunkChipnitService : MonoBehaviour, IService
{
    public int Size;
    public Vector3Int Offset;
    public float TypeNoise;
    public Vector4[] NoiseOctaves;
    public int Ceiling;
    public int Floor;
    private ComputeShader NoiseShader;
    private int NoiseKernel;
    private ComputeBuffer ChipsBuffer, OctaveBuffer;
    private int[] mChips;
    private void Awake()
    {
        mChips = new int[Size * Size * Size];
        for (int i = 0; i < mChips.Length; i++) mChips[i] = 0;
        NoiseShader = Resources.Load("Compute Shaders/Noise3D") as ComputeShader;
        NoiseKernel = NoiseShader.FindKernel("Noise3D");
    }

    public void Execute(GameObject gameObject)
    {
        Chunk chunk = gameObject.GetComponent<Chunk>();
        if (chunk == null) return;
        ChipsBuffer = new ComputeBuffer(Size * Size * Size, sizeof(int));

        ChipsBuffer.SetData(mChips);
        NoiseShader.SetBuffer(NoiseKernel, "chips", ChipsBuffer);

        OctaveBuffer = new ComputeBuffer(NoiseOctaves.Length, sizeof(float) * 4);

        OctaveBuffer.SetData(NoiseOctaves);
        NoiseShader.SetBuffer(NoiseKernel, "octaves", OctaveBuffer);

        NoiseShader.SetInt("size", Size);
        NoiseShader.SetInt("X", chunk.position.x);
        NoiseShader.SetInt("Y", chunk.position.y);
        NoiseShader.SetInt("Z", chunk.position.z);

        NoiseShader.SetInt("offX", Offset.x);
        NoiseShader.SetInt("offY", Offset.y);
        NoiseShader.SetInt("offZ", Offset.z);

        NoiseShader.SetInt("minType", 1);
        NoiseShader.SetInt("maxType", 23);
        NoiseShader.SetInt("emptyType", 0);

        NoiseShader.SetInt("ceiling", Ceiling);
        NoiseShader.SetInt("floor", Floor);

        NoiseShader.SetFloat("typeN", TypeNoise);
        NoiseShader.SetInt("noctaves", NoiseOctaves.Length);

        NoiseShader.Dispatch(NoiseKernel, Size, Size, Size);

        ChipsBuffer.GetData(mChips);

        ChipsBuffer.Dispose();
        OctaveBuffer.Dispose();

        Chip[] chips = new Chip[Size * Size * Size];
        for (int i = 0; i < mChips.Length; i++)
        {
            chips[i].iso = (byte)((mChips[i] >> 24) & 0xff);
            chips[i].type = (byte)((mChips[i] >> 16) & 0xff);
            chips[i].data = (ushort)(mChips[i] & 0xffff);
        }
        chunk.marcher.chips = chips;
        chunk.ChipUpdate();
    }
}
