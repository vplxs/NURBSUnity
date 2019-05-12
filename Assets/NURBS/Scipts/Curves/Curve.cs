using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NurbsUnity
{
    /// <summary>
    /// A MonoBehaviour component for NURBS curves
    /// Based on the code taught in Adaptive Architecture and Computation, The Bartlett, UCL, London
    /// </summary>
    [AddComponentMenu("NURBS/Curve")]
    [RequireComponent(typeof(LineRenderer))]
    public class Curve : MonoBehaviour
    {

        #region Public Properties
        public int degree = 3;
        public CurveType type = CurveType.Polyline;
        public float resolution = 0.01f;
        public Transforms transforms = Transforms.FromParent;
        public Transform[] controlPointTransforms;
        public Transform controlPointsParent;
        public float[] weights;
        public bool initializeOnStart = false;
        public bool autoUpdate = true;

        public Vector3[] controlPoints { get; set; }
        public float[] knots;
        public Vector3[] curvePoints
        {
            get
            {
                Vector3[] temp = new Vector3[lineRenderer.positionCount];
                for (int i = 0; i < temp.Length; i++)
                {
                    temp[i] = lineRenderer.GetPosition(i);
                }
                return temp;
            }
        }
        #endregion

        #region Private properties
        private LineRenderer lineRenderer { get; set; }
        private Vector3[] prevPos { get; set; }
        #endregion

        // Use this for initialization
        void Awake()
        {
            if (initializeOnStart)
            {
                Initialize();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (autoUpdate)
            {
                for (int i = 0; i < prevPos.Length; i++)
                {
                    controlPoints[i] = controlPointTransforms[i].localPosition;
                    if (controlPoints[i] != prevPos[i])
                    {
                        UpdateCurve();
                        prevPos[i] = controlPoints[i];
                    }
                }
            }
        }

        #region Public Methods
        /// <summary>
        /// Initializes the Curve
        /// </summary>
        public void Initialize()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;
            if (transforms == Transforms.FromParent)
            {
                GetControlPointsFromTransform();
            }
            else
            {
                if (controlPointTransforms != null && controlPointTransforms.Length != 0 && (controlPoints == null || controlPoints.Length == 0))
                {
                    GetControlPointsFromTransforms();
                }
            }
            knots = NurbsUtils.Knots(controlPoints.Length, degree);
            UpdateCurve();
        }

        /// <summary>
        /// Updates the geometry of the Curve
        /// </summary>
        public void UpdateCurve()
        {
            if (type == CurveType.NURBS)
            {
                int num = Mathf.CeilToInt((knots[knots.Length - degree - 1] - knots[degree]) / resolution);
                lineRenderer.positionCount = num + 1;
                for (int i = 0; i <= num; i++)
                {
                    if (i != num)
                    {
                        float u = knots[degree] + (i / (float)num);
                        lineRenderer.SetPosition(i, CurvePos(u));
                    }
                    else
                    {
                        lineRenderer.SetPosition(i, controlPoints.Last());
                    }
                }
            }
            else
            {
                int num = Mathf.CeilToInt(1.0f / resolution);
                lineRenderer.positionCount = num + 1;
                for (int i = 0; i <= num; i++)
                {
                    float u = i / (float)num;
                    lineRenderer.SetPosition(i, CurvePos(u));
                }
            }
        }

        /// <summary>
        /// Returns a point on the curve based on a normalized parameter
        /// </summary>
        /// <param name="parameter">The curve parameter (0-1)</param>
        /// <returns>Vector3</returns>
        public Vector3 PointOnCurve(float parameter)
        {
            return CurvePos(parameter);
        }

        /// <summary>
        /// Returns a point on the curve based on a normalized parameter
        /// </summary>
        /// <param name="parameter">The curve parameter (0-1)</param>
        /// <param name="frame">The plane with normal the tangent of the curve at the specific point</param>
        /// <returns>Vector3</returns>
        public Vector3 PointOnCurve(float parameter, out Plane frame)
        {
            float step = 0.0001f;
            if (parameter + step >= 1)
            {
                var pos0 = CurvePos(parameter);
                var pos1 = CurvePos(parameter + step);
                Vector3 normal = (pos1 - pos0).normalized;
                frame = new Plane(normal, pos0);
                return pos0;
            }
            else
            {
                step *= -1;
                var pos0 = CurvePos(parameter + step);
                var pos1 = CurvePos(parameter);
                Vector3 normal = (pos1 - pos0).normalized;
                frame = new Plane(normal, pos1);
                return pos1;
            }
        }

        public List<Vector3> GetFaders(out List<Color> colors)
        {

            if (type == CurveType.NURBS)
            {
                int num = Mathf.CeilToInt((knots[knots.Length - degree - 1] - knots[degree]) / resolution);
                List<Vector3> faderPos = new List<Vector3>();
                colors = new List<Color>();
                for (int k = 0; k < controlPoints.Length; k++)
                {
                    for (int i = 0; i <= num; i++)
                    {
                        float u = knots[degree] + (i / (float)num);
                        faderPos.Add(new Vector3(u, FaderPos(u, k)));
                        colors.Add(Color.HSVToRGB(k / (float)controlPoints.Length, 1, 1));
                    }
                }
                return faderPos;
            }
            else
            {
                int num = Mathf.CeilToInt(1.0f / resolution);
                List<Vector3> faderPos = new List<Vector3>();
                colors = new List<Color>();
                for (int k = 0; k < controlPoints.Length; k++)
                {
                    for (int i = 0; i <= num; i++)
                    {
                        float u = i / (float)num;
                        faderPos.Add(new Vector3(u, FaderPos(u, k)));
                        colors.Add(Color.HSVToRGB(k / (float)controlPoints.Length, 1, 1));
                    }
                }
                return faderPos;
            }
        }
        #endregion

        #region Private Methods
        private void GetControlPointsFromTransforms()
        {
            controlPoints = new Vector3[controlPointTransforms.Length];
            prevPos = new Vector3[controlPoints.Length];
            for (int i = 0; i < controlPointTransforms.Length; i++)
            {
                controlPoints[i] = controlPointTransforms[i].localPosition;
                prevPos[i] = controlPoints[i];
            }
        }

        private void GetControlPointsFromTransform()
        {
            controlPointTransforms = new Transform[controlPointsParent.childCount];
            controlPoints = new Vector3[controlPointTransforms.Length];
            prevPos = new Vector3[controlPoints.Length];
            for (int i = 0; i < controlPointTransforms.Length; i++)
            {
                controlPointTransforms[i] = controlPointsParent.GetChild(i);
                controlPoints[i] = controlPointTransforms[i].localPosition;
                prevPos[i] = controlPoints[i];
            }
        }

        /// <summary>
        /// Returns the position in space at a specific parameteer of the curve
        /// </summary>
        /// <param name="u">The parameter of the curve</param>
        /// <returns>Vector3</returns>
        private Vector3 CurvePos(float u)
        {
            Vector3 pt = new Vector3();
            int num = controlPoints.Length;
            for (int i = 0; i < controlPoints.Length; i++)
            {
                Vector3 temp = new Vector3(controlPoints[i].x, controlPoints[i].y, controlPoints[i].z);
                float multiplier = 1.0f;
                switch (type)
                {
                    case CurveType.Polyline:
                        multiplier = FaderLerp(u, i, num);
                        break;
                    case CurveType.Bezier:
                        multiplier = FaderBezier(u, i, num);
                        break;
                    case CurveType.BSPline:
                        multiplier = faderBSPline(u, i, degree, knots);
                        break;
                    case CurveType.NURBS:
                        multiplier = faderBSPline(u, i, degree, knots);
                        break;
                }
                temp *= multiplier;
                pt += temp;
            }
            return pt;
        }

        private float FaderPos(float u, int i)
        {
            int num = controlPoints.Length;
            float multiplier = 0.0f;
            switch (type)
            {
                case CurveType.Polyline:
                    multiplier = FaderLerp(u, i, num);
                    break;
                case CurveType.Bezier:
                    multiplier = FaderBezier(u, i, num);
                    break;
                case CurveType.BSPline:
                    multiplier = faderBSPline(u, i, degree, knots);
                    break;
                case CurveType.NURBS:
                    multiplier = faderBSPline(u, i, degree, knots);
                    break;
            }
            return multiplier;
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Returns the fader value for a specific polyline-curve parameter
        /// </summary>
        /// <param name="u">The curve parameter</param>
        /// <param name="i">the index of the control point in check</param>
        /// <param name="cptsNum">The number of control points</param>
        /// <returns>Float</returns>
        public static float FaderLerp(float u, int i, int cptsNum)
        {
            float faderLvl = 0;
            int n = cptsNum - 1;
            u *= n;
            if (i == Mathf.FloorToInt(u))
            {
                if (Mathf.Ceil(u) == u)
                {
                    faderLvl = 1;
                }
                else
                {
                    faderLvl = Mathf.Ceil(u) - u;
                }
            }
            else if (i == Mathf.CeilToInt(u))
            {
                if (Mathf.Floor(u) == u)
                {
                    faderLvl = 1;
                }
                else
                {
                    faderLvl = u - Mathf.Floor(u);
                }
            }
            return faderLvl;
        }

        /// <summary>
        /// Returns the fader level for a specific Bezier-curve parameter
        /// </summary>
        /// <param name="u">The curve parameter</param>
        /// <param name="i">the index of the control point in check</param>
        /// <param name="cptsNum">The number of control points</param>
        /// <returns>Float</returns>
        public static float FaderBezier(float u, int i, int cptsNum)
        {
            int n = cptsNum - 1;
            float faderLvl = Mathf.Pow(u, i) * Mathf.Pow(1 - u, n - i) * NurbsUtils.Factorial(n) / (NurbsUtils.Factorial(i) * NurbsUtils.Factorial(n - i));
            return faderLvl;
        }

        /// <summary>
        /// Returns the fader level for a specific NURBS-curve parameter
        /// </summary>
        /// <param name="u">The curve parameter</param>
        /// <param name="i">the index of the control point in check</param>
        /// <param name="degree">The degree of the curve</param>
        /// <param name="knots">The knot vector of the curve</param>
        /// <returns>Float</returns>
        public static float faderBSPline(float u, int i, int degree, float[] knots)
        {
            return basisn(u, i, degree, knots);
        }
        private static float basisn(float u, int i, int degree, float[] knots)
        {
            if (degree == 0)
            {
                return basis0(u, i, knots);
            }
            else
            {
                float b1 = basisn(u, i, degree - 1, knots) * (u - knots[i]) / (knots[i + degree] - knots[i]);
                float b2 = basisn(u, i + 1, degree - 1, knots) * (knots[i + degree + 1] - u) / (knots[i + degree + 1] - knots[i + 1]);
                return b1 + b2;
            }
        }
        private static float basis0(float u, int k, float[] knots)
        {
            if (u >= knots[k] && u < knots[k + 1]) return 1;
            else return 0;
        }
        #endregion

    }
}
