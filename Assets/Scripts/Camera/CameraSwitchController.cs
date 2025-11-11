using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CameraSwitchController : MonoBehaviour
{
    [SerializeField] private GameObject cameraController1;
    [SerializeField] private GameObject cameraController2;
    [SerializeField] private Cinemachine.CinemachineVirtualCamera cinemachineVirtualCamera1;
    [SerializeField] private Cinemachine.CinemachineVirtualCamera cinemachineVirtualCamera2;
   
    private bool isCamera1Active = true; // 初始相机状态

    //相机切换UI
    [SerializeField] private TextMeshProUGUI cameNumberText;
    [SerializeField] private GameObject leftArrow;
    [SerializeField] private GameObject rightArrow;


    private void Awake()
    {
        // 设置初始状态：激活第一个相机及其控制器，禁用第二个
        ActivateCameraController(cameraController1, cinemachineVirtualCamera1, true);
        ActivateCameraController(cameraController2, cinemachineVirtualCamera2, false);
        UpdateCame1Text();
    }
   

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (!isCamera1Active)
            {
                ActivateCameraController(cameraController1, cinemachineVirtualCamera1, true);
                ActivateCameraController(cameraController2, cinemachineVirtualCamera2, false);
                isCamera1Active = true;
                UpdateCame1Text();
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (isCamera1Active)
            {
                ActivateCameraController(cameraController1, cinemachineVirtualCamera1, false);
                ActivateCameraController(cameraController2, cinemachineVirtualCamera2, true);
                isCamera1Active = false;
                UpdateCame2Text();
            }
        }
    }

    private void ActivateCameraController(GameObject cameraController, Cinemachine.CinemachineVirtualCamera camera, bool isActive)
    {
        cameraController.SetActive(isActive);
        camera.Priority = isActive ? 10 : 0; // 控制优先级以激活/停用相机
    }

    private void UpdateCame1Text()
    {
        leftArrow.SetActive(false);
        rightArrow.SetActive(true);
        cameNumberText.text = "Camera " + 1;
    }

    private void UpdateCame2Text()
    {
        leftArrow.SetActive(true);
        rightArrow.SetActive(false);
        cameNumberText.text = "Camera " + 2;
    }

}
