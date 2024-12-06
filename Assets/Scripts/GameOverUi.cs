using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUi : MonoBehaviour
{

    public TextMeshProUGUI Points; 
    public void Open(int points) {
        gameObject.SetActive(true);
        Points.text = points.ToString() + " points";
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
