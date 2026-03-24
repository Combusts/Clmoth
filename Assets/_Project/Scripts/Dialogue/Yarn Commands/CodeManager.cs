using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

[System.Serializable]
public class CodeLinesList
{
    public List<string> CodeString;
    public int Index;
    public Color VertexColor = Color.white;
    public float WaitTime = 0.3f;
}

[RequireComponent(typeof(TextMeshProUGUI))]
public class CodeManager : MonoBehaviour
{
    [SerializeField] private List<CodeLinesList> CodeLines = new List<CodeLinesList>();
    private TextMeshProUGUI codeDisplayText;
    private void Awake()
    {
        codeDisplayText = GetComponent<TextMeshProUGUI>();
    }

    public IEnumerator ShowCode(int index)
    {
        // 隔一段时间显示一段代码
        yield return ShowCodeCoroutine(CodeLines[index]);
    }

    private IEnumerator ShowCodeCoroutine(CodeLinesList codeLinesList)
    {
        // 设置该 CodeLinesList 的顶点颜色
        codeDisplayText.color = codeLinesList.VertexColor;
        
        foreach (string code in codeLinesList.CodeString)
        {
            // 实现打字机效果
            for (int i = 0; i < code.Length; i++)
            {
                codeDisplayText.text += code[i];
                yield return new WaitForSeconds(0.03f);
            }
            codeDisplayText.text += "\n";
            yield return new WaitForSeconds(codeLinesList.WaitTime);
            codeLinesList.Index++;
            if (codeLinesList.Index >= codeLinesList.CodeString.Count)
            {
                codeDisplayText.text = "";
                yield break;
            }
        }
    }

    [YarnCommand("codePrint")]
    public IEnumerator CodePrint(int index)
    {
        yield return ShowCode(index);
    }
}