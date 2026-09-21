using System;
using UnityEngine;

public static class PlayerOM
{
    // Evento disparado quando um jogador coleta uma moeda (PlayerID, TotalMoedas)
    public static Action<int, int> OnCoinCountChanged;

    // Evento disparado quando um jogador coleta uma estrela (PlayerID, TotalEstrelas)
    public static Action<int, int> OnStarCountChanged;

    // Evento disparado no fim do jogo
    public static Action<string> OnGameOver;
}