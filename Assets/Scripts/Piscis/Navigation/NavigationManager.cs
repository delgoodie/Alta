using System.Collections.Generic;
using UnityEngine;

public class NavigationManager : MonoBehaviour
{
    public static NavigationManager Instance;
    public bool gizmosEnabled;
    public int maxIter;
    private Queue<Navigator> NavQueue;

    private void Awake()
    {
        Instance = this;
        NavQueue = new Queue<Navigator>();
    }

    private void Update()
    {
        if (NavQueue.Count > 0) Navigate(NavQueue.Dequeue());
    }

    private void OnDrawGizmos()
    {
        foreach (Navigator n in NavQueue) Gizmos.DrawCube(n.transform.position, Vector3.one);
    }

    public void RequestNavigation(Navigator n) => NavQueue.Enqueue(n);

    public void Navigate(Navigator n)
    {
        n.pathStack.Clear();
        n.headPath.Clear();
        n.used.Clear();

        Vector3Int head = new Vector3Int((int)n.transform.position.x, (int)n.transform.position.y, (int)n.transform.position.z);
        n.headPath.Add(head);
        int iterations = 0;
        while ((head - n.target).sqrMagnitude > 2f * 2f)
        {
            Vector3Int[] moves = new Vector3Int[6]{
                Vector3Int.up,
                Vector3Int.down,
                Vector3Int.right,
                Vector3Int.left,
                Vector3Int.forward,
                Vector3Int.back
            };
            Vector3 targetDirection = n.target - head;
            bool sorted = false;
            Vector3Int temp;
            while (!sorted)
            {
                sorted = true;
                for (int i = 1; i < 6; i++)
                    if (Vector3.Angle(targetDirection, moves[i]) < Vector3.Angle(targetDirection, moves[i - 1]))
                    {
                        temp = moves[i];
                        moves[i] = moves[i - 1];
                        moves[i - 1] = temp;
                        sorted = false;
                    }
            }

            bool success = false;
            for (int i = 0; i < 6 && !success; i++)
                if (!n.headPath.Contains(head + moves[i]) && checkBox(n, head + moves[i], ref n.headPath, ref n.used))
                {
                    head += moves[i];
                    success = true;
                }
            if (!success || iterations > maxIter)
            {
                //Debug.Log("Failed to find path");
                n.stack = n.pathStack;
                return;
            }
        }

        Vector3Int curHead = n.headPath[0];
        int curIndex = 0;
        iterations = 0;
        while (iterations < 100)
        {
            iterations++;
            bool end = false;
            for (int i = curIndex + 1; i < n.headPath.Count; i++)
                if (Physics.Linecast(n.headPath[curIndex], n.headPath[i], ~(1 << LayerMask.NameToLayer("Monster"))))
                {
                    n.pathStack.Add(n.headPath[i - 1]);
                    curIndex = i - 1;
                    break;
                }
                else if (i == n.headPath.Count - 1) end = true;
            if (end)
            {
                n.pathStack.Add(n.headPath[n.headPath.Count - 1]);
                break;
            }
        }
        n.stack = n.pathStack;
        return;
    }

    private bool checkBox(Navigator n, Vector3Int head, ref List<Vector3Int> headPath, ref List<Vector3Int> used)
    {
        bool valid = true;
        for (int x = head.x - n.box.x / 2; x <= head.x + n.box.x / 2; x++)
        {
            for (int y = head.y - n.box.y / 2; y <= head.y + n.box.y / 2; y++)
            {
                for (int z = head.z - n.box.z / 2; z <= head.z + n.box.z / 2; z++)
                {
                    if (!used.Contains(new Vector3Int(x, y, z)) && Chips.PathBlocking[ChunkManager.Instance.WorldToChip(new Vector3(x, y, z)).type]) valid = false;
                    if (!valid) break;
                }
                if (!valid) break;
            }
            if (!valid) break;
        }
        if (valid)
        {
            for (int x = head.x - n.box.x / 2; x <= head.x + n.box.x / 2; x++)
                for (int y = head.y - n.box.y / 2; y <= head.y + n.box.y / 2; y++)
                    for (int z = head.z - n.box.z / 2; z <= head.z + n.box.z / 2; z++)
                        if (!used.Contains(new Vector3Int(x, y, z))) used.Add(new Vector3Int(x, y, z));
            headPath.Add(head);
        }
        return valid;
    }
}