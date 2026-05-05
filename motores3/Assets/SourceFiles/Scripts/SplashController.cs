using UnityEngine;

public class SplashController : MonoBehaviour
{
    private void Start()
    {
        Invoke("GoToMenu", 2f);
    }

    void GoToMenu()
    {
        GameManager.Instance.LoadScene("MenuPrincipal");
    }
}