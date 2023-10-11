using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardZoom : MonoBehaviour
{
    public GameObject canvas;
    private GameObject zoomCard;


    private void Awake()
    {
        canvas = GameObject.Find("Canvas");
    }

    public void OnMouseDown()
    {
        zoomCard = Instantiate(gameObject, new Vector2(Input.mousePosition.x, Input.mousePosition.y), Quaternion.identity);
        zoomCard.transform.SetParent(canvas.transform, false);

        RectTransform rectTransform = zoomCard.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(500, 600);

        zoomCard.GetComponent<DragDrop>().enabled = false;
    }

    public void OnMouseUp()
    {
        Destroy(zoomCard);
    }
}

