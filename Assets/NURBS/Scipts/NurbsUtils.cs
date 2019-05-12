using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Collection of NURBS components for Unity 3d
/// Based on the code taught in Adaptive Architecture and Computation, The Bartlett, UCL, London
/// </summary>
namespace NurbsUnity
{
    public enum CurveType
    {
        Polyline = 0,
        Bezier = 1,
        BSPline = 2,
        NURBS = 3
    }

    public enum SurfaceType
    {
        BSPline = 0,
        NURBS = 2,
    }

    public enum Transforms
    {
        FromParent,
        AsList
    }

    /// <summary>
    /// Class with helper methods for NURBS
    /// </summary>
    public static class NurbsUtils
    {
        /// <summary>
        /// Returns the knot vector for fixed start and end
        /// </summary>
        /// <param name="cptsNum">The number of control points in that direction</param>
        /// <param name="degree">The degree in that direction</param>
        /// <returns>Float Array</returns>
        public static float[] Knots(int cptsNum, int degree)
        {
            int order = degree + 1;
            int numKnots = cptsNum + order;
            float[] knots = new float[numKnots];

            float counter = 0;
            float increment = 0.001f;
            float counterMid = 1.0f;

            for (int i=0; i<knots.Length; i++)
            {
                if (i < order)
                {
                    knots[i] = counter;
                    counter += increment;
                }
                else if (i >= knots.Length - order)
                {
                    counter -= increment;
                    knots[i] = 1.0f - counter;
                }
                else
                {
                    knots[i] = counterMid / (float)(cptsNum - degree);
                    counterMid += 1.0f;
                }
            }
            return knots;
        }

        /// <summary>
        /// Returns the Factorial of a number
        /// </summary>
        /// <param name="v">The number</param>
        /// <returns>Float</returns>
        public static float Factorial(int v)
        {
            float f = 1;
            for(int i=v; i>1; i--)
            {
                f *= i;
            }
            return f;
        }
        
    }
}
