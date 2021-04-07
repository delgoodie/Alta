using UnityEngine;

[RequireComponent(typeof(MarchManager))]
[RequireComponent(typeof(PlayerManager))]
[RequireComponent(typeof(ChunkManager))]
class GameManager : MonoBehaviour
{
    public int tickSpan;
    public int tick;
    public bool tick1;
    public bool tick2;
    public bool tick3;
    public bool tick4;
    public bool tick5;
    public bool tick6;

    public static GameManager Instance;

    private int countT;
    private int count2;
    private int count3;
    private int count4;
    private int count5;
    private int count6;
    private Vector3Int previousPlayerChunk;
    [SerializeField]
    private Transform UnderwaterVolume;
    [SerializeField]
    private Transform SurfaceBottom;

    private void Awake()
    {
        Instance = this;
        tick = 0;
        tick1 = false;
        tick2 = false;
        tick3 = false;
        tick4 = false;
        tick5 = false;
        tick6 = false;
        countT = 0;
        count2 = 0;
        count3 = 0;
        count4 = 0;
        count5 = 0;
        count6 = 0;
        previousPlayerChunk = Vector3Int.zero;
    }

    private void Start()
    {
        ChunkManager.Instance.ChunkUpdate(Vector3Int.zero);
    }
    private void Update()
    {
        TickHandler();
        if (PlayerManager.Instance.activePlayer)
        {
            Vector3 playerPosition = PlayerManager.Instance.PlayerPosition();
            Vector3Int playerChunk = new Vector3Int((int)(playerPosition.x * 0.0625f), (int)(playerPosition.y * 0.0625f), (int)(playerPosition.z * 0.0625f));
            if (!previousPlayerChunk.Equals(playerChunk))
            {
                ChunkManager.Instance.ChunkUpdate(playerChunk);
                previousPlayerChunk = playerChunk;
            }
            SurfaceBottom.position = new Vector3(playerPosition.x, SurfaceBottom.position.y, playerPosition.z);
            UnderwaterVolume.position = playerPosition;
        }
        else
        {
            if (MarchManager.Instance.noMarch) PlayerManager.Instance.CreatePlayer(Vector3.zero, Quaternion.identity);
        }

    }

    private void TickHandler()
    {
        countT++;
        if (countT == tickSpan)
        {
            countT = 0;
            tick++;
            tick1 = true;

            count2++;
            count3++;
            count4++;
            count5++;
            count6++;
            if (count2 == 2)
            {
                tick2 = true;
                count2 = 0;
            }
            if (count3 == 3)
            {
                tick3 = true;
                count3 = 0;
            }
            if (count4 == 4)
            {
                tick4 = true;
                count4 = 0;
            }
            if (count5 == 5)
            {
                tick5 = true;
                count5 = 0;
            }
            if (count6 == 6)
            {
                tick6 = true;
                count6 = 0;
            }
        }
        else
        {
            tick1 = false;
            tick2 = false;
            tick3 = false;
            tick4 = false;
            tick5 = false;
            tick6 = false;
        }
    }
}


struct GameStartData
{
    Vector3 playerPosition;
    Quaternion playerRotation;

}