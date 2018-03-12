using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    public enum SweetsType
    {
        EMPTY,
        NORMAL,
        BARRIER,
        ROW_CLEAR,
        COLUMN_CLEAR,
        RAINBOWCANDY,
        COUNT
    };

    [System.Serializable]
    public struct SweetsPrefab
    {
        public SweetsType type;
        public GameObject prefab;
    }

    public SweetsPrefab[] sweetPrefabs;

    public Dictionary<SweetsType, GameObject> sweetPrefabDict;

    public int xColumn;
    public int yRow;

    public float fillTime;

    public GameObject gridPrefab;

    public GameSweet[,] sweets;

    private GameSweet pressedSweet;
    private GameSweet enteredSweet;

    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            return _instance;
        }

        set
        {
            _instance = value;
        }
    }

    public Text timeText;

    private float gameTime = 60;

    private bool isGameOver;

    public int playerScore;

    public Text scoreText;

    private float addScoreTime;

    private int currentScore;

    public GameObject gameOverPanel;

    public Text finalScoreText;
    private void Awake()
    {
        _instance = this;
    }

    // Use this for initialization
    void Start () {

        sweetPrefabDict = new Dictionary<SweetsType, GameObject>();

        for (int i = 0;i < sweetPrefabs.Length;i++)
        {
            if (!sweetPrefabDict.ContainsKey(sweetPrefabs[i].type))
            {
                sweetPrefabDict.Add(sweetPrefabs[i].type, sweetPrefabs[i].prefab);
            }
        }

        for (int i = 0;i < xColumn;i++)
        {
            for (int j = 0; j < yRow; j++)
            {
                GameObject chocolate = Instantiate(gridPrefab, CorrectPosition(i, j), Quaternion.identity);
                chocolate.transform.SetParent(transform);
            }
        }

        sweets = new GameSweet[xColumn, yRow];
        for (int i = 0; i < xColumn; i++)
        {
            for (int j = 0; j < yRow; j++)
            {
                CreateNewSweet(i, j, SweetsType.EMPTY);
            }
        }

        Destroy(sweets[3, 4].gameObject);
        CreateNewSweet(3, 4, SweetsType.BARRIER);
        Destroy(sweets[4, 4].gameObject);
        CreateNewSweet(4, 4, SweetsType.BARRIER);
        Destroy(sweets[5, 4].gameObject);
        CreateNewSweet(5, 4, SweetsType.BARRIER);

        StartCoroutine(AllFill());
    }
	
	// Update is called once per frame
	void Update () {

        if (isGameOver)
        {
            return;
        }
      
        gameTime -= Time.deltaTime;
        if (gameTime <= 0)
        {
            gameTime = 0;
            isGameOver = true;
            gameOverPanel.SetActive(true);
            finalScoreText.text = playerScore.ToString();
            scoreText.text = playerScore.ToString();
            return;
        }

        if (addScoreTime <= 0.1f)
        {
            addScoreTime += Time.deltaTime;
        }
        else
        {
            if (currentScore < playerScore)
            {
                currentScore += 5;
                if (currentScore > playerScore)
                {
                    currentScore = playerScore;
                }
                scoreText.text = currentScore.ToString();
            }
        }

        timeText.text = gameTime.ToString("0");
	}

    public Vector3 CorrectPosition(int x, int y)
    {
        return new Vector3(transform.position.x - xColumn / 2 + x, transform.position.y + yRow / 2 - y, 0);
    }

    public GameSweet CreateNewSweet(int x, int y, SweetsType type)
    {
        GameObject obj = Instantiate(sweetPrefabDict[type], CorrectPosition(x, y), Quaternion.identity);
        obj.transform.SetParent(transform);

        sweets[x, y] = obj.GetComponent<GameSweet>();
        sweets[x, y].Init(x, y, this, type);

        return sweets[x, y];
    }

    public IEnumerator AllFill()
    {
        bool isNeedFill = true;
        while (isNeedFill)
        {
            yield return new WaitForSeconds(fillTime);

            while (StepFill())
            {
                yield return new WaitForSeconds(fillTime);
            }

            isNeedFill = ClearMatchedSweet();
        }
    }

    private bool StepFill()
    {
        bool fillNotFinished = false;

        for (int y = yRow - 2;y >= 0;y--)
        {
            for (int x = 0;x < xColumn;x++)
            {
                GameSweet sweet = sweets[x, y];        
                if (sweet.CanMove())
                {
                    GameSweet belowSweet = sweets[x, y + 1];
                    if (belowSweet.Type == SweetsType.EMPTY)
                    {
                        Destroy(belowSweet.gameObject);
                        sweet.MovedCommpont.Move(x, y + 1, fillTime);
                        sweets[x, y + 1] = sweet;
                        CreateNewSweet(x, y, SweetsType.EMPTY);
                        fillNotFinished = true;
                    }
                    else
                    {
                        for (int down = -1; down <= 1; down++)
                        {
                            if (down != 0)
                            {
                                int downX = x + down;

                                if (downX >= 0 && downX < xColumn)
                                {
                                    GameSweet downSweet = sweets[downX, y + 1];

                                    if (downSweet.Type == SweetsType.EMPTY)
                                    {
                                        bool canfill = true;//用来判断垂直填充是否可以满足填充要求

                                        for (int aboveY = y; aboveY >= 0; aboveY--)
                                        {
                                            GameSweet sweetAbove = sweets[downX, aboveY];
                                            if (sweetAbove.CanMove())
                                            {
                                                break;
                                            }
                                            else if (!sweetAbove.CanMove() && sweetAbove.Type != SweetsType.EMPTY)
                                            {
                                                canfill = false;
                                                break;
                                            }
                                        }

                                        if (!canfill)
                                        {
                                            Destroy(downSweet.gameObject);
                                            sweet.MovedCommpont.Move(downX, y + 1, fillTime);
                                            sweets[downX, y + 1] = sweet;
                                            CreateNewSweet(x, y, SweetsType.EMPTY);
                                            fillNotFinished = true;
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

        for (int x = 0; x < xColumn; x++)
        {
            GameSweet sweet = sweets[x, 0];
            if (sweet.Type == SweetsType.EMPTY)
            {
                GameObject obj = Instantiate(sweetPrefabDict[SweetsType.NORMAL], CorrectPosition(x, -1), Quaternion.identity);
                obj.transform.SetParent(transform);

                Destroy(sweets[x, 0].gameObject);
                sweets[x, 0] = obj.GetComponent<GameSweet>();
                sweets[x, 0].Init(x, -1, this, SweetsType.NORMAL);
                sweets[x, 0].MovedCommpont.Move(x, 0, fillTime);
                sweets[x, 0].ColorCommpont.SetColor((ColorSweet.ColorType)Random.Range(0, sweets[x, 0].ColorCommpont.NumColors));
                fillNotFinished = true;
            }
        }

        return fillNotFinished;
    }

    private bool IsFriend(GameSweet sweet1, GameSweet sweet2)
    {
        return (sweet1.X == sweet2.X && Mathf.Abs(sweet1.Y - sweet2.Y) == 1) ||
            (sweet1.Y == sweet2.Y && Mathf.Abs(sweet1.X - sweet2.X) == 1);
    }

    private void ExchangeSweet(GameSweet sweet1, GameSweet sweet2)
    {
        if (sweet1.CanMove()&&sweet2.CanMove())
        {
            sweets[sweet1.X, sweet1.Y] = sweet2;
            sweets[sweet2.X, sweet2.Y] = sweet1;       

            if (MatchSweets(sweet1, sweet2.X, sweet2.Y) != null ||
                MatchSweets(sweet2, sweet1.X, sweet1.Y) != null || 
                sweet1.Type == SweetsType.RAINBOWCANDY || sweet2.Type == SweetsType.RAINBOWCANDY)
            {
                int tempX = sweet1.X;
                int tempY = sweet1.Y;
                sweet1.MovedCommpont.Move(sweet2.X, sweet2.Y, fillTime);
                sweet2.MovedCommpont.Move(tempX, tempY, fillTime);

                if (sweet1.Type == SweetsType.RAINBOWCANDY && sweet2.CanClear())
                {
                    ClearColorSweet clearColor = sweet1.GetComponent<ClearColorSweet>();
                    if (clearColor != null)
                    {
                        clearColor.ClearColor = sweet2.ColorCommpont.Color;
                    }

                    ClearSweet(sweet1.X, sweet1.Y);
                }

                if (sweet2.Type == SweetsType.RAINBOWCANDY && sweet1.CanClear())
                {
                    ClearColorSweet clearColor = sweet2.GetComponent<ClearColorSweet>();
                    if (clearColor != null)
                    {
                        clearColor.ClearColor = sweet2.ColorCommpont.Color;
                    }

                    ClearSweet(sweet2.X, sweet2.Y);
                }

                ClearMatchedSweet();
                StartCoroutine(AllFill());
            }
            else
            {
                sweets[sweet1.X, sweet1.Y] = sweet1;
                sweets[sweet2.X, sweet2.Y] = sweet2;
            }        
        }
    }

    public void PressSweet(GameSweet sweet)
    {
        if (isGameOver)
        {
            return;
        }
        pressedSweet = sweet;
    }

    public void EnterSweet(GameSweet sweet)
    {
        if (isGameOver)
        {
            return;
        }
        enteredSweet = sweet;
    }

    public void ReleaseSweet()
    {
        if (isGameOver)
        {
            return;
        }
        if (IsFriend(pressedSweet, enteredSweet))
        {
            ExchangeSweet(pressedSweet, enteredSweet);
        }
    }
    public List<GameSweet> MatchSweets(GameSweet sweet, int newX, int newY)
    {
        if (sweet.CanColor())
        {
            ColorSweet.ColorType color = sweet.ColorCommpont.Color;
            List<GameSweet> matchRowSweets = new List<GameSweet>();
            List<GameSweet> matchLineSweets = new List<GameSweet>();
            List<GameSweet> finishedMatchingSweets = new List<GameSweet>();

            //行匹配
            matchRowSweets.Add(sweet);

            //i=0代表往左，i=1代表往右
            for (int i = 0; i <= 1; i++)
            {
                for (int xDistance = 1; xDistance < xColumn; xDistance++)
                {
                    int x;
                    if (i == 0)
                    {
                        x = newX - xDistance;
                    }
                    else
                    {
                        x = newX + xDistance;
                    }
                    if (x < 0 || x >= xColumn)
                    {
                        break;
                    }

                    if (sweets[x, newY].CanColor() && sweets[x, newY].ColorCommpont.Color == color)
                    {
                        matchRowSweets.Add(sweets[x, newY]);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (matchRowSweets.Count >= 3)
            {
                for (int i = 0; i < matchRowSweets.Count; i++)
                {
                    finishedMatchingSweets.Add(matchRowSweets[i]);
                }
            }

            //L T型匹配
            //检查一下当前行遍历列表中的元素数量是否大于3
            if (matchRowSweets.Count >= 3)
            {
                for (int i = 0; i < matchRowSweets.Count; i++)
                {
                    //行匹配列表中满足匹配条件的每个元素上下依次进行列遍历
                    // 0代表上方 1代表下方
                    for (int j = 0; j <= 1; j++)
                    {
                        for (int yDistance = 1; yDistance < yRow; yDistance++)
                        {
                            int y;
                            if (j == 0)
                            {
                                y = newY - yDistance;
                            }
                            else
                            {
                                y = newY + yDistance;
                            }
                            if (y < 0 || y >= yRow)
                            {
                                break;
                            }

                            if (sweets[matchRowSweets[i].X, y].CanColor() && sweets[matchRowSweets[i].X, y].ColorCommpont.Color == color)
                            {
                                matchLineSweets.Add(sweets[matchRowSweets[i].X, y]);
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
                        for (int j = 0; j < matchLineSweets.Count; j++)
                        {
                            finishedMatchingSweets.Add(matchLineSweets[j]);
                        }
                        break;
                    }
                }
            }

            if (finishedMatchingSweets.Count >= 3)
            {
                return finishedMatchingSweets;
            }

            matchRowSweets.Clear();
            matchLineSweets.Clear();

            matchLineSweets.Add(sweet);

            //列匹配

            //i=0代表往左，i=1代表往右
            for (int i = 0; i <= 1; i++)
            {
                for (int yDistance = 1; yDistance < yRow; yDistance++)
                {
                    int y;
                    if (i == 0)
                    {
                        y = newY - yDistance;
                    }
                    else
                    {
                        y = newY + yDistance;
                    }
                    if (y < 0 || y >= yRow)
                    {
                        break;
                    }

                    if (sweets[newX, y].CanColor() && sweets[newX, y].ColorCommpont.Color == color)
                    {
                        matchLineSweets.Add(sweets[newX, y]);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (matchLineSweets.Count >= 3)
            {
                for (int i = 0; i < matchLineSweets.Count; i++)
                {
                    finishedMatchingSweets.Add(matchLineSweets[i]);
                }
            }

            //L T型匹配
            //检查一下当前行遍历列表中的元素数量是否大于3
            if (matchLineSweets.Count >= 3)
            {
                for (int i = 0; i < matchLineSweets.Count; i++)
                {
                    //行匹配列表中满足匹配条件的每个元素上下依次进行列遍历
                    // 0代表上方 1代表下方
                    for (int j = 0; j <= 1; j++)
                    {
                        for (int xDistance = 1; xDistance < xColumn; xDistance++)
                        {
                            int x;
                            if (j == 0)
                            {
                                x = newY - xDistance;
                            }
                            else
                            {
                                x = newY + xDistance;
                            }
                            if (x < 0 || x >= xColumn)
                            {
                                break;
                            }

                            if (sweets[x, matchLineSweets[i].Y].CanColor() && sweets[x, matchLineSweets[i].Y].ColorCommpont.Color == color)
                            {
                                matchRowSweets.Add(sweets[x, matchLineSweets[i].Y]);
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
                        break;
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

    public bool ClearSweet(int x, int y)
    {
        if (sweets[x, y].CanClear()&&!sweets[x, y].ClearCommpont.IsClearing)
        {
            sweets[x, y].ClearCommpont.Clear();
            CreateNewSweet(x, y, SweetsType.EMPTY);

            ClearBarrier(x, y);

            return true;
        }

        return false;
    }

    public bool ClearBarrier(int x, int y)
    {
        for (int i = 0; i < 2; i++)
        {
            int newX = (i == 0) ? (x - 1) : (x + 1);
            if (newX < 0 || newX >= xColumn)
            {
                continue;
            }

            if (sweets[newX, y].Type == SweetsType.BARRIER &&
                sweets[newX, y].CanClear() && !sweets[newX, y].ClearCommpont.IsClearing)
            {
                sweets[newX, y].ClearCommpont.Clear();
                CreateNewSweet(newX, y, SweetsType.EMPTY);
            }
        }

        for (int j = 0; j < 2; j++)
        {
            int newY = (j == 0) ? (y - 1) : (y + 1);
            if (newY < 0 || newY >= yRow)
            {
                continue;
            }

            if (sweets[x, newY].Type == SweetsType.BARRIER &&
                sweets[x, newY].CanClear() && !sweets[x, newY].ClearCommpont.IsClearing)
            {
                sweets[x, newY].ClearCommpont.Clear();
                CreateNewSweet(x, newY, SweetsType.EMPTY);
            }
        }

        return false;
    }

    private bool ClearMatchedSweet()
    {
        bool isNeedFill = false;
        for (int x = 0; x < xColumn; x++)
        {
            for (int y = 0; y < yRow; y++)
            {
                if (sweets[x, y].CanClear())
                {
                    List<GameSweet> matchList = MatchSweets(sweets[x, y], x, y);
                    if (matchList != null)
                    {
                        SweetsType specialType = SweetsType.COUNT;

                        GameSweet randomSweet = matchList[Random.Range(0, matchList.Count)];
                        int specialX = randomSweet.X;
                        int specialY = randomSweet.Y;

                        if (matchList.Count == 4)
                        {
                            specialType = (SweetsType)Random.Range((int)SweetsType.ROW_CLEAR, (int)SweetsType.COLUMN_CLEAR);
                        }

                        if (matchList.Count == 5)
                        {
                            specialType = SweetsType.RAINBOWCANDY;
                        }

                        for (int i = 0; i < matchList.Count; i++)
                        {
                            ClearSweet(matchList[i].X, matchList[i].Y);
                            isNeedFill = true;
                        }

                        if (specialType != SweetsType.COUNT)
                        {
                            Destroy(sweets[specialX, specialY]);
                            GameSweet gameSweet = CreateNewSweet(specialX, specialY, specialType);
                            if ((gameSweet.Type == SweetsType.ROW_CLEAR || gameSweet.Type == SweetsType.COLUMN_CLEAR)&&
                                gameSweet.CanColor()&&matchList[0].CanColor())
                            {
                                gameSweet.ColorCommpont.SetColor(matchList[0].ColorCommpont.Color);
                            }
                            else if (gameSweet.Type == SweetsType.RAINBOWCANDY)
                            {
                                gameSweet.ColorCommpont.SetColor(ColorSweet.ColorType.ANY);
                            }
                        }
                    }
                }
            }
        }

        return isNeedFill;
    }

    public void ReturnToMain()
    {
        SceneManager.LoadScene(0);
    }

    public void Replay()
    {
        SceneManager.LoadScene(1);
    }

    public void ClearRow(int row)
    {
        for (int x = 0; x < xColumn; x++)
        {
            ClearSweet(x, row);
        }
    }

    public void ClearCloumn(int cloumn)
    {
        for (int y = 0; y < yRow; y++)
        {
            ClearSweet(cloumn, y);
        }
    }

    public void ClearColor(ColorSweet.ColorType color)
    {
        for (int x = 0; x < xColumn; x++)
        {
            for (int y = 0; y < yRow; y++)
            {
                if (sweets[x, y].CanColor() && (sweets[x, y].ColorCommpont.Color == color ||
                    ColorSweet.ColorType.ANY == color))
                {
                    ClearSweet(x, y);
                }
            }
        }
    }

    /*public List<GameSweet> MatchSweet(GameSweet sweet, int newX, int newY)
    {
        if (sweet.CanColor())
        {
            List<GameSweet> matchRowSweets = new List<GameSweet>();
            List<GameSweet> matchLineSweets = new List<GameSweet>();
            List<GameSweet> matchFinshSweets = new List<GameSweet>();

            matchRowSweets.Add(sweet);
            for (int i = 0; i < 2; i++)
            {
                int nextX = (i == 0) ? (newX - 1) : (newX + 1);
                while (nextX >= 0&&nextX < xColumn)  
                {                   
                    GameSweet nextSweet = sweets[nextX, newY];
                    if (nextSweet.CanColor()&&nextSweet.ColorCommpont.Color == sweet.ColorCommpont.Color)
                    {
                        matchRowSweets.Add(nextSweet);
                        nextX = (i == 0) ? (nextX - 1) : (nextX + 1);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            int count = 1;
            if (matchRowSweets.Count >= 3)
            {
                count = matchRowSweets.Count;
                for (int i = 0; i < matchRowSweets.Count; i++)
                {
                    matchFinshSweets.Add(matchRowSweets[i]);
                }
            }

            for (int k = 0; k < count; k++)
            {
                GameSweet rowSweet = matchRowSweets[k];
                matchLineSweets.Add(rowSweet);
                for (int i = 0; i < 2; i++)
                {
                    int nextY = (i == 0) ? (newY - 1) : (newY + 1);
                    while (nextY >= 0 && nextY < yRow)
                    {
                        GameSweet nextSweet = sweets[rowSweet.X, nextY];
                        if (nextSweet.CanColor() && nextSweet.ColorCommpont.Color == rowSweet.ColorCommpont.Color)
                        {
                            matchLineSweets.Add(nextSweet);
                            nextY = (i == 0) ? (nextY - 1) : (nextY + 1);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (matchLineSweets.Count >= 3)
                {
                    int start = (count == 1) ? 0 : 1;
                    for (int i = start; i < matchLineSweets.Count; i++)
                    {
                        matchFinshSweets.Add(matchLineSweets[i]);
                    }                 
                    break;
                }

                matchLineSweets.Clear();
            }

            if (matchFinshSweets.Count >= 3)
            {
                return matchFinshSweets;
            }
            
            return null;
        }

        return null;
    }*/
}