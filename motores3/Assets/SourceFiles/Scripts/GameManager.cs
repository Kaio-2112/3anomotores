using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState currentState;

    public PlayerInput playerInput;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetState(GameState.Iniciando);
        LoadScene("Splash");
    }

    public void SetState(GameState newState)
    {
        currentState = newState;
        Debug.Log("Estado atual: " + currentState);
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);

        switch (sceneName)
        {
            case "Splash":
                SetState(GameState.Iniciando);
                break;

            case "MenuPrincipal":
                SetState(GameState.MenuPrincipal);
                break;

            case "SampleScene":
                SetState(GameState.Gameplay);
                break;
        }
    }

    public void AssignInputToPlayer()
    {
        if (playerInput != null)
        {
            playerInput.ActivateInput();
            Debug.Log("Input ativado");
        }
    }
}
