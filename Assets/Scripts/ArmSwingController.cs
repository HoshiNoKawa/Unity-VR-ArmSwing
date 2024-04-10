using UnityEngine;
using UnityEngine.Serialization;

// using Valve.VR;

public class ArmSwingController : MonoBehaviour
{
    public float gravity = 30.0f;
    public float maxSpeed = 1.0f;
    public Transform leftHandTracker, rightHandTracker;
    public float maxHandMoveVel = 20;
    public AnimationCurve speedCurve;

    private float actualSpeed;
    private CharacterController characterController;
    private Transform head = null;
    private Vector3 lastPosL, nowPosL, lastPosR, nowPosR, lastVelL, nowVelL, lastVelR, nowVelR;
    private float velMagL, velMagR, velMagLR;
    private float speedRatio;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        // head = SteamVR_Render.Top().head;
        
        if (leftHandTracker is null || rightHandTracker is null)
        {
            print("请先绑定对应的Tracker！");
        }
        else
        {
            lastPosR = nowPosR = rightHandTracker.position;
            lastVelR = nowVelR = Vector3.zero;

            lastPosL = nowPosL = leftHandTracker.position;
            lastVelL = nowVelL = Vector3.zero;

            velMagLR = 0;
        }
    }

    private void Update()
    {
        HandleHeight();
        CalculateSpeed();
        CalculateMovement();
    }

    private void HandleHeight()
    {
        
        head ??= transform;     // 正常需要配合SteamVR（28行）使用，此处仅为demo演示
        Vector3 headPos = head.localPosition;
        float headHeight = Mathf.Clamp(headPos.y, 1, 2);
        characterController.height = headHeight;

        Vector3 newCenter = Vector3.zero;
        newCenter.y = characterController.height / 2;
        newCenter.y += characterController.skinWidth;

        newCenter.x = headPos.x;
        newCenter.z = headPos.z;

        characterController.center = newCenter;
    }

    private void CalculateMovement()
    {
        Vector3 orientationEuler = new Vector3(0, head.eulerAngles.y, 0);
        Quaternion orientation = Quaternion.Euler(orientationEuler);
        Vector3 movement = Vector3.zero;

        actualSpeed = speedRatio * maxSpeed;
        movement += orientation * (actualSpeed * Vector3.forward);

        movement.y -= gravity * Time.deltaTime;

        characterController.Move(movement * Time.deltaTime);
    }

    private void CalculateSpeed()
    {
        nowPosL = leftHandTracker.position;
        nowPosR = rightHandTracker.position;

        if (nowPosL == lastPosL)
        {
            // 针对由于Tracker信号同步频率不足导致意外减速，非必要
            nowVelL = lastVelL;
        }
        else
        {
            nowVelL = (nowPosL - lastPosL) / Time.deltaTime;
        }

        if (nowPosR == lastPosR)
        {
            nowVelR = lastVelR;
        }
        else
        {
            nowVelR = (nowPosR - lastPosR) / Time.deltaTime;
        }

        velMagL = nowVelL.magnitude;
        velMagR = nowVelR.magnitude;

        velMagL = speedCurve.Evaluate(Mathf.Clamp(velMagL, 0, maxHandMoveVel) / maxHandMoveVel);
        velMagR = speedCurve.Evaluate(Mathf.Clamp(velMagR, 0, maxHandMoveVel) / maxHandMoveVel);

        velMagLR = Mathf.Max(velMagL, velMagR);
        speedRatio = Mathf.Clamp01(speedRatio + velMagLR);

        lastPosL = nowPosL;
        lastVelL = nowVelL;
        lastPosR = nowPosR;
        lastVelR = nowVelR;
    }
}
