using System.Collections.Generic;
using UnityEngine;

public class MarchManager : MonoBehaviour
{
    [HideInInspector]
    public bool noMarch;
    [HideInInspector]
    public static MarchManager Instance;
    public bool gizmosEnabled;
    private Queue<Marcher> MarchQueue;
    private ComputeShader Shader, CombineShader;
    private int Kernel;
    private int CombineKernel;
    private ComputeBuffer triangleBuffer, chipsBuffer, triCountBuffer, cTriBuffer, cVertBuffer;
    private const int maxTriangleCount = 21845;
    private Vector3[] verticies;
    private Vector3[] normals;
    private Color[] colors;
    private int[] triangles;
    private Vector2[] uv;
    private int[] triCount;
    private int[] mChips;


    private void Awake()
    {
        Instance = this;
        MarchQueue = new Queue<Marcher>();
        Shader = Resources.Load("Compute Shaders/March") as ComputeShader;
        Kernel = Shader.FindKernel("March");
        CombineShader = Resources.Load("Compute Shaders/CombineVerticies") as ComputeShader;
        CombineKernel = CombineShader.FindKernel("CombineVerticies");
        noMarch = false;
    }

    private void Update()
    {
        if (MarchQueue.Count > 0)
        {
            noMarch = false;
            March(MarchQueue.Dequeue());
        }
        else noMarch = true;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
            foreach (Marcher m in MarchQueue) Gizmos.DrawWireCube(m.transform.position + Vector3.one * 7.5f, Vector3.one * 15f);
    }

    public void RequestMarch(Marcher m)
    {
        if (!MarchQueue.Contains(m)) MarchQueue.Enqueue(m);
    }

    public void March(Marcher m)
    {
        if (m.closed)
        {
            mChips = new int[(m.size + 2) * (m.size + 2) * (m.size + 2)];
            for (int x = 0; x < m.size + 2; x++) for (int y = 0; y < m.size + 2; y++) for (int z = 0; z < m.size + 2; z++)
                    {
                        int mIndex = x * (m.size + 2) * (m.size + 2) + y * (m.size + 2) + z;
                        if (x == 0 || x == m.size + 1 || y == 0 || y == m.size + 1 || z == 0 || z == m.size + 1)
                            mChips[mIndex] = 0;
                        else
                        {
                            int index = (x - 1) * m.size * m.size + (y - 1) * m.size + (z - 1);
                            mChips[mIndex] = m.chips[index].iso << 24 | m.chips[index].type << 16 | m.chips[index].data;
                        }
                    }
        }
        else
        {
            mChips = new int[m.chips.Length];
            for (int i = 0; i < m.chips.Length; i++) mChips[i] = m.chips[i].iso << 24 | m.chips[i].type << 16 | m.chips[i].data;
        }

        #region Shader PREP
        chipsBuffer = new ComputeBuffer(mChips.Length, 1 * sizeof(int));
        triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        triCount = new int[1] { 0 };
        triangleBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 3 + sizeof(float) * 2 * 3 + sizeof(float) * 3 + sizeof(int), ComputeBufferType.Append);

        triangleBuffer.SetCounterValue(0);
        Shader.SetBuffer(Kernel, "triangles", triangleBuffer);

        chipsBuffer.SetData(mChips);
        Shader.SetBuffer(Kernel, "chips", chipsBuffer);


        Shader.SetInt("size", m.size + (m.closed ? 2 : 0));
        #endregion

        //Shader RUN
        Shader.Dispatch(Kernel, m.size + (m.closed ? 2 : 0), m.size + (m.closed ? 2 : 0), m.size + (m.closed ? 2 : 0));

        //Shader GET
        #region
        ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
        triCount = new int[1] { 0 };
        triCountBuffer.GetData(triCount);

        Triangle[] computeTriangles = new Triangle[triCount[0]];
        triangleBuffer.GetData(computeTriangles);

        triangleBuffer.Dispose();
        chipsBuffer.Dispose();
        triCountBuffer.Dispose();

        verticies = new Vector3[triCount[0] * 3];
        normals = new Vector3[triCount[0] * 3];
        colors = new Color[triCount[0] * 3];
        triangles = new int[triCount[0] * 3];
        uv = new Vector2[triCount[0] * 3];

        for (int t = 0, i = 0; t < computeTriangles.Length; t++, i += 3)
        {
            verticies[i] = computeTriangles[t].vertexA * m.scale + m.offset;
            normals[i] = computeTriangles[t].normal;
            colors[i] = Chips.Colors[computeTriangles[t].type];
            triangles[i] = i;
            uv[i] = computeTriangles[t].uvA;
            verticies[i + 1] = computeTriangles[t].vertexB * m.scale + m.offset;
            normals[i + 1] = computeTriangles[t].normal;
            colors[i + 1] = Chips.Colors[computeTriangles[t].type];
            triangles[i + 1] = i + 1;
            uv[i + 1] = computeTriangles[t].uvB;
            verticies[i + 2] = computeTriangles[t].vertexC * m.scale + m.offset;
            normals[i + 2] = computeTriangles[t].normal;
            colors[i + 2] = Chips.Colors[computeTriangles[t].type];
            triangles[i + 2] = i + 2;
            uv[i + 2] = computeTriangles[t].uvC;
        }
        #endregion


        if (verticies.Length > 0)
        {
            m.mesh.Clear();
            m.mesh.vertices = verticies;
            m.mesh.triangles = triangles;
            m.mesh.colors = colors;
            // m.mesh.normals = normals;
            m.mesh.RecalculateNormals();
            m.mesh.uv = uv;
            // TODO: Manually calculate bounds in Shader
            // TODO: Generate vertex sharing during march and correct normals
            m.mesh.RecalculateBounds();
            m.mesh.MarkModified();
            if (triangles.Length > 10 * 3) m.meshCollider.sharedMesh = m.mesh;

            m.updated = true;
        }
    }

    private void CombineVerticies()
    {
        cTriBuffer = new ComputeBuffer(triangles.Length, sizeof(int));
        cVertBuffer = new ComputeBuffer(verticies.Length, sizeof(float) * 3);

        cTriBuffer.SetData(triangles);
        CombineShader.SetBuffer(CombineKernel, "triangles", cTriBuffer);

        cVertBuffer.SetData(verticies);
        CombineShader.SetBuffer(CombineKernel, "verticies", cVertBuffer);

        CombineShader.Dispatch(Kernel, 1, 1, 1);

        cTriBuffer.GetData(triangles);
        cVertBuffer.GetData(verticies);

        cTriBuffer.Dispose();
        cVertBuffer.Dispose();
    }

    /*
        public RaycastHit Raycast(Vector3 pos, Vector3 dir, float max)
        {
            RaycastHit closestValidHit = new RaycastHit();
            RaycastHit[] hits = Physics.RaycastAll(pos, dir, max);
            foreach (RaycastHit hit in hits)
                if (hit.transform != transform && hit.transform.IsChildOf(transform) && (closestValidHit.collider == null || closestValidHit.distance > hit.distance)) closestValidHit = hit;
            return closestValidHit;
        }

        public RaycastHit Spherecast(Vector3 pos, Vector3 dir, float radius, float max)
        {
            RaycastHit closestValidHit = new RaycastHit();
            RaycastHit[] hits = Physics.SphereCastAll(pos, radius, dir, max);
            foreach (RaycastHit hit in hits)
                if (hit.transform != transform && hit.transform.IsChildOf(transform) && (closestValidHit.collider == null || closestValidHit.distance > hit.distance)) closestValidHit = hit;
            return closestValidHit;
        }

        public RaycastHit Boxcast(Vector3 pos, Vector3 dir, Vector3 halfExtends, Quaternion rotation, float max)
        {
            RaycastHit closestValidHit = new RaycastHit();
            RaycastHit[] hits = Physics.BoxCastAll(pos, halfExtends, dir, rotation, max);
            foreach (RaycastHit hit in hits)
                if (hit.transform != transform && hit.transform.IsChildOf(transform) && (closestValidHit.collider == null || closestValidHit.distance > hit.distance)) closestValidHit = hit;
            return closestValidHit;
        }
        */
}

struct Triangle
{
    public Vector3 vertexA;
    public Vector3 vertexB;
    public Vector3 vertexC;
    public Vector2 uvA;
    public Vector2 uvB;
    public Vector2 uvC;
    public Vector3 normal;
    public int type;
}