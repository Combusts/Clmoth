using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : ColoredItem
{
    [SerializeField]
    private float moveSpeed = 10f;


    private new void Start()
    {
        base.Start();
    }

    void Update()
    {
        transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);

        if (transform.position.x > 10)
        {
            Destroy(gameObject);
        }
    }

}
