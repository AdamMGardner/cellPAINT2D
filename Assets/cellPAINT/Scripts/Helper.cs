using System;
using System.IO;
using UnityEngine;

using SimpleJSON;
using System.Collections;
using System.Collections.Generic;

using Accord;
using Accord.Math;
using Accord.Math.Geometry;
using Accord.MachineLearning;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Statistics;
using Accord.Statistics.Distributions.Fitting;
using Accord.Statistics.Distributions.DensityKernels;
using Accord.Math.Decompositions;

using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3; // Note: this might be necessary often
using Vector4 = UnityEngine.Vector4;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Plane = UnityEngine.Plane;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class Helper
{
    public static Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f)
    {

        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference

        Sprite NewSprite = null;
        Texture2D SpriteTexture = LoadTexture(FilePath);
        if (SpriteTexture)
            NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), PixelsPerUnit);
        return NewSprite;
    }

    public static Texture2D LoadTexture(string FilePath)
    {

        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails

        Texture2D Tex2D;
        byte[] FileData;

        if (File.Exists(FilePath))
        {
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
            if (Tex2D.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
                return Tex2D;                 // If data = readable -> return texture
        }
        return null;                     // Return null if load failed
    }

    public static Matrix4x4 quaternion_outer(Quaternion q1, Quaternion q2)
    {
        Matrix4x4 m = Matrix4x4.identity;
        for (int i = 0; i < 4; i++)
        {
            m.SetRow(i, new Vector4(q1[i] * q2[0], q1[i] * q2[1], q1[i] * q2[2], q1[i] * q2[3]));
        }
        return m;
    }

    public static Matrix4x4 quaternion_matrix(Quaternion quat)
    {
        float _EPS = 8.8817841970012523e-16f;
        float n = Quaternion.Dot(quat, quat);
        Matrix4x4 m = Matrix4x4.identity;
        if (n > _EPS)
        {
            quat = new Quaternion(quat[0] * Mathf.Sqrt(2.0f / n), quat[1] * Mathf.Sqrt(2.0f / n), quat[2] * Mathf.Sqrt(2.0f / n), quat[3] * Mathf.Sqrt(2.0f / n));
            Matrix4x4 q = quaternion_outer(quat, quat);
            m.SetRow(0, new Vector4(1.0f - q[2, 2] - q[3, 3], q[1, 2] - q[3, 0], q[1, 3] + q[2, 0], 0.0f));
            m.SetRow(1, new Vector4(q[1, 2] + q[3, 0], 1.0f - q[1, 1] - q[3, 3], q[2, 3] - q[1, 0], 0.0f));
            m.SetRow(2, new Vector4(q[1, 3] - q[2, 0], q[2, 3] + q[1, 0], 1.0f - q[1, 1] - q[2, 2], 0.0f));
            m.SetRow(3, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
        }
        return m;
    }

    public static Matrix4x4 quatToMatrix(Quaternion q)
    {
        float sqw = q.w * q.w;
        float sqx = q.x * q.x;
        float sqy = q.y * q.y;
        float sqz = q.z * q.z;

        // invs (inverse square length) is only required if quaternion is not already normalised
        float invs = 1 / (sqx + sqy + sqz + sqw);
        Matrix4x4 m = Matrix4x4.identity;
        m[0, 0] = (sqx - sqy - sqz + sqw) * invs; 
        m[1, 1] = (-sqx + sqy - sqz + sqw) * invs;
        m[2, 2] = (-sqx - sqy + sqz + sqw) * invs;

        float tmp1 = q.x * q.y;
        float tmp2 = q.z * q.w;
        m[1, 0] = 2.0f * (tmp1 + tmp2) * invs;
        m[0, 1] = 2.0f * (tmp1 - tmp2) * invs;

        tmp1 = q.x * q.z;
        tmp2 = q.y * q.w;
        m[2, 0] = 2.0f * (tmp1 - tmp2) * invs;
        m[0, 2] = 2.0f * (tmp1 + tmp2) * invs;
        tmp1 = q.y * q.z;
        tmp2 = q.x * q.w;
        m[2, 1] = 2.0f * (tmp1 + tmp2) * invs;
        m[1, 2] = 2.0f * (tmp1 - tmp2) * invs;

        //convertion to actual matrix
        Matrix4x4 m2 = Matrix4x4.identity;
        m2[0, 0] = -m[0, 1];
        m2[1, 0] = m[2, 1];
        m2[2, 0] = m[1, 1];

        m2[0, 1] = -m[0, 0];
        m2[1, 1] = m[2, 0];
        m2[2, 1] = m[1, 0];

        m2[0, 2] = m[0, 2];
        m2[1, 2] = -m[2, 2];
        m2[2, 2] = -m[1, 2];

        return m2;
    }

    public static Vector3 euler_from_matrix(Matrix4x4 M)
    {
        float _EPS = 8.8817841970012523e-16f;
        Vector4 _NEXT_AXIS = new Vector4(1, 2, 0, 1);
        Vector4 _AXES2TUPLE = new Vector4(0, 0, 0, 0);//sxyz
        int firstaxis = (int)_AXES2TUPLE[0];
        int parity = (int)_AXES2TUPLE[1];
        int repetition = (int)_AXES2TUPLE[2];
        int frame = (int)_AXES2TUPLE[3];

        int i = firstaxis;
        int j = (int)_NEXT_AXIS[i + parity];
        int k = (int)_NEXT_AXIS[i - parity + 1];
        float ax = 0.0f;
        float ay = 0.0f;
        float az = 0.0f;
        if (repetition == 1)
        {
            float sy = Mathf.Sqrt(M[i, j] * M[i, j] + M[i, k] * M[i, k]);
            if (sy > _EPS)
            {
                ax = Mathf.Atan2(M[i, j], M[i, k]);
                ay = Mathf.Atan2(sy, M[i, i]);
                az = Mathf.Atan2(M[j, i], -M[k, i]);
            }
            else
            {
                ax = Mathf.Atan2(-M[j, k], M[j, j]);
                ay = Mathf.Atan2(sy, M[i, i]);
                az = 0.0f;
            }
        }
        else
        {
            float cy = Mathf.Sqrt(M[i, i] * M[i, i] + M[j, i] * M[j, i]);
            if (cy > _EPS)
            {
                ax = Mathf.Atan2(M[k, j], M[k, k]);
                ay = Mathf.Atan2(-M[k, i], cy);
                az = Mathf.Atan2(M[j, i], M[i, i]);
            }
            else
            {
                ax = Mathf.Atan2(-M[j, k], M[j, j]);
                ay = Mathf.Atan2(-M[k, i], cy);
                az = 0.0f;
            }
        }
        if (parity == 1)
        {
            ax = -ax;
            ay = -ay;
            az = -az;
        }
        if (frame == 1)
        {
            ax = az;
            az = ax;
        }
        Vector3 euler = new Vector3(ax * Mathf.Rad2Deg, ay * Mathf.Rad2Deg, az * Mathf.Rad2Deg);
        return euler;
    }

    public static Quaternion MayaRotationToUnity(Vector3 rotation)
    {
        var flippedRotation = new Vector3(rotation.x, -rotation.y, -rotation.z); // flip Y and Z axis for right->left handed conversion
        // convert XYZ to ZYX
        var qy90 = Quaternion.AngleAxis(90.0f, Vector3.up);
        var qy180 = Quaternion.AngleAxis(180.0f, Vector3.up);
        var qx = Quaternion.AngleAxis(flippedRotation.x, Vector3.right);
        var qy = Quaternion.AngleAxis(flippedRotation.y, Vector3.up);
        var qz = Quaternion.AngleAxis(flippedRotation.z, Vector3.forward);
        var qq = qz * qy * qx; // this is the order
        return qq;
    }
    
    public static Quaternion RotationMatrixToQuaternion(Matrix4x4 a)
    {
        Quaternion q = Quaternion.identity;
        float trace = a[0, 0] + a[1, 1] + a[2, 2]; // I removed + 1.0f; see discussion with Ethan
        if (trace > 0)
        {
            float s = 0.5f / Mathf.Sqrt(trace + 1.0f);
            q.w = 0.25f / s;
            q.x = (a[2, 1] - a[1, 2]) * s;
            q.y = (a[0, 2] - a[2, 0]) * s;
            q.z = (a[1, 0] - a[0, 1]) * s;
        }
        else
        {
            if (a[0, 0] > a[1, 1] && a[0, 0] > a[2, 2])
            {
                float s = 2.0f * Mathf.Sqrt(1.0f + a[0, 0] - a[1, 1] - a[2, 2]);
                q.w = (a[2, 1] - a[1, 2]) / s;
                q.x = 0.25f * s;
                q.y = (a[0, 1] + a[1, 0]) / s;
                q.z = (a[0, 2] + a[2, 0]) / s;
            }
            else if (a[1, 1] > a[2, 2])
            {
                float s = 2.0f * Mathf.Sqrt(1.0f + a[1, 1] - a[0, 0] - a[2, 2]);
                q.w = (a[0, 2] - a[2, 0]) / s;
                q.x = (a[0, 1] + a[1, 0]) / s;
                q.y = 0.25f * s;
                q.z = (a[1, 2] + a[2, 1]) / s;
            }
            else
            {
                float s = 2.0f * Mathf.Sqrt(1.0f + a[2, 2] - a[0, 0] - a[1, 1]);
                q.w = (a[1, 0] - a[0, 1]) / s;
                q.x = (a[0, 2] + a[2, 0]) / s;
                q.y = (a[1, 2] + a[2, 1]) / s;
                q.z = 0.25f * s;
            }
        }
        return q;
    }

    public static Vector3 QuaternionTransform(Quaternion q, Vector3 v)
    {
        var tt = new Vector3(q.x, q.y, q.z);
        var t = 2 * Vector3.Cross(tt, v);
        return v + q.w * t + Vector3.Cross(tt, t);
    }

    public static Vector4 QuanternionToVector4(Quaternion q)
    {
        return new Vector4(q.x, q.y, q.z, q.w);
    }

    public static Vector4 PlaneToVector4(Plane plane)
    {
        return new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);
    }

    public static Quaternion Vector4ToQuaternion(Vector4 v)
    {
        return new Quaternion(v.x, v.y, v.z, v.w);
    }

    public static Color GetRandomColor()
    {
        return new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f));
    }

    public static float[] ReadBytesAsFloats(string filePath)
    {
        if (!File.Exists(filePath)) throw new Exception("File not found: " + filePath);

        var bytes = File.ReadAllBytes(filePath);
		//BitConverter.IsLittleEndian (bytes [0]);
        var floats = new float[bytes.Length / sizeof(float)];//4 ?
        Buffer.BlockCopy(bytes, 0, floats, 0, bytes.Length);
        return floats;
    }

    //try to read the json so I can test different quaternion/angle/matrice
    //int i indice of name
    //float x y z 
    //float x y z w
    public static JSONNode ParseJson(string filePath)
    {
        var source = new StreamReader(filePath);
        var fileContents = source.ReadToEnd();
        var data = JSONNode.Parse(fileContents);
		source.Close ();
        return data;
    }

    public static int GetIdFromColor(Color color)
    {
        int b = (int)(color.b * 255.0f);
        int g = (int)(color.g * 255.0f) << 8;
        int r = (int)(color.r * 255.0f) << 16;

        return r + g + b;
    }
    
    public static Vector3 CubicInterpolate(Vector3 y0, Vector3 y1, Vector3 y2, Vector3 y3, float mu)
    {
        float mu2 = mu * mu;
        Vector3 a0, a1, a2, a3;

        a0 = y3 - y2 - y0 + y1;
        a1 = y0 - y1 - a0;
        a2 = y2 - y0;
        a3 = y1;

        return (a0 * mu * mu2 + a1 * mu2 + a2 * mu + a3);
    }

    public static Matrix4x4 FloatArrayToMatrix4X4(float[] floatArray)
    {
        var matrix = new Matrix4x4();

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                matrix[i, j] = floatArray[i * 4 + j];
            }
        }

        return matrix;
    }

    public static float[] Matrix4X4ToFloatArray(Matrix4x4 matrix)
    {
        var floatArray = new float[16];

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                floatArray[i * 4 + j] = matrix[j, i];
            }
        }

        return floatArray;
    }

    public static GameObject FindChildByName(GameObject root, string name)
    {
        GameObject child = null;

        for (int i = 0; i < root.transform.childCount; i++)
        {
            child = root.transform.GetChild(i).gameObject;

            if (string.CompareOrdinal(child.name, name) == 0)
            {
                return child;
            }
            else 
            {
                var child_2 = FindChildByName(child, name);
                if (child_2 != null) return child_2;
            }
        }

        return null;
    }

    public static float[] FrustrumPlanesAsFloats(Camera _camera)
    {
        var planes = GeometryUtility.CalculateFrustumPlanes(_camera);
        var planesAsFloats = new float[6 * 4];
        for (int i = 0; i < planes.Length; i++)
        {
            planesAsFloats[i * 4] = planes[i].normal.x;
            planesAsFloats[i * 4 + 1] = planes[i].normal.y;
            planesAsFloats[i * 4 + 2] = planes[i].normal.z;
            planesAsFloats[i * 4 + 3] = planes[i].distance;
        }

        return planesAsFloats;
    }

    public static int ReadPixelId(RenderTexture texture, Vector2 coord)
    {
        var outBuffer = new ComputeBuffer(1, sizeof(int));

        var pixelId = new[] { 0 };
        outBuffer.GetData(pixelId);
        outBuffer.Release();

        return pixelId[0];
    }

	//---------------------------helper for autopack file system -----------------------------------
	public static List<List<Vector4>> gatherSphereTree(JSONNode idic){
		List<List<Vector4>> spheres = new List<List<Vector4>> ();
		
		for (int ilevel = 0; ilevel < idic["positions"].Count; ilevel++)
		{
			spheres.Add (new List<Vector4>());
			for (int isph=0;isph < idic["positions"][ilevel].Count;isph++)
			{
				var p = idic["positions"][ilevel][isph];
				var r = idic["radii"][ilevel][isph].AsFloat;
				spheres[ilevel].Add (new Vector4(-p[0].AsFloat, p[1].AsFloat, p[2].AsFloat,r));
			}
		}
		return spheres;
	}

	public static JSONNode GetAllIngredientsInfo()
	{
		Debug.Log("Downloading all ingredients file");
		var www = new WWW("https://raw.githubusercontent.com/mesoscope/cellPACK_data/master/cellPACK_database_1.1.0/recipes/allIngredients.json");
		var path = PdbLoader.DefaultPdbDirectory + "allIngredients.json";
		if (!Directory.Exists(PdbLoader.DefaultPdbDirectory))
		{
			Directory.CreateDirectory(PdbLoader.DefaultPdbDirectory);
		}

		if (!File.Exists (path)) {
			while (!www.isDone)
			{
				#if UNITY_EDITOR
				EditorUtility.DisplayProgressBar("Downloading all ingredient info", "Downloading...", www.progress);
				#endif
			}
			#if UNITY_EDITOR
			EditorUtility.ClearProgressBar();
			#endif
			
			if (!string.IsNullOrEmpty (www.error))
				throw new Exception ("allIngredients.json" + www.error);
			File.WriteAllText (path, www.text);
		}
		var resultData = Helper.ParseJson(path);
		//filter the name
		for (int i=0; i<resultData.Count; i++) {
			if (resultData[i]["file"].Value.Contains("HIV")){
				string key = resultData.GetKey(i);
				if (key.Contains("NC"))
					resultData.ChangeKey(key,"HIV_"+key.Split('_')[1]+"_"+key.Split('_')[2]);
				else if (key.Contains("P6_VPR"))
					resultData.ChangeKey(key,"HIV_"+key.Split('_')[1]+"_"+key.Split('_')[2]);
				else 
					resultData.ChangeKey(key,"HIV_"+key.Split('_')[1]);
				Debug.Log ("new key is "+"HIV_"+key.Split('_')[1]+" "+key);
			}
		}
		return resultData;
	}

	public static JSONNode GetAllRecipeInfo()
	{
		Debug.Log("Downloading all ingredients file");
		//could use a special file for cellView
		var www = new WWW("https://raw.githubusercontent.com/mesoscope/cellPACK_data/master/cellPACK_database_1.1.0/autopack_recipe_cellview.json");
		var path = Application.dataPath + "/../Data/packing_results/autopack_recipe_cellview.json";
		if (!Directory.Exists(Application.dataPath + "/../Data/"))
		{
			Directory.CreateDirectory(Application.dataPath + "/../Data/");
		}
		if (!Directory.Exists(Application.dataPath + "/../Data/packing_results/"))
		{
			Directory.CreateDirectory(Application.dataPath + "/../Data/packing_results/");
		}
		if (!File.Exists (path)) {
			while (!www.isDone)
			{
				#if UNITY_EDITOR
				EditorUtility.DisplayProgressBar("Downloading recipes available infos", "Downloading...", www.progress);
				#endif
			}
			#if UNITY_EDITOR
			EditorUtility.ClearProgressBar();
			#endif
			
			if (!string.IsNullOrEmpty (www.error))
				throw new Exception ("autopack_recipe.json" + www.error);
			File.WriteAllText (path, www.text);
		}
		var resultData = Helper.ParseJson(path);
		return resultData;
	}
	public static string GetResultsFile(string url)
	{
		Debug.Log("Downloading results file "+url);
		var www = new WWW(url);
		string fname = GetFileName (url);
		var path = Application.dataPath + "/../Data/packing_results/"+fname;
		if (!Directory.Exists(Application.dataPath + "/../Data/"))
		{
			Directory.CreateDirectory(Application.dataPath + "/../Data/");
		}
		if (!Directory.Exists(Application.dataPath + "/../Data/packing_results/"))
		{
			Directory.CreateDirectory(Application.dataPath + "/../Data/packing_results/");
		}
		if (!File.Exists (path)) {
			while (!www.isDone)
			{
				#if UNITY_EDITOR
				EditorUtility.DisplayProgressBar("Downloading recipes results file", "Downloading...", www.progress);
				#endif

			}
			#if UNITY_EDITOR
			EditorUtility.ClearProgressBar();
			#endif

			if (!string.IsNullOrEmpty (www.error))
				throw new Exception (fname + www.error);
			File.WriteAllText (path, www.text);
		}
		return path;
	}

	public static string GetFileName(string hrefLink)
	{
		string[] parts = hrefLink.Split('/');
		string fileName = "";
		
		if (parts.Length > 0)
			fileName = parts[parts.Length - 1];
		else
			fileName = hrefLink;
		
		return fileName;
	}

	public static void FocusCameraOnGameObject(Camera c, Vector4 center, float radius, string ingname)
    {
    }

    public static float frac(float v)
	{
		return v - Mathf.Floor(v);
	}

	public static float hash(float n)
	{
		return frac(Mathf.Sin(n) * 43758.5453f);
	}

	public static Vector3 vector3Add(Vector3 avect, float anumber){
		for (int i=0; i<3; i++) {
			avect[i] = avect[i] + anumber;
		}
		return avect;
	}

	public static float noise_3D(Vector3 x)
	{
		// The noise function returns a value in the range -1.0f -> 1.0f
		
		Vector3 p = new Vector3 ( Mathf.Floor(x.x),Mathf.Floor(x.y),Mathf.Floor(x.z));
		Vector3 f = new Vector3 ( frac(x.x),frac(x.y),frac(x.z));

		for (int i=0; i<3; i++) {
			f[i] = f[i] * f[i] * (3.0f - 2.0f * f[i]);
		}
		float n = p.x + p.y * 57.0f + 113.0f * p.z;
		
		return Mathf.Lerp(Mathf.Lerp(Mathf.Lerp(hash(n + 0.0f), hash(n + 1.0f), f.x),
		                                 Mathf.Lerp(hash(n + 57.0f), hash(n + 58.0f), f.x), f.y),
		                    Mathf.Lerp(Mathf.Lerp(hash(n + 113.0f), hash(n + 114.0f), f.x),
		           Mathf.Lerp(hash(n + 170.0f), hash(n + 171.0f), f.x), f.y), f.z);
	}
		
	public static Vector4 randomMove(Vector3 input_pos)
	{

		float _Time = Time.realtimeSinceStartup;
        float speedFactor = 1.0f;
		float translationScaleFactor = 5;
		
		float randx = frac(Mathf.Sin(Vector2.Dot(new Vector2(1, input_pos.x), new Vector2(12.9898f, 78.233f))) * 43758.5453f);
		float randy = frac(Mathf.Sin(Vector2.Dot(new Vector2(1, input_pos.y), new Vector2(12.9898f, 78.233f))) * 43758.5453f);
		float randz = frac(Mathf.Sin(Vector2.Dot(new Vector2(1, input_pos.z), new Vector2(12.9898f, 78.233f))) * 43758.5453f);
		
		Vector4 tt = new Vector4(_Time / 20, _Time, _Time * 2, _Time * 3);
		Vector3 ttxyz = new Vector3 (tt.x, tt.y, tt.z);
		Vector3 ttyzx = new Vector3 (tt.y, tt.z, tt.x);
		Vector3 ttzxy = new Vector3 (tt.z, tt.x, tt.y);

		Vector3 posyzx = new Vector3 (input_pos.y, input_pos.z, input_pos.x);
		Vector3 poszxy = new Vector3 (input_pos.z, input_pos.x, input_pos.y);

		Vector3 pn = Vector3.zero;
		pn.x = noise_3D(vector3Add(input_pos + ttxyz * speedFactor,randx + 100.0f ));
		pn.y = noise_3D(vector3Add(posyzx + ttyzx* speedFactor,randy + 200.0f) );
		pn.z = noise_3D(vector3Add(poszxy + ttzxy * speedFactor,randz + 300.0f ) );
		pn =vector3Add(pn,-0.5f);

		Vector3 newp = input_pos + pn * translationScaleFactor;
		return new Vector4(newp.x,newp.y,newp.z, 1);
	}

    public static double[,] ComputeCovarianceMatrix2D(Vector2[] cluster)
    {
        double[,] C = new double[3, 3];
        Vector3 mu = Vector3.zero;

        for (int i = 0; i < cluster.Length; i++)
        {
            mu += new Vector3(cluster[i].x, cluster[i].y, 0.0f);
        }
        mu /= cluster.Length;
        // loop over the points again to build the 
        // covariance matrix.  Note that we only have
        // to build terms for the upper trianglular 
        // portion since the matrix is symmetric
        double cxx = 0.0, cxy = 0.0, cxz = 0.0, cyy = 0.0, cyz = 0.0, czz = 0.0;
        for (int i = 0; i < (int)cluster.Length; i++)
        {
            Vector3 p = cluster[i];
            cxx += p.x * p.x - mu.x * mu.x;
            cxy += p.x * p.y - mu.x * mu.y;
            cxz += p.x * p.z - mu.x * mu.z;
            cyy += p.y * p.y - mu.y * mu.y;
            cyz += p.y * p.z - mu.y * mu.z;
            czz += p.z * p.z - mu.z * mu.z;
        }

        // now build the covariance matrix
        C[0, 0] = cxx; C[0, 1] = cxy; C[0, 2] = cxz;
        C[1, 0] = cxy; C[1, 1] = cyy; C[1, 2] = cyz;
        C[2, 0] = cxz; C[2, 1] = cyz; C[2, 2] = czz;
        return C;
    }
    public static List<Vector4> BuildOBB2D(Vector2[] cluster, float radius)
    {
        double[,] C = new double[cluster.Length, 2];
        for (int i = 0; i < cluster.Length; i++)
        {
            C[i, 0] = cluster[i].x;
            C[i, 1] = cluster[i].y;
        }
        double[,] gm = Measures.Covariance(C);
        EigenvalueDecomposition E = new EigenvalueDecomposition(gm);
        var eig_vec = E.Eigenvectors;
        Debug.Log(eig_vec.Length.ToString());
        for (int i = 0; i < eig_vec.Length / 2; i++)
        {
            Debug.Log(eig_vec[i, 0].ToString());
            Debug.Log(eig_vec[i, 1].ToString());
        }
        Vector2 r = new Vector2((float)eig_vec[0, 0], (float)eig_vec[1, 0]);
        Vector2 u = new Vector2((float)eig_vec[0, 1], (float)eig_vec[1, 1]);
        r.Normalize(); u.Normalize(); 
        Matrix4x4 m_rot = Matrix4x4.identity;
        // set the rotation matrix using the eigvenvectors
        m_rot[0, 0] = r.x; m_rot[0, 1] = u.x;
        m_rot[1, 0] = r.y; m_rot[1, 1] = u.y;

        // now build the bounding box extents in the rotated frame
        Vector2 minim = new Vector2(1e10f, 1e10f);
        Vector2 maxim = new Vector2(-1e10f, -1e10f);
        for (int i = 0; i < (int)cluster.Length; i++)
        {
            Vector2 pt = new Vector2(cluster[i].x, cluster[i].y);
            Vector2 p_prime = new Vector2(Vector2.Dot(r, pt), Vector2.Dot(u, pt));
            minim = Vector2.Min(minim, p_prime);
            maxim = Vector2.Max(maxim, p_prime);
        }
        // set the center of the OBB to be the average of the 
        // minimum and maximum, and the extents be half of the
        // difference between the minimum and maximum
        List<Vector4> principal_vector = new List<Vector4>();
        principal_vector.Add(new Vector4(maxim.x + radius, maxim.y + radius, 0, 1.0f));
        principal_vector.Add(new Vector4(minim.x - radius, minim.y - radius, 0, 1.0f));
        principal_vector.Add(new Vector4(m_rot.rotation.x, m_rot.rotation.y, m_rot.rotation.z, m_rot.rotation.w));
        return principal_vector;
    }

    public static List<Vector4> BuildOBB2D1(Vector2[] cluster, float radius)
    {
        double[,] C = new double[cluster.Length, 2];
        for (int i = 0; i < cluster.Length; i++)
        {
            C[i, 0] = cluster[i].x;
            C[i, 1] = cluster[i].y;
        }
        double[,] gm = Measures.Covariance(C);
        EigenvalueDecomposition E = new EigenvalueDecomposition(gm);
        var eig_vec = E.Eigenvectors;
        Debug.Log(eig_vec.Length.ToString());
        for (int i = 0; i < eig_vec.Length/2; i++) {
            Debug.Log(eig_vec[i, 0].ToString());
            Debug.Log(eig_vec[i, 1].ToString());
        }
        Vector3 r = new Vector3((float)eig_vec[0, 0], (float)eig_vec[1, 0], (float)eig_vec[2, 0]);
        Vector3 u = new Vector3((float)eig_vec[0, 1], (float)eig_vec[1, 1], (float)eig_vec[2, 1]);
        Vector3 f = new Vector3((float)eig_vec[0, 2], (float)eig_vec[1, 2], (float)eig_vec[2, 2]);
        r.Normalize(); u.Normalize(); f.Normalize();
        Matrix4x4 m_rot = Matrix4x4.identity;
        // set the rotation matrix using the eigvenvectors
        m_rot[0, 0] = r.x; m_rot[0, 1] = u.x; m_rot[0, 2] = f.x;
        m_rot[1, 0] = r.y; m_rot[1, 1] = u.y; m_rot[1, 2] = f.y;
        m_rot[2, 0] = r.z; m_rot[2, 1] = u.z; m_rot[2, 2] = f.z;

        // now build the bounding box extents in the rotated frame
        Vector3 minim = new Vector3(1e10f, 1e10f, 1e10f);
        Vector3 maxim = new Vector3(-1e10f, -1e10f, -1e10f);
        for (int i = 0; i < (int)cluster.Length; i++)
        {
            Vector3 pt = new Vector3(cluster[i].x, cluster[i].y, 0.0f);
            Vector3 p_prime = new Vector3(Vector3.Dot(r, pt), Vector3.Dot(u, pt), Vector3.Dot(f, pt));
            minim = Vector3.Min(minim, p_prime);
            maxim = Vector3.Max(maxim, p_prime);
        }
        // set the center of the OBB to be the average of the 
        // minimum and maximum, and the extents be half of the
        // difference between the minimum and maximum
        List<Vector4> principal_vector = new List<Vector4>();
        principal_vector.Add(new Vector4(maxim.x + radius, maxim.y + radius, maxim.z + radius, 1.0f));
        principal_vector.Add(new Vector4(minim.x - radius, minim.y - radius, minim.z - radius, 1.0f));
        principal_vector.Add(new Vector4(m_rot.rotation.x, m_rot.rotation.y, m_rot.rotation.z, m_rot.rotation.w));
        //problem with fibrinogen ?
        return principal_vector;
    }
    //if accordmath not fast enough check loic lib
    //http://loyc.net/2014/2d-convex-hull-in-cs.html
    public static List<Vector2> ComputeConvexHull(List<IntPoint> points, Vector3 center, bool optimize = false)
    {
        IConvexHullAlgorithm hullFinder = new GrahamConvexHull( );
        IShapeOptimizer opt = new ClosePointsMergingOptimizer();
        List<IntPoint> hull = hullFinder.FindHull( points );
        if (optimize) {
            hull =  opt.OptimizeShape(hull);
        }

        List<Vector2> hull_opt2d = new List<Vector2>();
        foreach (var p in hull) {
            hull_opt2d.Add(new Vector2(p.X-center.x,p.Y-center.y));
        }
        return hull_opt2d;
    }

    public static List<Vector2> ComputeConvexHull(List<Vector2> points2d, Vector3 center, bool optimize = false)
    {
        List<IntPoint> points = new List<IntPoint>();
        foreach ( var p in points2d) {
            points.Add(new IntPoint(Mathf.CeilToInt(p.x),Mathf.CeilToInt(p.y)));
        }
        IConvexHullAlgorithm hullFinder = new GrahamConvexHull( );
        IShapeOptimizer opt = new ClosePointsMergingOptimizer();
        List<IntPoint> hull = hullFinder.FindHull( points );
        if (optimize) {
            hull =  opt.OptimizeShape(hull);
        }
        //convert to Vector2
        List<Vector2> hull_opt2d = new List<Vector2>();
        foreach (var p in hull) {
            hull_opt2d.Add(new Vector2(p.X-center.x,p.Y-center.y));
        }
        return hull_opt2d;
    }


    public static int ComputeCluster(List<Vector2> points2d, double radius, ref List<int> labels){
        Accord.Math.Random.Generator.Seed = 0;
        double[][] input = new double[points2d.Count][];
        for (int i = 0; i < points2d.Count; i++)
        {
            input[i] = new double[] {points2d[i].x,points2d[i].y};
        }

        MeanShift meanShift = new MeanShift()
        {
            // Use a uniform kernel density
            Kernel = new UniformKernel(),
            Bandwidth = radius
        };
        MeanShiftClusterCollection clustering = meanShift.Learn(input);
        Debug.Log("found "+clustering.Count.ToString());
        labels = new List<int>(clustering.Decide(input));
        return clustering.Count;
    }
}


public static class HalfHelper
{
    private static uint[] mantissaTable = GenerateMantissaTable();
    private static uint[] exponentTable = GenerateExponentTable();
    private static ushort[] offsetTable = GenerateOffsetTable();
    private static ushort[] baseTable = GenerateBaseTable();
    private static sbyte[] shiftTable = GenerateShiftTable();

    // Transforms the subnormal representation to a normalized one. 
    private static uint ConvertMantissa(int i)
    {
        uint m = (uint)(i << 13); // Zero pad mantissa bits
        uint e = 0; // Zero exponent

        // While not normalized
        while ((m & 0x00800000) == 0)
        {
            e -= 0x00800000; // Decrement exponent (1<<23)
            m <<= 1; // Shift mantissa                
        }
        m &= unchecked((uint)~0x00800000); // Clear leading 1 bit
        e += 0x38800000; // Adjust bias ((127-14)<<23)
        return m | e; // Return combined number
    }

    private static uint[] GenerateMantissaTable()
    {
        uint[] mantissaTable = new uint[2048];
        mantissaTable[0] = 0;
        for (int i = 1; i < 1024; i++)
        {
            mantissaTable[i] = ConvertMantissa(i);
        }
        for (int i = 1024; i < 2048; i++)
        {
            mantissaTable[i] = (uint)(0x38000000 + ((i - 1024) << 13));
        }

        return mantissaTable;
    }
    private static uint[] GenerateExponentTable()
    {
        uint[] exponentTable = new uint[64];
        exponentTable[0] = 0;
        for (int i = 1; i < 31; i++)
        {
            exponentTable[i] = (uint)(i << 23);
        }
        exponentTable[31] = 0x47800000;
        exponentTable[32] = 0x80000000;
        for (int i = 33; i < 63; i++)
        {
            exponentTable[i] = (uint)(0x80000000 + ((i - 32) << 23));
        }
        exponentTable[63] = 0xc7800000;

        return exponentTable;
    }
    private static ushort[] GenerateOffsetTable()
    {
        ushort[] offsetTable = new ushort[64];
        offsetTable[0] = 0;
        for (int i = 1; i < 32; i++)
        {
            offsetTable[i] = 1024;
        }
        offsetTable[32] = 0;
        for (int i = 33; i < 64; i++)
        {
            offsetTable[i] = 1024;
        }

        return offsetTable;
    }
    private static ushort[] GenerateBaseTable()
    {
        ushort[] baseTable = new ushort[512];
        for (int i = 0; i < 256; ++i)
        {
            sbyte e = (sbyte)(127 - i);
            if (e > 24)
            { // Very small numbers map to zero
                baseTable[i | 0x000] = 0x0000;
                baseTable[i | 0x100] = 0x8000;
            }
            else if (e > 14)
            { // Small numbers map to denorms
                baseTable[i | 0x000] = (ushort)(0x0400 >> (18 + e));
                baseTable[i | 0x100] = (ushort)((0x0400 >> (18 + e)) | 0x8000);
            }
            else if (e >= -15)
            { // Normal numbers just lose precision
                baseTable[i | 0x000] = (ushort)((15 - e) << 10);
                baseTable[i | 0x100] = (ushort)(((15 - e) << 10) | 0x8000);
            }
            else if (e > -128)
            { // Large numbers map to Infinity
                baseTable[i | 0x000] = 0x7c00;
                baseTable[i | 0x100] = 0xfc00;
            }
            else
            { // Infinity and NaN's stay Infinity and NaN's
                baseTable[i | 0x000] = 0x7c00;
                baseTable[i | 0x100] = 0xfc00;
            }
        }

        return baseTable;
    }
    private static sbyte[] GenerateShiftTable()
    {
        sbyte[] shiftTable = new sbyte[512];
        for (int i = 0; i < 256; ++i)
        {
            sbyte e = (sbyte)(127 - i);
            if (e > 24)
            { // Very small numbers map to zero
                shiftTable[i | 0x000] = 24;
                shiftTable[i | 0x100] = 24;
            }
            else if (e > 14)
            { // Small numbers map to denorms
                shiftTable[i | 0x000] = (sbyte)(e - 1);
                shiftTable[i | 0x100] = (sbyte)(e - 1);
            }
            else if (e >= -15)
            { // Normal numbers just lose precision
                shiftTable[i | 0x000] = 13;
                shiftTable[i | 0x100] = 13;
            }
            else if (e > -128)
            { // Large numbers map to Infinity
                shiftTable[i | 0x000] = 24;
                shiftTable[i | 0x100] = 24;
            }
            else
            { // Infinity and NaN's stay Infinity and NaN's
                shiftTable[i | 0x000] = 13;
                shiftTable[i | 0x100] = 13;
            }
        }

        return shiftTable;
    }

    public static float HalfToSingle(ushort half)
    {
        uint result = mantissaTable[offsetTable[half >> 10] + (half & 0x3ff)] + exponentTable[half >> 10];
        byte[] bytes = BitConverter.GetBytes(result);
        return BitConverter.ToSingle(bytes, 0);
    }

    public static ushort SingleToHalf(float single)
    {
        byte[] bytes = BitConverter.GetBytes(single);
        uint value = BitConverter.ToUInt32(bytes, 0);

        ushort result = (ushort)(baseTable[(value >> 23) & 0x1ff] + ((value & 0x007fffff) >> shiftTable[value >> 23]));
        return result; 
    }

    public static float SingleToSingle(float single)
    {
        ushort half = SingleToHalf(single);
        return HalfToSingle(half);
    }
}
