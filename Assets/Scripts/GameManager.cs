using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    //����
    #region

    //UI
    public Text timeText; //ʱ����ʾ�ı�
    private float gameTime = 60f;//��ʼ��Ϸʱ��
    private bool gameOver;//��Ϸ����
    private float addScoreTime;
    private float currentScore;
    public int playerScore;//��Ϸ�÷�
    public Text playerScoreText;//��Ϸ�÷��ı�
    public Text finalScoreText;//���յ÷��ı�
    public GameObject gameOverPanel;//��Ϸ����ҳ�滭��
    

    //������
    public int xColumn;
    public int yRow;
    public GameObject gritPrefab;//����Ԥ����

    //��Ʒ�ű�����
    private GameSweet[,] sweets;

    //Ҫ���н�����������Ʒ
    private GameSweet pressedSweet;
    private GameSweet enteredSweet;

    //�����ṹ������
    public SweetPrefab[] sweetPrefabs;

    //����һ����ƷԤ������ֵ䣬ͨ������õ���Ӧ����Ʒ��Ϸ����
    public Dictionary<SweetsType, GameObject> sweetPrefabDict;//��Ϊ����ö�٣�ֵΪ��Ϸ����

    //����
    private static GameManager _instance;
    public static GameManager Instance { get => _instance; set => _instance = value; }
    //��ö�����Ͷ��� ����
    public enum SweetsType
    {
        EMPTY,
        NORMAL,
        BARRRIER,
        ROW_CLEAR,
        COLUMN_CLEAR,
        RAINBOWCANDY,
        COUNT//�������
    }

    //����һ���ṹ��
    [System.Serializable]//�����л�
    public struct SweetPrefab
    {
        public SweetsType type;
        public GameObject prefab;
    }
    #endregion

    private void Awake()
    {
        //ʵ������������
        _instance = this;
    }

    private void Start()
    {
        //�ֵ�ʵ����
        sweetPrefabDict = new Dictionary<SweetsType, GameObject>();
        //Ϊ�ֵ丳ֵ
        for (int i = 0; i < sweetPrefabs.Length; i++)
        {
            //�ж��ֵ�ļ����Ƿ���ڵ�ǰ�ṹ�������е�type����
            if (!sweetPrefabDict.ContainsKey(sweetPrefabs[i].type))
            {
                //���û���򽫵�ǰ�ṹ�帳���ֵ䣬type����Ϊ����prefab����Ϊֵ
                sweetPrefabDict.Add(sweetPrefabs[i].type, sweetPrefabs[i].prefab);
            }
        }   
        //ʵ��������
        for(int x = 0; x < xColumn; x++)
        {
            for(int y = 0; y < yRow; y++)
            {
                GameObject chocolate = Instantiate(gritPrefab,CorrectPosition(x,y),Quaternion.identity);
                chocolate.transform.SetParent(transform);
            }
        }
        //ʵ������Ʒ����
        sweets = new GameSweet[xColumn, yRow];
        for (int x = 0; x < xColumn; x++)
        {
            for (int y = 0; y < yRow; y++)
            {
                CreateNewSweet(x, y, SweetsType.EMPTY);
            }
        }
        Destroy(sweets[4, 4].gameObject);
        CreateNewSweet(4, 4, SweetsType.BARRRIER);

        StartCoroutine(AllFill());//����Я��
    }

    private void Update()
    {
        gameTime -= Time.deltaTime;//ˢ��ʱ��
        if(gameTime <= 0)//ʱ��Ϊ0����Ϸ����
        {
            gameTime = 0;
            
            gameOver = true;
            finalScoreText.text = playerScore.ToString("0");
            gameOverPanel.SetActive(true);
        }
        timeText.text = gameTime.ToString("0");//ȡ��,ʱ����ʾ

        //������ʾ
        if (addScoreTime <= 0.1f)
        {
            addScoreTime += Time.deltaTime;
        }
        else
        {
            if(currentScore < playerScore)
            {
                currentScore++;
                playerScoreText.text = currentScore.ToString("0");
                addScoreTime = 0;
            }
        }
        
    }

    //������Ʒ�ķ���
    public GameSweet CreateNewSweet(int x,int y,SweetsType type)
    {
        GameObject newSweet = Instantiate(sweetPrefabDict[type], CorrectPosition(x, y), Quaternion.identity);
        newSweet.transform.SetParent(transform);
        sweets[x, y] = newSweet.GetComponent<GameSweet>();//��ȡ�ű�
        sweets[x, y].Init(x, y, this, type);//���ó�ʼֵ
        return sweets[x, y];
    }
    //ȫ������Я�̷���
    public IEnumerator AllFill()
    {
        bool needRefill = true;
        while (needRefill)
        {
            yield return new WaitForSeconds(0.25f);
            while (Fill())
            {
                yield return new WaitForSeconds(0.25f);//�ȴ�0.25����ܼ���ִ��
            }

            //��������Ѿ�ƥ��õ���Ʒ
            needRefill = ClearAllMatchSweets();
        }
        
    }
    //�֚i���
    public bool Fill() 
    {
        bool filledNotFinished = false; //�жϱ�������Ƿ����

        for (int y = yRow - 2; y >= 0; y--)
        {
            for (int x = 0; x < xColumn; x++)
            {
                GameSweet sweet = sweets[x, y];//��õ�ǰԪ��λ�õ���Ʒ����
                
                if (sweet.CanMove())//��������ƶ������������
                {
                    GameSweet sweetBelow = sweets[x, y + 1];//��ȡ��ǰԪ���·���Ԫ�ص���Ʒ����
                    if (sweetBelow.Type == SweetsType.EMPTY)//�·�Ԫ��Ϊ��,��ֱ�ƶ�
                    {
                        Destroy(sweetBelow.gameObject);
                        sweet.MovedComponent.Move(x, y + 1,0.25f);//����ǰԪ��������
                        sweets[x, y + 1] = sweet;//����ǰԪ�ظ����·�Ԫ��
                        CreateNewSweet(x, y, SweetsType.EMPTY);//�ڵ�ǰԪ��λ������һ��������
                        filledNotFinished = true;
                    }
                    else//б�����
                    {
                        for (int down = -1; down <= 1; down++)
                        {
                            if (down != 0)
                            {
                                int downX = x + down;//��ԭ�ȵ�x�������-1��ʾ��ߵ����壬0Ϊ�Լ���1Ϊ�ұߵ�����
                                if (downX >= 0 && downX < xColumn)
                                {
                                    GameSweet downSweet = sweets[downX, y + 1];
                                    if (downSweet.Type == SweetsType.EMPTY)
                                    {
                                        bool canFill = true;//�����ж��Ƿ����ʹ�ô�ֱ���������

                                        for (int aboveY = y;aboveY >= 0; aboveY--)
                                        {
                                            GameSweet AboveSweet = sweets[downX, aboveY];
                                            if (AboveSweet.CanMove())
                                            {
                                                break;
                                            }
                                            else if(!AboveSweet.CanMove() && AboveSweet.Type != SweetsType.EMPTY)//SweetsType.EMPTY���͵�����Ҳû��MovedSweet�ű�
                                            {
                                                canFill = false;
                                                break;
                                            }
                                        }
                                        if (!canFill)
                                        {
                                            Destroy(downSweet.gameObject);
                                            sweet.MovedComponent.Move(downX, y + 1, 0.25f);
                                            sweets[downX, y + 1] = sweet;
                                            CreateNewSweet(x, y, SweetsType.EMPTY);
                                            filledNotFinished = true;
                                            break;
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
                
            }
        }
        //���Ϸ����������
        for (int x = 0; x < xColumn; x++)
        {
            GameSweet sweet = sweets[x, 0];//��ȡ��һ�е���Ʒ����
            if (sweet.Type == SweetsType.EMPTY)//�����ǰ��Ʒ����Ϊ��
            {
                //���ڵ�ǰ�����Ϸ�����һ���µ���Ʒ����
                GameObject newSweet = Instantiate(sweetPrefabDict[SweetsType.NORMAL], CorrectPosition(x, -1), Quaternion.identity);
                newSweet.transform.parent = transform;

                //���µ���Ʒ��������ȡ����ǰ�յ���Ʒ����
                sweets[x, 0] = newSweet.GetComponent<GameSweet>();
                sweets[x, 0].Init(x, -1, this, SweetsType.NORMAL);
                sweets[x, 0].MovedComponent.Move(x, 0, 0.25f);
                sweets[x, 0].ColorComponent.SetColor((ColorSweet.ColorType)Random.Range(0, sweets[x, 0].ColorComponent.NumColors));

                filledNotFinished = true;
            }
        }

        return filledNotFinished;
    }

    //�ж�������Ʒ�Ƿ����ڵķ���
    private bool IsFriend(GameSweet sweet1,GameSweet sweet2)
    {//x����ͬ���ж�y�����Ƿ�Ϊ1��y����ͬ���ж�x�����Ƿ�Ϊ1
        return (sweet1.X == sweet2.X && Mathf.Abs(sweet1.Y - sweet2.Y) == 1)
                   || (sweet1.Y == sweet2.Y && Mathf.Abs(sweet1.X - sweet2.X) == 1);
    }
    //����������Ʒ�ķ���
    private void ExchangeSweet(GameSweet sweet1, GameSweet sweet2)
    {
        if (sweet1.CanMove() && sweet2.CanMove())
        {
            sweets[sweet1.X, sweet1.Y] = sweet2;
            sweets[sweet2.X, sweet2.Y] = sweet1;
            if (MatchSweets(sweet1, sweet2.X, sweet2.Y) != null || MatchSweets(sweet2, sweet1.X, sweet1.Y) != null || sweet1.Type == SweetsType.RAINBOWCANDY|| sweet2.Type == SweetsType.RAINBOWCANDY)
            {

                int tempX = sweet1.X;
                int tempY = sweet1.Y;
                sweet1.MovedComponent.Move(sweet2.X, sweet2.Y, 0.2f);
                sweet2.MovedComponent.Move(tempX, tempY, 0.2f);

                if(sweet1.Type == SweetsType.RAINBOWCANDY && sweet1.CanClear() && sweet2.CanClear())
                {
                    ClearColorSweet clearColor = sweet1.GetComponent<ClearColorSweet>();
                    if(clearColor != null)
                    {
                        clearColor.ClearColor = sweet2.ColorComponent.Color;
                    }
                    ClearedSweet(sweet1.X, sweet1.Y);
                }else if (sweet2.Type == SweetsType.RAINBOWCANDY && sweet1.CanClear() && sweet2.CanClear())
                {
                    ClearColorSweet clearColor = sweet2.GetComponent<ClearColorSweet>();
                    if (clearColor != null)
                    {
                        clearColor.ClearColor = sweet1.ColorComponent.Color;
                    }
                    ClearedSweet(sweet2.X, sweet2.Y);
                }

                ClearAllMatchSweets();//���
                StartCoroutine(AllFill());//���
            }
            else
            {
                sweets[sweet1.X, sweet1.Y] = sweet1;
                sweets[sweet2.X, sweet2.Y] = sweet2;
            }
        }
    }
    // ��Ʒ����������¼�������
    #region
    public void PreeSweet(GameSweet sweet)
    {
        if (gameOver)
        {
            return;
        }
        pressedSweet = sweet;
    }
    public void EnterSweet(GameSweet sweet)
    {
        if (gameOver)
        {
            return;
        }
        enteredSweet = sweet;
    }
    public void ReleaseSweet()
    {
        if (gameOver)
        {
            return;
        }
        if (IsFriend(pressedSweet, enteredSweet))
        {
            ExchangeSweet(pressedSweet, enteredSweet);
        }
    }
    #endregion

    //ʵ������������Ʒ��λ�õķ���
    public Vector3 CorrectPosition(int x,int y)
    {
        /*ʵ���������λ��
        x�� = ������λ�� - ���巽���� / 2 + ���������Ӧ��x����
        y�� = ������λ�� + ���巽���� / 2 - ���������Ӧ��x����*/
        return new Vector3(transform.position.x - xColumn / 2f + x, transform.position.y + yRow / 2f - y, 0);
    }

    //ƥ���㷨����
    public List<GameSweet> MatchSweets(GameSweet sweet,int newX,int newY)
    {
        //ƥ��ǰ�����Ҽ�����Ʒ�Ƿ�Ϊͬһ��
        if (sweet.CanColor())
        {
            ColorSweet.ColorType  color = sweet.ColorComponent.Color;//��ȡ����Ʒ��ɫ
            List<GameSweet> matchRowSweets = new List<GameSweet>();
            List<GameSweet> matchLineSweets = new List<GameSweet>();
            List<GameSweet> finishedMatchingSweets = new List<GameSweet>();

            //��ƥ��
            matchRowSweets.Add(sweet);
            for(int i = 0; i <= 1; i++)
            {
                for(int xDistance = 1;xDistance < xColumn; xDistance++)
                {
                    int x;
                    //i = 0 ����i = 1 ����
                    if (i == 0)
                    {
                        x = newX - xDistance;
                    }
                    else
                    {
                        x = newX + xDistance;
                    }
                    if(x < 0 || x >= xColumn)//�����涨��Χ����ƥ��
                    {
                        break;
                    }
                    if(sweets[x,newY].CanColor() && sweets[x,newY].ColorComponent.Color == color)
                    { //ƥ��������ɫ�Ƿ���ͬ��
                        matchRowSweets.Add(sweets[x, newY]);//��ɫ��ͬ��ӽ�����������
                    }
                    else
                    {
                        break;
                    }
                }
            }
            //L Tƥ��
            if (matchRowSweets.Count >= 3)
            {
                for(int i = 0;i < matchRowSweets.Count; i++)
                {
                    finishedMatchingSweets.Add(matchRowSweets[i]);

                    for(int j = 0;j <= 1;j++)
                    {
                        for (int yDistance = 1; yDistance < yRow; yDistance++)
                        {
                            int y;
                            //i = 0 ���ϣ�i = 1 ����
                            if (j == 0)
                            {
                                y = newY - yDistance;
                            }
                            else
                            {
                                y = newY + yDistance;
                            }
                            if (y < 0 || y >= yRow)//�����涨��Χ����ƥ��
                            {
                                break;
                            }

                            if (sweets[matchRowSweets[i].X, y].CanColor() && sweets[matchRowSweets[i].X, y].ColorComponent.Color == color)
                            { //ƥ��������ɫ�Ƿ���ͬ��
                                matchLineSweets.Add(sweets[matchRowSweets[i].X, y]);//��ɫ��ͬ��ӽ�����������
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    
                    if (matchLineSweets.Count < 2)
                    {
                        matchLineSweets.Clear();
                    }
                    else
                    {
                        for(int j = 0;j < matchLineSweets.Count; j++)
                        {
                            finishedMatchingSweets.Add(matchLineSweets[j]);
                        }
                    }
                }
            }
            
            if(finishedMatchingSweets.Count >= 3)
            {
                return finishedMatchingSweets;
            }


            matchRowSweets.Clear();
            matchLineSweets.Clear();
            finishedMatchingSweets.Clear();

            //��ƥ��
            matchLineSweets.Add(sweet);
            for (int i = 0; i <= 1; i++)
            {
                for (int yDistance = 1; yDistance < yRow; yDistance++)
                {
                    int y;
                    //i = 0 ���ϣ�i = 1 ����
                    if (i == 0)
                    {
                        y = newY - yDistance;
                    }
                    else
                    {
                        y = newY + yDistance;
                    }

                    if (y < 0 || y >= yRow)//�����涨��Χ����ƥ��
                    {
                        break;
                    }

                    if (sweets[newX, y].CanColor() && sweets[newX, y].ColorComponent.Color == color)
                    { //ƥ��������ɫ�Ƿ���ͬ��
                        matchLineSweets.Add(sweets[newX, y]);//��ɫ��ͬ��ӽ�����������
                    }
                    else
                    {
                        break;
                    }
                }
            }
            //L Tƥ��
            if (matchLineSweets.Count >= 3)
            {
                for (int i = 0; i < matchLineSweets.Count; i++)
                {
                    finishedMatchingSweets.Add(matchLineSweets[i]);
                    for (int j = 0; j <= 1; j++)
                    {
                        for (int xDistance = 1; xDistance < xColumn; xDistance++)
                        {
                            int x;
                            //i = 0 ���ϣ�i = 1 ����
                            if (j == 0)
                            {
                                x = newX - xDistance;
                            }
                            else
                            {
                                x = newX + xDistance;
                            }
                            
                            if (x < 0 || x >= xColumn)//�����涨��Χ����ƥ��
                            {
                                break;
                            }

                            if (sweets[x, matchLineSweets[i].Y].CanColor() && sweets[x, matchLineSweets[i].Y].ColorComponent.Color == color)
                            { //ƥ��������ɫ�Ƿ���ͬ��
                                matchRowSweets.Add(sweets[x, matchLineSweets[i].Y]);//��ɫ��ͬ��ӽ�����������
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    if (matchRowSweets.Count < 2)
                    {
                        matchRowSweets.Clear();
                    }
                    else
                    {
                        for (int j = 0; j < matchRowSweets.Count; j++)
                        {
                            finishedMatchingSweets.Add(matchRowSweets[j]);
                        }
                    }
                }
            }
            
            if (finishedMatchingSweets.Count >= 3)
            {
                return finishedMatchingSweets;
            }
        }
        return null;
    }
    //���ƥ��
    #region
    //�������
    public bool ClearedSweet(int x,int y)
    {
        if(sweets[x,y].CanClear() && !sweets[x, y].ClearedComponent.IsClearing)
        {
            sweets[x, y].ClearedComponent.Clear();
            CreateNewSweet(x, y, SweetsType.EMPTY);
            ClearBarrier(x, y);
            return true;
        }
        return false;
    }

    //������ɵķ���
    private void ClearBarrier(int x,int y)
    {
        for(int friendX = x - 1;friendX <= x + 1; friendX++)
        {
            if(friendX != x && friendX >= 0 && friendX < xColumn)
            {
                if(sweets[friendX,y].Type == SweetsType.BARRRIER && sweets[friendX, y].CanClear())
                {
                    sweets[friendX, y].ClearedComponent.Clear();
                    CreateNewSweet(friendX, y, SweetsType.EMPTY);
                }
            }
        }
        for (int friendY = y - 1; friendY <= y + 1; friendY++)
        {
            if (friendY != y && friendY >= 0 && friendY < yRow)
            {
                if (sweets[x, friendY].Type == SweetsType.BARRRIER && sweets[x, friendY].CanClear())
                {
                    sweets[x, friendY].ClearedComponent.Clear();
                    CreateNewSweet(x, friendY, SweetsType.EMPTY);
                }
            }
        }
    }
    
    //���ƥ�����Ʒ
    private bool ClearAllMatchSweets()
    {
        bool needRefill = false;
        for(int y = 0;y < yRow; y++)
        {
            for (int x = 0; x < xColumn; x++)
            {
                if (sweets[x, y].CanClear())
                {
                    //�ü��ϴ�����Ѿ�ƥ��ÿ���������GameSweet�ű�
                    List<GameSweet> matchList = MatchSweets(sweets[x, y], x, y);

                    if(matchList != null)
                    {
                        SweetsType specialSweetsType = SweetsType.COUNT;
                        GameSweet randomSweet = matchList[Random.Range(0,matchList.Count)];
                        int specialSweetX = randomSweet.X;
                        int specialSweetY = randomSweet.Y;
                        // ͬʱ����4��������һ��������һ���л��е���Ʒ
                        if(matchList.Count == 4)
                        {
                            //����Ҫ���ɵ���Ʒ����
                            specialSweetsType = (SweetsType)Random.Range((int)SweetsType.ROW_CLEAR, (int)SweetsType.COLUMN_CLEAR+1);
                        }
                        //ͬʱ����5�����Ͼ����ɲʺ���
                        else if (matchList.Count >= 5)
                        {
                            //����Ҫ���ɵ���Ʒ����
                            specialSweetsType = SweetsType.RAINBOWCANDY;
                        }

                        //���
                        for (int i = 0;i < matchList.Count; i++)
                        {
                            if (ClearedSweet(matchList[i].X, matchList[i].Y))
                            {
                                needRefill = true;
                            }
                        }

                        if(specialSweetsType != SweetsType.COUNT)
                        {
                            Destroy(sweets[specialSweetX, specialSweetY]);
                            GameSweet newSweet = CreateNewSweet(specialSweetX, specialSweetY, specialSweetsType);
                            if(specialSweetsType == SweetsType.ROW_CLEAR || specialSweetsType == SweetsType.COLUMN_CLEAR && newSweet.CanColor()&&matchList[0])
                            {
                                newSweet.ColorComponent.SetColor(matchList[0].ColorComponent.Color);
                            }else if(specialSweetsType == SweetsType.RAINBOWCANDY && newSweet.CanColor())
                            {
                                newSweet.ColorComponent.SetColor(ColorSweet.ColorType.ANY);
                            }
                        }
                    }
                }
            }
        }

        return needRefill;
    }
    #endregion

    //�ص����˵���ť�¼�
    public void ReturnToMain()
    {
        SceneManager.LoadScene(0);
    }
    //���¼�����Ϸ������ť�¼�
    public void RePlay()
    {
        SceneManager.LoadScene(1);
    }

    //������������
    #region
    //���һ���еķ���
    public void ClearRow(int row)
    {
        for(int x = 0;x < xColumn; x++)
        {
            ClearedSweet(x, row);
        }
    }
    //���һ���еķ���
    public void ClearColumn(int column)
    {
        for (int y = 0; y < yRow; y++)
        {
            ClearedSweet(column, y);
        }
    }
    //ͬɫ����ķ���
    public void ClearColor(ColorSweet.ColorType color)
    {
        for(int x = 0;x < xColumn; x++)
        {
            for(int y = 0;y < yRow; y++)
            {
                if(sweets[x,y].CanColor() && (sweets[x,y].ColorComponent.Color == color || color == ColorSweet.ColorType.ANY))
                {
                    ClearedSweet(x, y);

                }
            }
        }
    }
    #endregion
}
