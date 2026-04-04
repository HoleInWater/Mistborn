using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Creates a lightweight visual ghost of a target's mesh.
/// Used for Oraculum Shadows, Gold Shadows, and Electrum Shadows.
/// </summary>
public class GhostRenderer : MonoBehaviour
{
    private GameObject ghostObject;
    private List<MeshRenderer> ghostRenderers = new List<MeshRenderer>();
    private List<Mesh> bakedMeshes = new List<Mesh>(); // tracked so we can Destroy them
    private Material ghostMaterial;

    /// <summary>
    /// Initializes or updates the ghost representation of a target.
    /// </summary>
    public void SetupGhost(GameObject target, Color color, float alpha)
    {
        if (ghostObject == null)
        {
            ghostObject = new GameObject($"{target.name}_Ghost");
            ghostObject.transform.SetParent(null); // Keep it top-level or child of a pool
        }

        // Clear old renderers and baked meshes from any previous call
        foreach (var r in ghostRenderers) if (r != null) Destroy(r.gameObject);
        ghostRenderers.Clear();
        foreach (var m in bakedMeshes) if (m != null) Destroy(m);
        bakedMeshes.Clear();

        // Copy MeshFilters/Renderers
        MeshFilter[] filters = target.GetComponentsInChildren<MeshFilter>();
        foreach (var filter in filters)
        {
            GameObject part = new GameObject(filter.gameObject.name);
            part.transform.SetParent(ghostObject.transform);
            part.transform.localPosition = filter.transform.localPosition;
            part.transform.localRotation = filter.transform.localRotation;
            part.transform.localScale = filter.transform.localScale;

            MeshFilter mf = part.AddComponent<MeshFilter>();
            mf.sharedMesh = filter.sharedMesh;

            MeshRenderer mr = part.AddComponent<MeshRenderer>();
            MeshRenderer sourceMr = filter.GetComponent<MeshRenderer>();
            if (sourceMr != null) mr.sharedMaterials = sourceMr.sharedMaterials;
            
            // Set transparency
            foreach (Material m in mr.materials)
            {
                SetupTransparentMaterial(m, color, alpha);
            }
            ghostRenderers.Add(mr);
        }

        // Copy SkinnedMeshRenderers (baked to mesh for static shadow)
        SkinnedMeshRenderer[] skinned = target.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var smr in skinned)
        {
            GameObject part = new GameObject(smr.gameObject.name);
            part.transform.SetParent(ghostObject.transform);
            
            MeshFilter mf = part.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh();
            smr.BakeMesh(mesh);
            mf.sharedMesh = mesh;
            bakedMeshes.Add(mesh); // track for explicit cleanup

            MeshRenderer mr = part.AddComponent<MeshRenderer>();
            mr.sharedMaterials = smr.sharedMaterials;

            foreach (Material m in mr.materials)
            {
                SetupTransparentMaterial(m, color, alpha);
            }
            ghostRenderers.Add(mr);
        }
    }

    public void UpdateTransform(Vector3 position, Quaternion rotation)
    {
        if (ghostObject != null)
        {
            ghostObject.transform.position = position;
            ghostObject.transform.rotation = rotation;
        }
    }

    public void SetVisibility(bool visible)
    {
        if (ghostObject != null) ghostObject.SetActive(visible);
    }

    private void SetupTransparentMaterial(Material m, Color color, float alpha)
    {
        m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetInt("_ZWrite", 0);
        m.DisableKeyword("_ALPHATEST_ON");
        m.EnableKeyword("_ALPHABLEND_ON");
        m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        m.renderQueue = 3000;
        m.color = new Color(color.r, color.g, color.b, alpha);
    }

    void OnDestroy()
    {
        foreach (var m in bakedMeshes) if (m != null) Destroy(m);
        bakedMeshes.Clear();
        if (ghostObject != null) Destroy(ghostObject);
    }
}
