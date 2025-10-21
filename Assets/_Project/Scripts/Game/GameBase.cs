using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameBase : MonoBehaviour
{
    [SerializeField]
    private GameObject controlItem;
    [SerializeField]
    private float upperBound = 0;
    [SerializeField]
    private float lowerBound = -4;
    [SerializeField]
    private float moveSpeed = 2f;
    [SerializeField]
    private GameObject bulletPrefab;
    [SerializeField]
    private float shotCooldown = 0.5f;
    private float lastShotTime = 0;
    [SerializeField]
    private GameObject hint;
    [SerializeField]
    private GameObject Shooter;
    private TargetColor nextColor;

    public static GameBase Instance { get; private set; }
    private int enemyCount = 0;
    private void Awake()
    {
        Instance = this;
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    private void Start()
    {
        nextColor = (TargetColor)Random.Range(0, System.Enum.GetNames(typeof(TargetColor)).Length);
        hint.GetComponent<ColoredItem>().SwitchToColor(nextColor);
        Shooter.GetComponent<ColoredItem>().SwitchToColor(nextColor);
    }
    void Update()
    {
        if (Keyboard.current.wKey.isPressed)
        {
            if (controlItem.transform.position.y < upperBound)
            {
                controlItem.transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
            }
        } else if (Keyboard.current.sKey.isPressed)
        {
            if (controlItem.transform.position.y > lowerBound)
            {
                controlItem.transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
            }
        }
        if (Keyboard.current.spaceKey.isPressed)
        {
            if (Time.time - lastShotTime > shotCooldown)
            {
                lastShotTime = Time.time;
                GameObject bullet = bulletPrefab;
                bullet.transform.position = controlItem.transform.position;
                bullet.GetComponent<ColoredItem>().color = nextColor;
                Instantiate(bullet, controlItem.transform.position, Quaternion.identity);

                nextColor = (TargetColor)Random.Range(0, System.Enum.GetNames(typeof(TargetColor)).Length);
                hint.GetComponent<ColoredItem>().SwitchToColor(nextColor);
                Shooter.GetComponent<ColoredItem>().SwitchToColor(nextColor);
            }
        }
    }

    public void AddEnemyCount()
    {
        enemyCount++;
    }
    public void RemoveEnemyCount()
    {
        enemyCount--;
    }
    public void CheckGameWin()
    {   
        Debug.Log("enemyCount: " + enemyCount);
        if (enemyCount == 0)
        {
            Debug.Log("GameWin");
            UIManager.Instance.HideAllUI();
            UIManager.Instance.ShowUI("MiniGameWin");
            Time.timeScale = 0;
        }
    }

    public void GameOver()
    {
        Debug.Log("GameOver");
        UIManager.Instance.HideAllUI();
        UIManager.Instance.ShowUI("MiniGameOver");
        Time.timeScale = 0;
    }

}
