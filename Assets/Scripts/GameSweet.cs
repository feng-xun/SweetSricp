using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSweet : MonoBehaviour
{
    private int x;
    private int y;
    private GameManager.SweetsType type;
    private MovedSweet movedComponent;
    private ColorSweet colorComponent;
    private ClearedSweet clearedComponent;

    [HideInInspector] //�ù������Բ���ʾ�������
    public GameManager gameManager;


    public int X { get => x; 
        set 
        { 
            if(CanMove())
            x = value; 
        } 
    }
    public int Y { get => y; set
        {
            if (CanMove())
                y = value;
        }
    }
    public GameManager.SweetsType Type { get => type; }
    public MovedSweet MovedComponent { get => movedComponent; }
    public ColorSweet ColorComponent { get => colorComponent; }
    public ClearedSweet ClearedComponent { get => clearedComponent; }
    

    //��ʼ���ű�
    public void Init(int _x,int _y,GameManager _gameManager,GameManager.SweetsType _type)
    {
        X = _x;
        y = _y;
        gameManager = _gameManager;
        type = _type;
    }
    //�ж��Ƿ�����ƶ�
    public bool CanMove()
    {
        return movedComponent != null;
    }
    public bool CanColor()
    {
        return colorComponent != null;
    }
    public bool CanClear()
    {
        return clearedComponent != null;
    }

    private void Awake()
    {
        movedComponent = GetComponent<MovedSweet>();
        colorComponent = GetComponent<ColorSweet>();
        clearedComponent = GetComponent<ClearedSweet>();
    }

    private void OnMouseEnter()
    {
        
        gameManager.EnterSweet(this);
    }
    private void OnMouseDown()
    {
        gameManager.PreeSweet(this);
    }
    private void OnMouseUp()
    {
        gameManager.ReleaseSweet();
    }

}
