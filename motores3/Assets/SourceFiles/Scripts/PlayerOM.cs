using System;

public static class PlayerOM
{
    public static Action<int> OnCoinCountChanged;

    public static void AtualizarMoedas(int quantidade)
    {
        OnCoinCountChanged?.Invoke(quantidade);
    }
}