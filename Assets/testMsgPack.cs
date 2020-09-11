using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using MsgPack;
using MsgPack.Serialization;
//using MsgPack.Serialization.UnpackHelpers;
using System.Linq;
using System.Text;
using mmtf;

#if UNITY_EDITOR
using UnityEditor;
#endif

using MarchingCubesProject;

public class testMsgPack : MonoBehaviour {

    public string pdbid;
    public bool pdb_method;
    public bool mmtf_method;

    public GameObject Sphere;
    public bool do_instance;
    public bool build_volume;
    public bool marching_cube_mesh;
    public int nAtoms;
    public Texture3D _volumeTexture;
    public float dpad;
    public float size_max;
    public float pixel_angstrom;
    public float box_scaling;
    public float Scale = 1;

    public GameObject CubeObject;
    public Mesh CubeMesh;

    public Material m_material;
    public MARCHING_MODE mode = MARCHING_MODE.CUBES;
    public int seed = 0;

    List<GameObject> meshes = new List<GameObject>();

    private List<Vector4> coords;
    private Bounds bounds;
    private float[] voxelCPUBuffer;

    int decodeBigEndian(byte[] input_byte, int offset) {
        return BitConverter.ToInt32(new byte[]{
            input_byte[offset + 3],
            input_byte[offset + 2],
            input_byte[offset + 1],
            input_byte[offset]
        }, 0);
    }
    
    // Use this for initialization
    void Start () {
        coords = new List<Vector4>();
        if (mmtf_method) MmtfMethod();
        if (pdb_method) PdbMethod();
        if (build_volume)
        {
            CreateDistanceField();
            if (!marching_cube_mesh)
            {
                CubeObject.SetActive(true);
                CubeObject.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_VolumeTex", _volumeTexture);
            }
            else {
                CubeObject.SetActive(false);
                MarchingCubeMesh();
            }
        }
    }

    void MmtfMethod() {
        float t1 = Time.realtimeSinceStartup;
        FileStream stream = null;
        stream = File.Open(Application.dataPath + "/../Data/proteins/"+ pdbid+".mmtf", FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        //StreamReader stream = new StreamReader();
        MessagePackObjectDictionary packobject = MessagePackSerializer.UnpackMessagePackObject(stream).AsDictionary();
        /*foreach (KeyValuePair<MessagePackObject, MessagePackObject> test in packobject) {
            //if byte array or isRaw-> try to unpack ?
            if (test.Value.IsRaw) {
                Debug.Log(test.Key);
                Debug.Log(test.Value);
                Debug.Log(test.Value.UnderlyingType);
                byte[] data = test.Value.AsBinary();//signed ?
                //Debug.Log(test.Value.AsStringUtf8());//doesnt work with actual binary data
                var signed = new sbyte[data.Length];
                Buffer.BlockCopy(data, 0, signed, 0, data.Length);
                //how to decode the data
                Debug.Log("test");
                PrintByteArray(data);
                Debug.Log("signed");
                PrintByteArray(signed);
                var result = data.Select(Convert.ToInt32);
                foreach (var r in result)
                    Debug.Log(r);
            }
        }*/
        byte[] datax = packobject["xCoordList"].AsBinary();
        int codecs = decodeBigEndian(datax, 0);//10
        int length = decodeBigEndian(datax, 4);//327
        int param = decodeBigEndian(datax, 8);//1000
        float[] xCoordList = DeltaRecursiveFloat.decode(datax.Skip(12).ToArray(), param);
        byte[] datay = packobject["yCoordList"].AsBinary();
        codecs = decodeBigEndian(datay, 0);//10
        length = decodeBigEndian(datay, 4);//327
        param = decodeBigEndian(datay, 8);//1000
        float[] yCoordList = DeltaRecursiveFloat.decode(datay.Skip(12).ToArray(), param);
        byte[] dataz = packobject["zCoordList"].AsBinary();
        codecs = decodeBigEndian(dataz, 0);//10
        length = decodeBigEndian(dataz, 4);//327
        param = decodeBigEndian(dataz, 8);//1000
        float[] zCoordList = DeltaRecursiveFloat.decode(dataz.Skip(12).ToArray(), param);
        Debug.Log(Time.realtimeSinceStartup - t1);
        Vector3 center = Vector3.zero;

        for (int i = 0; i < zCoordList.Length; i++)
        {
            coords.Add(new Vector4(xCoordList[i], yCoordList[i], zCoordList[i],1.4f));
            center += new Vector3(xCoordList[i], yCoordList[i], zCoordList[i]);
            if (do_instance)
            {
                var inst = Instantiate(Sphere, new Vector3(xCoordList[i], yCoordList[i], zCoordList[i]), Quaternion.identity);
            }
        }
        center /= (float)zCoordList.Length;
        AtomHelper.OffsetSpheres(ref coords, center);
        nAtoms = zCoordList.Length;
    }

    void PdbMethod() {
        float t2 = Time.realtimeSinceStartup;
        var atoms_data = PdbLoader.LoadAtomSpheres(pdbid);
        Debug.Log(Time.realtimeSinceStartup - t2);
        for (int i = 0; i < atoms_data.Count(); i++)
        {
            if (do_instance)
            {
                var inst = Instantiate(Sphere, new Vector3(atoms_data[i].x, atoms_data[i].y, atoms_data[i].z), Quaternion.identity);
            }
        }
    }

    // Update is called once per frame
    void Update () {
		
	}

    private void MarchingCubeMesh() {
        Marching marching = new MarchingCubes();
        List<Vector3> verts = new List<Vector3>();
        List<int> indices = new List<int>();
        //The mesh produced is not optimal. There is one vert for each index.
        //Would need to weld vertices for better quality mesh.
        marching.Surface = 0.278f;
        marching.Generate(voxelCPUBuffer, 64, 64, 64, verts, indices);

        //A mesh in unity can only be made up of 65000 verts.
        //Need to split the verts between multiple meshes.

        int maxVertsPerMesh = 30000; //must be divisible by 3, ie 3 verts == 1 triangle
        int numMeshes = verts.Count / maxVertsPerMesh + 1;

        for (int i = 0; i < numMeshes; i++)
        {

            List<Vector3> splitVerts = new List<Vector3>();
            List<int> splitIndices = new List<int>();

            for (int j = 0; j < maxVertsPerMesh; j++)
            {
                int idx = i * maxVertsPerMesh + j;

                if (idx < verts.Count)
                {
                    splitVerts.Add(verts[idx]);
                    splitIndices.Add(j);
                }
            }

            if (splitVerts.Count == 0) continue;

            Mesh mesh = new Mesh();
            mesh.SetVertices(splitVerts);
            mesh.SetTriangles(splitIndices, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            GameObject go = new GameObject("Mesh");
            go.transform.parent = transform;
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.GetComponent<Renderer>().material = m_material;
            go.GetComponent<MeshFilter>().mesh = mesh;
            go.transform.localPosition = new Vector3(-64 / 2, -64 / 2, -64 / 2);
            meshes.Add(go);
        }
    }

    private void CreateDistanceField()
    {
        if (coords.Count() == 0) return;
        var size = 64;
        var pdbName = pdbid;// "MA_matrix_G1";
        string path = "Assets/Resources/3DTextures/" + pdbName + ".asset";

        //only work with unityEDitor
        Texture3D tmp = null;
#if UNITY_EDITOR
        tmp = (Texture3D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture3D));
#endif
        if (tmp)
        {
            _volumeTexture = tmp;
        }
        else
        {
            RenderTexture _distanceFieldRT;
            //cubeMap?
            _distanceFieldRT = new RenderTexture(size, size, 0, RenderTextureFormat.R8);
            _distanceFieldRT.volumeDepth = size;
            ///_distanceFieldRT.isVolume = true;
            _distanceFieldRT.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
            _distanceFieldRT.isPowerOfTwo = true;
            _distanceFieldRT.enableRandomWrite = true;
            _distanceFieldRT.filterMode = FilterMode.Trilinear;
            _distanceFieldRT.name = pdbName;
            _distanceFieldRT.hideFlags = HideFlags.HideAndDontSave;
            _distanceFieldRT.autoGenerateMips = true;
            _distanceFieldRT.useMipMap = true;
            _distanceFieldRT.Create();

            bounds = AtomHelper.ComputeBounds(coords);
            size_max = Mathf.Max(Mathf.Max(bounds.extents.x, bounds.extents.y), bounds.extents.z) + dpad;
            Vector3 bmax = new Vector3(size_max, size_max, size_max);
            bounds.SetMinMax(-bmax, bmax);
            Debug.Log(bounds.size);
            Debug.Log(bounds.extents);
            //the main scale/ratio pixel/angstrom. will help scale up the box
            pixel_angstrom = bounds.size.x / size;
            box_scaling = size_max;
            CubeObject.transform.localScale = new Vector3(box_scaling, box_scaling, box_scaling);
            //GetComponent<Camera>().orthographicSize = box_scaling;
            //Camera.main.orthographicSize = box_scaling;
            //make it cubic ?
            //bounds.min = new Vector3(-250, -250, -250);
            //bounds.max = new Vector3(250, 250, 250);
            Debug.Log(bounds.min);
            Debug.Log(bounds.max);
            Debug.Log(bounds.center);
            var atomSphereGPUBuffer = new ComputeBuffer(coords.Count, sizeof(float) * 4, ComputeBufferType.Default);
            atomSphereGPUBuffer.SetData(coords.ToArray());
           
            Graphics.SetRenderTarget(_distanceFieldRT);
            GL.Clear(true, true, new Color(0, 0, 0));

            var createDistanceFieldCS = Resources.Load("Shaders/CreateDistanceField") as ComputeShader;
            createDistanceFieldCS.SetInt("_GridSize", size);
            createDistanceFieldCS.SetInt("_NumAtoms", coords.Count);
            createDistanceFieldCS.SetFloat("_Volumesize", bounds.size.x);
            createDistanceFieldCS.SetVector("_bbox_min", bounds.min);
            createDistanceFieldCS.SetVector("_bbox_max", bounds.max);
            createDistanceFieldCS.SetBuffer(0, "_SpherePositions", atomSphereGPUBuffer);
            createDistanceFieldCS.SetTexture(0, "_VolumeTexture", _distanceFieldRT);
            createDistanceFieldCS.Dispatch(0, Mathf.CeilToInt(size / 10.0f), Mathf.CeilToInt(size / 10.0f), Mathf.CeilToInt(size / 10.0f));
            Debug.Log(bounds.min);
            Debug.Log(bounds.max);
            atomSphereGPUBuffer.Release();
            Debug.Log(Mathf.CeilToInt(size / 10.0f));
            //****
            var flatSize = size * size * size;
            var voxelGPUBuffer = new ComputeBuffer(flatSize, sizeof(float));

            var readVoxelsCS = Resources.Load("Shaders/ReadVoxels") as ComputeShader;
            readVoxelsCS.SetInt("_VolumeSize", size);
            readVoxelsCS.SetBuffer(0, "_VoxelBuffer", voxelGPUBuffer);
            readVoxelsCS.SetTexture(0, "_VolumeTexture", _distanceFieldRT);
            readVoxelsCS.Dispatch(0, size, size, size);

            voxelCPUBuffer = new float[flatSize];
            voxelGPUBuffer.GetData(voxelCPUBuffer);

            var volumeColors = new Color[flatSize];
            for (int i = 0; i < flatSize; i++)
            {
                volumeColors[i] = new Color(0, 0, 0, voxelCPUBuffer[i]);
            }
            //Debug.Log(voxelCPUBuffer[128]);
            var texture3D = new Texture3D(size, size, size, TextureFormat.Alpha8, true);
            texture3D.SetPixels(volumeColors);
            texture3D.wrapMode = TextureWrapMode.Clamp;
            texture3D.anisoLevel = 0;
            texture3D.Apply();

#if UNITY_EDITOR
            AssetDatabase.CreateAsset(texture3D, path);
            AssetDatabase.SaveAssets();

            // Print the path of the created asset
            Debug.Log(AssetDatabase.GetAssetPath(texture3D));
#endif
            voxelGPUBuffer.Release();
            Graphics.SetRenderTarget(null);
            _distanceFieldRT.Release();
            DestroyImmediate(_distanceFieldRT);

            _volumeTexture = texture3D;
        }
    }
}
