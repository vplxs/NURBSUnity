using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NurbsUnity
{
    /// <summary>
    /// A MonoBehaviour Component for NURBS Surfaces
    /// Based on the code taught in Adaptive Architecture and Computation, The Bartlett, UCL, London
    /// </summary>
    [AddComponentMenu("NURBS/Surface")]
    [RequireComponent(typeof(MeshFilter))]
    public class Surface : MonoBehaviour
    {
        #region Public Properties
        public int degreeU = 3;
        public int degreeV = 3;
        public int countU = 5;
        public SurfaceType type = SurfaceType.BSPline;
        public Transforms transforms = Transforms.FromParent;
        public float resolution = 0.01f;
        public Transform[] controlPointTransforms;
        public Transform controlPointsParent;
        public float[] weights;
        public bool initializaOnStart = false;
        public bool autoUpdate = true;
        public Vector3[][] controlPoints { get; set; }
        public float[] knotsU;
        public float[] knotsV;
        public Vector3[] srfPoints
        {
            get
            {
                return new Vector3[0];
            }
        }
        #endregion

        #region Private Properties
        private int countV { get; set; }
        private MeshFilter meshFilter { get; set; }
        private Vector3[][] prevPos { get; set; }
        private Mesh mesh { get; set; }
        private List<Vector3> vertices { get; set; }
        private List<int> triangles { get; set; }
        #endregion

        // Use this for initialization
        void Awake()
        {
            if (initializaOnStart)
            {
                Initialize();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (autoUpdate)
            {
                for (int i=0; i<controlPoints.Length; i++)
                {
                    for (int j=0; j<controlPoints[i].Length; j++)
                    {
                        controlPoints[i][j] = controlPointTransforms[i * countV + j].localPosition;
                        if (controlPoints[i][j]!=prevPos[i][j])
                        {
                            UpdateSurface();
                        }
                        prevPos[i][j] = controlPoints[i][j];
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (controlPoints != null)
            {
                for (int i = 0; i < controlPoints.Length; i++)
                {
                    for (int j = 0; j < controlPoints[i].Length; j++)
                    {
                        Gizmos.color = Color.black;
                        if (i < controlPoints.Length - 1)
                            Gizmos.DrawLine(controlPoints[i][j], controlPoints[i+1][j]);
                        if (j < controlPoints[i].Length - 1)
                            Gizmos.DrawLine(controlPoints[i][j], controlPoints[i][j+1]);
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the surface instance
        /// </summary>
        public void Initialize()
        {
            meshFilter = GetComponent<MeshFilter>();
            if (transforms == Transforms.FromParent)
            {
                GetControlPointsFromTranssform();
            }
            else
            {

            }
            knotsU = NurbsUtils.Knots(countU, degreeU);
            knotsV = NurbsUtils.Knots(countV, degreeV);
            mesh = new Mesh();
            mesh.name = gameObject.name + "_surface";
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            UpdateSurface();
        }

        /// <summary>
        /// Updates the geometry of the surface
        /// </summary>
        public void UpdateSurface()
        {
            if (countU == 2 && countV == 2)
            {
                vertices = new List<Vector3>();
                triangles = new List<int>();
                int numU = Mathf.CeilToInt(1.0f / resolution);
                int numV = Mathf.CeilToInt(1.0f / resolution);
                for (int i = 0; i <= numU; i++)
                {
                    for (int j = 0; j <= numV; j++)
                    {
                        float u = i / (float)numU;
                        float v = j / (float)numV;
                        Vector3 position = PointOnSurface(u, v, true);
                        //GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        //obj.transform.localScale = Vector3.one * 0.2f;
                        //obj.transform.position = position;
                        vertices.Add(PointOnSurface(u, v, true));
                        if (i < numU && j < numV)
                        {
                            triangles.Add(i * (numV + 1) + j);
                            triangles.Add((i + 1) * (numV + 1) + j);
                            triangles.Add(i * (numV + 1) + j + 1);

                            triangles.Add((i + 1) * (numV + 1) + j);
                            triangles.Add((i + 1) * (numV + 1) + j + 1);
                            triangles.Add(i * (numV + 1) + j + 1);
                        }
                    }
                }
            }
            else
            {
                if (type == SurfaceType.BSPline)
                {
                    vertices = new List<Vector3>();
                    triangles = new List<int>();
                    int numU = Mathf.CeilToInt((knotsU[knotsU.Length - degreeU] - knotsU[degreeU]) / resolution);
                    int numV = Mathf.CeilToInt((knotsV[knotsV.Length - degreeV] - knotsU[degreeV]) / resolution);
                    for (int i = 0; i <= numU; i++)
                    {
                        for (int j = 0; j <= numV; j++)
                        {
                            float u = knotsU[degreeU] + (i / ((float)numU+1));
                            float v = knotsV[degreeV] + (j / ((float)numV+1));
                            if (i != numU && j != numV)
                            {
                                vertices.Add(PointOnSurface(u, v));
                            }
                            else if (i == numU && j == numV)
                            {
                                vertices.Add(controlPoints[countU - 1][countV - 1]);
                            }
                            else if (i == numU)
                            {
                                vertices.Add(PointOnSurface(knotsU[knotsU.Length - degreeU - 1], v));
                            }
                            else if (j == numV)
                            {
                                vertices.Add(PointOnSurface(u, knotsV[knotsV.Length - degreeV-1]));
                            }
                            if (i < numU && j < numV)
                            {
                                triangles.Add(i * (numV+1) + j);
                                triangles.Add(i * (numV+1) + j + 1);
                                triangles.Add((i + 1) * (numV+1) + j);

                                triangles.Add((i + 1) * (numV+1) + j);
                                triangles.Add(i * (numV+1) + j + 1);
                                triangles.Add((i + 1) * (numV+1) + j + 1);
                            }
                        }
                    }
                    mesh.vertices = vertices.ToArray();
                    mesh.triangles = triangles.ToArray();
                }
            }
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            meshFilter.mesh = mesh;
        }

        public Vector3 PointOnSurface(float u, float v, bool linear = false)
        {
            Vector3 point = new Vector3();

            if (!linear)
            {
                if (u > knotsU[knotsU.Length - degreeU - 1]) u = knotsU[knotsU.Length - degreeU - 1];
                if (v > knotsV[knotsV.Length - degreeV - 1]) v = knotsV[knotsV.Length - degreeV - 1];
                for (int i = 0; i < countU; i++)
                {
                    for (int j = 0; j < countV; j++)
                    {
                        Vector3 pt = new Vector3(controlPoints[i][j].x, controlPoints[i][j].y, controlPoints[i][j].z);
                        pt *= (Curve.faderBSPline(u, i, degreeU, knotsU) * Curve.faderBSPline(v, j, degreeV, knotsV));
                        point += pt;
                    }
                }
            }
            else
            {
                for (int i = 0; i < countU; i++)
                {
                    for (int j = 0; j < countV; j++)
                    {
                        Vector3 pt = new Vector3(controlPoints[i][j].x, controlPoints[i][j].y, controlPoints[i][j].z);
                        pt *= (Curve.FaderLerp(u, i, countU) * Curve.FaderLerp(v, j, countV));
                        point += pt;
                    }
                }
            }
            return point;
        }




        private void GetControlPointsFromTranssform()
        {
            controlPointTransforms = new Transform[controlPointsParent.childCount];
            countV = controlPointTransforms.Length / countU;
            controlPoints = new Vector3[countU][];
            prevPos = new Vector3[countU][];
            for (int i = 0; i < countU; i++)
            {
                controlPoints[i] = new Vector3[countV];
                prevPos[i] = new Vector3[countV];
                for (int j = 0; j < countV; j++)
                {
                    int index = i * countV + j;
                    controlPointTransforms[index] = controlPointsParent.GetChild(index);
                    controlPoints[i][j] = controlPointTransforms[index].localPosition;
                    prevPos[i][j] = controlPoints[i][j];
                }
            }
        }
    }
}
