using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovedSweet : MonoBehaviour
{
    private GameSweet sweet;

    private IEnumerator moveCoroutine;//得到其他指令时终止此协程
    private void Awake()
    {
        sweet = GetComponent<GameSweet>();
    }

    //开启或关闭一个协程
    public void Move(int newX,int newY,float time)
    {
       if(moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);//停止协程
        }

        moveCoroutine = MoveCoroutine(newX,newY, time);//将协程方法赋值并存储到moveCoroutine中
        StartCoroutine(moveCoroutine);//开启协程
    }
    //负责移动的协程程序
    private IEnumerator MoveCoroutine(int x,int y,float time)
    {
        sweet.X = x;
        sweet.Y = y;

        //每帧移动一点
        Vector3 startPos = transform.position;
        Vector3 endPos = sweet.gameManager.CorrectPosition(x, y);
        for(float t = 0;t < time; t+=Time.deltaTime)
        {
            sweet.transform.position = Vector3.Lerp(startPos, endPos,t/time);
            yield return 0;
        }
        sweet.transform.position = endPos;//强制移动到指定位置
    }
}
