using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public static GameManager instance = null;

    [SerializeField]
    private TextMeshProUGUI text;

    [SerializeField]
    private GameObject gameOverPanel;
    
    private int coin = 0;

    [HideInInspector]
    public bool isGameOver = false;

    public int coinLevel = 0;
    public float moveSpeed = 5;


    void Awake() {
        if (instance == null) {
            instance = this;
        }
    }

    public void IncreaseCoin(int score) {
        coin += score;
        text.SetText(coin.ToString());

        if (coin % 20 == 0) {
            Player player = FindObjectOfType<Player>();
            if (player != null) {
                coinLevel += 1;
                player.Upgrade();
            }
        }
    }

    public float GetCoin() {
        float value;
        float.TryParse(text.text, out value);

        return value;
    }

    public float GetCoinLevel() {
        float value;
        float.TryParse(text.text, out value);

        return value;
    }

    public void SetGameOver() {
        isGameOver = true;
        
        EnemySpwner enemySpwner = FindObjectOfType<EnemySpwner>();
        if(enemySpwner != null) {
            enemySpwner.StopEnemtRoutine();
        }

        Invoke("ShowGameOverPanel", 1f);
    }

    void ShowGameOverPanel() {
        gameOverPanel.SetActive(true);

        Invoke("playAgain", 1f);
    }

    public void playAgain() {
        SceneManager.LoadScene("SampleScene");
    }
}
