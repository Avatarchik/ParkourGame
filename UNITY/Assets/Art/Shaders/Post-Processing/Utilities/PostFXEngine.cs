using UnityEngine;
using System.Collections;

#region Namespace
namespace PostFXEngine
{
    public static class Properties
    {
        public static Material material;

        public static Material Tonemap
        {
            get { return material; }
            set { material = value; }
        }
    }

    public static class Shading
    {
        public static Material GetMaterial(Material mat, Shader shader)
        {
            if (!mat)
            {
                mat = new Material(shader);
                mat.hideFlags = HideFlags.HideAndDontSave;
            }

            return mat;
        }

    }

    public static class Math
    {
        public static int Max(float a, float b)
        {
            if (a > b) return (int)a;
            return (int)b;
        }
    }

}
#endregion
