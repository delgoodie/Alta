using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    #region PUBLIC
    public int RenderDistance;
    public int Size;
    public bool RunChunkUpdate;
    public bool EnableGizmos;

    [HideInInspector]
    public static ChunkManager Instance;
    #endregion
    #region PRIVATE
    private Dictionary<Vector3Int, Chunk> ChunkDict;
    //TODO: Make update list static and just use offsets to find update coords
    private List<Vector3Int> UpdateList;
    #endregion

    private void Awake()
    {
        Instance = this;
        //TODO: Calculate fixed capacity from render distance
        ChunkDict = new Dictionary<Vector3Int, Chunk>();

        UpdateList = new List<Vector3Int>();
        for (int y = -RenderDistance / 2; y <= RenderDistance / 2; y++)
        {
            for (int x = 0 - RenderDistance; x <= RenderDistance; x++)
                UpdateList.Add(new Vector3Int(x, y, 0));

            for (int i = RenderDistance; i > 0; i--)
                for (int x = -i + 1; x <= i - 1; x++)
                {
                    UpdateList.Add(new Vector3Int(x, y, RenderDistance - i + 1));
                    UpdateList.Add(new Vector3Int(x, y, -RenderDistance + i - 1));
                }
        }
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
    }

    public Chunk GetChunk(Vector3Int position)
    {
        if (ChunkDict.ContainsKey(position))
            return ChunkDict[position];
        else return null;
    }

    public void ChunkUpdate(Vector3Int position)
    {
        #region RELEASE UNUSED CHUNKS TO ENTITY MANAGER
        List<Vector3Int> removes = new List<Vector3Int>();
        foreach (KeyValuePair<Vector3Int, Chunk> pair in ChunkDict)
        {
            bool keep = false;
            for (int i = 0; i < UpdateList.Count; i++)
                if (pair.Key.Equals(position + UpdateList[i])) keep = true;
            if (!keep)
            {
                EntityManager.Instance.Release("chunk", pair.Value.gameObject);
                removes.Add(pair.Key);
            }
        }
        foreach (Vector3Int p in removes) ChunkDict.Remove(p);
        #endregion


        #region RETRIEVE NEW CHUNKS FROM ENTITY MANAGER
        for (int i = 0; i < UpdateList.Count; i++)
        {
            Vector3Int c_pos = UpdateList[i] + position;
            Vector3 w_pos = c_pos * (Size - 1);
            if (!ChunkDict.ContainsKey(c_pos))
            {
                GameObject c_go = EntityManager.Instance.Retrieve("chunk");
                if (c_go != null)
                {
                    Chunk c = c_go.GetComponent<Chunk>();
                    ChunkDict.Add(c_pos, c);
                    // TODO: Chunk Saving and Loading
                    // ChunkLoader.SaveChunk(ChunkList[UpdateList[i]]);
                    ChunkDict[c_pos].transform.position = w_pos;
                    // TODO: Chunk Saving and Loading
                    if (ChunkLoader.ChunkExists(c_pos))
                        ChunkDict[c_pos].Init(ChunkLoader.LoadChunk(c_pos));
                    else
                        ChunkDict[c_pos].Init(c_pos);
                }
                else
                {
                    Debug.Log("No Chunk Available");
                }
            }
        }
        #endregion
    }

    public Chip WorldToChip(Vector3 world)
    {
        Vector3Int pos = new Vector3Int(Mathf.RoundToInt(world.x) / (Size - 1) - (world.x < 0 ? 1 : 0), Mathf.RoundToInt(world.y) / (Size - 1) - (world.y < 0 ? 1 : 0), Mathf.RoundToInt(world.z) / (Size - 1) - (world.z < 0 ? 1 : 0));
        if (ChunkDict.ContainsKey(pos)) return ChunkDict[pos].marcher.WorldToChip(world);
        else
        {
            Debug.Log("No chunk at coords");
            return new Chip(1, 0, 0);
        }
    }

    public void SaveMemory()
    {
        foreach (KeyValuePair<Vector3Int, Chunk> chunkEntry in ChunkDict) ChunkLoader.SaveChunk(chunkEntry.Value);
        Debug.Log("Saved");
    }

    public void DeleteMemory()
    {
        ChunkLoader.DeleteAllChunks();
        Debug.Log("Deleted");
    }

    public void Add(Vector3 p, float r, float power)
    {
        foreach (KeyValuePair<Vector3Int, Chunk> chunkEntry in ChunkDict)
        {
            if ((chunkEntry.Value.center - p).sqrMagnitude < 24f * 24f + r * r)
                chunkEntry.Value.marcher.Add(p, r, power);
        }
    }

    public void Subtract(Vector3 p, float r, float power)
    {
        foreach (KeyValuePair<Vector3Int, Chunk> chunkEntry in ChunkDict)
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
                    if (ChunkDict[p].marcher.chips[x * Size * Size + y * Size + z].type == 0)
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
                            ChunkDict[p].marcher.chips[x * Size * Size + y * Size + z].type = 0;
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