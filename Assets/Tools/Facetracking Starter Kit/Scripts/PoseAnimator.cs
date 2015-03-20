using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.Collections;

public class DeltaInformation
{
    public int VertexIndex;
    public Vector3 VertexDelta;
    public Vector3 NormalDelta;
}

public class PoseInformation
{
    public DeltaInformation[] DeltaInformations;
}



public class PoseAnimator : MonoBehaviour
{
    public Mesh[] Poses;

    private PoseInformation[] _poseInformations;
    private Vector3[] _baseVertices;
    private Vector3[] _baseNormals;
    private float[] _weights;
    private Mesh _animatedMesh;
    private bool _isInitialized;
    private bool _isDirty;

    void Awake()
    {
        _poseInformations = new PoseInformation[Poses.Length];
        _weights = new float[Poses.Length];
        StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        Debug.Log("Caching pose animations for the mesh... this might take a few seconds.");
        yield return 0;

        try
        {
            var baseMesh = GetComponent<MeshFilter>().sharedMesh;
            _animatedMesh = (Mesh) Mesh.Instantiate(baseMesh);
            GetComponent<MeshFilter>().sharedMesh = _animatedMesh;

            _baseVertices = (Vector3[]) baseMesh.vertices.Clone();
            _baseNormals = (Vector3[]) baseMesh.normals.Clone();
        }
        catch (Exception)
        {
            Debug.Log("No mesh found in the PoseAnimator. Aborting.");
            enabled = false;
            yield break;
        }

        CachePoseInformation();
        _isInitialized = true;

        Debug.Log("Pose animations cached.");
    }


    private void CachePoseInformation()
    {
        for (int i = 0; i < Poses.Length; i++)
        {
            _poseInformations[i] = new PoseInformation();
            InitializePose((Vector3[])Poses[i].vertices.Clone(), (Vector3[])Poses[i].normals.Clone(), _poseInformations[i]);
        }
    }

    private void InitializePose(Vector3[] poseVertices, Vector3[] poseNormals, PoseInformation poseInformation)
    {
        var deltaInfo = new List<DeltaInformation>();
        for (int i = 0; i < poseVertices.Length; i++)
        {
            var vertexDelta = poseVertices[i] - _baseVertices[i];
            if (!Mathf.Approximately(0f, vertexDelta.sqrMagnitude))
            {
                deltaInfo.Add(
                    new DeltaInformation
                    {
                        VertexIndex = i,
                        VertexDelta = vertexDelta,
                        NormalDelta = poseNormals[i] - _baseNormals[i],
                    });
            }
        }

        poseInformation.DeltaInformations = deltaInfo.ToArray();
    }

    void Update()
    {
        if (!_isInitialized)
            return;

        if (_isDirty)
        {
            AnimateMesh();
            _isDirty = false;
        }
    }

    private void AnimateMesh()
    {
        var vertices = (Vector3[])_baseVertices.Clone();
        var normals = (Vector3[])_baseNormals.Clone();
        for (int i = 0; i < _poseInformations.Length; i++)
        {
            for (int j = 0; j < _poseInformations[i].DeltaInformations.Length; j++)
            {
                vertices[_poseInformations[i].DeltaInformations[j].VertexIndex] += _poseInformations[i].DeltaInformations[j].VertexDelta * _weights[i];
                normals[_poseInformations[i].DeltaInformations[j].VertexIndex] += _poseInformations[i].DeltaInformations[j].NormalDelta * _weights[i];
            }
        }
        _animatedMesh.vertices = vertices;
        _animatedMesh.normals = normals;
    }

    public void SetWeight(int pose, float weight)
    {
        if (pose >= Poses.Length || pose < 0)
        {
            Debug.Log("Out of range pose in SetWeight. Pose: " + pose);
            return;
        }

        if (Mathf.Approximately(_weights[pose], weight))
            return;

        _weights[pose] = weight;
        _isDirty = true;
    }
}
