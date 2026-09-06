using UnityEngine;
using TMPro;

public class CoinUIController : MonoBehaviour
{
    [Header("Configurações do Jogo")]
    [Tooltip("Quantidade de moedas necessárias para vencer o jogo")]
    [SerializeField] private int moedasParaVencer = 7;

    [Header("Textos dos Jogadores (UI Split-Screen)")]
    [Tooltip("Texto da UI posicionado no lado esquerdo da tela (P1)")]
    [SerializeField] private TextMeshProUGUI textoMoedasP1;
    [Tooltip("Texto da UI posicionado no lado direito da tela (P2)")]
    [SerializeField] private TextMeshProUGUI textoMoedasP2;

    [Header("Painel de Vitória")]
    [SerializeField] private GameObject painelVencedor;
    [SerializeField] private TextMeshProUGUI textoVencedor;

    private bool _jogoFinalizado = false;

    private void Start()
    {
        // Garante a escala de tempo normal ao iniciar
        Time.timeScale = 1f;

        if (painelVencedor != null)
        {
            painelVencedor.SetActive(false);
        }

        AtualizarTextoP1(0);
        AtualizarTextoP2(0);
    }

    private void OnEnable()
    {
        PlayerOM.OnCoinCountChanged += OnCoinCollected;
        PlayerOM.OnGameOver += ExibirVencedor;
    }

    private void OnDisable()
    {
        PlayerOM.OnCoinCountChanged -= OnCoinCollected;
        PlayerOM.OnGameOver -= ExibirVencedor;
    }

    private void OnCoinCollected(int playerID, int totalAtual)
    {
        if (_jogoFinalizado) return;

        if (playerID == 1)
        {
            AtualizarTextoP1(totalAtual);
        }
        else if (playerID == 2)
        {
            AtualizarTextoP2(totalAtual);
        }

        // Condição de vitória ao atingir a meta de 7 moedas
        if (totalAtual >= moedasParaVencer)
        {
            _jogoFinalizado = true;
            string mensagemVitoria = $"JOGADOR {playerID} VENCEU!";
            
            ExibirVencedor(mensagemVitoria);
            PlayerOM.OnGameOver?.Invoke(mensagemVitoria);
        }
    }

    private void AtualizarTextoP1(int total)
    {
        if (textoMoedasP1 != null)
        {
            textoMoedasP1.text = $"P1 Moedas: {total}/{moedasParaVencer}";
        }
    }

    private void AtualizarTextoP2(int total)
    {
        if (textoMoedasP2 != null)
        {
            textoMoedasP2.text = $"P2 Moedas: {total}/{moedasParaVencer}";
        }
    }

    private void ExibirVencedor(string mensagem)
    {
        if (painelVencedor != null && textoVencedor != null)
        {
            textoVencedor.text = mensagem;
            painelVencedor.SetActive(true);

            // Congela o jogo ao finalizar
            Time.timeScale = 0f;
        }
    }
}