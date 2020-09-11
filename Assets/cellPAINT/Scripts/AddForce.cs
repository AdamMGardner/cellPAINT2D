using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class AddForce : MonoBehaviour {
    public Rigidbody2D player;
    public float timeScale=1.0f;
    private Slider cs;

    void Start()
    {
        player = GetComponent<Rigidbody2D>();
        cs = GameObject.Find("Slider").GetComponent<Slider>();
    }

    void FixedUpdate()
    {
        //toggle drag ?
        if (player)
        {
           
            player.drag = 20.0f;
            player.angularDrag = 20.0f;
            //player.AddForce(new Vector2(Random.Range(-timeScale, timeScale), Random.Range(-timeScale, timeScale)) * cs.value);
            player.AddTorque(Random.Range(-(timeScale), (timeScale)) * (cs.value / 2), 0);
            player.AddForce(UnityEngine.Random.insideUnitCircle * cs.value);
        }
    }
}
