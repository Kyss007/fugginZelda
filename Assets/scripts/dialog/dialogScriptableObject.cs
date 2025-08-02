using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "dialog", menuName = "ScriptableObjects/Dialog")]
public class dialogScriptableObject : ScriptableObject
{
    public string speaker;
    public List<dialogPage> dialogPages;
}

[System.Serializable]
public class dialogPage
{
    public bool terminator;
    public string text;
    public List<dialogOption> options;
}

[System.Serializable]
public class dialogOption
{
    public string optionText;
    public int targetPage;
}
