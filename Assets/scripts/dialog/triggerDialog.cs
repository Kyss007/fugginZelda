using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class triggerDialog : MonoBehaviour
{
    public GameObject dialogPrefab;
    public GameObject dialogOptionPrefab;


    private int currentPage = 0;
    private GameObject currentDialogObject = null;
    private dialogHelper currentDialogHelper = null;

    private bool doneDisplayPage = false;


    public bool getInDialog()
    {
        if(currentDialogObject == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void doTriggerDialog(dialogScriptableObject dialogToUse)
    {
        if(currentDialogObject != null)
            return;

        currentDialogObject = Instantiate(dialogPrefab);
        currentDialogHelper = currentDialogObject.GetComponent<dialogHelper>();

        currentDialogHelper.nameText.text = dialogToUse.speaker;

        StartCoroutine(doDialogRoutine(dialogToUse));
    }

    public IEnumerator doDialogRoutine(dialogScriptableObject dialogToUse)
    {
        bool isDone = false;

        while(!isDone)
        {
            FindFirstObjectByType<keanusCharacterController>().canMove = false;

            for (int i = 0; i < dialogToUse.dialogPages.Count && !isDone; i++)
            {
                bool hadOptions = false;

                doneDisplayPage = false;
                StartCoroutine(routineDelayedText(dialogToUse.dialogPages[currentPage].text, 0.01f));

                currentDialogHelper.endOfTextIndicator.SetActive(false);

                while (!doneDisplayPage)
                {
                    yield return null;
                }

                currentDialogHelper.endOfTextIndicator.SetActive(true);

                while (!Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.Mouse0))
                {
                    yield return null;
                }

                if (dialogToUse.dialogPages[currentPage].options.Count != 0)
                {
                    hadOptions = true;

                    dialogOptionText optionText = Instantiate(dialogOptionPrefab, currentDialogHelper.dialogOptionParent.transform).GetComponent<dialogOptionText>();

                    foreach (dialogOption option in dialogToUse.dialogPages[currentPage].options)
                    {
                        optionText.addOption(option.optionText);
                    }

                    currentDialogHelper.endOfTextIndicator.SetActive(false);

                    while (optionText.optionSelected() == -1)
                    {
                        yield return null;
                    }

                    currentPage = dialogToUse.dialogPages[currentPage].options[optionText.optionSelected()].targetPage;

                    Destroy(optionText.gameObject);
                }


                if (!hadOptions)
                {
                    if (dialogToUse.dialogPages[currentPage].terminator)
                        isDone = true;

                    currentPage++;
                }
            }

            yield return null;
        }

        FindFirstObjectByType<keanusCharacterController>().canMove = true;

        Destroy(currentDialogObject);

        currentDialogObject = null;
        currentDialogHelper = null;

        currentPage = 0;
    }

    private IEnumerator routineDelayedText(string textToDisplay, float timeDelay)
    {
        WaitForSeconds delay = new WaitForSeconds(timeDelay);

        for (int i = 0; i < textToDisplay.Length; ++i)
        {
            string delayedText = textToDisplay.Substring(0, i + 1);
            currentDialogHelper.dialogText.text = delayedText;

            yield return delay;
        }

        doneDisplayPage = true;
    }
}
