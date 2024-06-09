using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private GameObject coin;

    [SerializeField]
    private float moveSpeed = 5f;

    private float minY = -7f;


    [SerializeField] private float hp = 1f;

    public void SetMoveSpeed(float moveSpeed) {
        this.moveSpeed = moveSpeed;

    }

    // Update is called once per frame
    void Update()
    {

        transform.position += Vector3.down * moveSpeed * Time.deltaTime;

        if(transform.position.y < minY) {
            Destroy(gameObject);
        }
        
        // 보스가 안죽고 아래로 떨어지면 게임종료
        if(transform.position.y < -5 && gameObject.tag == "Boss") {
            GameManager.instance.SetGameOver();
        }

        


    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Weapon") {
            Weapon weapon = other.gameObject.GetComponent<Weapon>();
            hp -= weapon.damage;
            if (hp <= 0) {
                if (gameObject.tag == "Boss") {
                    GameManager gameManager = GameManager.instance;
                    gameManager.IncreaseCoin(30);
                    gameManager.SetGameOver();
                }
                Destroy(gameObject);
                Instantiate(coin, transform.position, Quaternion.identity);
            }
            Destroy(other.gameObject);
        }
    }
}
