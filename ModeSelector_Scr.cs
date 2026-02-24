using UnityEngine;
using UnityEngine.SceneManagement;

public class ModeSelector_Scr : MonoBehaviour
{
    public static ModeSelector_Scr instance;

    [SerializeField] private GameObject networkManager;
    [SerializeField] private GameObject rpcManager;
    [SerializeField] private GameObject gameManager;
    [SerializeField] private GameObject sPGameManager;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void SelectMP()
    {
        networkManager.SetActive(true);
        rpcManager.SetActive(true);
        gameManager.SetActive(true);
        sPGameManager.SetActive(false);
        DontDestroyOnLoad(networkManager);
        DontDestroyOnLoad(rpcManager);
        DontDestroyOnLoad(gameManager);
        DestroyOnLoad(sPGameManager);
    }
    public void SelectSP()
    {
        networkManager.SetActive(false);
        rpcManager.SetActive(false);
        gameManager.SetActive(false);
        sPGameManager.SetActive(true);
        DestroyOnLoad(networkManager);
        DestroyOnLoad(rpcManager);
        DestroyOnLoad(gameManager);
        DontDestroyOnLoad(sPGameManager);
    }

    private void DestroyOnLoad(GameObject gameObject)
    {
        SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
    }
}
