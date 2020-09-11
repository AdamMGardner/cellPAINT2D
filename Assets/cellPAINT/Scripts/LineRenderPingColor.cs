using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRenderPingColor : MonoBehaviour {
    public Color start = Color.cyan;
    public Color end = Color.blue;
    public LineRenderer line;
    public float alpha = 0.75f;
    // Use this for initialization

    void Start () {
        if (line ==null) line = GetComponent<LineRenderer>();
        Gradient gradient = new Gradient();
        gradient.mode = GradientMode.Fixed;
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(start, 0.5f), new GradientColorKey(end, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.5f), new GradientAlphaKey(alpha, 0.5f) }
            );
        line.colorGradient = gradient;
    }
	
	// Update is called once per frame
	void Update () {
        if (!line.enabled) return;
        Color lerpedColor1 = Color.Lerp(start, end, Mathf.PingPong(Time.time, 1));
        Color lerpedColor2 = Color.Lerp(end, start, Mathf.PingPong(Time.time, 1));
        Gradient gradient = new Gradient();
        gradient.mode = GradientMode.Fixed;
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(lerpedColor1, 0.5f), new GradientColorKey(lerpedColor2, 1.0f)},
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.5f), new GradientAlphaKey(alpha, 0.5f) }
            );
        line.colorGradient = gradient;
    }
}
