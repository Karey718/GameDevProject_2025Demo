using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexagonPrismGenerator : MonoBehaviour
{
    public float radius = 1f; // 六边形的半径
    public float height = 2f; // 柱体的高度

    void Start()
    {
        CreateHexagonPrism();
    }

    void CreateHexagonPrism()
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // 顶点数组：12个顶点（上下两个六边形）
        Vector3[] vertices = new Vector3[12];
        // 三角形数组：24个三角形（上下两个六边形各6个，侧面12个）
        int[] triangles = new int[36];

        // 生成底部六边形的顶点
        for (int i = 0; i < 6; i++)
        {
            float angle = 60 * i; // 每个顶点之间的角度差为60度
            float x = radius * Mathf.Cos(Mathf.Deg2Rad * angle);
            float z = radius * Mathf.Sin(Mathf.Deg2Rad * angle);
            vertices[i] = new Vector3(x, 0, z); // 底部顶点
            vertices[i + 6] = new Vector3(x, height, z); // 顶部顶点（Y轴偏移高度）
        }

        // 底部六边形的三角形
        for (int i = 0; i < 6; i++)
        {
            triangles[i * 3] = 0; // 中心点
            triangles[i * 3 + 1] = i;
            triangles[i * 3 + 2] = (i + 1) % 6;
        }

        // 顶部六边形的三角形
        for (int i = 0; i < 6; i++)
        {
            triangles[18 + i * 3] = 6; // 中心点
            triangles[18 + i * 3 + 1] = 6 + (i + 1) % 6;
            triangles[18 + i * 3 + 2] = 6 + i;
        }

        // 侧面的三角形
        for (int i = 0; i < 6; i++)
        {
            int current = i;
            int next = (i + 1) % 6;

            // 第一个三角形
            triangles[36 + i * 6] = current;
            triangles[36 + i * 6 + 1] = next;
            triangles[36 + i * 6 + 2] = current + 6;

            // 第二个三角形
            triangles[36 + i * 6 + 3] = next;
            triangles[36 + i * 6 + 4] = next + 6;
            triangles[36 + i * 6 + 5] = current + 6;
        }

        // 将顶点和三角形赋值给网格
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals(); // 重新计算法线
    }
}
