using UnityEngine;

public class AABBMono : MonoBehaviour
{
    public bool Enable;
    private MeshRenderer m_Renderer;
    private Camera m_Camera;
    private Matrix4x4 m_LocalToWorld;
    private Matrix4x4 m_WorldToCamMatrix;
    private Matrix4x4 m_ProjMatrix;

    private float m_Time;
    
    private void Awake()
    {
        m_Camera = Camera.main;
        m_Renderer = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        m_Time += Time.deltaTime;
        if (m_Time < 1f && Enable) return;
        m_Time = 0f;
        m_LocalToWorld = transform.localToWorldMatrix;
        m_WorldToCamMatrix = m_Camera.worldToCameraMatrix;
        m_ProjMatrix = GL.GetGPUProjectionMatrix(m_Camera.projectionMatrix, false);
        var center = m_Renderer.bounds.center;
        var extent = m_Renderer.bounds.extents;
        var localCenter = m_Renderer.localBounds.center;
        var localExtent = m_Renderer.localBounds.extents;
        Debug.Log($"{name} Center: {center.x}, {center.y}, {center.z}");
        Debug.Log($"{name} Extents: {extent.x}, {extent.y}, {extent.z}");
        Debug.Log("=====================");
        Debug.Log($"{name} Local Center: {localCenter.x}, {localCenter.y}, {localCenter.z}");
        Debug.Log($"{name} Local Extents: {localExtent.x}, {localExtent.y}, {localExtent.z}");
        Debug.Log("=====================");
        var worldCenter = m_LocalToWorld.MultiplyPoint(localCenter);
        var right = Abs(m_LocalToWorld.GetColumn(0) * localExtent.x);
        var up = Abs(m_LocalToWorld.GetColumn(1) * localExtent.y);
        var forward = Abs(m_LocalToWorld.GetColumn(2) * localExtent.z);
        var worldExtent = right + up + forward;
        Debug.Log($"{name} Center Local To World: {worldCenter.x}, {worldCenter.y}, {worldCenter.z}");
        Debug.Log($"{name} Extents Local To World: {worldExtent.x}, {worldExtent.y}, {worldExtent.z}");
        Debug.Log("=====================");
    }

    private Vector4 Abs(Vector4 i)
    {
        return new Vector4(Mathf.Abs(i.x), Mathf.Abs(i.y), Mathf.Abs(i.z), Mathf.Abs(i.w));
    }
}