using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

public class Card : MonoBehaviour
{
    //card attributes
    public int id;
    public string cardName;
    public string description;
    public int player;

    private bool isPlayed = false;

    private int[,] board = new int[8, 8];

    GameObject boardObject;

    public GameObject effectPrefab;

    private void Start()
    {
        boardObject = GameObject.Find("Board");
    }

    public void ActivateEffect()
    {
        board = boardObject.GetComponent<Board>().board;
        boardObject.GetComponent<Board>().cardBeingPlayed = true;
        isPlayed = true;

    }

    private void Update()
    {
        if (isPlayed)
        {
            switch (id)
            {
                case 1:
                    MoveRight();
                    isPlayed = false;
                    break;
                case 2:
                    MoveLeft();
                    isPlayed = false;
                    break;
                case 3:
                    Reshuffle();
                    isPlayed = false;
                    break;
                case 4:
                    QueenMove();
                    isPlayed = false;
                    break;
                case 5:
                    Upgrade();
                    isPlayed = false;

                    break;
                case 6:
                    GreatWall();
                    isPlayed = false;

                    break;
            }
        }
    }

    #region Scramble 3

    //Intercambia de posicion todas las piezas de los jugadores de manera aleatoria

    private void Reshuffle()
    {
        List<int> whitePieces = new();
        List<int> blackPieces = new();

        List<(int, int)> whitePiecesPos = new();
        List<(int, int)> blackPiecesPos = new();

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (board[i, j] != 0 && board[i, j] / 100 == 0 && board[i, j] != 10)
                {
                    whitePieces.Add(board[i, j]);
                    whitePiecesPos.Add((i, j));
                }
                else if (board[i, j] != 0 && board[i, j] / 100 == 1 && board[i, j] != 110)
                {
                    blackPieces.Add(board[i, j]);
                    blackPiecesPos.Add((i, j));
                }
            }
        }

        //shuffle the white pieces list
        System.Random rnd = new System.Random();
        whitePieces = whitePieces.OrderBy(x => rnd.Next()).ToList();

        //shuffle the black pieces list
        blackPieces = blackPieces.OrderBy(x => rnd.Next()).ToList();

        //set the new positions for the white pieces
        for (int i = 0; i < whitePieces.Count; i++)
        {
            board[whitePiecesPos[i].Item1, whitePiecesPos[i].Item2] = whitePieces[i];
            PlayEffect(whitePiecesPos[i].Item1, whitePiecesPos[i].Item2);
        }

        //set the new positions for the black pieces
        for (int i = 0; i < blackPieces.Count; i++)
        {
            board[blackPiecesPos[i].Item1, blackPiecesPos[i].Item2] = blackPieces[i];
            PlayEffect(blackPiecesPos[i].Item1, blackPiecesPos[i].Item2);
        }

        UpdateBoard();
    }



    #endregion

    #region MoveRight 1
    private void MoveRight()
    {
        // Move all pieces to the right
        for (int i = player == 0 ? 1 : 6; player == 0 ? i < 8 : i >= 0; i += player == 0 ? 1 : -1)
        {
            for (int j = player == 0 ? 6 : 7; player == 0 ? j >= 0 : j > 0; j--)
            {
                if (board[i, j] != 0 && board[i, j] < 900)
                {
                    // Check if the new position is valid
                    if (j < 7 && board[i, j + 1] == 0)
                    {
                        // Move the piece to the new position
                        board[i, j + 1] = board[i, j];
                        board[i, j] = 0;
                    }
                }
            }
        }
        // Update the board
        UpdateBoard();
    }


    #endregion

    #region MoveLeft 2
    private void MoveLeft()
    {
        // Move all pieces to the left
        for (int i = player == 0 ? 1 : 6; player == 0 ? i < 8 : i >= 0; i += player == 0 ? 1 : -1)
        {
            for (int j = player == 0 ? 1 : 0; player == 0 ? j < 8 : j < 7; j++)
            {
                if (board[i, j] != 0 && board[i, j] < 900)
                {
                    // Check if the new position is valid
                    if (j > 0 && board[i, j - 1] == 0)
                    {
                        // Move the piece to the new position
                        board[i, j - 1] = board[i, j];
                        board[i, j] = 0;
                    }
                }
            }
        }
        // Update the board
        UpdateBoard();
    }

    #endregion

    #region QueenMove 4

    private void QueenMove()
    {
        //Obtener todos los peones disponibles
        List<(int, int)> availablePawnsPos = new();
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (board[x, y] / 10 % 10 == 6 && board[x, y] / 100 == player)
                {
                    availablePawnsPos.Add((x, y));
                }
            }
        }

        //Transformar un peon random en reina
        if (availablePawnsPos.Count > 0)
        {
            int randomPawnIndex = UnityEngine.Random.Range(0, availablePawnsPos.Count);
            (int randomPawnX, int randomPawnY) = availablePawnsPos[randomPawnIndex];

            board[randomPawnX, randomPawnY] = player == 0 ? 22 : 122;
            UpdateBoard();
            PlayEffect(randomPawnX, randomPawnY);

        }
    }

    #endregion

    #region Upgrade 5
    private void Upgrade() //Convierte un peon random en torre (5), caballo (4) o alfil (3)
    {
        int[] upWhite = {30, 40, 50};
        int[] upBlack = {130, 140, 150};

        List<(int, int)> availablePawnsPos = new();
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (board[x, y] / 10 % 10 == 6 && board[x, y] / 100 == player)
                {
                    availablePawnsPos.Add((x, y));
                }
            }
        }

        if (availablePawnsPos.Count > 0)
        {
            int randomPawnIndex = UnityEngine.Random.Range(0, availablePawnsPos.Count);
            (int randomPawnX, int randomPawnY) = availablePawnsPos[randomPawnIndex];

            board[randomPawnX, randomPawnY] = player == 0 ? upWhite[UnityEngine.Random.Range(0, 3)] : upBlack[UnityEngine.Random.Range(0, 3)];
            UpdateBoard();
            PlayEffect(randomPawnX, randomPawnY);
        }
    }

    #endregion

    #region GreatWall 6

    private void GreatWall()
    {
        List<(int, int)> emptySpaces = new();

        //Obtener todos los espacios vacios
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (board[x, y] == 0)
                {
                    emptySpaces.Add((x, y));
                }
            }
        }

        //Assignar a un espacio vacio, un muro (999)
        if (emptySpaces.Count > 0)
        {
            int randomEmptySpace = UnityEngine.Random.Range(0, emptySpaces.Count);
            (int randomEmptySpaceX, int randomEmptySpaceY) = emptySpaces[randomEmptySpace];
            board[randomEmptySpaceX, randomEmptySpaceY] = 999;
            UpdateBoard();
            PlayEffect(randomEmptySpaceX, randomEmptySpaceY);
        }
    }


    #endregion


    private void UpdateBoard()
    {
        RealmController.Instance.UploadBoard(RealmController.LlenarStringTablero(board));
        boardObject.GetComponent<Board>().board = board;
        boardObject.GetComponent<Board>().RevalidarPosiciones();
        boardObject.GetComponent<Board>().cardBeingPlayed = false;

        //Actualizar el tablero
        //GameObject.Find("Board").GetComponent<Board>().RefreshTablero();
    }

    private void PlayEffect(int x, int y)
    {
        //Create a new effect that lasts 5 seconds and is located at the given position
        Instantiate(effectPrefab, new Vector3(y, 0.1f, x), Quaternion.identity);


    }
}
