using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class GameSaveCommands : MonoBehaviour
{
    /// <summary>
    /// 保存游戏
    /// </summary>
    [YarnCommand("save_game")]
    public void SaveGame()
    {
        SaveManager.Instance.SaveGame();
    }
}
