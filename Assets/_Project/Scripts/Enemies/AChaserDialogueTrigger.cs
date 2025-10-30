using UnityEngine;

[RequireComponent(typeof(CapsuleCollider2D))]
public class AChaserDialogueTrigger : MonoBehaviour
{
	[Header("Yarn 对话设置")]
	[SerializeField] private string dialogueNodeName = "AChaserEncounter";
	[SerializeField] private bool checkSaveState = true;
	[SerializeField] private bool onlyOnce = true;

	[Header("触发设置")]
	[SerializeField] private float retriggerCooldownSeconds = 2f;

	private bool hasTriggered = false;
	private float lastTriggerTime = -999f;

	private CapsuleCollider2D capsuleCollider;

	private void Awake()
	{
		capsuleCollider = GetComponent<CapsuleCollider2D>();
	}

	private void OnCollisionEnter2D(UnityEngine.Collision2D collision)
	{
		if (!collision.gameObject.CompareTag("Player")) return;

		if (onlyOnce && hasTriggered) return;

		if (Time.time - lastTriggerTime < retriggerCooldownSeconds) return;

		// 如果对话系统正处于对话中，避免重复触发
		if (YarnSpinnerManager.Instance != null && YarnSpinnerManager.Instance.IsDialogueActive)
		{
			return;
		}

		Debug.Log("[AChaserDialogueTrigger] 玩家进入触发区域，触发对话");

		StartDialogue();
	}

	private void StartDialogue()
	{
		lastTriggerTime = Time.time;
		hasTriggered = true;

		if (YarnSpinnerManager.Instance == null)
		{
			Debug.LogError("[AChaserDialogueTrigger] YarnSpinnerManager.Instance 为 null，无法开始对话。");
			return;
		}

		// 使用带存档检查的安全启动
		YarnSpinnerManager.Instance.StartDialogueSafe(dialogueNodeName, checkSaveState);
	}
}


