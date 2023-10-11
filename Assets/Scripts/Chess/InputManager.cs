
using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class InputManager : MonoBehaviour
{
    private int rbX = 0;
    private int rbY = 4;

    private int rnX = 7;
    private int rnY = 4;

    public GameObject selectedPiece;

    [SerializeField] BaseSounds soundManager;

    [SerializeField]
    TextMeshProUGUI turnText;

    [SerializeField]
    GameObject boardObject;

    int[,] board = new int[8, 8];
    readonly bool[,] enroqueRealizado = new bool[2, 2] { { false, false }, { false, false } };

    //bool jaque = false, jaqueMate = false;

    [SerializeField]
    GameObject turnPiece;

    [SerializeField]
    Material whiteMaterial;
    [SerializeField]
    Material blackMaterial;

    private void Start()
    {
        board = boardObject.GetComponent<Board>().board;
        soundManager = GameObject.Find("AudioManager").GetComponent<BaseSounds>();
    }

    void Update()
    {
        board = boardObject.GetComponent<Board>().board;
        selectedPiece = boardObject.GetComponent<Board>().selectedPieceCache;

        // Detect if the user left-clicked
        if (Input.GetMouseButtonDown(0) && !boardObject.GetComponent<Board>().cardBeingPlayed && boardObject.GetComponent<Board>().turn == boardObject.GetComponent<Board>().player)
        {
            // Cast a ray from the camera to the cursor position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Piece") && hit.collider.gameObject.GetComponent<Piece>().color == boardObject.GetComponent<Board>().turn)
                {
                    selectedPiece = hit.collider.gameObject;
                    //print(board[selectedPiece.GetComponent<Piece>().z, selectedPiece.GetComponent<Piece>().x]);

                }
                else if (selectedPiece != null && hit.collider.CompareTag("Board") && selectedPiece.GetComponent<Piece>().color == boardObject.GetComponent<Board>().turn)
                {
                    ClickTablero(hit);
                    selectedPiece = null;
                }
                else if (selectedPiece != null && hit.collider.gameObject.GetComponent<Piece>().color != boardObject.GetComponent<Board>().turn)
                {
                    if (hit.collider.GetComponent<Piece>().color != selectedPiece.GetComponent<Piece>().color)
                        ClickPieza(hit.collider.gameObject);
                    selectedPiece = null;
                }
                else
                {
                    selectedPiece = null;
                }
                boardObject.GetComponent<Board>().selectedPieceCache = selectedPiece;
            }
        }
    }

    private void ClickTablero(RaycastHit hit)
    {
        var newX = Mathf.RoundToInt(hit.point.x);
        var newZ = Mathf.RoundToInt(hit.point.z);
        var newPosition = new Vector3(newX, 0, newZ);

        if (newX >= 0 && newX <= 7 && newZ >= 0 && newZ <= 7)
        {
            if (MovimientoLegal(selectedPiece.GetComponent<Piece>().z, selectedPiece.GetComponent<Piece>().x, newZ, newX))
            {
                Mover(selectedPiece.GetComponent<Piece>().z, selectedPiece.GetComponent<Piece>().x, newZ, newX, newPosition);
            }
            else
            {
                //Debug.Log("Movimiento ilegal");
            }
        }
    }

    private void ClickPieza(GameObject pieza)
    {
        var newX = pieza.GetComponent<Piece>().x;
        var newZ = pieza.GetComponent<Piece>().z;
        var newPosition = new Vector3(newX, 0, newZ);

        if (newX >= 0 && newX <= 7 && newZ >= 0 && newZ <= 7)
        {
            if (MovimientoLegal(selectedPiece.GetComponent<Piece>().z, selectedPiece.GetComponent<Piece>().x, newZ, newX))
            {
                Mover(selectedPiece.GetComponent<Piece>().z, selectedPiece.GetComponent<Piece>().x, newZ, newX, newPosition);
            }
            else
            {
                //Debug.Log("Movimiento ilegal");
            }
        }
    }
    

    public void Mover(int x, int y, int x2, int y2, Vector3 newPosition)
    {
        try
        {
            int piezaMovida = board[x, y];

            //// Realizar el movimiento si no esta en jaque
            //if (!EsJaquePostMovimiento(x, y, x2, y2))
            //{

            //}

            board[x2, y2] = piezaMovida;
            board[x2, y2] = piezaMovida;
            board[x, y] = 0;

            // Actualizar posición del rey del turn actual si se movió un rey
            if (piezaMovida == 10)
            {
                rbX = x2;
                rbY = y2;
            }
            else if (piezaMovida == 110)
            {
                rnX = x2;
                rnY = y2;
            }

            ////Comprobar si el rey enemigo está en jaque
            //if (boardObject.GetComponent<Board>().turn == 0)
            //{
            //    if (EsJaque(rnX, rnY, board))
            //        //Debug.Log("Jaque - Negras");
            //}
            //else
            //{
            //    if (EsJaque(rbX, rbY, board))
            //        //Debug.Log("Jaque - Blancas");
            //}

            //Debug.LogWarning("Movimiento 2d realizado");
            // Mover pieza en el tablero 3D y actualizar la matriz de tablero

            //Debug.LogWarning("Movimiento realizado");
            boardObject.GetComponent<Board>().board = board;

            soundManager.GetComponent<BaseSounds>().PlayMovePieceSound();

            // Crear String base de datos i Actualizar tablero
            string tablero = RealmController.LlenarStringTablero(board);
            ActualizarTablero(tablero);

            //Debug.LogWarning("Tablero actualizado");

            // Cambiar turn y actualizar texto de turn
            CambiarTurno();

            //Debug.LogWarning("Turno cambiado");


            ////Comprobar si el rey enemigo está en jaque
            //if (boardObject.GetComponent<Board>().turn == 0)
            //{
            //    if (EsJaque(rnX, rnY, board))
            //        //Debug.Log("Jaque - Negras");
            //}
            //else
            //{
            //    if (EsJaque(rbX, rbY, board))
            //        //Debug.Log("Jaque - Blancas");
            //}

            Mover3D(x2, y2, newPosition);
            boardObject.GetComponent<Board>().CheckGameEnd();

        }
        catch (Exception e)
        {
            Debug.LogWarning("Error al mover la pieza: " + e.Message);
        }
    }

    private void CambiarTurno()
    {
        boardObject.GetComponent<Board>().turn = 1 - boardObject.GetComponent<Board>().turn;
        if (boardObject.GetComponent<Board>().turn == 0)
            turnPiece.GetComponent<Renderer>().material = whiteMaterial;
        else
            turnPiece.GetComponent<Renderer>().material = blackMaterial;

        //print("Turno: " + boardObject.GetComponent<Board>().turn);

        boardObject.GetComponent<Board>().turns = boardObject.GetComponent<Board>().turns + 1;

        RealmController.Instance.ChangeTurn(boardObject.GetComponent<Board>().turn);
    }

    public void CambiarTurnoVisual()
    {
        if (boardObject.GetComponent<Board>().turn == 0)
            turnPiece.GetComponent<Renderer>().material = whiteMaterial;
        else
            turnPiece.GetComponent<Renderer>().material = blackMaterial;
    }

    private void Mover3D(int x2, int y2, Vector3 newPosition)
    {
        selectedPiece.transform.position = newPosition;
        selectedPiece.GetComponent<Piece>().x = y2;
        selectedPiece.GetComponent<Piece>().z = x2;
        if (IsObjectHere(newPosition))
        {
            ObjectHere(newPosition).SetActive(false);
        }
    }

    private void RealizarEnroque(int x, int y, int x2, int y2)
    {

        // Mover el rey y la torre
        board[x2, y2] = board[x, y];
        board[x, y] = 0;

        if (y2 == y + 2)
        {
            board[x, y + 1] = board[x, y + 3];
            board[x, y + 3] = 0;
        }
        else // y2 == y - 2
        {
            board[x, y - 1] = board[x, y - 4];
            board[x, y - 4] = 0;
        }

        boardObject.GetComponent<Board>().RevalidarPosiciones();

        // Marcar el enroque como realizado
        enroqueRealizado[boardObject.GetComponent<Board>().turn, y2 > y ? 0 : 1] = true;
    }

    bool IsObjectHere(Vector3 position)
    {
        Collider[] intersecting = Physics.OverlapSphere(position, 0.01f);
        if (intersecting.Length == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    GameObject ObjectHere(Vector3 position)
    {
        Collider[] intersecting = Physics.OverlapSphere(position, 0.01f);
        if (intersecting.Length == 0)
        {
            return null;
        }
        else if (intersecting[0].gameObject.CompareTag("Piece"))
        {
            return intersecting[0].gameObject;
        }
        else { return null; }
    }


    #region COMPROBACIONES

    public bool MovimientoLegal(int x, int y, int x2, int y2)
    {
        //Comprovamos si es blanca la ficha, si son mas grandes de 100 són negras, si són más pequenas blancas
        if (board[x, y] / 100 == 0 && boardObject.GetComponent<Board>().turn == 0)
        {
            #region BLANCAS
            switch ((board[x, y] / 10) % 10)
            {
                //Si la resta dona 1 es rey, 2 reina, 3 alfil, 4 caball, 5 torre, 6 peó
                case 1:
                    #region Rey
                    
                    if (EsEnroqueValido(x, y, x2, y2))
                    {
                        RealizarEnroque(x, y, x2, y2);
                    }
                    else if ((x2 == x + 1 || x2 == x - 1 || x2 == x) && (y2 == y + 1 || y2 == y - 1 || y2 == y))
                    {
                        if (!CapturaBlancaInvalida(x, y, x2, y2))
                        {
                            return NoJaquePreMovimiento(x, y, x2, y2);
                        }
                        return false;
                    }
                    break;
                #endregion
                case 2:
                    #region Reina
                    
                    if (EsMovimientoDiagonal(x, y, x2, y2)) //MOVIMIENTO "ALFIL"
                    {
                        if (!CapturaBlancaInvalida(x, y, x2, y2) && MovimientoDiagonalValido(x, y, x2, y2))
                        {
                            return NoJaquePreMovimiento(x, y, x2, y2);
                        }
                    }
                    if (x2 > x || x2 < x) //MOVIMIENTO TORRE
                    {
                        if (y2 == y)
                        {
                            if (!CapturaBlancaInvalida(x, y, x2, y2) && MovimientoVerticalValido(x, y, x2))
                            {
                                return NoJaquePreMovimiento(x, y, x2, y2);
                            }
                        }
                    }
                    else if (y2 > y || y2 < y)
                    {
                        if (x2 == x)
                        {
                            if (!CapturaBlancaInvalida(x, y, x2, y2) && MovimientoHorizontalValido(x, y, y2))
                            {
                                return NoJaquePreMovimiento(x, y, x2, y2);
                            }
                        }
                    }
                    break;
                #endregion
                case 3:
                    #region Alfil
                    
                    if (EsMovimientoDiagonal(x, y, x2, y2)) //MOVIMIENTO "ALFIL"
                    {
                        if (!CapturaBlancaInvalida(x, y, x2, y2) && MovimientoDiagonalValido(x, y, x2, y2))
                        {
                            return NoJaquePreMovimiento(x, y, x2, y2);
                        }
                    }
                    break;
                #endregion
                case 4:
                    #region Caballo
                    
                    //Si la rbX rbY la rbY no son la mateixa
                    if (x2 != x && y2 != y && ((Math.Abs(x2 - x) == 1 && Math.Abs(y2 - y) == 2) ^ (Math.Abs(x2 - x) == 2 && Math.Abs(y2 - y) == 1)))
                    {
                        if (!CapturaBlancaInvalida(x, y, x2, y2))
                        {
                            return NoJaquePreMovimiento(x, y, x2, y2);
                        }
                    }
                    break;
                #endregion
                case 5:
                    #region Torre
                    
                    if (x2 > x || x2 < x)
                    {
                        if (y2 == y)
                        {
                            if (!CapturaBlancaInvalida(x, y, x2, y2) && MovimientoVerticalValido(x, y, x2))
                            {
                                return NoJaquePreMovimiento(x, y, x2, y2);
                            }
                        }
                    }
                    else if (y2 > y || y2 < y)
                    {
                        if (x2 == x)
                        {
                            if (!CapturaBlancaInvalida(x, y, x2, y2) && MovimientoHorizontalValido(x, y, y2))
                            {
                                return NoJaquePreMovimiento(x, y, x2, y2);
                            }
                        }
                    }
                    break;
                #endregion
                case 6:
                    #region Peon
                    if (y2 == y && board[x + 1, y] == 0)
                    {
                        if (x2 == x + 1)
                        {
                            return NoJaquePreMovimiento(x, y, x2, y2);
                        }
                        else if (x2 == x + 2 && x == 1 && board[x + 2, y] == 0 && board[x + 1, y] == 0)
                        {
                            return NoJaquePreMovimiento(x, y, x2, y2);
                        }
                    }
                    else if (y2 == y + 1 || y2 == y - 1)
                    {
                        if (x2 == x + 1)
                        {
                            if (board[x2, y2] > 100 && board[x2, y2] != 0)
                            {
                                if (board[x2, y2] == 160)
                                {
                                    board[x2 + 1, y2] = 0;
                                }
                                return NoJaquePreMovimiento(x, y, x2, y2);
                            }
                        }
                    }
                    break;
                    #endregion
            }
            #endregion
        }
        else
        {
            #region NEGRAS
            switch ((board[x, y] / 10) % 10)
            {
                //Si la resta dona 1 es rey, 2 reina, 3 alfil, 4 caball, 5 torre, 6 peó
                case 1:
                    #region Rey
                    if ((x2 == x + 1 || x2 == x - 1 || x2 == x) && (y2 == y + 1 || y2 == y - 1 || y2 == y))
                    {
                        if (!CapturaNegraInvalida(x, y, x2, y2))
                        {
                            return NoJaquePreMovimiento(x, y, x2, y2);
                        }
                    }
                    break;
                #endregion
                case 2:
                    #region Reina
                    if (EsMovimientoDiagonal(x, y, x2, y2))
                    {
                        if (!CapturaNegraInvalida(x, y, x2, y2) && MovimientoDiagonalValido(x, y, x2, y2))
                        {
                            return NoJaquePreMovimiento(x, y, x2, y2);
                        }
                    }
                    else //MOVIMIENTO "TORRE"
                    {
                        if (x2 > x || x2 < x)
                        {
                            if (y2 == y)
                            {
                                if (!CapturaNegraInvalida(x, y, x2, y2) && MovimientoVerticalValido(x, y, x2))
                                {
                                    return NoJaquePreMovimiento(x, y, x2, y2);
                                }
                            }
                        }
                        else if (y2 > y || y2 < y)
                        {
                            if (x2 == x)
                            {
                                if (!CapturaNegraInvalida(x, y, x2, y2) && MovimientoHorizontalValido(x, y, y2))
                                {
                                    return NoJaquePreMovimiento(x, y, x2, y2);
                                }
                            }
                        }
                    }
                    break;
                #endregion
                case 3:
                    #region Alfil
                    if (EsMovimientoDiagonal(x, y, x2, y2))
                    {
                        if (!CapturaNegraInvalida(x, y, x2, y2) && MovimientoDiagonalValido(x, y, x2, y2))
                        {
                            return NoJaquePreMovimiento(x, y, x2, y2);
                        }
                    }
                    break;
                #endregion
                case 4:
                    #region Caballo
                    //Si la x y la y no son la mateixa
                    if (x2 != x && y2 != y && ((Math.Abs(x2 - x) == 1 && Math.Abs(y2 - y) == 2) ^ (Math.Abs(x2 - x) == 2 && Math.Abs(y2 - y) == 1)))
                    {
                        if (!CapturaNegraInvalida(x, y, x2, y2))
                        {
                            return NoJaquePreMovimiento(x, y, x2, y2);
                        }
                    }

                    break;
                #endregion
                case 5:
                    #region Torre
                    if (x2 > x || x2 < x)
                    {
                        if (y2 == y)
                        {
                            if (!CapturaNegraInvalida(x, y, x2, y2) && MovimientoVerticalValido(x, y, x2))
                            {
                                return NoJaquePreMovimiento(x, y, x2, y2);
                            }
                        }
                    }
                    else if (y2 > y || y2 < y)
                    {
                        if (x2 == x)
                        {
                            if (!CapturaNegraInvalida(x, y, x2, y2) && MovimientoHorizontalValido(x, y, y2))
                            {
                                return NoJaquePreMovimiento(x, y, x2, y2);
                            }
                        }
                    }
                    break;
                #endregion
                case 6:
                    #region Peon
                    if (y2 == y && board[x - 1, y] == 0)
                    {
                        if (x2 == x - 1)
                        {
                            return NoJaquePreMovimiento(x, y, x2, y2);
                        }
                        else if (x2 == x - 2 && x == 6 && board[x - 2, y] == 0 && board[x - 1, y] == 0)
                        {
                            return NoJaquePreMovimiento(x, y, x2, y2);
                        }
                    }
                    else if (y2 == y + 1 || y2 == y - 1)
                    {
                        if (x2 == x - 1)
                        {
                            if (board[x2, y2] > 0 && board[x2, y2] < 100)
                            {
                                if (board[x2, y2] == 69)
                                {
                                    board[x2 - 1, y2] = 0;
                                }
                                return NoJaquePreMovimiento(x, y, x2, y2);
                            }
                        }
                    }
                    break;
                    #endregion
            }

            #endregion
        }
        return false;

    }

    private bool NoJaquePreMovimiento(int x, int y, int x2, int y2)
    {
        //Clone Board
        int[,] boardCopy = (int[,])board.Clone();

        //Move Piece
        boardCopy[x2, y2] = boardCopy[x, y];
        boardCopy[x, y] = 0;

        if (boardObject.GetComponent<Board>().turn == 0)
        {
            if (!EsJaque(rbX, rbY, boardCopy))
            {
                return true;
            }
        }
        else
        {
            if (!EsJaque(rnX, rnY, boardCopy))
            {
                return true;
            }
        }

        //return false; Cambiar luego
        return true;
    }

    private bool EsJaquePostMovimiento(int x, int y, int x2, int y2)
    {
        int[,] boardCopy = board;
        boardCopy[x2, y2] = boardCopy[x, y];
        boardCopy[x, y] = 0;
        if (boardObject.GetComponent<Board>().turn == 0)
        {
            if (!EsJaque(rnX, rnY, boardCopy))
            {
                return true;
            }
        }
        else
            if (!EsJaque(rbX, rbY, boardCopy))
        {
            return true;
        }
        return false;
    }


    private bool EsEnroqueValido(int x, int y, int x2, int y2)
    {
        // Verificar que el movimiento es horizontal y que la distancia es de dos casillas
        if (y2 != y + 2 && y2 != y - 2) return false;
        if (x2 != x) return false;

        // Verificar que el rey no esté en jaque
        if (!NoJaquePreMovimiento(x, y, x2, y2)) return false;

        // Verificar que las casillas entre el rey y la torre estén vacías
        int direccion = (y2 > y) ? 1 : -1;
        for (int j = y + direccion; j != y2; j += direccion)
        {
            if (board[x, j] != 0) return false;
        }

        // Verificar que la torre correspondiente esté en su posición original y que no se haya realizado un enroque antes
        int i = (boardObject.GetComponent<Board>().turn == 0) ? 0 : 7;
        if (y2 == y + 2)
        {
            if (enroqueRealizado[boardObject.GetComponent<Board>().turn, 0] || board[x, y + 3] != 55 || board[x, y + 1] != 0) return false;
        }
        else // y2 == y - 2
        {
            if (enroqueRealizado[boardObject.GetComponent<Board>().turn, 1] || board[x, y - 4] != 55 || board[x, y - 1] != 0 || board[x, y - 2] != 0) return false;
        }

        // Si ha pasado todas las validaciones, el enroque es válido
        return true;
    }

    public bool CapturaBlancaInvalida(int x, int y, int x2, int y2)
    {
        if (board[x2, y2] < 100 && board[x2, y2] != 0 && board[x2, y2] == 999)
        {
            return true;
        }
        return false;
    }

    public bool CapturaNegraInvalida(int x, int y, int x2, int y2)
    {
        if (board[x2, y2] > 100 && board[x2, y2] != 0 && board[x2, y2] == 999)
        {
            return true;
        }
        return false;
    }

    public bool EsMovimientoDiagonal(int x, int y, int x2, int y2)
    {
        int rowDiff = Math.Abs(y2 - y);
        int colDiff = Math.Abs(x2 - x);

        return rowDiff == colDiff;
    }

    private bool MovimientoDiagonalValido(int x, int y, int x2, int y2)
    {
        // Check if the move is diagonal
        int deltaX = x2 - x;
        int deltaY = y2 - y;
        if (Mathf.Abs(deltaX) != Mathf.Abs(deltaY))
        {
            return false;
        }

        // Check if the starting and target positions are the same
        if (deltaX == 0 && deltaY == 0)
        {
            return false;
        }

        // Determine the direction of the move
        int xDir = (int)Mathf.Sign(deltaX);
        int yDir = (int)Mathf.Sign(deltaY);

        // Check if there are any pieces in the way
        int currX = x + xDir;
        int currY = y + yDir;
        while (currX != x2 || currY != y2)
        {
            if (currX < 0 || currX >= board.GetLength(0) || currY < 0 || currY >= board.GetLength(1))
            {
                return false;
            }
            if (board[currX, currY] != 0)
            {
                return false;
            }
            currX += xDir;
            currY += yDir;
        }

        return true;
    }

    private bool MovimientoHorizontalValido(int x, int y, int y2)
    {
        if (y2 > y)
        {
            for (int i = y + 1; i < y2; i++)
            {
                if (board[x, i] != 0)
                {
                    return false;
                }
            }
        }
        else
        {
            for (int i = y - 1; i > y2; i--)
            {
                if (board[x, i] != 0)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private bool MovimientoVerticalValido(int x, int y, int x2)
    {
        if (x2 > x)
        {
            for (int i = x + 1; i < x2; i++)
            {
                if (board[i, y] != 0)
                {
                    return false;
                }
            }
        }
        else
        {
            for (int i = x - 1; i > x2; i--)
            {
                if (board[i, y] != 0)
                {
                    return false;
                }
            }
        }
        return true;
    }

    #endregion

    #region Jaque
    public bool EsJaque(int reyX, int reyY, int[,] boardCopy)
    {
        // Color del rey a comprobar
        int color = boardCopy[reyX, reyY] / 100;

        // Diferentes posibilidades en las que el rey puede estar en jaque
        if (JaqueHorizontal(reyX, reyY, color, boardCopy) || JaqueVertical(reyX, reyY, color, boardCopy) || JaqueDiagonal(reyX, reyY, color, boardCopy) || JaqueCaballo(reyX, reyY, color, boardCopy))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool JaqueHorizontal(int reyX, int reyY, int colorRey, int[,] boardCopy)
    {
        // Comprueba la fila hacia la izquierda
        for (int i = reyY - 1; i >= 0; i--)
        {
            if (boardCopy[reyX, i] == 0)
            {
                continue;
            }
            else if (boardCopy[reyX, i] / 100 == colorRey)
            {
                return false;
            }
            else if (boardCopy[reyX, i] / 100 != colorRey)
            {
                switch ((boardCopy[reyX, i] / 10) % 10)
                {
                    case 2 or 5: // Reina o Torre
                        //Debug.Log("Jaque - Izquierda");
                        return true;
                }
            }
        }
        // Comprueba la fila hacia la derecha
        for (int i = reyY + 1; i < 8; i++)
        {
            if (boardCopy[reyX, i] == 0)
            {
                continue;
            }
            else if (boardCopy[reyX, i] / 100 == colorRey)
            {
                return false;
            }
            else if (boardCopy[reyX, i] / 100 != colorRey)
            {
                switch ((boardCopy[reyX, i] / 10) % 10)
                {
                    case 2 or 5: // Reina o Torre
                        //Debug.Log("Jaque - Derecha");
                        return true;
                }
            }
        }
        return false;

    }

    private bool JaqueVertical(int reyX, int reyY, int colorRey, int[,] boardCopy)
    {
        //Comprueba la columna hacia arriba
        for (int i = reyX + 1; i < 8; i++)
        {
            if (boardCopy[i, reyY] == 0)
            {
                continue;
            }
            else if (boardCopy[i, reyY] / 100 == colorRey)
            {
                return false;
            }
            else if (boardCopy[i, reyY] / 100 != colorRey)
            {
                switch ((boardCopy[i, reyY] / 10) % 10)
                {
                    case 2 or 5: //Reina o Torre
                        //Debug.Log("Jaque - Arriba");
                        return true;
                }
            }
        }

        //Comprueba la columna hacia abajo
        for (int i = reyX - 1; i >= 0; i--)
        {
            if (boardCopy[i, reyY] == 0)
            {
                continue;
            }
            else if (boardCopy[i, reyY] / 100 == colorRey)
            {
                return false;
            }
            else if (boardCopy[i, reyY] / 100 != colorRey)
            {
                switch ((boardCopy[i, reyY] / 10) % 10)
                {
                    case 2 or 5: //Reina o Torre
                        //Debug.Log("Jaque - Abajo");
                        return true;
                }
            }
        }
        return false;
    }

    private bool JaqueDiagonal(int reyX, int reyY, int colorRey, int[,] boardCopy)
    {
        //Comprueba la diagonal hacia arriba a la izquierda
        for (int i = reyX + 1, j = reyY - 1; i < 8 && j >= 0; i++, j--)
        {
            if (boardCopy[i, j] == 0)
            {
                continue;
            }
            else if (boardCopy[i, j] / 100 == colorRey)
            {
                return false;
            }
            else if (boardCopy[i, j] / 100 != colorRey)
            {
                switch ((boardCopy[i, j] / 10) % 10)
                {
                    case 2 or 4: //Reina o Alfil
                        //Debug.Log("Jaque - Arriba Izquierda");
                        return true;
                }
            }
        }

        //Comprueba la diagonal hacia arriba a la derecha
        for (int i = reyX + 1, j = reyY + 1; i < 8 && j < 8; i++, j++)
        {
            if (boardCopy[i, j] == 0)
            {
                continue;
            }
            else if (boardCopy[i, j] / 100 == colorRey)
            {
                return false;
            }
            else if (boardCopy[i, j] / 100 != colorRey)
            {
                switch ((boardCopy[i, j] / 10) % 10)
                {
                    case 2 or 4: //Reina o Alfil
                        //Debug.Log("Jaque - Arriba Derecha");
                        return true;
                }
            }
        }

        //Comprueba la diagonal hacia abajo a la izquierda
        for (int i = reyX - 1, j = reyY - 1; i >= 0 && j >= 0; i--, j--)
        {
            if (boardCopy[i, j] == 0)
            {
                continue;
            }
            else if (boardCopy[i, j] / 100 == colorRey)
            {
                return false;
            }
            else if (boardCopy[i, j] / 100 != colorRey)
            {
                switch ((boardCopy[i, j] / 10) % 10)
                {
                    case 2 or 4: //Reina o Alfil
                        //Debug.Log("Jaque - Abajo Izquierda");
                        return true;
                }
            }
        }

        //Comprueba la diagonal hacia abajo a la derecha
        for (int i = reyX - 1, j = reyY + 1; i >= 0 && j < 8; i--, j++)
        {
            if (boardCopy[i, j] == 0)
            {
                continue;
            }
            else if (boardCopy[i, j] / 100 == colorRey)
            {
                return false;
            }
            else if (boardCopy[i, j] / 100 != colorRey)
            {
                switch ((boardCopy[i, j] / 10) % 10)
                {
                    case 2 or 4: //Reina o Alfil
                        //Debug.Log("Jaque - Abajo Derecha");
                        return true;
                }
            }
        }

        return false;
    }

    private bool JaqueCaballo(int reyX, int reyY, int colorRey, int[,] boardCopy)
    {
        //Arriba izquierda
        if (reyX + 2 <= 7 && reyY - 1 >= 0)
        {
            if (boardCopy[reyX + 2, reyY - 1] / 100 != colorRey)
            {
                switch ((boardCopy[reyX + 2, reyY - 1] / 10) % 10)
                {
                    case 3: //Caballo
                        //Debug.Log("Jaque - Arriba Izquierda");
                        return true;
                }
            }
        }

        //Arriba derecha
        if (reyX + 2 <= 7 && reyY + 1 <= 7)
        {
            if (boardCopy[reyX + 2, reyY + 1] / 100 != colorRey)
            {
                switch ((boardCopy[reyX + 2, reyY + 1] / 10) % 10)
                {
                    case 3: //Caballo
                        //Debug.Log("Jaque - Arriba Derecha");
                        return true;
                }
            }
        }

        //Abajo izquierda
        if (reyX - 2 >= 0 && reyY - 1 >= 0)
        {
            if (boardCopy[reyX - 2, reyY - 1] / 100 != colorRey)
            {
                switch ((boardCopy[reyX - 2, reyY - 1] / 10) % 10)
                {
                    case 3: //Caballo
                        //Debug.Log("Jaque - Abajo Izquierda");
                        return true;
                }
            }
        }

        //Abajo derecha
        if (reyX - 2 >= 0 && reyY + 1 <= 7)
        {
            if (boardCopy[reyX - 2, reyY + 1] / 100 != colorRey)
            {
                switch ((boardCopy[reyX - 2, reyY + 1] / 10) % 10)
                {
                    case 3: //Caballo
                        //Debug.Log("Jaque - Abajo Derecha");
                        return true;
                }
            }
        }

        //Izquierda arriba
        if (reyX + 1 <= 7 && reyY - 2 >= 0)
        {
            if (boardCopy[reyX + 1, reyY - 2] / 100 != colorRey)
            {
                switch ((boardCopy[reyX + 1, reyY - 2] / 10) % 10)
                {
                    case 3: //Caballo
                        //Debug.Log("Jaque - Izquierda Arriba");
                        return true;
                }
            }
        }

        //Izquierda abajo
        if (reyX - 1 >= 0 && reyY - 2 >= 0)
        {
            if (boardCopy[reyX - 1, reyY - 2] / 100 != colorRey)
            {
                switch ((boardCopy[reyX - 1, reyY - 2] / 10) % 10)
                {
                    case 3: //Caballo
                        //Debug.Log("Jaque - Izquierda Abajo");
                        return true;
                }
            }
        }

        //Derecha arriba
        if (reyX + 1 <= 7 && reyY + 2 <= 7)
        {
            if (boardCopy[reyX + 1, reyY + 2] / 100 != colorRey)
            {
                switch ((boardCopy[reyX + 1, reyY + 2] / 10) % 10)
                {
                    case 3: //Caballo
                        //Debug.Log("Jaque - Derecha Arriba");
                        return true;
                }
            }
        }

        //Derecha abajo
        if (reyX - 1 >= 0 && reyY + 2 <= 7)
        {
            if (boardCopy[reyX - 1, reyY + 2] / 100 != colorRey)
            {
                switch ((boardCopy[reyX - 1, reyY + 2] / 10) % 10)
                {
                    case 3: //Caballo
                        //Debug.Log("Jaque - Derecha Abajo");
                        return true;
                }
            }
        }

        return false;
    }

    public bool EsJaqueMate(int reyX, int reyY)
    {
        //JAQUEMATE
        return false;
    }

    #endregion

    #region ONLINE

    private int[,] RecuperarTablero()
    {
        int[,] board = new int[8, 8];

        if (RealmController.Instance.IsRealmReady())
        {
            BoardDataDB gameBoardData = RealmController.Instance.GetBoardClass();

            string stringBoard = gameBoardData.StringBoard;

            if (stringBoard != null)
            {
                string[] elements = stringBoard.Split(',');

                for (int i = 0; i < elements.Length; i++)
                {
                    int row = i / 8;
                    int col = i % 8;
                    board[row, col] = int.Parse(elements[i]);
                }
            }

            //Debug.Log("Petición");
        }
        return board;
    }

    private void ActualizarTablero(string tablero)
    {
        if (RealmController.Instance.IsRealmReady())
        {
            RealmController.Instance.UploadBoard(tablero);
        }
    }

    #endregion
}
