using MongoDB.Bson;
using Realms;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardDataDB : IRealmObject
{
    [PrimaryKey]
    [MapTo("_id")]
    public ObjectId Id { get; set; }

    [MapTo("white_player_id")]
    public string WhitePlayerId { get; set; }

    [MapTo("black_player_id")]
    public string BlackPlayerId { get; set; }

    [MapTo("white_cards")]
    public string WhitheCards { get; set; }

    [MapTo("black_cards")]
    public string BlackCards { get; set; }

    [MapTo("string_board")]
    public string StringBoard { get; set; }

    [MapTo("turn")]
    public int Turn { get; set; }

    [MapTo("turns")]
    public int Turns { get; set; }
}