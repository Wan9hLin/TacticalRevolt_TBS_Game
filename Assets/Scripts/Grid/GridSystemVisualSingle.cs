using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridSystemVisualSingle : MonoBehaviour
{
    [SerializeField] private Image image;

    // 引用 8 个护盾的 GameObject
    [SerializeField] private GameObject fullNorth;
    [SerializeField] private GameObject fullSouth;
    [SerializeField] private GameObject fullEast;
    [SerializeField] private GameObject fullWest;
    [SerializeField] private GameObject halfNorth;
    [SerializeField] private GameObject halfSouth;
    [SerializeField] private GameObject halfEast;
    [SerializeField] private GameObject halfWest;

    [SerializeField] private GameObject warningIcon;

    public void Show(Color color)
    {
        image.enabled = true;
        image.color = color;

    }

    public void Hide()
    {
        image.enabled = false;
    }

    // 显示护盾UI
    public void ShowCoverUI(string coverType, string direction)
    {
        HideCoverUI(); // 先隐藏所有护盾

        // 根据 coverType 和 direction 显示对应的护盾
        if (coverType == "FullCover")
        {
            switch (direction)
            {
                case "North":
                    fullNorth.SetActive(true);
                    break;
                case "South":
                    fullSouth.SetActive(true);
                    break;
                case "East":
                    fullEast.SetActive(true);
                    break;
                case "West":
                    fullWest.SetActive(true);
                    break;
            }
        }
        else if (coverType == "HalfCover")
        {
            switch (direction)
            {
                case "North":
                    halfNorth.SetActive(true);
                    break;
                case "South":
                    halfSouth.SetActive(true);
                    break;
                case "East":
                    halfEast.SetActive(true);
                    break;
                case "West":
                    halfWest.SetActive(true);
                    break;
            }
        }
    }

    // 隐藏所有护盾UI
    public void HideCoverUI()
    {
        fullNorth.SetActive(false);
        fullSouth.SetActive(false);
        fullEast.SetActive(false);
        fullWest.SetActive(false);
        halfNorth.SetActive(false);
        halfSouth.SetActive(false);
        halfEast.SetActive(false);
        halfWest.SetActive(false);
    }

    public void ShowWarningIcon()
    {
        warningIcon.SetActive(true);
    }

    public void HideWarningIcon()
    {
        warningIcon.SetActive(false);
    }

}
