using Michsky.UI.ModernUIPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject loadingPanel;
    private Button[] buttons;

    [SerializeField] private Button submitBtn;


    private void Awake()
    {
        buttons = FindObjectsOfType<Button>();
    }

    private void OnEnable()
    {
        UploadManager.instance.loadingAction += Loading;
        UploadManager.instance.uploadDoneAction += Done;
    }
    private void OnDisable()
    {
        UploadManager.instance.loadingAction -= Loading;
        UploadManager.instance.uploadDoneAction -= Done;
    }

    private void Loading()
    {
        loadingPanel.SetActive(true);

        foreach (Button btn in buttons)
        {
            btn.interactable = false;
        }
    }

    private void Done()
    {
        loadingPanel.SetActive(false);

        foreach (Button btn in buttons)
        {
            btn.interactable = true;
        }
    }

    private void Update()
    {
        submitBtn.interactable = UploadManager.instance.CanSubmit();
    }
}
