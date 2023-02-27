using System;
using UnityEngine;

public class DrawRadar : MonoBehaviour
{
    #region Fields

    public Color lineColor = Color.red;
    public Color[] lineColorRange;
    public bool gradientColour = false;
    public float[] gradientValue;

    public bool drawLines = true;

    public bool centerWorld = true;
    public bool centerTransform = false;
    public bool centerTrigger = false;
    public bool centerRealWorld = false;



    public float circlePoints;
    public int circleCount;

    public int linePoints = 100;
    public int lineCount;

    public float[] radiusLine;
    public float[] radiusCircle;
    private static Material lineMaterial;

    private Vector3 center;

    #endregion Fields

    #region Methods


    public void OnRenderObject()
    {
        DrawCenterLine();
        DrawCircles();
    }



    public void Start()
    {
        if (centerWorld)
        {
            center = new Vector3(0, 0.25f, 0);
        }

        if(centerTransform)
        {
            center = new Vector3(transform.position.x, 0.25f, transform.position.z);
        }

    }

    private void Update()
    {

        if (centerTrigger)
        {
            center = new Vector3(transform.GetComponent<EventManager>().currentLocation.x, 0.25f, transform.GetComponent<EventManager>().currentLocation.z);
        }

        if (centerRealWorld)
        {
            center = new Vector3(transform.GetComponent<EventManager>().realCenter.x, 0.25f, transform.GetComponent<EventManager>().realCenter.z);
        }


    }


    private static void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    private void DrawCenterLine()
    {
        for (int i = 0; i < lineCount; i++)
        {
            CreateLineMaterial();
            // Apply the line material
            lineMaterial.SetPass(0);

            GL.PushMatrix();
            // Set transformation matrix for drawing to
            // match our transform


            // Draw lines
            GL.Begin(GL.LINES);
            if (gradientColour)
            {
                Color lineColorAdjusted = lineColor;
                lineColorAdjusted.a = gradientValue[i];
                GL.Color(lineColorAdjusted);
            }
            else
            {
                GL.Color(lineColorRange[i]);
            }
            for (int j = 0; j < linePoints; ++j)
            {
                float a = j / (float)linePoints;
                float angle = a * Mathf.PI * 2;
                // Vertex colors change from red to green

                // One vertex at transform position
                GL.Vertex3(center.x,center.y,center.z);
                // Another vertex at edge of circle
                GL.Vertex3(Mathf.Cos(angle) * radiusLine[i] + center.x, center.y, Mathf.Sin(angle) * radiusLine[i] + center.z);
            }
            GL.End();
            GL.PopMatrix();
        }
    }

    private void DrawCircles()
    {
        for (int i = 0; i < circleCount; i++)
        {
            GL.PushMatrix();
            lineMaterial.SetPass(0);
            GL.Begin(GL.LINES);
            if (gradientColour)
            {
                Color lineColorAdjusted = lineColor;
                lineColorAdjusted.a = gradientValue[i];
                GL.Color(lineColorAdjusted);
            }
            else
            {
                GL.Color(lineColorRange[i]);
            }

            for (int j = 0; j < circlePoints - 1; ++j)
            {
                float a = j / (float)circlePoints;
                float angle = a * Mathf.PI * 2;
                Vector3 ci = (new Vector3(Mathf.Cos(angle) * radiusCircle[i] + center.x, center.y, Mathf.Sin(angle) * radiusCircle[i] + center.z));
                GL.Vertex3(ci.x, ci.y, ci.z);
            }
            for (int j = 0; j < circlePoints - 1; ++j)
            {
                float a = j / (float)circlePoints;
                float angle = a * Mathf.PI * 2;
                Vector3 ci = (new Vector3(Mathf.Cos(angle) * radiusCircle[i] + center.x, center.y, Mathf.Sin(angle) * radiusCircle[i] + center.z));
         
                GL.Vertex3(ci.x, ci.y, ci.z);
            }

            GL.End();
            GL.PopMatrix();
        }
    }

    private void OnValidate()
    {
        if (radiusLine.Length != lineCount)
        {
            Array.Resize(ref radiusLine, lineCount);
        }

        if (radiusCircle.Length != circleCount)
        {
            Array.Resize(ref radiusCircle, circleCount);
        }

        if (gradientColour)
        {
            Array.Resize(ref gradientValue, circleCount);
            AdjustGradient();
        }
        else
        {
            Array.Resize(ref lineColorRange, circleCount);
        }
    }

    private void AdjustGradient()
    {
        for (int i = 1; i < gradientValue.Length; i++)
        {
            float gradient = 1f - (i * (1f / gradientValue.Length));
         
            gradientValue[i] = gradient;

            if (gradientValue[i] == 0)
            {
                gradientValue[i] = 1f;
                Debug.Log("YES!");
            }
        }
    }

    private void OnApplicationQuit()
    {
        DestroyImmediate(lineMaterial);
    }

    #endregion Methods
}