using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class AutocompleteInputField : MonoBehaviour
{
    public InputField inputField;
    public RectTransform resultsParent;
    public RectTransform prefab;
    public List<string> options= new List<string>();
    public bool no_filter = false;//always show all options
    private void Awake()
    {
        inputField.onValueChanged.AddListener( OnInputValueChanged );
        inputField.onEndEdit.AddListener (OnInputEnd);
    }

    private void OnInputValueChanged( string newText )
    {
        ClearResults();
        FillResults( GetResults( newText ) );
    }

    private void OnInputEnd( string newText) {
        resultsParent.GetComponent<Image>().enabled = false;
        ClearResults();
    }

    public void OnPClick(BaseEventData aevent) {
        ClearResults();
        FillResults( GetResults( "" ) );        
    }

    private void ClearResults()
    {
        // Reverse loop since you destroy children
        for( int childIndex = resultsParent.childCount - 1 ; childIndex >= 0 ; --childIndex )
        {
            Transform child = resultsParent.GetChild( childIndex );
            child.SetParent( null );
            Destroy( child.gameObject );
        }
    }

    private void FillResults(List<string> results)
    {
        resultsParent.GetComponent<Image>().enabled = (results.Count != 0);
        for (int resultIndex = 0 ; resultIndex < results.Count ; resultIndex++)
        {
            RectTransform child = Instantiate( prefab ) as RectTransform;
            child.GetComponentInChildren<Text>().text = results[resultIndex];
            child.SetParent( resultsParent );
        }
    }

    private List<string> GetResults( string input )
    {
        if (no_filter) input = "";
        List<string> result = new List<string>();
        result = options.FindAll( (str) => str.IndexOf( input ) >= 0 );
        return result;
    }

    public void Reset(string input ){
        ClearResults();
        FillResults( GetResults( input ) );
    }
}