using MongoDB.Bson;
using Realms;
using Realms.Exceptions;
using Realms.Sync;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class RealmController : MonoBehaviour
{
    //Instance used one time the game is open/started
    static public RealmController Instance;

    private Realm _realm;
    private App _realmApp;
    private User _realmUser;

    private bool started = false;

    // Generar GameID Random
    private string partitionID;

    //AppId to connect
    [SerializeField] private string _realmAppId = "parcagames-sttfq";
    private ObjectId? localPlayer = null;

    [SerializeField] private GameObject[] availableCards;

    async void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;

        partitionID = GenerateID();

        if (_realm == null)
        {
            // Crea conexio mongoDB Realm
            _realmApp = App.Create(new AppConfiguration(_realmAppId));

            // Mirar si hi ha un usuari a cache d'aquesta app
            if (_realmApp.CurrentUser == null)
            {
                //Fer Login anonim
                _realmUser = await _realmApp.LogInAsync(Credentials.Anonymous());
                try
                {
                    _realm = await Realm.GetInstanceAsync(new PartitionSyncConfiguration(Connection.Instance.GameId, _realmUser));
                }
                catch (RealmFileAccessErrorException ex)
                {
                    Debug.Log($@"Error creating or opening the realm file. {ex.Message}");
                }
            }
            else
            {
                _realmUser = _realmApp.CurrentUser;
                try
                {
                    _realm = Realm.GetInstance(new PartitionSyncConfiguration(Connection.Instance.GameId, _realmUser));
                    //_realm = await Realm.GetInstanceAsync(new PartitionSyncConfiguration(_realmApp.CurrentUser.Id, _realmUser));
                }
                catch (RealmFileAccessErrorException ex)
                {
                    Debug.Log($@"Error creating or opening the realm file 2. {ex.Message}");
                }
            }
        }
    }


    // Treure Realm
    void OnDisable()
    {
        if (_realm != null)
        {
            _realm.Dispose();
        }
    }

    // Saber si esta conectat el Realm
    public bool IsRealmReady()
    {
        return _realm != null;
    }

    // Crear o Recuperar la partida
    private GameDataDB GetOrCreateData()
    {
        GameDataDB gameData;
        if (Connection.Instance.Host)
        {
            gameData = CreateData();
        }
        else
        {
            gameData = GetData();
        }

        return gameData;
    }

    private GameDataDB CreateData()
    {
        GameDataDB gameData;

        if (!started)
        {
            // Recuperar les partides que tinguin aquesta id
            gameData = _realm.All<GameDataDB>().Where(d => d.GameId == Connection.Instance.GameId).FirstOrDefault();

            if (localPlayer == null)
            {
                // Si no hi cap inicialitzarla
                BoardDataDB start = CreateBoard(ObjectId.Parse(partitionID));

                _realm.Write(() =>
                {
                    gameData = _realm.Add(new GameDataDB()
                    {
                        Id = ObjectId.Parse(partitionID),
                        GameId = Connection.Instance.GameId,
                        Board = start,
                        Current = false
                    });
                });
                localPlayer = start.Id;
                started = true;
            }
            else
            {
                //Debug.Log("No se ha podido crear host");
            }
            //Debug.Log("Hola");
        }
        else
        {
            gameData = _realm.Find<GameDataDB>(localPlayer);

            if (gameData == null)
            {
                //Debug.Log("No se ha podido conectar host");
            }
        }

        return gameData;
    }


    private GameDataDB GetData()
    {
        GameDataDB gameData;

        if (!started)
        {
            // Recuperar les partides que tinguin aquesta id
            //Debug.LogWarning(Connection.Instance.GameId);
            gameData = _realm.All<GameDataDB>().Where(d => d.GameId == Connection.Instance.GameId).FirstOrDefault();
            if (gameData != null)
            {
                localPlayer = gameData.Id;
                started = true;
                if (gameData.Board.BlackPlayerId == "")
                {
                    UploadBlackPlayerId(_realmUser.Id);
                }
            }
            else
            {
                //Debug.Log("No se ha podido conectar cliente o encontrar partida - Primero");
            }
        }
        else
        {
            gameData = _realm.Find<GameDataDB>(localPlayer);

            if (gameData == null)
            {
                //Debug.Log("No se ha podido conectar cliente o encontrar partida - Segundo");
            }
        }

        return gameData;
    }

    private string GenerateID()
    {
        Int64 val = 0;
        var st = new DateTime(1970, 1, 1);
        TimeSpan t = (DateTime.Now.ToUniversalTime() - st);
        val = (Int64)(t.TotalMilliseconds + 0.5);
        char[] startVal = val.ToString("X").ToCharArray();
        string timestamp = "";
        for (int i = 0; i < 10; i++)
        {
            timestamp += startVal[i];
        }
        int num = 0;
        for (int i = 0; i < 14; i++)
        {
            num = UnityEngine.Random.Range(1, 16);
            timestamp += num.ToString("X");
        }
        return timestamp.ToLower();
    }

    // Creem la classe taula amb els camps que volem(taulell escacs)
    public BoardDataDB CreateBoard(ObjectId idboard)
    {
        int[,] matrix = new int[8, 8];

        // Blancas 0
        matrix[0, 0] = 051; //Torre
        matrix[0, 1] = 041; //Caballo
        matrix[0, 2] = 031; //Alfil
        matrix[0, 3] = 020; //Reina
        matrix[0, 4] = 010; //Rey
        matrix[0, 5] = 032; //Alfil
        matrix[0, 6] = 042; //Caballo
        matrix[0, 7] = 052; //Torre

        matrix[1, 0] = 061; //Peon
        matrix[1, 1] = 062; //Peon
        matrix[1, 2] = 063; //Peon
        matrix[1, 3] = 064; //Peon
        matrix[1, 4] = 065; //Peon
        matrix[1, 5] = 066; //Peon
        matrix[1, 6] = 067; //Peon
        matrix[1, 7] = 068; //Peon


        //Negras 1

        matrix[6, 0] = 161; //Peon
        matrix[6, 1] = 162; //Peon
        matrix[6, 2] = 163; //Peon
        matrix[6, 3] = 164; //Peon
        matrix[6, 4] = 165; //Peon
        matrix[6, 5] = 166; //Peon
        matrix[6, 6] = 167; //Peon
        matrix[6, 7] = 168; //Peon

        matrix[7, 0] = 151; //Torre
        matrix[7, 1] = 141; //Caballo
        matrix[7, 2] = 131; //Alfil
        matrix[7, 3] = 120; //Reina
        matrix[7, 4] = 110; //Rey
        matrix[7, 5] = 132; //Alfil
        matrix[7, 6] = 142; //Caballo
        matrix[7, 7] = 152; //Torre

        string stringBoard = LlenarStringTablero(matrix);

        //Debug.Log(stringBoard);

        int[] cardsToPlay = ChooseRandomCards(5);


        string whiteCards = cardsToPlay[0].ToString() + "," + cardsToPlay[1].ToString() + "," + cardsToPlay[2].ToString() + "," + cardsToPlay[3].ToString() + "," + cardsToPlay[4].ToString();
        string blackCards = whiteCards;

        BoardDataDB newBoard = new BoardDataDB()
        {
            Id = idboard,
            WhitePlayerId = _realmUser.Id,
            BlackPlayerId = "",
            Turn = 0,
            WhitheCards = whiteCards,
            BlackCards = blackCards,
            StringBoard = stringBoard
        };

        return newBoard;
    }

    private int[] ChooseRandomCards(int size)
    {
        List<int> cards = new List<int>();

        for (int i = 0; i < size; i++)
        {
            cards.Add(UnityEngine.Random.Range(1, availableCards.Length + 1));
        }

        while (cards.Distinct().Count() != cards.Count())
        {
            cards.Clear();
            for (int i = 0; i < size; i++)
            {
                cards.Add(UnityEngine.Random.Range(1, availableCards.Length + 1));
            }
        }

        return cards.ToArray();
    }

    // Recuperar taula
    public BoardDataDB GetBoardClass()
    {
        GameDataDB gameData = GetOrCreateData();

        if (gameData == null)
            SceneManager.LoadScene("MainMenu");

        //Send notification

        return (BoardDataDB)gameData.Board;
    }
    public int[,] GetBoard()
    {
        BoardDataDB gameBoardData = GetBoardClass();

        int[,] board = new int[8, 8];
        string stringBoard = gameBoardData.StringBoard;

        if (stringBoard != null)
        {
            string[] elements = stringBoard.Split(',');

            //print(elements.Length);

            for (int i = 0; i < elements.Length; i++)
            {
                int row = i / 8;
                int col = i % 8;
                board[row, col] = int.Parse(elements[i]);
            }
        }
        return board;
    }

    public int[] GetWhiteCards()
    {
        BoardDataDB gameBoardData = GetBoardClass();

        int[] cards = new int[5];
        string stringWhiteCards = gameBoardData.WhitheCards;

        if (stringWhiteCards != null)
        {
            string[] elements = stringWhiteCards.Split(',');

            //print(elements.Length);

            for (int i = 0; i < elements.Count(); i++)
            {
                cards[i] = int.Parse(elements[i]);
            }
        }
        return cards;
    }
    public int[] GetBlackCards()
    {
        BoardDataDB gameBoardData = GetBoardClass();

        int[] cards = new int[5];
        string stringBlackCards = gameBoardData.BlackCards;

        if (stringBlackCards != null)
        {
            string[] elements = stringBlackCards.Split(',');

            //print(elements.Length);

            for (int i = 0; i < elements.Count(); i++)
            {
                cards[i] = int.Parse(elements[i]);
            }
        }
        return cards;
    }

    public int GetTurn()
    {
        GameDataDB gameData = GetOrCreateData();
        return (int)gameData.Board.Turn;
    }

    public int GetTurns()
    {
        GameDataDB gameData = GetOrCreateData();
        return (int)gameData.Board.Turns;
    }

    public string GetGameID()
    {
        GameDataDB gameData = GetOrCreateData();
        return (string)gameData.GameId;
    }

    public string GetWhitePlayerId()
    {
        GameDataDB gameData = GetOrCreateData();
        return (string)gameData.Board.WhitePlayerId;
    }
    public string GetBlackPlayerId()
    {
        GameDataDB gameData = GetOrCreateData();
        return (string)gameData.Board.BlackPlayerId;
    }

    public void UploadBoard(string value)
    {
        GameDataDB gameData = GetOrCreateData();
        _realm.Write(() =>
        {
            gameData.Board.StringBoard = value;
        });
    }

    public void UploadBlackPlayerId(string value)
    {
        GameDataDB gameData = GetOrCreateData();
        _realm.Write(() =>
        {
            gameData.Board.BlackPlayerId = value;
        });
    }

    public void DeleteBlackCard(int cardId)
    {
        GameDataDB gameData = GetOrCreateData();

        string[] cards = gameData.Board.BlackCards.Split(',');
        string newCards = "";
        bool found = false;

        for (int i = 0; i < cards.Length; i++)
        {
            if (int.Parse(cards[i]) == cardId && !found)
            {
                found = true;
            }
            else
            {
                if (i == cards.Length - 1)
                {
                    newCards += cards[i];
                }
                else
                {
                    newCards += cards[i] + ",";
                }
            }
        }
        if (newCards[newCards.Length - 1] == ',')
        {
            newCards = newCards.Substring(0, newCards.Length - 1);
        }
        UploadBlackCards(newCards);
    }

    public void DeleteWhiteCard(int cardId)
    {
        GameDataDB gameData = GetOrCreateData();

        string[] cards = gameData.Board.WhitheCards.Split(',');
        string newCards = "";
        bool found = false;

        for (int i = 0; i < cards.Length; i++)
        {
            if (int.Parse(cards[i]) == cardId && !found)
            {
                found = true;
            }
            else
            {
                if (i == cards.Length - 1)
                {
                    newCards += cards[i];
                }
                else
                {
                    newCards += cards[i] + ",";
                }
            }
        }
        if (newCards != string.Empty)
            if (newCards[newCards.Length - 1] == ',')
            {
                newCards = newCards.Substring(0, newCards.Length - 1);
            }

        UploadWhiteCards(newCards);
    }

    public void UploadWhiteCards(string value)
    {
        GameDataDB gameData = GetOrCreateData();
        _realm.Write(() =>
        {
            gameData.Board.WhitheCards = value;
        });
    }
    public void UploadBlackCards(string value)
    {
        GameDataDB gameData = GetOrCreateData();
        _realm.Write(() =>
        {
            gameData.Board.BlackCards = value;
        });
    }

    public void ResetearTabla()
    {
        GameDataDB gameData = GetOrCreateData();
        _realm.Write(() =>
        {
            gameData.Board = CreateBoard(gameData.Id);
        });
    }

    public bool GetState()
    {
        GameDataDB gameData = GetOrCreateData();
        return gameData.Current;
    }
    public void ChangeTurn(int turn)
    {
        GameDataDB gameData = GetOrCreateData();
        _realm.Write(() =>
        {
            gameData.Board.Turn = turn;
            gameData.Board.Turns += 1;
        });
    }

    public static string LlenarStringTablero(int[,] tablero)
    {
        string stringBoard = "";

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (j == 7 && i == 7)
                {
                    stringBoard += tablero[i, j].ToString();
                }
                else
                {
                    stringBoard += tablero[i, j].ToString() + ",";
                }
            }
        }
        return stringBoard;
    }

    public bool LoadedGame()
    {
        GameDataDB gameData;

        gameData = _realm.All<GameDataDB>().Where(d => d.GameId == Connection.Instance.GameId).FirstOrDefault();
        if (gameData != null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public bool IsGameCreated(string GameID)
    {
        GameDataDB gameData;
        gameData = _realm.All<GameDataDB>().Where(d => d.GameId == GameID).FirstOrDefault();
        if (gameData != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

