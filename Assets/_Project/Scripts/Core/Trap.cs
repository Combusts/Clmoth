using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Collider2D))]
public class Trap : MonoBehaviour
{
    [SerializeField] public string dialogueNodeName;
    [SerializeField] public bool isOneTime = true;
}
