using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public DialogueLine[] lines;
}

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    public Sprite portrait;
    [TextArea(2, 5)]
    public string text;
    public AudioClip voice;
}

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] Canvas dialogueCanvas;
    [SerializeField] GameObject dialoguePanel;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] Image portrait;
    [SerializeField] GameObject continueIndicator;

    [Header("Settings")]
    [SerializeField] float typeSpeed = 0.04f;

    private DialogueLine[] lines;
    private int currentLine;
    private bool isTyping;
    private bool dialogueActive;
    private bool justStarted;
    private Coroutine typingCoroutine;
    private float blinkTimer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (!dialogueActive) return;

        if (!isTyping)
        {
            blinkTimer += Time.deltaTime;
            if (blinkTimer >= 0.4f)
            {
                blinkTimer = 0f;
                continueIndicator.SetActive(!continueIndicator.activeSelf);
            }
        }

        if (justStarted) { justStarted = false; return; }

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return))
            Advance();
    }

    public void StartDialogue(DialogueData data)
    {
if (dialogueActive || data == null || data.lines.Length == 0) return;

        lines = data.lines;
        currentLine = 0;
        dialogueActive = true;
        justStarted = true;
        if (dialogueCanvas != null) dialogueCanvas.gameObject.SetActive(true);
        dialoguePanel.SetActive(true);

        if (InputHander.Instance != null)
            InputHander.Instance.DisableInputs();

        ShowLine(currentLine);
    }

    void ShowLine(int index)
    {
        DialogueLine line = lines[index];

        nameText.text = line.speakerName;
        portrait.sprite = line.portrait;
        portrait.enabled = line.portrait != null;
        continueIndicator.SetActive(false);
        blinkTimer = 0f;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLine(line));
    }

    IEnumerator TypeLine(DialogueLine line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in line.text)
        {
            dialogueText.text += c;

            if (line.voice != null && c != ' ')
                AudioSource.PlayClipAtPoint(line.voice, Camera.main.transform.position, 0.4f);

            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
    }

    void Advance()
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = lines[currentLine].text;
            isTyping = false;
            continueIndicator.SetActive(true);
            return;
        }

        currentLine++;

        if (currentLine >= lines.Length)
        {
            EndDialogue();
            return;
        }

        ShowLine(currentLine);
    }

    void EndDialogue()
    {
        dialogueActive = false;
        dialoguePanel.SetActive(false);
        if (dialogueCanvas != null) dialogueCanvas.gameObject.SetActive(false);
        if (InputHander.Instance != null)
            InputHander.Instance.EnableInputs();
    }
}
