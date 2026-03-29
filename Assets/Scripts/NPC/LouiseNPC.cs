using System.Collections;
using UnityEngine;

public class LouiseNPC : MonoBehaviour
{
    [SerializeField] HeartUI heartUI;
    [SerializeField] LouiseHealPrompt healPrompt;
    [SerializeField] DialogueData preHealDialogue;
    [SerializeField] DialogueData acceptDialogue;
    [SerializeField] DialogueData denyDialogue;

    private bool playerInRange;
    private bool sequenceRunning;

    void Update()
    {
        if (playerInRange && !sequenceRunning && (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return)))
            StartCoroutine(HealSequence());
    }

    IEnumerator HealSequence()
    {
        sequenceRunning = true;

        DialogueManager.Instance.StartDialogue(preHealDialogue);
        yield return new WaitUntil(() => !DialogueManager.Instance.IsActive);

        bool? choice = null;
        healPrompt.Show(() => choice = true, () => choice = false);
        yield return new WaitUntil(() => choice.HasValue);

        if (choice.Value)
        {
            heartUI.FullHeal();
            DialogueManager.Instance.StartDialogue(acceptDialogue);
            yield return new WaitUntil(() => !DialogueManager.Instance.IsActive);
        }
        else
        {
            DialogueManager.Instance.StartDialogue(denyDialogue);
            yield return new WaitUntil(() => !DialogueManager.Instance.IsActive);
        }

        sequenceRunning = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}
