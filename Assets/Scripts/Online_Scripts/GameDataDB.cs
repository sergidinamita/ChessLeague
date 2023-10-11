using MongoDB.Bson;
using Realms;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GameDataDB : IRealmObject
{
    [PrimaryKey]
    [MapTo("_id")]
    public ObjectId Id { get; set; }

    [Required]
    [MapTo("game_id")]
    public string GameId { get; set; }

    [MapTo("board_data")]
    public BoardDataDB Board { get; set; }

    [MapTo("current")]
    public bool Current { get; set; }
}
