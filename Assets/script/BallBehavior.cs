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
    
    // 新增：防止垂直死循环的最小水平速度
    [SerializeField] private float minHorizontalSpeed = 0.2f; 

    [Header("Initial Direction")]
    [SerializeField] private Vector2 initialDirection = new Vector2(0.5f, 0.3f); // 初速度的方向 (X, Y)

    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // 使用 Inspector 中设置的方向，并进行归一化
        Vector2 ini = initialDirection.normalized; 
        rb.velocity = ini * Speed;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ObSquare") || collision.gameObject.CompareTag("ObTriangle"))
        {
            Debug.Log("hit");
            Vector2 curV = rb.velocity;
            if(curV.magnitude > minimumSpeed)
            {
                // 1. 先计算原本逻辑中的减速后速度
                Vector2 newV = curV.normalized * (curV.magnitude - decreaseSpeed);

                // 2. 【关键修改】检查 X 轴速度绝对值是否过小
                if (Mathf.Abs(newV.x) < minHorizontalSpeed)
                {
                    // 如果 X 速度太小（比如撞墙后几乎垂直），强制给它一个水平分量
                    // 如果当前 x 是 0，随机给一个左或右的方向；否则保持原有方向
                    float sign = (newV.x == 0) ? (Random.value > 0.5f ? 1f : -1f) : Mathf.Sign(newV.x);
                    
                    // 强制修改 X 轴速度
                    newV.x = sign * minHorizontalSpeed;
                }

                rb.velocity = newV;
            }
            
        }
    }
}
