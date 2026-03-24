using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eye : MonoBehaviour
{
    [SerializeField] private GameObject player;

    [SerializeField] private float moveLimit = 2f;
    [SerializeField] private float maxRange = 10f;

    Vector2 originalPosition;

    private void Awake()
    {
        originalPosition = transform.position;
        if (player == null) {
            player = GameObject.FindGameObjectWithTag("Player");
        }
    }

    private void Update()
    {
        if (Mathf.Abs(player.transform.position.x - transform.position.x) > maxRange) {
            transform.position = new Vector3(Mathf.Clamp(player.transform.position.x, originalPosition.x - moveLimit, originalPosition.x + moveLimit), transform.position.y, transform.position.z);
        }else {
            transform.position = new Vector3((player.transform.position.x - transform.position.x)/maxRange*moveLimit + originalPosition.x, transform.position.y, transform.position.z);
        }
        
    }



}
