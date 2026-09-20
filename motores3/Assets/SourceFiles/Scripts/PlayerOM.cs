using System;

public static class PlayerOM
{
    // Evento disparado para a contagem de Estrelas (Pontuação e UI)
    public static Action<int, int> OnStarCountChanged;

    // Evento mantido para compatibilidade de moedas, se necessário
    public static Action<int, int> OnCoinCountChanged;

    // Evento de Fim de Jogo
    public static Action<string> OnGameOver;
}