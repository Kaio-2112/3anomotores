using UnityEngine;
using TMPro;

public class CoinUIController : MonoBehaviour
{
    [Header("Configurações do Jogo")]
    [SerializeField] private int estrelasParaVencer = 7;

    [Header("Textos das Estrelas (UI Split-Screen)")]
    [SerializeField] private TextMeshProUGUI textoEstrelasP1;
    [SerializeField] private TextMeshProUGUI textoEstrelasP2;

    [Header("Textos das Moedas (UI Split-Screen)")]
    [SerializeField] private TextMeshProUGUI textoMoedasP1;
    [SerializeField] private TextMeshProUGUI textoMoedasP2;

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

        AtualizarTextoEstrelas(1, 0);
        AtualizarTextoEstrelas(2, 0);
        AtualizarTextoMoedas(1, 0);
        AtualizarTextoMoedas(2, 0);
    }

    private void OnEnable()
    {
        PlayerOM.OnStarCountChanged += OnStarCollected;
        PlayerOM.OnCoinCountChanged += OnCoinCollected;
        PlayerOM.OnGameOver += ExibirVencedor;
    }

    private void OnDisable()
    {
        PlayerOM.OnStarCountChanged -= OnStarCollected;
        PlayerOM.OnCoinCountChanged -= OnCoinCollected;
        PlayerOM.OnGameOver -= ExibirVencedor;
    }

    private void OnCoinCollected(int playerID, int totalMoedas)
    {
        AtualizarTextoMoedas(playerID, totalMoedas);
    }

    private void OnStarCollected(int playerID, int totalEstrelas)
    {
        if (_jogoFinalizado) return;

        AtualizarTextoEstrelas(playerID, totalEstrelas);

        if (totalEstrelas >= estrelasParaVencer)
        {
            _jogoFinalizado = true;
            string mensagemVitoria = $"JOGADOR {playerID} É O GRANDE VENCEDOR DO JOGO!";
            
            ExibirVencedor(mensagemVitoria);
            PlayerOM.OnGameOver?.Invoke(mensagemVitoria);
        }
    }

    private void AtualizarTextoEstrelas(int playerID, int total)
    {
        if (playerID == 1 && textoEstrelasP1 != null)
            textoEstrelasP1.text = $"P1 Estrelas: {total}/{estrelasParaVencer}";
        else if (playerID == 2 && textoEstrelasP2 != null)
            textoEstrelasP2.text = $"P2 Estrelas: {total}/{estrelasParaVencer}";
    }

    private void AtualizarTextoMoedas(int playerID, int total)
    {
        if (playerID == 1 && textoMoedasP1 != null)
            textoMoedasP1.text = $"P1 Moedas: {total}";
        else if (playerID == 2 && textoMoedasP2 != null)
            textoMoedasP2.text = $"P2 Moedas: {total}";
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