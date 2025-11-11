using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogueText; // 对话框文本
    [SerializeField] private Button nextButton; // 下一步按钮
    [SerializeField] private Button skipButton; //跳过按钮

    [SerializeField] private GameObject dialoguePanel; // 对话框面板

    [TextArea(3, 10)]
    [SerializeField] private string[] dialogueLines; // 对话内容，每段单独存储

    [SerializeField] private float typingSpeed = 0.05f; // 打字速度

    private int currentLineIndex = 0; // 当前对话索引
    private bool isTyping = false; // 当前是否正在打字
    private bool dialogueComplete = false; // 是否显示完成

    private Animator animator;


    private void Start()
    {
        dialoguePanel.SetActive(true);

        nextButton.onClick.AddListener(() =>
        {
            if (currentLineIndex >= dialogueLines.Length)
            {
                CloseDialogue();
            }
            else
            {
                OnNextButtonClicked();
            }
        });

        skipButton.onClick.AddListener(SkipDialogue);
        StartDialogue();
        animator = gameObject.GetComponent<Animator>();
    }

    // 开始对话
    public void StartDialogue()
    {
        currentLineIndex = 0;
        ShowLine();
    }

    // 显示当前对话行
    private void ShowLine()
    {
        if (currentLineIndex < dialogueLines.Length)
        {
            StopAllCoroutines(); // 停止任何正在进行的打字
            StartCoroutine(TypeLine(dialogueLines[currentLineIndex]));
        }
       
    }

    // 打字效果的协程
    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = ""; // 清空对话框
        AudioManager.Instance.Play("Typing");
        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed); // 控制打字速度
        }
        isTyping = false;
        AudioManager.Instance.Stop("Typing");
    }

    // 下一步按钮点击事件
    private void OnNextButtonClicked()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = dialogueLines[currentLineIndex];
            isTyping = false;
            AudioManager.Instance.Stop("Typing");
        }
        else
        {
            // 如果当前是最后一行，直接关闭对话框
            if (currentLineIndex >= dialogueLines.Length - 1)
            {
                CloseDialogue();
                AudioManager.Instance.Play("Button_2");
            }
            else
            {
                // 继续显示下一行
                currentLineIndex++;
                ShowLine();
                AudioManager.Instance.Play("Button_2");
            }
        }
    }

    // 关闭对话框
    private void CloseDialogue()
    {
        animator.SetTrigger("Close");       
        AudioManager.Instance.Stop("Typing");
    }

    // 关闭对话框
    private void SkipDialogue()
    {
        animator.SetTrigger("Close");
        AudioManager.Instance.Stop("Typing");
        AudioManager.Instance.Play("Button_2");
    }
}
