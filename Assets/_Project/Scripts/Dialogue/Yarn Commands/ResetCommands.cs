using UnityEngine;
using Yarn.Unity;

public class ResetObject : MonoBehaviour
{
    private Transform player;
    private Transform aChaser;
    private Vector3 playerInitialPosition;
    private Vector3 aChaserInitialPosition;
    private Quaternion playerInitialRotation;
    private Quaternion aChaserInitialRotation;
    private bool playerInitialRunning =true;

    void Start()
    {
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        var chaserGO = GameObject.FindGameObjectWithTag("AChaser");

        if (playerGO != null)
        {
            player = playerGO.transform;
            playerInitialPosition = player.position;
            playerInitialRotation = player.rotation;
        }

        if (chaserGO != null)
        {
            aChaser = chaserGO.transform;
            aChaserInitialPosition = aChaser.position;
            aChaserInitialRotation = aChaser.rotation;
        }
    }

    [YarnCommand("reset_object")]
    public void ResetObjectState()
    {
        // 重置 Player
        if (player != null)
        {
            player.SetPositionAndRotation(playerInitialPosition, playerInitialRotation);
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            var anim = player.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetBool("Running", playerInitialRunning);
            }
        }

        // 重置 AChaser
        if (aChaser != null)
        {
            aChaser.SetPositionAndRotation(aChaserInitialPosition, aChaserInitialRotation);
            var rb = aChaser.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

        // 可选：关卡容器做一次启停，避免销毁导致引用断裂
        var wasActive = gameObject.activeSelf;
        if (wasActive)
        {
            gameObject.SetActive(false);
            gameObject.SetActive(true);
        }
    }
}