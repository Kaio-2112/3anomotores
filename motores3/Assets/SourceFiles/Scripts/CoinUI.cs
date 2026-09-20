using UnityEngine;
using TMPro;

public class CoinUIController : MonoBehaviour
{
    [Header("Configurações do Jogo")]
    [Tooltip("Quantidade de estrelas necessárias para vencer o jogo")]
    [SerializeField] private int estrelasParaVencer = 7;

    [Header("Textos dos Jogadores (UI Split-Screen)")]
    [SerializeField] private TextMeshProUGUI textoEstrelasP1;
    [SerializeField] private TextMeshProUGUI textoEstrelasP2;

    [Header("Painel de Vitória")]
    [SerializeField] private GameObject painelVencedor;
    [SerializeField] private TextMeshProUGUI textoVencedor;

    private bool _jogoFinalizado = false;

    private void Start()
    {
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
        PlayerOM.OnStarCountChanged += OnStarCollected;
        PlayerOM.OnGameOver += ExibirVencedor;
    }

    private void OnDisable()
    {
        PlayerOM.OnStarCountChanged -= OnStarCollected;
        PlayerOM.OnGameOver -= ExibirVencedor;
    }

    private void OnStarCollected(int playerID, int totalAtual)
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

        if (totalAtual >= estrelasParaVencer)
        {
            _jogoFinalizado = true;
            string mensagemVitoria = $"JOGADOR {playerID} É O GRANDE VENCEDOR DO JOGO!";
            
            ExibirVencedor(mensagemVitoria);
            PlayerOM.OnGameOver?.Invoke(mensagemVitoria);
        }
    }

    private void AtualizarTextoP1(int total)
    {
        if (textoEstrelasP1 != null)
        {
            textoEstrelasP1.text = $"P1:  {total}/{estrelasParaVencer}";
        }
    }

    private void AtualizarTextoP2(int total)
    {
        if (textoEstrelasP2 != null)
        {
            textoEstrelasP2.text = $"P2: {total}/{estrelasParaVencer}";
        }
    }

    private void ExibirVencedor(string mensagem)
    {
        if (painelVencedor != null && textoVencedor != null)
        {
            textoVencedor.text = mensagem;
            painelVencedor.SetActive(true);
            Time.timeScale = 0f;
        }
    }
}