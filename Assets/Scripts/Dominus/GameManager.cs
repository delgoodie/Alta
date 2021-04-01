using UnityEngine;
class GameManager : MonoBehaviour
{
    public short tickSpan;
    public int tick;
    public bool tick1;
    public bool tick2;
    public bool tick3;
    public bool tick4;
    public bool tick5;
    public bool tick6;

    public static GameManager Instance;

    private short countT;
    private short count2;
    private short count3;
    private short count4;
    private short count5;
    private short count6;

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
        Debug.Log("GameManager Awake");
    }

    private void Update()
    {
        TickHandler();
        if (!PlayerManager.Instance.activePlayer && MarchManager.Instance.noMarch) PlayerManager.Instance.CreatePlayer();
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