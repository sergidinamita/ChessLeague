using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class DragDrop : MonoBehaviour
{
    public bool isDragging = false;
    private Vector3 startPosition;
    public bool isOverDropZone = false;
    public GameObject dropZone;
    private Camera uiCamera; // Referencia a la cámara de la interfaz de usuario
    private Camera mainCamera; // Referencia a la cámara principal
    private Vector3 endPosition;

    public GameObject CardBanner;

    public GameObject effect;

    private void Start()
    {
        // Obtener la referencia a la cámara de la interfaz de usuario desde algún objeto en tu escena
        uiCamera = GameObject.Find("UICamera").GetComponent<Camera>();
        dropZone = GameObject.Find("PlayArea");

    }

    private void Update()
    {
        if (isDragging)
        {

            mainCamera = GameObject.Find("White Cam").GetComponent<Camera>();

            mainCamera.GetComponent<CameraController>().enabled = false;

            Vector3 mousePosition = uiCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 10f; // Mantener la misma profundidad
            transform.position = mousePosition;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject == dropZone)
        {
            isOverDropZone = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject == dropZone)
        {
            isOverDropZone = false;
        }
    }

    public void StartDrag()
    {
        if (this.GetComponent<Card>().player == GameObject.Find("Board").GetComponent<Board>().player && GetComponent<Card>().player == GameObject.Find("Board").GetComponent<Board>().turn)
        {
            startPosition = transform.position;
            isDragging = true;
        }
    }

    public void EndDrag()
    {
        if (this.GetComponent<Card>().player == GameObject.Find("Board").GetComponent<Board>().player && GetComponent<Card>().player == GameObject.Find("Board").GetComponent<Board>().turn)
        {
            isDragging = false;
            if (isOverDropZone)
            {
                Instantiate(effect, transform.position, Quaternion.identity);
                gameObject.GetComponent<Card>().ActivateEffect();

                if (GameObject.Find("Board").GetComponent<Board>().player == 0)
                {
                    RealmController.Instance.DeleteWhiteCard(GetComponent<Card>().id);
                }
                else
                {
                    RealmController.Instance.DeleteBlackCard(GetComponent<Card>().id);
                }

                StartCoroutine(UsedCardAnimation());
            }
            else
            {
                endPosition = startPosition;
                StartCoroutine(MoveCardBackToOrigin());
            }

            mainCamera = GameObject.Find("White Cam").GetComponent<Camera>();

            mainCamera.GetComponent<CameraController>().enabled = true;
        }
    }



    public IEnumerator UsedCardAnimation()
    {
        //make it child of canvas
        transform.SetParent(dropZone.transform, false);
        //move it to the center of the canvas
        transform.position = dropZone.transform.position;


        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.localScale = new Vector3(3f, 3f, 3f);
        yield return new WaitForSeconds(2f);

        //move it to the right of the canvas
        float duration = 1f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(dropZone.transform.position.x + 10f, dropZone.transform.position.y, dropZone.transform.position.z), elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }


        //destroy it
        Destroy(gameObject);
    }


    private IEnumerator MoveCardBackToOrigin()
    {
        float duration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(transform.position, endPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;
    }




}
