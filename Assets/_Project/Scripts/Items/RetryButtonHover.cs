using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RetryButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField] private TextMeshProUGUI primaryText;
	[SerializeField] private TextMeshProUGUI shadowText;

	[SerializeField] private string normalText = "RETRY";
	[SerializeField] private string hoverText = "> RETRY <";

	[Header("Yarn 重试对话设置")]
	[SerializeField] private string retryDialogueNodeName = ""; // 在 Inspector 中指定对应的重试节点

	private Button cachedButton;

	private void Reset()
	{
		primaryText = GetComponentInChildren<TextMeshProUGUI>();
		if (primaryText == null) return;

		var allTmps = GetComponentsInChildren<TextMeshProUGUI>(true);
		foreach (var tmp in allTmps)
		{
			if (tmp != primaryText)
			{
				shadowText = tmp;
				break;
			}
		}
	}

	private void Awake()
	{
		cachedButton = GetComponent<Button>();
		if (cachedButton != null)
		{
			cachedButton.onClick.AddListener(OnRetryClicked);
		}
		ApplyText(normalText);
	}

	private void OnEnable()
	{
		ApplyText(normalText);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		ApplyText(hoverText);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		ApplyText(normalText);
	}

	private void ApplyText(string value)
	{
		if (primaryText != null) primaryText.text = value;
		if (shadowText != null) shadowText.text = value;
	}

	private void OnDestroy()
	{
		if (cachedButton != null)
		{
			cachedButton.onClick.RemoveListener(OnRetryClicked);
		}
	}

	private void OnRetryClicked()
	{
		if (string.IsNullOrWhiteSpace(retryDialogueNodeName))
		{
			Debug.LogWarning("[RetryButtonHover] 未设置重试的 Yarn 节点名，无法开始对话。");
			return;
		}

		if (YarnSpinnerManager.Instance == null)
		{
			Debug.LogError("[RetryButtonHover] YarnSpinnerManager.Instance 为 null，无法开始对话。");
			return;
		}

		// 重试不需要存档检查，直接进入对应节点
		YarnSpinnerManager.Instance.StartDialogueSafe(retryDialogueNodeName, false);
	}
}


