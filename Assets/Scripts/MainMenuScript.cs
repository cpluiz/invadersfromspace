using UnityEngine;
using TMPro;

public class MainMenuScript : MonoBehaviour{
    [SerializeField]
    private TMP_Text txt_highscore;

    void Awake() {
        int hiScore = PlayerPrefs.GetInt("HiSocre",0);
        string score = hiScore < 10 ? "000" + hiScore : (hiScore >= 10 && hiScore < 100 ? "00" + hiScore : (hiScore >= 100 && hiScore < 1000 ? "0" + hiScore : hiScore.ToString()));
        txt_highscore.text = "HI-SCORE: " + score;
    }

    public void StartGame() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

}
