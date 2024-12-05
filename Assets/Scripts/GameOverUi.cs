using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUi : MonoBehaviour {

    public void Open() {
        gameObject.SetActive(true);
    }

    public void Close() {
        gameObject.SetActive(false);
    }

    public void OnRetryButton() {
        //Optional
        //Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
