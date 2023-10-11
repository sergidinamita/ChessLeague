using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCard : MonoBehaviour
{
    public GameObject[] deck;
    public GameObject meArea;
    public GameObject enemyArea;
    public int deckSize = 5;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void OnClick()
    {
        for (int i = 0; i < deckSize; i++)
        {
            GameObject playerCard = Instantiate(deck[Random.Range(0, deck.Length)]);
            playerCard.transform.SetParent(meArea.transform, false);
        }

        for (int i = 0; i < deckSize; i++)
        {
            GameObject enemyCard = Instantiate(deck[Random.Range(0, deck.Length)]);
            enemyCard.transform.SetParent(enemyArea.transform, false);
        }
    }

}
