using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovedSweet : MonoBehaviour
{
    private GameSweet sweet;

    private IEnumerator moveCoroutine;//�õ�����ָ��ʱ��ֹ��Э��
    private void Awake()
    {
        sweet = GetComponent<GameSweet>();
    }

    //������ر�һ��Э��
    public void Move(int newX,int newY,float time)
    {
       if(moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);//ֹͣЭ��
        }

        moveCoroutine = MoveCoroutine(newX,newY, time);//��Э�̷�����ֵ���洢��moveCoroutine��
        StartCoroutine(moveCoroutine);//����Э��
    }
    //�����ƶ���Э�̳���
    private IEnumerator MoveCoroutine(int x,int y,float time)
    {
        sweet.X = x;
        sweet.Y = y;

        //ÿ֡�ƶ�һ��
        Vector3 startPos = transform.position;
        Vector3 endPos = sweet.gameManager.CorrectPosition(x, y);
        for(float t = 0;t < time; t+=Time.deltaTime)
        {
            sweet.transform.position = Vector3.Lerp(startPos, endPos,t/time);
            yield return 0;
        }
        sweet.transform.position = endPos;//ǿ���ƶ���ָ��λ��
    }
}
