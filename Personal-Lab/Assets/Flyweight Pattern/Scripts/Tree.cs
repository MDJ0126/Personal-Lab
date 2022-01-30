using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    private static MeshRenderer _treeMeshRenderer = null;
    private static MeshRenderer TreeMeshRenderer
    {
        get
        {
            if (_treeMeshRenderer == null)
                _treeMeshRenderer = Resources.Load<MeshRenderer>("Low Poly Tree/TREE");
            return _treeMeshRenderer;
        }
    }
    private static Material[] _materials = null;

    private static Material GetMaterial()
    {
        if (_materials == null)
        {
            _materials = Resources.LoadAll<Material>("Low Poly Tree/Materials");
        }
        return _materials[Random.Range(0, _materials.Length)];
    }

    #region Inspector

    public GameObject mesh;

    #endregion

    private void Awake()
    {
        MeshRenderer meshRenderer = mesh.AddComponent<MeshRenderer>();
        meshRenderer.material = GetMaterial();
        MeshFilter meshFilter = mesh.AddComponent<MeshFilter>();
        meshFilter.mesh = TreeMeshRenderer.GetComponent<MeshFilter>().sharedMesh;

        // Set Size
        this.transform.localRotation = Quaternion.Euler(new Vector3(-90f, 0f, 0f));
        this.transform.localScale = new Vector3(10f, 10f, 10f);
    }
}