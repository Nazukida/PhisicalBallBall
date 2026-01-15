using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BallBehavior : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float Speed = 5.0f; // 初速度大小
    [SerializeField] private float decreaseSpeed = 0.1f; // 碰撞后的减速值
    [SerializeField] private float minimumSpeed = 0.3f; // 最小速度阈值
    [SerializeField] private float minHorizontalSpeed = 0.2f; // 防止垂直死循环的最小水平速度
    [SerializeField] private float stuckRecoverySpeed = 0.5f; // 卡住时恢复的速度

    [Header("Sliding Prevention")]
    [SerializeField] private float slidingSpeedThreshold = 0.05f; // 判定为"贴边滑动"的速度阈值
    [SerializeField] private float slidingPushForce = 5f; // 贴边时推离的力度
    [SerializeField] private float maxContactTime = 0.5f; // 允许的最大接触时间（秒）

    [Header("Initial Direction")]
    [SerializeField] private Vector2 initialDirection = new Vector2(0.5f, 0.3f); // 初速度的方向 (X, Y)

    private Rigidbody2D rb;
    private float contactTimer = 0f; // 记录持续接触时间
    private bool isInContact = false; // 是否正在接触
    private Vector2 lastContactNormal; // 最近一次接触的法线方向

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Vector2 ini = initialDirection.normalized; 
        rb.velocity = ini * Speed;
    }

    void FixedUpdate()
    {
        // 每帧检测：如果球几乎停止，强制给它一个随机方向的速度
        if (rb.velocity.magnitude < 0.05f)
        {
            Vector2 randomDir = new Vector2(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            ).normalized;

            if (randomDir == Vector2.zero)
            {
                randomDir = Vector2.right;
            }

            rb.velocity = randomDir * stuckRecoverySpeed;
            Debug.Log("Ball was stuck, applying recovery velocity.");
        }

        // 如果正在接触且速度过低，累加接触时间
        if (isInContact && rb.velocity.magnitude < slidingSpeedThreshold)
        {
            contactTimer += Time.fixedDeltaTime;

            // 如果接触时间超过阈值，说明球在"贴边滑动"
            if (contactTimer >= maxContactTime)
            {
                // 沿法线方向推离
                rb.velocity += lastContactNormal * slidingPushForce;
                Debug.Log("Ball was sliding along edge, applying push force.");
                contactTimer = 0f; // 重置计时器
            }
        }
    }

    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ObSquare") || collision.gameObject.CompareTag("ObTriangle") || collision.gameObject.CompareTag("ObjectHex"))
        {
            Debug.Log("hit");
            Vector2 curV = rb.velocity;
            if(curV.magnitude > minimumSpeed)
            {
                Vector2 newV = curV.normalized * (curV.magnitude - decreaseSpeed);

                if (Mathf.Abs(newV.x) < minHorizontalSpeed)
                {
                    float sign = (newV.x == 0) ? (Random.value > 0.5f ? 1f : -1f) : Mathf.Sign(newV.x);
                    newV.x = sign * minHorizontalSpeed;
                }

                rb.velocity = newV;
            }
        }
    }

    // 持续接触时调用
    private void OnCollisionStay2D(Collision2D collision)
    {
        isInContact = true;

        // 获取接触点的法线（垂直于接触面的方向）
        if (collision.contactCount > 0)
        {
            lastContactNormal = collision.GetContact(0).normal;
        }
    }

    // 离开接触时调用
    private void OnCollisionExit2D(Collision2D collision)
    {
        isInContact = false;
        contactTimer = 0f; // 重置计时器
    }
}
