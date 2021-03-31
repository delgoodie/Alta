using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class ChunkLoader
{
    public static void SaveChunk(Chunk chunk)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/c" + chunk.position.x + "_" + chunk.position.y + "_" + chunk.position.z + ".chnk";
        FileStream stream = new FileStream(path, FileMode.Create);
        ChunkData data = new ChunkData(chunk);
        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static ChunkData LoadChunk(Vector3Int pos)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(Application.persistentDataPath + "/c" + pos.x + "_" + pos.y + "_" + pos.z + ".chnk", FileMode.Open);
        ChunkData data = formatter.Deserialize(stream) as ChunkData;
        stream.Close();
        return data;
    }

    public static bool ChunkExists(Vector3Int pos) => File.Exists(Application.persistentDataPath + "/c" + pos.x + "_" + pos.y + "_" + pos.z + ".chnk");

    public static void DeleteChunk(Vector3Int pos) => File.Delete(Application.persistentDataPath + "/c" + pos.x + "_" + pos.y + "_" + pos.z + ".chnk");

    public static void DeleteAllChunks()
    {
        DirectoryInfo di = new DirectoryInfo(Application.persistentDataPath);
        foreach (FileInfo fi in di.GetFiles()) fi.Delete();
    }

}