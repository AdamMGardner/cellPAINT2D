using UnityEngine;
using System.Collections;

public class GUI_Controle : MonoBehaviour {
	
	private int preNo = -1;
	private int currNo = 0;
	private int nextNo = 1;
	private bool[] moveTo = new bool[6];
	
	public GameObject[] cards;
	public GameObject[] lerpPoints;
	public float ftimeInTo;
		
	

	// Use this for initialization
	void Start () 
	{
		moveTo[0] = false;
		moveTo[1] = false;
		moveTo[2] = false;
		moveTo[3] = false;
		moveTo[4] = false;
		moveTo[5] = false;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(moveTo[0])
		{
			cards[0].transform.position = Vector3.Lerp(cards[0].transform.position , lerpPoints[0].transform.position , Time.deltaTime * ftimeInTo);
			cards[1].transform.position = Vector3.Lerp(cards[1].transform.position , lerpPoints[1].transform.position , Time.deltaTime * ftimeInTo);
			cards[2].transform.position = Vector3.Lerp(cards[2].transform.position , lerpPoints[2].transform.position , Time.deltaTime * ftimeInTo);
			cards[3].transform.position = Vector3.Lerp(cards[3].transform.position , lerpPoints[3].transform.position , Time.deltaTime * ftimeInTo);
			cards[4].transform.position = Vector3.Lerp(cards[4].transform.position , lerpPoints[4].transform.position , Time.deltaTime * ftimeInTo);
			cards[5].transform.position = Vector3.Lerp(cards[5].transform.position , lerpPoints[5].transform.position , Time.deltaTime * ftimeInTo);
		}
		if(moveTo[1])
		{
			cards[0].transform.position = Vector3.Lerp(cards[0].transform.position , lerpPoints[6].transform.position , Time.deltaTime * ftimeInTo);
			cards[1].transform.position = Vector3.Lerp(cards[1].transform.position , lerpPoints[0].transform.position , Time.deltaTime * ftimeInTo);
			cards[2].transform.position = Vector3.Lerp(cards[2].transform.position , lerpPoints[1].transform.position , Time.deltaTime * ftimeInTo);
			cards[3].transform.position = Vector3.Lerp(cards[3].transform.position , lerpPoints[2].transform.position , Time.deltaTime * ftimeInTo);
			cards[4].transform.position = Vector3.Lerp(cards[4].transform.position , lerpPoints[3].transform.position , Time.deltaTime * ftimeInTo);
			cards[5].transform.position = Vector3.Lerp(cards[5].transform.position , lerpPoints[4].transform.position , Time.deltaTime * ftimeInTo);
		}
		if(moveTo[2])
		{
			cards[0].transform.position = Vector3.Lerp(cards[0].transform.position , lerpPoints[7].transform.position , Time.deltaTime * ftimeInTo);
			cards[1].transform.position = Vector3.Lerp(cards[1].transform.position , lerpPoints[6].transform.position , Time.deltaTime * ftimeInTo);
			cards[2].transform.position = Vector3.Lerp(cards[2].transform.position , lerpPoints[0].transform.position , Time.deltaTime * ftimeInTo);
			cards[3].transform.position = Vector3.Lerp(cards[3].transform.position , lerpPoints[1].transform.position , Time.deltaTime * ftimeInTo);
			cards[4].transform.position = Vector3.Lerp(cards[4].transform.position , lerpPoints[2].transform.position , Time.deltaTime * ftimeInTo);
			cards[5].transform.position = Vector3.Lerp(cards[5].transform.position , lerpPoints[3].transform.position , Time.deltaTime * ftimeInTo);			
		}
		if(moveTo[3])
		{
			cards[0].transform.position = Vector3.Lerp(cards[0].transform.position , lerpPoints[8].transform.position , Time.deltaTime * ftimeInTo);
			cards[1].transform.position = Vector3.Lerp(cards[1].transform.position , lerpPoints[7].transform.position , Time.deltaTime * ftimeInTo);
			cards[2].transform.position = Vector3.Lerp(cards[2].transform.position , lerpPoints[6].transform.position , Time.deltaTime * ftimeInTo);
			cards[3].transform.position = Vector3.Lerp(cards[3].transform.position , lerpPoints[0].transform.position , Time.deltaTime * ftimeInTo);
			cards[4].transform.position = Vector3.Lerp(cards[4].transform.position , lerpPoints[1].transform.position , Time.deltaTime * ftimeInTo);
			cards[5].transform.position = Vector3.Lerp(cards[5].transform.position , lerpPoints[2].transform.position , Time.deltaTime * ftimeInTo);			
		}
		if(moveTo[4])
		{
			cards[0].transform.position = Vector3.Lerp(cards[0].transform.position , lerpPoints[9].transform.position , Time.deltaTime * ftimeInTo);
			cards[1].transform.position = Vector3.Lerp(cards[1].transform.position , lerpPoints[8].transform.position , Time.deltaTime * ftimeInTo);
			cards[2].transform.position = Vector3.Lerp(cards[2].transform.position , lerpPoints[7].transform.position , Time.deltaTime * ftimeInTo);
			cards[3].transform.position = Vector3.Lerp(cards[3].transform.position , lerpPoints[6].transform.position , Time.deltaTime * ftimeInTo);
			cards[4].transform.position = Vector3.Lerp(cards[4].transform.position , lerpPoints[0].transform.position , Time.deltaTime * ftimeInTo);
			cards[5].transform.position = Vector3.Lerp(cards[5].transform.position , lerpPoints[1].transform.position , Time.deltaTime * ftimeInTo);			
		}
		if(moveTo[5])
		{
			cards[0].transform.position = Vector3.Lerp(cards[0].transform.position , lerpPoints[10].transform.position , Time.deltaTime * ftimeInTo);
			cards[1].transform.position = Vector3.Lerp(cards[1].transform.position , lerpPoints[9].transform.position , Time.deltaTime * ftimeInTo);
			cards[2].transform.position = Vector3.Lerp(cards[2].transform.position , lerpPoints[8].transform.position , Time.deltaTime * ftimeInTo);
			cards[3].transform.position = Vector3.Lerp(cards[3].transform.position , lerpPoints[7].transform.position , Time.deltaTime * ftimeInTo);
			cards[4].transform.position = Vector3.Lerp(cards[4].transform.position , lerpPoints[6].transform.position , Time.deltaTime * ftimeInTo);
			cards[5].transform.position = Vector3.Lerp(cards[5].transform.position , lerpPoints[0].transform.position , Time.deltaTime * ftimeInTo);			
		}		
	}
	
	
	void OnGUI()
	{
		if(GUI.Button(new Rect(10.0f,10.0f,50.0f,100.0f),"Back"))
		{
			if(currNo > 0)
			{
				nextNo +=1;
				currNo = preNo;
				preNo -=1;
				UpdatePosition(currNo);
			}				
		}
		if(GUI.Button(new Rect(10.0f,110.0f,50.0f,100.0f),"Next"))
		{
			if(currNo < 5)
			{
				preNo = currNo;
				currNo += 1;
				nextNo +=1;
				UpdatePosition(currNo);
			}			
		}		
	}
	
	void UpdatePosition(int switchNo)
	{
		switch(switchNo)
		{
			case 0:
				moveTo[0] = true;
				moveTo[1] = false;
				moveTo[2] = false;
				moveTo[3] = false;
				moveTo[4] = false;
				moveTo[5] = false;				
				break;
			case 1:
				moveTo[0] = false;
				moveTo[1] = true;
				moveTo[2] = false;
				moveTo[3] = false;
				moveTo[4] = false;
				moveTo[5] = false;				
				break;
			case 2:
				moveTo[0] = false;
				moveTo[1] = false;
				moveTo[2] = true;
				moveTo[3] = false;
				moveTo[4] = false;
				moveTo[5] = false;				
				break;			
			case 3:
				moveTo[0] = false;
				moveTo[1] = false;
				moveTo[2] = false;
				moveTo[3] = true;
				moveTo[4] = false;
				moveTo[5] = false;				
			break;			
			case 4:
				moveTo[0] = false;
				moveTo[1] = false;
				moveTo[2] = false;
				moveTo[3] = false;
				moveTo[4] = true;
				moveTo[5] = false;				
				break;			
			case 5:
				moveTo[0] = false;
				moveTo[1] = false;
				moveTo[2] = false;
				moveTo[3] = false;
				moveTo[4] = false;
				moveTo[5] = true;				
				break;			
		}
	}
}
