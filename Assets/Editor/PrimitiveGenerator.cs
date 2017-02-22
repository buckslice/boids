using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PrimitiveGenerator : EditorWindow {

    [MenuItem("Generation/Primitives")]
    static void Init() {
        GetWindow(typeof(PrimitiveGenerator));
    }

    private Material mat;
    void OnGUI() {
        if (GUILayout.Button("Square Pyramid")) {
            squarePyramid();
        }
        if (GUILayout.Button("Triangular Pyramid")) {
            triangularPyramid();
        }
        //size = EditorGUILayout.Slider("Size", size, 100f, 1000f);
        //number = EditorGUILayout.IntSlider("Number", number, 10, 5000);
        mat = (Material)EditorGUILayout.ObjectField(mat, typeof(Material), false);
    }

    private void squarePyramid() {
        GameObject go = new GameObject("Pyramid");
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = mat;

        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        // make 4 sided one and 3 sided

        Vector3 a = new Vector3(0.0f, 0.0f, 1.0f);

        Vector3 b = new Vector3(1.0f, 1.0f, -1.0f);
        Vector3 c = new Vector3(1.0f, -1.0f, -1.0f);
        Vector3 d = new Vector3(-1.0f, -1.0f, -1.0f);
        Vector3 e = new Vector3(-1.0f, 1.0f, -1.0f);

        verts.Add(a);
        verts.Add(b);
        verts.Add(e);

        verts.Add(a);
        verts.Add(c);
        verts.Add(b);

        verts.Add(a);
        verts.Add(d);
        verts.Add(c);

        verts.Add(a);
        verts.Add(e);
        verts.Add(d);

        verts.Add(b);
        verts.Add(c);
        verts.Add(d);

        verts.Add(d);
        verts.Add(e);
        verts.Add(b);

        for (int i = 0; i < 18; i++) {
            tris.Add(i);
        }

        Mesh m = new Mesh();
        m.vertices = verts.ToArray();
        m.triangles = tris.ToArray();
        m.RecalculateNormals();
        m.RecalculateBounds();

        go.AddComponent<MeshFilter>().mesh = m;
    }

    private void triangularPyramid() {
        GameObject go = new GameObject("Pyramid");
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = mat;

        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        // make 4 sided one and 3 sided

        float baseEdgeLength = 1.0f;
        float height = 2.0f;


        float hw = baseEdgeLength / 2.0f;
        float fx = (float)0.57735026919 * hw;	// distance from center of edge to center of equilateral
        float fy = (float)1.15470053838 * hw;	// distance from any point to center of equilateral
        Vector3 w = new Vector3();


        Vector3 l = w - Vector3.right * hw - Vector3.forward * fx;
        Vector3 r = w + Vector3.right * hw - Vector3.forward * fx;
        Vector3 u = w + Vector3.up * height;
        Vector3 f = w + Vector3.forward * fy;

        verts.Add(l);
        verts.Add(u);
        verts.Add(r);
        
        verts.Add(f);
        verts.Add(u);
        verts.Add(l);

        verts.Add(r);
        verts.Add(u);
        verts.Add(f);

        verts.Add(l);
        verts.Add(r);
        verts.Add(f);

        for (int i = 0; i < 12; i++) {
            tris.Add(i);
        }

        Mesh m = new Mesh();
        m.vertices = verts.ToArray();
        m.triangles = tris.ToArray();
        m.RecalculateNormals();
        m.RecalculateBounds();

        go.AddComponent<MeshFilter>().mesh = m;

        AssetDatabase.CreateAsset(m, "Assets/Models/triangular_pyramid.asset");
    }
}
