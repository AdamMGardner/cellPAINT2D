using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveCarret : MonoBehaviour
{
    public bool moved = false;
    public Image the_image;
    public Transform the_carret;
    void MoveCarretInputField(){
        if (the_carret) {
            the_carret.transform.SetAsLastSibling();
            moved = true;
        }
    }

    void MoveImageInputField(){
        if (the_image) {
            var sib = the_image.transform.GetSiblingIndex();
            if (sib != 0) {
                the_image.transform.SetSiblingIndex(0);
                moved = true;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!the_image) the_image = GetComponentInChildren<Image>();
        if (!the_carret) the_carret = transform.Find(gameObject.name+" Input Caret");
        MoveCarretInputField();
    }

    // Update is called once per frame
    void Update()
    {
        if (!the_carret) the_carret = transform.Find(gameObject.name+" Input Caret");
        if (the_carret && !moved) MoveCarretInputField();
    }
}
