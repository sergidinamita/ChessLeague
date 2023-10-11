using Realms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class Board : MonoBehaviour
{
    public int[,] board = new int[8, 8];
    public int turn = 0;
    public int player = 0;
    public int turns = 0;
    private bool started;

    [SerializeField] Material whiteMaterial;
    [SerializeField] Material blackMaterial;


    [SerializeField] GameObject king;
    [SerializeField] GameObject queen;
    [SerializeField] GameObject bishop;
    [SerializeField] GameObject knight;
    [SerializeField] GameObject rook;
    [SerializeField] GameObject pawn;

    readonly GameObject[] Pieces = new GameObject[7];

    [SerializeField] GameObject wallObject;
    public bool cardBeingPlayed = false;


    [SerializeField] Canvas checkMateText;

    public GameObject selectedPieceCache;

    public bool whiteWin = false;
    public bool blackWin = false;

    public GameObject[] deck;

    private int[] whiteCardsId;
    private int[] blackCardsId;

    public GameObject meArea;
    public GameObject enemyArea;
    public int deckSize = 5;

    [SerializeField] BaseSounds soundManager;

    [SerializeField] Canvas winCanvas;
    [SerializeField] Canvas loseCanvas;
    [SerializeField] Canvas mainCanvas;

    //private bool loaded = false;
    private float lastRefresh;

    private void Awake()
    {
        /*
        if (!Connection.Instance.Host)
        {
            GameObject.Find("Loading").SetActive(false);
        } else
        {
            GameObject.Find("Loading").SetActive(true);
        }
        */
        soundManager = GameObject.Find("AudioManager").GetComponent<BaseSounds>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Pieces[0] = king;
        Pieces[1] = queen;
        Pieces[2] = bishop;
        Pieces[3] = knight;
        Pieces[4] = rook;
        Pieces[5] = pawn;
        Pieces[6] = wallObject;


        if (!Connection.Instance.Host)
        {
            player = 1;
            CheckIfGameIsCreated();
        }

        whiteCardsId = RealmController.Instance.GetWhiteCards();
        blackCardsId = RealmController.Instance.GetBlackCards();

        RefreshTablero();


        //Debug.LogWarning(Connection.Instance.GameId);

        // Recuperar Realm el Realm crea toda la partida
        if (RealmController.Instance.IsRealmReady())
        {
            GameObject.Find("GameID").GetComponent<TextMeshProUGUI>().text = RealmController.Instance.GetGameID();

            if (RealmController.Instance.GetWhitePlayerId() != null)
            {
                InicialitzarTablero();
                InicializarPiezas();
                RevalidarCartas();
                started = true;
            }
        }

        if (GameObject.Find("GameID").GetComponent<TextMeshProUGUI>().text == "9999")
        {
            CheckIfGameIsCreated();

            Destroy(GameObject.Find("RealmControllerManager"));
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void RefreshTablero()
    {
        if (started && RealmController.Instance.IsRealmReady())
        {
            int[,] recoveredBoard = RealmController.Instance.GetBoard();
            if (recoveredBoard != board)
            {
                board = recoveredBoard;

                RevalidarPosiciones();

                soundManager.GetComponent<BaseSounds>().PlayMovePieceSound();
            }
        }
    }

    public void RefreshCards()
    {
        if (started && RealmController.Instance.IsRealmReady())
        {
            int[] recoveredWhiteCards = RealmController.Instance.GetWhiteCards();
            int[] recoveredBlackCards = RealmController.Instance.GetBlackCards();

            if (whiteCardsId != recoveredWhiteCards || blackCardsId != recoveredBlackCards)
            {
                whiteCardsId = recoveredWhiteCards;
                blackCardsId = recoveredBlackCards;

                RevalidarCartas();
            }
        }
    }

    public void RevalidarCartas()
    {
        //Destroy all childs of meArea
        foreach (Transform child in meArea.transform)
        {
            Destroy(child.gameObject);
        }

        //Destroy all childs of enemyArea
        foreach (Transform child in enemyArea.transform)
        {
            Destroy(child.gameObject);
        }


        if (Connection.Instance.Host) //Perspectiva host
        {
            for (int i = 0; i < whiteCardsId.Length; i++)
            {
                foreach (GameObject card in deck)
                {
                    if (card.GetComponent<Card>().id == whiteCardsId[i])
                    {
                        GameObject playerCard = Instantiate(card);
                        playerCard.GetComponent<Card>().cardName = card.ToString();
                        playerCard.GetComponent<Card>().description = card.ToString();
                        playerCard.GetComponent<Card>().player = 0;
                        playerCard.transform.SetParent(meArea.transform, false);
                    }
                }
            }

            for (int i = 0; i < blackCardsId.Length; i++)
            {
                foreach (GameObject card in deck)
                {
                    if (card.GetComponent<Card>().id == blackCardsId[i])
                    {
                        GameObject enemyCard = Instantiate(card);
                        enemyCard.GetComponent<Card>().cardName = card.ToString();
                        enemyCard.GetComponent<Card>().description = card.ToString();
                        enemyCard.GetComponent<Card>().player = 1;
                        enemyCard.transform.SetParent(enemyArea.transform, false);
                    }
                }
            }
        }
        else // Perspectiva cliente
        {
            for (int i = 0; i < whiteCardsId.Length; i++)
            {
                foreach (GameObject card in deck)
                {
                    if (card.GetComponent<Card>().id == whiteCardsId[i])
                    {
                        GameObject playerCard = Instantiate(card);
                        playerCard.GetComponent<Card>().cardName = card.ToString();
                        playerCard.GetComponent<Card>().description = card.ToString();
                        playerCard.GetComponent<Card>().player = 0;
                        playerCard.transform.SetParent(enemyArea.transform, false);
                    }
                }
            }
            for (int i = 0; i < blackCardsId.Length; i++)
            {
                foreach (GameObject card in deck)
                {
                    if (card.GetComponent<Card>().id == blackCardsId[i])
                    {
                        GameObject enemyCard = Instantiate(card);
                        enemyCard.GetComponent<Card>().cardName = card.ToString();
                        enemyCard.GetComponent<Card>().description = card.ToString();
                        enemyCard.GetComponent<Card>().player = 1;
                        enemyCard.transform.SetParent(meArea.transform, false);
                    }
                }
            }
        }
    }

    public void InicializarPiezas()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                int pieceValue = board[i, j];
                if (pieceValue != 0 && pieceValue < 500)
                {
                    int type = pieceValue / 10 % 10;
                    // Crear un objeto para la pieza
                    GameObject pieceObject = Instantiate(Pieces[type - 1]);
                    // Asignar la posición del objeto en el tablero
                    pieceObject.transform.position = new Vector3(j, 0, i);
                    pieceObject.tag = "Piece";
                    pieceObject.GetComponent<Piece>().x = j;
                    pieceObject.GetComponent<Piece>().z = i;
                    pieceObject.GetComponent<Piece>().type = type;
                    pieceObject.GetComponent<Piece>().color = pieceValue / 100;


                    // Asignar el material del objeto según el color de la pieza
                    int colorValue = pieceValue / 100;
                    Material pieceMaterial;
                    if (colorValue == 0)
                        pieceMaterial = whiteMaterial;
                    else
                    {
                        pieceMaterial = blackMaterial;
                        pieceObject.transform.Rotate(0, 180, 0);
                    }

                    pieceObject.GetComponent<Renderer>().material = pieceMaterial;
                    // Agregar otros componentes y configuraciones según sea necesario
                }
            }
        }
    }



    public void InicialitzarTablero()
    {
        // Recuperar Tablero DB
        board = RealmController.Instance.GetBoard();

        if (board != null)
        {
            //Debug.Log("Tablero Inicializado");
        }
    }

    public void RevalidarPosiciones()
    {
        GameObject[] pieces = GameObject.FindGameObjectsWithTag("Piece");
        foreach (GameObject piece in pieces)
        {
            Destroy(piece);
        }



        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                int pieceValue = board[i, j];
                if (pieceValue != 0 && pieceValue < 500)
                {
                    //print("Pieza " + pieceValue + " " + i + " " + j);
                    int type = pieceValue / 10 % 10;
                    // Crear un objeto para la pieza
                    GameObject pieceObject = Instantiate(Pieces[type - 1]);
                    // Asignar la posición del objeto en el tablero
                    pieceObject.transform.position = new Vector3(j, 0, i);
                    pieceObject.tag = "Piece";
                    pieceObject.GetComponent<Piece>().x = j;
                    pieceObject.GetComponent<Piece>().z = i;
                    pieceObject.GetComponent<Piece>().type = type;
                    pieceObject.GetComponent<Piece>().color = pieceValue / 100;


                    // Asignar el material del objeto según el color de la pieza
                    int colorValue = pieceValue / 100;
                    Material pieceMaterial;
                    if (colorValue == 0)
                        pieceMaterial = whiteMaterial;
                    else
                    {
                        pieceMaterial = blackMaterial;
                        pieceObject.transform.Rotate(0, 180, 0);
                    }

                    pieceObject.GetComponent<Renderer>().material = pieceMaterial;
                    // Agregar otros componentes y configuraciones según sea necesario
                }
                else if (pieceValue != 0 && pieceValue == 999)
                {
                    // Crear un objecto para el muro
                    GameObject wallOb = Instantiate(Pieces[6]);
                    // Asignar la posición del objeto en el tablero
                    wallOb.transform.position = new Vector3(j, 0, i);
                    wallOb.tag = "Wall";
                } //Otros items...
            }
        }

        //Debug.Log("Posiciones Revalidadas");
    }

    public void RefreshTurn()
    {
        if (started && RealmController.Instance.IsRealmReady())
        {
            turn = RealmController.Instance.GetTurn();
            turns = RealmController.Instance.GetTurns();
            GetComponent<InputManager>().CambiarTurnoVisual();
        }
    }

    private void Update()
    {
        /*
        if (!loaded)
        {
            loaded = RealmController.Instance.LoadedGame();
        } else if (loaded)
        {
            GameObject.Find("Loading").SetActive(false);
        }
        */

        //Refresh tablero cada 3 segundos
        if (Time.time - lastRefresh > 3 && turns < RealmController.Instance.GetTurns())
        {
            lastRefresh = Time.time;
            RefreshTablero();
            RefreshTurn();
            RefreshCards();
            CheckGameEnd();
        }
    }

    private void CheckIfGameIsCreated()
    {
        if (!RealmController.Instance.IsGameCreated(Connection.Instance.GameId))
        {
            //Wait 2 sec and try again
            Thread.Sleep(2000);

            if (!RealmController.Instance.IsGameCreated(Connection.Instance.GameId))
            {
                soundManager.GetComponent<BaseSounds>().PlayErrorSound();
                //Load mainmenu
                Destroy(GameObject.Find("RealmControllerManager"));
                Destroy(GameObject.Find("Connection"));
                Destroy(GameObject.Find("AudioManager"));
                Destroy(GameObject.Find("BackGroundMusic"));
                SceneManager.LoadScene("MainMenu");
            }
        }
    }

    public void LoadMainMenu()
    {
        //Load mainmenu
        Destroy(GameObject.Find("RealmControllerManager"));
        Destroy(GameObject.Find("Connection"));
        Destroy(GameObject.Find("AudioManager"));
        Destroy(GameObject.Find("BackGroundMusic"));
        SceneManager.LoadScene("MainMenu");
    }

    public void CheckGameEnd()
    {
        int whiteKing = 10;
        int blackKing = 110;
        bool whiteKingAlive = false;
        bool blackKingAlive = false;

        //Check if white king is alive
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (board[x, y] == whiteKing)
                {
                    whiteKingAlive = true;
                }
                else if (board[x, y] == blackKing)
                {
                    blackKingAlive = true;
                }
            }
        }

        if (!whiteKingAlive)
        {
            if (player == 0)
            {
                YouLose();
            } else
            {
                YouWin();
            }
        }
        else if (!blackKingAlive)
        {
            if (player == 1)
            {
                YouLose();
            } else
            {
                YouWin();
            }
        }
    }

    private void YouWin()
    {
        mainCanvas.gameObject.SetActive(false);
        winCanvas.gameObject.SetActive(true);
        Invoke(nameof(LoadMainMenu), 5);
    }

    private void YouLose()
    {
        mainCanvas.gameObject.SetActive(false);
        loseCanvas.gameObject.SetActive(true);
        Invoke(nameof(LoadMainMenu), 5);
    }
}
