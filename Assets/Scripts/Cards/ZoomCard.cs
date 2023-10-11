using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomCard : MonoBehaviour
{
    private Vector3 originalPosition;
    bool isZoomed = false;

    private void Start()
    {
    }
    public void OnTap()
    {
        if (!GetComponent<DragDrop>().isDragging)
        {
            if (isZoomed)
            {
                transform.localScale = new Vector3(1, 1, 1);
                StartCoroutine(MoveCardBack());
                isZoomed = false;

            }
            else
            {
                originalPosition = transform.position;

                //Check if parent is Enemy
                if (transform.parent.name == "Enemy")
                    transform.localScale = new Vector3(3.3f, 3.3f, 3.3f);
                else
                    transform.localScale = new Vector3(3f, 3f, 3f);
                StartCoroutine(MoveCardCenter());
                isZoomed = true;
            }

            var dragDrop = GetComponent<DragDrop>();
            if (dragDrop != null)
            {
                dragDrop.enabled = !isZoomed;
                dragDrop.isOverDropZone = !isZoomed;
            }

            var inputManager = GameObject.Find("Board").GetComponent<InputManager>();
            if (inputManager != null)
            {
                inputManager.enabled = !isZoomed;
            }
        }
    }

    private IEnumerator MoveCardCenter()
    {
        float duration = 0.5f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(transform.position, GameObject.Find("PlayArea").transform.position, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = GameObject.Find("PlayArea").transform.position;
    }


    private IEnumerator MoveCardBack()
    {
        float duration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(transform.position, originalPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
    }
}
