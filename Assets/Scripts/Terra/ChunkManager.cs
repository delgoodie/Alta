using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    #region PUBLIC
    [Header("Manager")]
    public int RenderDistance;
    [Header("Chunk")]
    public int Size;
    [Header("Noise")]
    public Vector3Int Offset;
    public float TypeNoise;
    public Vector4[] NoiseOctaves;
    public bool RunChunkUpdate;

    [HideInInspector]
    public static ChunkManager Instance;
    [HideInInspector]
    public bool noMarch;
    #endregion
    #region PRIVATE
    private Dictionary<Vector3Int, Chunk> ChunkList;
    private Queue<Chunk> RecycledChunkList;
    private Queue<Chunk> ChipnitQueue;
    private GameObject ChunkObject;
    private List<Vector3Int> UpdateList;
    private ComputeShader NoiseShader;
    private int NoiseKernel;
    private ComputeBuffer ChipsBuffer, OctaveBuffer;
    private int[] mChips;
    #endregion

    private void Awake()
    {
        Instance = this;
        UpdateList = new List<Vector3Int>();
        ChunkList = new Dictionary<Vector3Int, Chunk>();
        RecycledChunkList = new Queue<Chunk>();
        ChipnitQueue = new Queue<Chunk>();
        mChips = new int[Size * Size * Size];
        for (int i = 0; i < mChips.Length; i++) mChips[i] = 0;
        ChunkObject = Resources.Load("Prefabs/ChunkObject") as GameObject;
        NoiseShader = Resources.Load("Compute Shaders/Noise3D") as ComputeShader;
        NoiseKernel = NoiseShader.FindKernel("Noise3D");
    }

    private void Update()
    {
        // ! Update All Chunks
        // if (RunChunkUpdate)
        // {
        //     RunChunkUpdate = false;
        //     foreach (KeyValuePair<Vector3Int, Chunk> chunkEntry in ChunkList)
        //     {
        //         chunkEntry.Value.marcher.chips = Chipnit(chunkEntry.Key);
        //         chunkEntry.Value.marcher.March();
        //     }
        // }

        if (ChipnitQueue.Count > 0)
        {
            noMarch = false;
            Chipnit(ChipnitQueue.Dequeue());
        }
        else noMarch = true;

    }

    public void ChunkUpdate(Vector3Int position)
    {
        UpdateList.Clear();

        // FIND CHUNKS AROUND POSITION FROM RENDER DISTANCE
        for (int y = position.y - RenderDistance / 2 - 1; y < position.y + RenderDistance / 2; y++)
        {
            for (int x = position.x - RenderDistance; x <= position.x + RenderDistance; x++)
                UpdateList.Add(new Vector3Int(x, y, position.z));

            for (int i = RenderDistance; i > 0; i--)
                for (int x = position.x - (i - 1); x <= position.x + (i - 1); x++)
                {
                    UpdateList.Add(new Vector3Int(x, y, position.z + (RenderDistance - i + 1)));
                    UpdateList.Add(new Vector3Int(x, y, position.z - (RenderDistance - i + 1)));
                }
        }


        // QUEUE FAR AWAY CHUNKS FOR RECYCLING
        foreach (KeyValuePair<Vector3Int, Chunk> pair in ChunkList)
            if (Mathf.Abs((position - pair.Value.position).sqrMagnitude) > (RenderDistance + 1) * (RenderDistance + 1))
                RecycledChunkList.Enqueue(pair.Value);

        foreach (Chunk chunk in RecycledChunkList)
        {
            if (chunk.gameObject.activeSelf)
            {
                chunk.Deactivate();
                ChunkList.Remove(chunk.position);
                chunk.position = Vector3Int.zero;
                chunk.transform.position = Vector3.zero;
            }
        }

        // ADD / DEQUEUE NEW CHUNKS FROM UPDATE LIST
        for (int i = 0; i < UpdateList.Count; i++)
        {
            if (!ChunkList.ContainsKey(UpdateList[i]))
            {
                Vector3 newPos = UpdateList[i] * (Size - 1);
                if (UpdateList[i].x < 0) newPos.x -= 0f;
                if (UpdateList[i].y < 0) newPos.y -= 0f;
                if (UpdateList[i].z < 0) newPos.z -= 0f;
                if (RecycledChunkList.Count > 0)
                {
                    ChunkList.Add(UpdateList[i], RecycledChunkList.Dequeue());
                    // TODO: Chunk Saving and Loading
                    // ChunkLoader.SaveChunk(ChunkList[UpdateList[i]]);
                    ChunkList[UpdateList[i]].transform.position = newPos;
                    ChunkList[UpdateList[i]].gameObject.SetActive(true);
                }
                else
                {
                    GameObject chunkObject = Instantiate(ChunkObject, newPos, Quaternion.identity, transform);
                    ChunkList.Add(UpdateList[i], chunkObject.GetComponent<Chunk>());
                }
                // TODO: Chunk Saving and Loading
                if (ChunkLoader.ChunkExists(UpdateList[i]))
                    ChunkList[UpdateList[i]].Init(ChunkLoader.LoadChunk(UpdateList[i]));
                else
                    ChunkList[UpdateList[i]].Init(UpdateList[i]);
            }
        }
        UpdateList.Clear();
    }

    public void RequestChipnit(Chunk c)
    {
        if (!ChipnitQueue.Contains(c)) ChipnitQueue.Enqueue(c);
    }

    private void Chipnit(Chunk chunk)
    {
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

        NoiseShader.SetFloat("typeN", TypeNoise);
        NoiseShader.SetInt("octaveSize", NoiseOctaves.Length);

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

    public Chip WorldToChip(Vector3 world)
    {
        Vector3Int pos = new Vector3Int(Mathf.RoundToInt(world.x) / (Size - 1) - (world.x < 0 ? 1 : 0), Mathf.RoundToInt(world.y) / (Size - 1) - (world.y < 0 ? 1 : 0), Mathf.RoundToInt(world.z) / (Size - 1) - (world.z < 0 ? 1 : 0));
        if (ChunkList.ContainsKey(pos)) return ChunkList[pos].marcher.WorldToChip(world);
        else
        {
            Debug.Log("No chunk at coords");
            return new Chip(1, 0, 0);
        }
    }

    public void SaveMemory()
    {
        foreach (KeyValuePair<Vector3Int, Chunk> chunkEntry in ChunkList) ChunkLoader.SaveChunk(chunkEntry.Value);
        Debug.Log("Saved");
    }

    public void DeleteMemory()
    {
        ChunkLoader.DeleteAllChunks();
        Debug.Log("Deleted");
    }

    public void Add(Vector3 p, float r, float power)
    {
        foreach (KeyValuePair<Vector3Int, Chunk> chunkEntry in ChunkList)
        {
            if ((chunkEntry.Value.center - p).sqrMagnitude < 24f * 24f + r * r)
                chunkEntry.Value.marcher.Add(p, r, power);
        }
    }

    public void Subtract(Vector3 p, float r, float power)
    {
        foreach (KeyValuePair<Vector3Int, Chunk> chunkEntry in ChunkList)
        {
            if ((chunkEntry.Value.center - p).sqrMagnitude < 24f * 24f + r * r)
                chunkEntry.Value.marcher.Subtract(p, r, power);
        }
    }

    public bool LineCast(Vector3 from, Vector3 to)
    {
        Vector3 point = new Vector3(from.x, from.y, from.z);
        Vector3 add = (to - from).normalized;
        while ((point - to).sqrMagnitude > 1f)
        {
            point += add;
            if (WorldToChip(point).iso > 0x80) return false;
        }
        return true;
    }

    public void Markup(Vector3Int p)
    {
        int mkDist = 6;
        for (int x = 0; x < Size; x++) for (int y = 0; y < Size; y++) for (int z = 0; z < Size; z++)
                {
                    if (ChunkList[p].marcher.chips[x * Size * Size + y * Size + z].type == 0)
                    {
                        Vector3Int center = p * Size + new Vector3Int(x, y, z);
                        bool grey = false;
                        for (int x2 = center.x - mkDist / 2; x2 < center.x + mkDist / 2; x2++)
                            for (int y2 = center.y - mkDist / 2; y2 < center.y + mkDist / 2; y2++)
                                for (int z2 = center.z - mkDist / 2; z2 < center.z + mkDist / 2; z2++)
                                {
                                    if (WorldToChip(new Vector3(x, y, z)).iso > 0x80) grey = true;
                                }
                        if (grey)
                        {
                            ChunkList[p].marcher.chips[x * Size * Size + y * Size + z].type = 0;
                        }
                    }
                }
    }

    public bool BoxCheck(Vector3 pos, Vector3 size)
    {
        bool passed = true;
        for (int x = 0; x < size.x; x++) for (int y = 0; y < size.y; y++) for (int z = 0; z < size.z; z++)
                    if (Chips.Opaque[WorldToChip(new Vector3(pos.x + (float)x - size.x * .5f - .5f, pos.y + (float)y - size.y * .5f - .5f, pos.z + (float)z - size.z * .5f - .5f)).type]) passed = false;
        return passed;
    }

    public bool BoxCheck(Vector3 pos, Quaternion rot, Vector3 size)
    {
        bool passed = true;
        transform.rotation = rot;
        for (int x = 0; x < size.x; x++) for (int y = 0; y < size.y; y++) for (int z = 0; z < size.z; z++)
                    if (Chips.Opaque[WorldToChip(pos + transform.TransformVector(new Vector3(x - (float)size.x * .5f - .5f, (float)y - size.y * .5f - .5f, (float)z - size.z * .5f - .5f))).type]) passed = false;
        transform.rotation = Quaternion.identity;
        return passed;
    }

    public bool Raycast(Vector3 origin, Vector3 direction, float distance, out ChipCastHit result)
    {
        direction.Normalize();
        Chip tc = new Chip(0x00, 0x00, 0x0000);
        Vector3 caster = origin;
        bool omitX = direction.x == 0f;
        bool omitY = direction.y == 0f;
        bool omitZ = direction.z == 0f;
        float dx = 0f;
        float dy = 0f;
        float dz = 0f;
        int it = 0;
        while ((caster - origin).sqrMagnitude < distance * distance)
        {
            it++;
            tc = WorldToChip(caster);
            Vector3 tcPoint = new Vector3(Mathf.Round(caster.x), Mathf.Round(caster.y), Mathf.Round(caster.z));
            if (Chips.Opaque[tc.type])
            {
                result = new ChipCastHit(tcPoint, tc, (origin - tcPoint).magnitude);
                return true;
            }
            else
            {
                if (!omitX) dx = tcPoint.x + (direction.x > 0 ? .501f : -.501f) - caster.x;
                if (!omitY) dy = tcPoint.y + (direction.y > 0 ? .501f : -.501f) - caster.y;
                if (!omitZ) dz = tcPoint.z + (direction.z > 0 ? .501f : -.501f) - caster.z;

                float maxVal = -Mathf.Infinity;
                char max = ' ';
                if (!omitX && direction.x / dx > maxVal)
                {
                    maxVal = direction.x / dx;
                    max = 'x';
                }
                if (!omitY && direction.y / dy > maxVal)
                {
                    maxVal = direction.y / dy;
                    max = 'y';
                }
                if (!omitZ && direction.z / dz > maxVal)
                {
                    max = 'z';
                }

                Vector3 preCast = caster;

                if (!omitX && max == 'x') caster += direction * (dx / direction.x);
                else if (!omitY && max == 'y') caster += direction * (dy / direction.y);
                else if (!omitZ && max == 'z') caster += direction * (dz / direction.z);
                else Debug.Log("WHAT THE HECK");
            }
            if (it > 500) break;
        }
        result = new ChipCastHit(origin, tc, 0f);
        return false;
    }

    public bool Raycast(Vector3 origin, Vector3 direction, float distance)
    {
        direction.Normalize();
        Chip tc = new Chip(0x00, 0x00, 0x0000);
        Vector3 caster = origin;
        bool omitX = direction.x == 0f;
        bool omitY = direction.y == 0f;
        bool omitZ = direction.z == 0f;
        float dx = 0f;
        float dy = 0f;
        float dz = 0f;
        int it = 0;
        while ((caster - origin).sqrMagnitude < distance * distance)
        {
            it++;
            tc = WorldToChip(caster);
            Vector3 tcPoint = new Vector3(Mathf.Round(caster.x), Mathf.Round(caster.y), Mathf.Round(caster.z));
            if (Chips.Opaque[tc.type]) return true;
            else
            {
                if (!omitX) dx = tcPoint.x + (direction.x > 0 ? .501f : -.501f) - caster.x;
                if (!omitY) dy = tcPoint.y + (direction.y > 0 ? .501f : -.501f) - caster.y;
                if (!omitZ) dz = tcPoint.z + (direction.z > 0 ? .501f : -.501f) - caster.z;

                float maxVal = -Mathf.Infinity;
                char max = ' ';
                if (!omitX && direction.x / dx > maxVal)
                {
                    maxVal = direction.x / dx;
                    max = 'x';
                }
                if (!omitY && direction.y / dy > maxVal)
                {
                    maxVal = direction.y / dy;
                    max = 'y';
                }
                if (!omitZ && direction.z / dz > maxVal)
                {
                    max = 'z';
                }

                Vector3 preCast = caster;

                if (!omitX && max == 'x') caster += direction * (dx / direction.x);
                else if (!omitY && max == 'y') caster += direction * (dy / direction.y);
                else if (!omitZ && max == 'z') caster += direction * (dz / direction.z);
                else Debug.Log("WHAT THE HECK");
            }
            if (it > 500) break;
        }
        return false;
    }
}

public struct ChipCastHit
{
    public Vector3 point;
    public Chip chip;
    public float distance;
    public ChipCastHit(Vector3 p, Chip c, float d)
    {
        point = p;
        chip = c;
        distance = d;
    }
}