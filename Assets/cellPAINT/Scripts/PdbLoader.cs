using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

public static class PdbLoader
{
    public static string cellpack_repo = "https://raw.githubusercontent.com/mesoscope/cellPACK_data/master/cellPACK_database_1.1.0/";
    public static string DefaultPdbDirectory = Application.dataPath + "/../Data/proteins/";

    public static string DefaultDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"cellPAINT_data");
/*
#if UNITY_STANDALONE_OSX
    public static string DefaultDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"cellPAINT";//Application.dataPath + "/../Data/";
#else
    public static string DefaultDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);//Application.dataPath + "/../Data/";
#endif  
*/
    public static List<string> DataDirectories = new List<string>();

    public static List<Atom> LoadAtomSet(string fileName)
    {
        var path = GetPdbFile(fileName, DefaultPdbDirectory);
        return ReadAtomData(path);
    }

    public static List<Matrix4x4> LoadBiomtTransforms(string fileName)
    {
        var path = GetPdbFile(fileName, DefaultPdbDirectory);
        return ReadBiomtData(path);
    }

    public static List<Vector4> LoadAtomSpheres(string fileName, bool buildBiomt = false)
    {
        var path = GetPdbFile(fileName, DefaultPdbDirectory);

        var atomData = ReadAtomData(path);
        var atomSpheres = AtomHelper.GetAtomSpheres(atomData);

        if (buildBiomt)
        {
            var biomtTransforms = ReadBiomtData(path);
            atomSpheres = AtomHelper.BuildBiomt(atomSpheres, biomtTransforms);
        }

        return atomSpheres;
    }

    public static string GetAFile(string filename, string type, string extension) {
        //look for filename in repo and local directory
        //priority on which folder ?
        Debug.Log("GetAFile "+filename+" "+extension);
        var filePath="";
        foreach (var dir in DataDirectories)
        {
            filePath = dir + "/" + filename + extension;
            Debug.Log(filePath);
            if (File.Exists(filePath))
            {
                return filePath;
            }
        }
        filePath = DefaultDataDirectory + "/" + type + "/"  + filename + extension;
        if (!File.Exists(filePath))
        {
            filePath = DownloadFile(filename, cellpack_repo + "/" + type, DefaultDataDirectory + "/" + type, extension);
        }
        return filePath;
    }

    private static string GetPdbFile(string fileName, string directory)
    {
        var filePath = directory + fileName + ".pdb";

        if (!File.Exists(filePath))
        {
            filePath = "";

            // Download from protein data bank
            if (fileName.Count() <= 4)
            {
                filePath = DownloadPdbFile(fileName, "http://www.rcsb.org/pdb/download/downloadFile.do?fileFormat=pdb&compression=NO&structureId=", directory);
            }

            // Download from cellPACK repository
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = DownloadPdbFile(fileName, "https://raw.githubusercontent.com/mesoscope/cellPACK_data/master/cellPACK_database_1.1.0/other/", directory, ".pdb");
            }

            if (string.IsNullOrEmpty(filePath))
            {
                throw new Exception("File not found: " + fileName);
            }
        }

        return filePath;
    }

    private static string DownloadPdbFile(string fileName, string url, string directory, string extension = "")
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        url = url + WWW.EscapeURL(fileName + extension);

		//Debug.Log("Downloading pdb file "+url);
		var www = new WWW(url);


        while (!www.isDone)
        {
			#if UNITY_EDITOREditor
			Utility.DisplayProgressBar("Downloading " + fileName, "Downloading...", www.progress);
			#endif
        }
		#if UNITY_EDITOREditor
		EditorUtility.ClearProgressBar();
		#endif

        if (!string.IsNullOrEmpty(www.error))
        {
            //Debug.Log(www.error);
            return null;
        }

        var filePath = directory + fileName + ".pdb";
        File.WriteAllText(filePath, www.text);

        return filePath;
    }

    public static string DownloadFile(string fileName, string url, string directory, string extension = "")
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        url = url + WWW.EscapeURL(fileName + extension);

        //Debug.Log("Downloading pdb file "+url);
        var www = new WWW(url);


        while (!www.isDone)
        {
#if UNITY_EDITOREditor
			Utility.DisplayProgressBar("Downloading " + fileName, "Downloading...", www.progress);
#endif
        }
#if UNITY_EDITOREditor
		EditorUtility.ClearProgressBar();
#endif

        if (!string.IsNullOrEmpty(www.error))
        {
            //Debug.Log(www.error);
            return null;
        }

        var filePath = directory + fileName + extension;
        File.WriteAllText(filePath, www.text);
        return filePath;
    }

    //http://deposit.rcsb.org/adit/docs/pdb_atom_format.html#ATOM
    public static List<Atom> ReadAtomData(string path)
    {
        if (!File.Exists(path)) throw new Exception("File not found at: " + path);

        var atoms = new List<Atom>();

        foreach (var line in File.ReadAllLines(path))
        {
            if (line.StartsWith("ATOM"))// || line.StartsWith("HETATM"))
            {
                var x = float.Parse(line.Substring(30, 8));
                var y = float.Parse(line.Substring(38, 8));
                var z = float.Parse(line.Substring(46, 8));

                var name = line.Substring(12, 4).Trim();
                var chainId = line.Substring(23, 3)[0];
				var residueId = line.Substring(23, 3);//int.Parse(line.Substring(23, 3));

                // Remove numbers from the name
                var t = Regex.Replace(name, @"[\d-]", string.Empty).Trim();
                var symbolId = Array.IndexOf(AtomHelper.AtomSymbols, t[0].ToString());
                if (symbolId < 0)
                {
					symbolId = Array.IndexOf(AtomHelper.AtomSymbols, "A");
					name = "A";
					//Debug.Log ("Atom symbol unknown: " + name+ " "+t+"\n"+line+"\n"+path);
					//throw new Exception("Atom symbol unknown: " + name+ " "+t+"\n"+line+"\n"+path);
                }

                var atom = new Atom
                {
                    name = name,
                    symbol = name[0],
                    chainId = chainId,
                    residueId = residueId,
                    position = new Vector3(-x, y, z)
                };

                atoms.Add(atom);
            }

            if (line.StartsWith("ENDMDL")) // Only parse first model of MDL files
            {
                break;
            }
        }

        return atoms;
    }


    //http://www.rcsb.org/pdb/101/static101.do?p=education_discussion/Looking-at-Structures/bioassembly_tutorial.html
    public static List<Matrix4x4> ReadBiomtData(string path)
    {
        if (!File.Exists(path)) throw new Exception("File not found at: " + path);

        var matrices = new List<Matrix4x4>();
        var matrix = new Matrix4x4();

        foreach (var line in File.ReadAllLines(path))
        {
            if (line.StartsWith("REMARK 350"))
            {
                if (line.Contains("BIOMT1"))
                {
                    matrix = Matrix4x4.identity;
                    var split = line.Substring(24).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    matrix[0, 0] = float.Parse(split[0]);
                    matrix[0, 1] = float.Parse(split[1]);
                    matrix[0, 2] = float.Parse(split[2]);
                    matrix[0, 3] = float.Parse(split[3]);
                }

                if (line.Contains("BIOMT2"))
                {
                    var split = line.Substring(24).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    matrix[1, 0] = float.Parse(split[0]);
                    matrix[1, 1] = float.Parse(split[1]);
                    matrix[1, 2] = float.Parse(split[2]);
                    matrix[1, 3] = float.Parse(split[3]);
                }

                if (line.Contains("BIOMT3"))
                {
                    var split = line.Substring(24).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    matrix[2, 0] = float.Parse(split[0]);
                    matrix[2, 1] = float.Parse(split[1]);
                    matrix[2, 2] = float.Parse(split[2]);
                    matrix[2, 3] = float.Parse(split[3]);

                    matrices.Add(matrix);
                }
            }
        }

        return matrices;
    }

}

public class Atom
{
    public int residue;
    public string residueId;

    public char symbol;
    public char chainId;
    public string name;

    public Vector3 position;
}

public static class AtomHelper
{
	public static float[] AtomRadii = { 1.548f, 1.100f, 1.400f, 1.348f, 1.880f, 1.808f,1.100f };
    public static string[] AtomSymbols = { "C", "H", "N", "O", "P", "S", "A" };

    // Color scheme taken from http://life.nthu.edu.tw/~fmhsu/rasframe/COLORS.HTM
	// not used
    public static Color[] AtomColors = 
    { 
        new Color(100,100,100) / 255,     // C        light grey
        new Color(255,255,255) / 255,     // H        white       
        new Color(143,143,255) / 255,     // N        light blue
        new Color(220,10,10) / 255,       // O        red         
        new Color(255,165,0) / 255,       // P        orange      
        new Color(255,200,50) / 255 ,      // S        yellow    
		new Color(255,255,255) / 255     // D        white   
    };

    public static bool ContainsACarbonOnly(List<Atom> atoms)
    {
        return atoms.All(atom => String.CompareOrdinal(atom.name, "CA") == 0);
    }

    public static List<Vector3> GetAtomPoints(List<Atom> atoms)
    {
        var points = new List<Vector3>();
        for (int i = 0; i < atoms.Count; i++)
        {
            points.Add(new Vector3(atoms[i].position.x, atoms[i].position.y, atoms[i].position.z));
        }
        return points;
    }

    public static List<Vector4> GetAtomSpheres(List<Atom> atoms)
    {
        var spheres = new List<Vector4>();
        for (int i = 0; i < atoms.Count; i++)
        {
            var symbolId = Array.IndexOf(AtomSymbols, atoms[i].symbol);
            if (symbolId < 0) symbolId = 0;

            spheres.Add(new Vector4(atoms[i].position.x, atoms[i].position.y, atoms[i].position.z, AtomRadii[symbolId]));
        }

        return spheres;
    }

    public static void OffsetAtoms(ref List<Atom> atoms, Vector3 offset)
    {
        for (var i = 0; i < atoms.Count(); i++)
        {
            atoms[i].position = atoms[i].position - offset;
        }
    }

    public static void OffsetSpheres(ref List<Vector4> spheres, Vector3 offset)
    {
        var offsetVector = new Vector4(offset.x, offset.y, offset.z, 0);

        for (var i = 0; i < spheres.Count(); i++)
        {
            spheres[i] -= offsetVector;
        }
    }

    public static void OffsetPoints(ref List<Vector3> points, Vector3 offset)
    {
        var offsetVector = new Vector4(offset.x, offset.y, offset.z, 0);

        for (var i = 0; i < points.Count(); i++)
        {
            points[i] -= offset;
        }
    }

    public static Bounds ComputeBounds(List<Atom> atoms)
    {
        var bbMin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        var bbMax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        foreach (var atom in atoms)
        {
            bbMin = Vector3.Min(bbMin, new Vector3(atom.position.x, atom.position.y, atom.position.z));
            bbMax = Vector3.Max(bbMax, new Vector3(atom.position.x, atom.position.y, atom.position.z));
        }

        var bbSize = bbMax - bbMin;
        var bbCenter = bbMin + bbSize * 0.5f;

        return new Bounds(bbCenter, bbSize);
    }
	//this doesnt use the sphere radius ?
    public static Bounds ComputeBounds(List<Vector4> spheres)
    {
        var bbMin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        var bbMax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        foreach (var sphere in spheres)
        {
            bbMin = Vector3.Min(bbMin, sphere);
            bbMax = Vector3.Max(bbMax, sphere);
        }

        var bbSize = bbMax - bbMin;
        var bbCenter = bbMin + bbSize * 0.5f;
		if (spheres.Count == 1) {
			bbSize = new Vector3(spheres[0].w-0.5f,spheres[0].w-0.5f,spheres[0].w-0.5f);
			bbCenter = new Vector3(spheres[0].x,spheres[0].y,spheres[0].z);
		}
        return new Bounds(bbCenter, bbSize);
    }

    public static Bounds ComputeBounds(List<Vector3> points)
    {
        var bbMin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        var bbMax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        foreach (var point in points)
        {
            bbMin = Vector3.Min(bbMin, point);
            bbMax = Vector3.Max(bbMax, point);
        }

        var bbSize = bbMax - bbMin;
        var bbCenter = bbMin + bbSize * 0.5f;

        return new Bounds(bbCenter, bbSize);
    }

    public static Bounds ComputeBounds(float[] x, float[] y, float[] z)
    {
        var bbMin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        var bbMax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        for (int i=0;i < x.Length;i++)
        {
            var point = new Vector3(x[i], y[i], z[i]);
            bbMin = Vector3.Min(bbMin, point);
            bbMax = Vector3.Max(bbMax, point);
        }

        var bbSize = bbMax - bbMin;
        var bbCenter = bbMin + bbSize * 0.5f;

        return new Bounds(bbCenter, bbSize);
    }

    public static List<Vector4> BuildBiomt(List<Vector4> atomSpheres, List<Matrix4x4> transforms)
    {
        // Code de debug, permet de comparer avec un resultat valide
        // La je load tous les atoms d'un coup et je les transform individuelement
        var biomtSpheres = new List<Vector4>();

        foreach (var transform in transforms)
        {
            var posBiomt = new Vector3(transform.m03, transform.m13, transform.m23);
            var rotBiomt = Helper.RotationMatrixToQuaternion(transform);

            foreach (var sphere in atomSpheres)
            {
                //var atomPos = Helper.QuaternionTransform(rotBiomt, sphere) + posBiomt;
                var atomPos = transform.MultiplyVector(sphere) + posBiomt;
                biomtSpheres.Add(new Vector4(atomPos.x, atomPos.y, atomPos.z, sphere.w));
            }
        }

        return biomtSpheres;
    }

    public static Vector3 GetBiomtCenter(List<Matrix4x4> transforms, Vector3 center)
    {
        if (transforms.Count <= 0) return Vector3.zero;

        var bbMin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        var bbMax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        foreach (var transform in transforms)
        {
			var euler = Helper.euler_from_matrix(transform);
			var rotBiomt = Helper.MayaRotationToUnity(euler);
			var offset = Helper.QuaternionTransform(rotBiomt,center);//Helper.RotationMatrixToQuaternion(matBiomt), GetCenter());
			var posBiomt = new Vector3(-transform.m03, transform.m13, transform.m23);

            bbMin = Vector3.Min(bbMin, new Vector3(posBiomt.x, posBiomt.y, posBiomt.z));
            bbMax = Vector3.Max(bbMax, new Vector3(posBiomt.x, posBiomt.y, posBiomt.z));
        }

        var bbSize = bbMax - bbMin;
        var bbCenter = bbMin + bbSize * 0.5f;
        var bounds = new Bounds(bbCenter, bbSize);

        return bounds.center;
    }
}
