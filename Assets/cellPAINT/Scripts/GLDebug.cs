using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class GLDebug : MonoBehaviour
{
    private struct Line
    {
        public Vector3 start;
        public Vector3 end;
        public Color color;
        public float startTime;
        public float duration;

        public Line(Vector3 start, Vector3 end, Color color, float startTime, float duration)
        {
            this.start = start;
            this.end = end;
            this.color = color;
            this.startTime = startTime;
            this.duration = duration;
        }

        public bool DurationElapsed(bool drawLine)
        {
            if (drawLine)
            {
                GL.Color(color);
                GL.Vertex(Camera.main.WorldToScreenPoint(start));
                GL.Vertex(Camera.main.WorldToScreenPoint(end));
            }
            return Time.time - startTime >= duration;
        }
    }

    private static GLDebug instance;
    private static Material matZOn;
    private static Material matZOff;

    public KeyCode toggleKey;
    public bool displayLines = true;
#if UNITY_EDITOR
    public bool displayGizmos = true;
#endif

    private List<Line> linesZOn;
    private List<Line> linesZOff;
    private float milliseconds;

    void Awake()
    {
        if (instance)
        {
            DestroyImmediate(this);
            return;
        }
        instance = this;
        SetMaterial();
        linesZOn = new List<Line>();
        linesZOff = new List<Line>();
    }

    private void _Clear(){
        instance.linesZOn.Clear();
        instance.linesZOff.Clear();
    }
    public void Clear(){
        _Clear();
    }
  
    int Count(){
        if (linesZOn.Count==0) return linesZOff.Count;
        return linesZOn.Count;
    }

    void SetMaterial()
    {
        Shader shader1 = Shader.Find("Custom/GLlineZOn");
        matZOn = new Material(shader1);

        matZOn.hideFlags = HideFlags.HideAndDontSave;
        matZOn.shader.hideFlags = HideFlags.HideAndDontSave;
        Shader shader2 = Shader.Find("Custom/GLlineZOff");
        matZOff = new Material(shader2);

        matZOff.hideFlags = HideFlags.HideAndDontSave;
        matZOff.shader.hideFlags = HideFlags.HideAndDontSave;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            displayLines = !displayLines;

        if (!displayLines)
        {
            Stopwatch timer = Stopwatch.StartNew();

            linesZOn = linesZOn.Where(l => !l.DurationElapsed(false)).ToList();
            linesZOff = linesZOff.Where(l => !l.DurationElapsed(false)).ToList();

            timer.Stop();
            milliseconds = timer.Elapsed.Ticks / 10000f;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        UnityEngine.Debug.Log("OnDrawGizmos");
        if (!displayGizmos || !Application.isPlaying)
            return;
        for (int i = 0; i < linesZOn.Count; i++)
        {
            Gizmos.color = linesZOn[i].color;
            Gizmos.DrawLine(linesZOn[i].start, linesZOn[i].end);
        }
        for (int i = 0; i < linesZOff.Count; i++)
        {
            Gizmos.color = linesZOff[i].color;
            Gizmos.DrawLine(linesZOff[i].start, linesZOff[i].end);
        }
    }
#endif

    void OnPostRender()
    {
        
        if (!displayLines) return;

        Stopwatch timer = Stopwatch.StartNew();
        GL.PushMatrix();
        GL.LoadOrtho();

        matZOn.SetPass(0);
        GL.Begin(GL.LINES);
        linesZOn = linesZOn.Where(l => !l.DurationElapsed(true)).ToList();
        GL.End();

        matZOff.SetPass(0);
        GL.Begin(GL.LINES);
        linesZOff = linesZOff.Where(l => !l.DurationElapsed(true)).ToList();
        GL.End();

        GL.PopMatrix();
        timer.Stop();
        milliseconds = timer.Elapsed.Ticks / 10000f;
    }

    private void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0, bool depthTest = false)
    {
        if (duration == 0 && !instance.displayLines)
            return;
        if (start == end)
            return;
        if (depthTest)
            instance.linesZOn.Add(new Line(start, end, color, Time.time, duration));
        else
            instance.linesZOff.Add(new Line(start, end, color, Time.time, duration));
    }

    /// <summary>
    /// Draw a line from start to end with color for a duration of time and with or without depth testing.
    /// If duration is 0 then the line is rendered 1 frame.
    /// </summary>
    /// <param name="start">Point in world space where the line should start.</param>
    /// <param name="end">Point in world space where the line should end.</param>
    /// <param name="color">Color of the line.</param>
    /// <param name="duration">How long the line should be visible for.</param>
    /// <param name="depthTest">Should the line be obscured by objects closer to the camera ?</param>
    public void DrawLine(Vector3 start, Vector3 end, Color? color = null, float duration = 0, bool depthTest = false)
    {
        DrawLine(start, end, color ?? Color.white, duration, depthTest);
    }
}