using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    //变量
    #region

    //UI
    public Text timeText; //时间显示文本
    private float gameTime = 60f;//初始游戏时间
    private bool gameOver;//游戏结束
    private float addScoreTime;
    private float currentScore;
    public int playerScore;//游戏得分
    public Text playerScoreText;//游戏得分文本
    public Text finalScoreText;//最终得分文本
    public GameObject gameOverPanel;//游戏结束页面画板
    

    //行列数
    public int xColumn;
    public int yRow;
    public GameObject gritPrefab;//方格预制体

    //甜品脚本数组
    private GameSweet[,] sweets;

    //要进行交换的两个甜品
    private GameSweet pressedSweet;
    private GameSweet enteredSweet;

    //创建结构体数组
    public SweetPrefab[] sweetPrefabs;

    //定义一个甜品预制体的字典，通过种类得到对应的甜品游戏物体
    public Dictionary<SweetsType, GameObject> sweetPrefabDict;//键为种类枚举，值为游戏对象

    //单例
    private static GameManager _instance;
    public static GameManager Instance { get => _instance; set => _instance = value; }
    //用枚举类型定义 种类
    public enum SweetsType
    {
        EMPTY,
        NORMAL,
        BARRRIER,
        ROW_CLEAR,
        COLUMN_CLEAR,
        RAINBOWCANDY,
        COUNT//标记类型
    }

    //创建一个结构体
    [System.Serializable]//可序列化
    public struct SweetPrefab
    {
        public SweetsType type;
        public GameObject prefab;
    }
    #endregion

    private void Awake()
    {
        //实例化单例对象
        _instance = this;
    }

    private void Start()
    {
        //字典实例化
        sweetPrefabDict = new Dictionary<SweetsType, GameObject>();
        //为字典赋值
        for (int i = 0; i < sweetPrefabs.Length; i++)
        {
            //判断字典的键中是否存在当前结构体数组中的type变量
            if (!sweetPrefabDict.ContainsKey(sweetPrefabs[i].type))
            {
                //如果没有则将当前结构体赋予字典，type变量为键，prefab变量为值
                sweetPrefabDict.Add(sweetPrefabs[i].type, sweetPrefabs[i].prefab);
            }
        }   
        //实例化方格
        for(int x = 0; x < xColumn; x++)
        {
            for(int y = 0; y < yRow; y++)
            {
                GameObject chocolate = Instantiate(gritPrefab,CorrectPosition(x,y),Quaternion.identity);
                chocolate.transform.SetParent(transform);
            }
        }
        //实例化甜品数组
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

        StartCoroutine(AllFill());//开启携程
    }

    private void Update()
    {
        gameTime -= Time.deltaTime;//刷新时间
        if(gameTime <= 0)//时间为0，游戏结束
        {
            gameTime = 0;
            
            gameOver = true;
            finalScoreText.text = playerScore.ToString("0");
            gameOverPanel.SetActive(true);
        }
        timeText.text = gameTime.ToString("0");//取整,时间显示

        //分数显示
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

    //产生甜品的方法
    public GameSweet CreateNewSweet(int x,int y,SweetsType type)
    {
        GameObject newSweet = Instantiate(sweetPrefabDict[type], CorrectPosition(x, y), Quaternion.identity);
        newSweet.transform.SetParent(transform);
        sweets[x, y] = newSweet.GetComponent<GameSweet>();//获取脚本
        sweets[x, y].Init(x, y, this, type);//设置初始值
        return sweets[x, y];
    }
    //全部填充的携程方法
    public IEnumerator AllFill()
    {
        bool needRefill = true;
        while (needRefill)
        {
            yield return new WaitForSeconds(0.25f);
            while (Fill())
            {
                yield return new WaitForSeconds(0.25f);//等待0.25秒才能继续执行
            }

            //清除所有已经匹配好的甜品
            needRefill = ClearAllMatchSweets();
        }
        
    }
    //分歩填充
    public bool Fill() 
    {
        bool filledNotFinished = false; //判断本次填充是否完成

        for (int y = yRow - 2; y >= 0; y--)
        {
            for (int x = 0; x < xColumn; x++)
            {
                GameSweet sweet = sweets[x, y];//获得当前元素位置的甜品对象
                
                if (sweet.CanMove())//如果可以移动，则向下填充
                {
                    GameSweet sweetBelow = sweets[x, y + 1];//获取当前元素下方的元素的甜品对象
                    if (sweetBelow.Type == SweetsType.EMPTY)//下方元素为空,垂直移动
                    {
                        Destroy(sweetBelow.gameObject);
                        sweet.MovedComponent.Move(x, y + 1,0.25f);//将当前元素往下移
                        sweets[x, y + 1] = sweet;//将当前元素赋予下方元素
                        CreateNewSweet(x, y, SweetsType.EMPTY);//在当前元素位置生成一个空物体
                        filledNotFinished = true;
                    }
                    else//斜向填充
                    {
                        for (int down = -1; down <= 1; down++)
                        {
                            if (down != 0)
                            {
                                int downX = x + down;//让原先的x坐标加上-1表示左边的物体，0为自己，1为右边的物体
                                if (downX >= 0 && downX < xColumn)
                                {
                                    GameSweet downSweet = sweets[downX, y + 1];
                                    if (downSweet.Type == SweetsType.EMPTY)
                                    {
                                        bool canFill = true;//用来判断是否可以使用垂直填充进行填充

                                        for (int aboveY = y;aboveY >= 0; aboveY--)
                                        {
                                            GameSweet AboveSweet = sweets[downX, aboveY];
                                            if (AboveSweet.CanMove())
                                            {
                                                break;
                                            }
                                            else if(!AboveSweet.CanMove() && AboveSweet.Type != SweetsType.EMPTY)//SweetsType.EMPTY类型的物体也没有MovedSweet脚本
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
        //最上方的特殊情况
        for (int x = 0; x < xColumn; x++)
        {
            GameSweet sweet = sweets[x, 0];//获取第一行的甜品对象
            if (sweet.Type == SweetsType.EMPTY)//如果当前甜品对象为空
            {
                //就在当前对象上方创建一个新的甜品对象
                GameObject newSweet = Instantiate(sweetPrefabDict[SweetsType.NORMAL], CorrectPosition(x, -1), Quaternion.identity);
                newSweet.transform.parent = transform;

                //让新的甜品对象下落取代当前空的甜品对象
                sweets[x, 0] = newSweet.GetComponent<GameSweet>();
                sweets[x, 0].Init(x, -1, this, SweetsType.NORMAL);
                sweets[x, 0].MovedComponent.Move(x, 0, 0.25f);
                sweets[x, 0].ColorComponent.SetColor((ColorSweet.ColorType)Random.Range(0, sweets[x, 0].ColorComponent.NumColors));

                filledNotFinished = true;
            }
        }

        return filledNotFinished;
    }

    //判断两个甜品是否相邻的方法
    private bool IsFriend(GameSweet sweet1,GameSweet sweet2)
    {//x轴相同则判断y轴间隔是否为1；y轴相同则判断x轴间隔是否为1
        return (sweet1.X == sweet2.X && Mathf.Abs(sweet1.Y - sweet2.Y) == 1)
                   || (sweet1.Y == sweet2.Y && Mathf.Abs(sweet1.X - sweet2.X) == 1);
    }
    //交换两个甜品的方法
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

                ClearAllMatchSweets();//清除
                StartCoroutine(AllFill());//填充
            }
            else
            {
                sweets[sweet1.X, sweet1.Y] = sweet1;
                sweets[sweet2.X, sweet2.Y] = sweet2;
            }
        }
    }
    // 甜品操作的鼠标事件处理方法
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

    //实例化方格与甜品的位置的方法
    public Vector3 CorrectPosition(int x,int y)
    {
        /*实例化方格的位置
        x轴 = 父物体位置 - 总体方格数 / 2 + 方格自身对应的x坐标
        y轴 = 父物体位置 + 总体方格数 / 2 - 方格自身对应的x坐标*/
        return new Vector3(transform.position.x - xColumn / 2f + x, transform.position.y + yRow / 2f - y, 0);
    }

    //匹配算法方法
    public List<GameSweet> MatchSweets(GameSweet sweet,int newX,int newY)
    {
        //匹配前后左右几个甜品是否为同一个
        if (sweet.CanColor())
        {
            ColorSweet.ColorType  color = sweet.ColorComponent.Color;//获取本甜品颜色
            List<GameSweet> matchRowSweets = new List<GameSweet>();
            List<GameSweet> matchLineSweets = new List<GameSweet>();
            List<GameSweet> finishedMatchingSweets = new List<GameSweet>();

            //行匹配
            matchRowSweets.Add(sweet);
            for(int i = 0; i <= 1; i++)
            {
                for(int xDistance = 1;xDistance < xColumn; xDistance++)
                {
                    int x;
                    //i = 0 往左，i = 1 往右
                    if (i == 0)
                    {
                        x = newX - xDistance;
                    }
                    else
                    {
                        x = newX + xDistance;
                    }
                    if(x < 0 || x >= xColumn)//超出规定范围结束匹配
                    {
                        break;
                    }
                    if(sweets[x,newY].CanColor() && sweets[x,newY].ColorComponent.Color == color)
                    { //匹配两个颜色是否相同，
                        matchRowSweets.Add(sweets[x, newY]);//颜色相同添加进入行数组中
                    }
                    else
                    {
                        break;
                    }
                }
            }
            //L T匹配
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
                            //i = 0 往上，i = 1 往下
                            if (j == 0)
                            {
                                y = newY - yDistance;
                            }
                            else
                            {
                                y = newY + yDistance;
                            }
                            if (y < 0 || y >= yRow)//超出规定范围结束匹配
                            {
                                break;
                            }

                            if (sweets[matchRowSweets[i].X, y].CanColor() && sweets[matchRowSweets[i].X, y].ColorComponent.Color == color)
                            { //匹配两个颜色是否相同，
                                matchLineSweets.Add(sweets[matchRowSweets[i].X, y]);//颜色相同添加进入行数组中
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

            //列匹配
            matchLineSweets.Add(sweet);
            for (int i = 0; i <= 1; i++)
            {
                for (int yDistance = 1; yDistance < yRow; yDistance++)
                {
                    int y;
                    //i = 0 往上，i = 1 往下
                    if (i == 0)
                    {
                        y = newY - yDistance;
                    }
                    else
                    {
                        y = newY + yDistance;
                    }

                    if (y < 0 || y >= yRow)//超出规定范围结束匹配
                    {
                        break;
                    }

                    if (sweets[newX, y].CanColor() && sweets[newX, y].ColorComponent.Color == color)
                    { //匹配两个颜色是否相同，
                        matchLineSweets.Add(sweets[newX, y]);//颜色相同添加进入行数组中
                    }
                    else
                    {
                        break;
                    }
                }
            }
            //L T匹配
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
                            //i = 0 往上，i = 1 往下
                            if (j == 0)
                            {
                                x = newX - xDistance;
                            }
                            else
                            {
                                x = newX + xDistance;
                            }
                            
                            if (x < 0 || x >= xColumn)//超出规定范围结束匹配
                            {
                                break;
                            }

                            if (sweets[x, matchLineSweets[i].Y].CanColor() && sweets[x, matchLineSweets[i].Y].ColorComponent.Color == color)
                            { //匹配两个颜色是否相同，
                                matchRowSweets.Add(sweets[x, matchLineSweets[i].Y]);//颜色相同添加进入行数组中
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
    //清除匹配
    #region
    //清除方法
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

    //清除饼干的方法
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
    
    //清除匹配的甜品
    private bool ClearAllMatchSweets()
    {
        bool needRefill = false;
        for(int y = 0;y < yRow; y++)
        {
            for (int x = 0; x < xColumn; x++)
            {
                if (sweets[x, y].CanClear())
                {
                    //该集合存放了已经匹配好可以消除的GameSweet脚本
                    List<GameSweet> matchList = MatchSweets(sweets[x, y], x, y);

                    if(matchList != null)
                    {
                        SweetsType specialSweetsType = SweetsType.COUNT;
                        GameSweet randomSweet = matchList[Random.Range(0,matchList.Count)];
                        int specialSweetX = randomSweet.X;
                        int specialSweetY = randomSweet.Y;
                        // 同时消除4个就生成一个能消除一整行或列的甜品
                        if(matchList.Count == 4)
                        {
                            //设置要生成的甜品类型
                            specialSweetsType = (SweetsType)Random.Range((int)SweetsType.ROW_CLEAR, (int)SweetsType.COLUMN_CLEAR+1);
                        }
                        //同时消除5个以上就生成彩虹糖
                        else if (matchList.Count >= 5)
                        {
                            //设置要生成的甜品类型
                            specialSweetsType = SweetsType.RAINBOWCANDY;
                        }

                        //清除
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

    //回到主菜单按钮事件
    public void ReturnToMain()
    {
        SceneManager.LoadScene(0);
    }
    //重新加载游戏场景按钮事件
    public void RePlay()
    {
        SceneManager.LoadScene(1);
    }

    //特殊消除方法
    #region
    //清除一整行的方法
    public void ClearRow(int row)
    {
        for(int x = 0;x < xColumn; x++)
        {
            ClearedSweet(x, row);
        }
    }
    //清除一整列的方法
    public void ClearColumn(int column)
    {
        for (int y = 0; y < yRow; y++)
        {
            ClearedSweet(column, y);
        }
    }
    //同色清除的方法
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
