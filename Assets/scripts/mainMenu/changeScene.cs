using UnityEngine;
using UnityEngine.SceneManagement;

public class changeScene : MonoBehaviour
{
    public void loadNextScene()
    {
        if (Time.timeScale == 0)
            Time.timeScale = 1;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void loadLastScene()
    {
        if (Time.timeScale == 0)
            Time.timeScale = 1;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
}
