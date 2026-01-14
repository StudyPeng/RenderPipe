using UnityEngine;

public class IndirectRender : MonoBehaviour
{
    public int InstanceCount;
    public Mesh Mesh;
    public ComputeShader FrustumCullingCS;

    private Matrix4x4[] m_objectsTRS;

    private void Start()
    {
        m_objectsTRS = new Matrix4x4[InstanceCount];
        CreateMatrix();
    }

    void Update()
    {
    }

    private void CreateMatrix()
    {
        for (int i = 0; i < InstanceCount; i++)
        {
            Vector3 t = new Vector3(Random.Range(-50, 50), Random.Range(-50, 50), Random.Range(-50, 50));
            Quaternion r = new Quaternion(Random.Range(-180, 180), Random.Range(-180, 180), Random.Range(-180, 180), Random.Range(-180, 180));
            Vector3 s = new Vector3(Random.Range(0.2f, 1.4f), Random.Range(0.2f, 1.4f), Random.Range(0.2f, 1.4f));
            Matrix4x4 m = Matrix4x4.TRS(t, r, s);
            m_objectsTRS[i] = m;
        }
    }
}