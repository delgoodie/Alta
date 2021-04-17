[System.Serializable]
public class ChunkData
{
    public int[] coordinate;
    public int size;
    public Chip[] chips;

    public ChunkData(Chunk chunk)
    {
        coordinate = new int[3] { chunk.coordinate.x, chunk.coordinate.y, chunk.coordinate.z };
        size = chunk.marcher.size;
        chips = chunk.marcher.chips;
    }
}