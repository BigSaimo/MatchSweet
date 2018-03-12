using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSweet : MonoBehaviour {

    private int x;
    public int X
    {
        get
        {
            return x;
        }

        set
        {
            if (CanMove())
            {
                x = value;
            }
        }
    }

    private int y;
    public int Y
    {
        get
        {
            return y;
        }

        set
        {
            if (CanMove())
            {
                y = value;
            }        
        }
    }

    private GameManager.SweetsType type;
    public GameManager.SweetsType Type
    {
        get
        {
            return type;
        }
    }

    [HideInInspector]
    public GameManager gameManager;

    private MovedSweet movedCommpont;
    public MovedSweet MovedCommpont
    {
        get
        {
            return movedCommpont;
        }
    }

    private ColorSweet colorCommpont;
    public ColorSweet ColorCommpont
    {
        get
        {
            return colorCommpont;
        }
    }

    private ClearSweet clearCommpont;
    public ClearSweet ClearCommpont
    {
        get
        {
            return clearCommpont;
        }
    }

    private void Awake()
    {
        movedCommpont = GetComponent<MovedSweet>();
        colorCommpont = GetComponent<ColorSweet>();
        clearCommpont = GetComponent<ClearSweet>();
    }

    public void Init(int _x, int _y, GameManager _gameManager, GameManager.SweetsType _type)
    {
        x = _x;
        y = _y;
        gameManager = _gameManager;
        type = _type;
    }

    public bool CanMove()
    {
        return movedCommpont != null;
    }

    public bool CanColor()
    {
        return colorCommpont != null;
    }

    public bool CanClear()
    {
        return clearCommpont != null;
    }

    private void OnMouseEnter()
    {
        gameManager.EnterSweet(this);
    }

    private void OnMouseDown()
    {
        gameManager.PressSweet(this);
    }

    private void OnMouseUp()
    {
        gameManager.ReleaseSweet();
    }
}
