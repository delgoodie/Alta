[System.Serializable]
public class ChunkData
{
    public int[] position;
    public int size;
    public Chip[] chips;

    public ChunkData(Chunk chunk)
    {
        position = new int[3] { chunk.position.x, chunk.position.y, chunk.position.z };
        size = chunk.marcher.size;
        chips = chunk.marcher.chips;
    }
}