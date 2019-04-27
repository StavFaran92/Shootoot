using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameState
{
    InitGame,
    HoldAndWait,
    PickAction,
    GameFinished
}

enum PlayerState
{
    Idle = 0,
    Fire = 1,
    Defense = 2,
    Reload = 3,
    Ban = 4
}

enum GameResult
{
    Draw,
    P1_Win,
    P2_Win
}

public class GameControlScript : MonoBehaviour {

    int savedDifficulty;

    float gapBetween1to2;
    float gapBetween2to3;
    float gapBetweenBoxGroup;

    GameState gameState;

    Player[] players;
    
    public Button[] buttons;
    bool[] enabledButtons;

    public Canvas canvas;
    public RectTransform createBoxPosition;
    public Collider2D lineCollider;

    public Animator[] animator = new Animator[2];

    public Texture aTexture;

    public Image[] bullets_p1;
    public Image[] bullets_p2;

    public TextMeshProUGUI[] text;

    public Image[] heart_p1;
    public Image[] heart_p2;

    public Image[] screenDamage;

    public GameObject bloodSplatter;
    public GameObject[] gameObjectPlayer;

    public Image pressBox;

    List<GameObject> pressBoxList;

    int boxSize;

    public Image PressBoxAnim;

    bool[,] isPressCorrect;

    List<PlayerState[]> p1_win = new List<PlayerState[]> {
            new PlayerState[2] { PlayerState.Fire, PlayerState.Idle },
            new PlayerState[2] { PlayerState.Fire, PlayerState.Reload },
            new PlayerState[2] { PlayerState.Fire, PlayerState.Ban }};

    List<PlayerState[]> p2_win = new List<PlayerState[]> {
            new PlayerState[2] { PlayerState.Idle, PlayerState.Fire },
            new PlayerState[2] { PlayerState.Reload, PlayerState.Fire },
            new PlayerState[2] { PlayerState.Ban, PlayerState.Fire }};

    public AudioClip clipPlayerFire;
    public AudioClip clipPlayerDefense;
    public AudioClip clipPlayerReload;
    public AudioClip clipPlayerHurt;

    public AudioClip bgMusicClip;
    public AudioClip bgMusicClip_slow;

    public AudioSource[] musicSource;

    void Start()
    {
        Debug.ClearDeveloperConsole();

        savedDifficulty = PlayerPrefs.GetInt("Difficulty", 2);

        gameState = GameState.InitGame;

        players = new Player[2];

        players[0] = new Player();
        players[1] = new Player();

        enabledButtons = new bool[6]{ true,true,true,true,true,true};

        isPressCorrect = new bool[2,2]{ { false, false},  { false, false } };

        pressBoxList = new List<GameObject>();
        
        foreach (Button button in buttons)
        {
            if (button != null)
            {
                button.interactable = false;
            }
        }

        for (int i = 0; i < 2; i++)
        {
            EnableDisablePlayerAttributes(i);
            screenDamage[i].enabled = false;
        }

        StartCoroutine("CountDown");

    }

    void SetActRangeByDifficulty(int difficulty)
    {
        switch (difficulty) {
            case 1:
                boxSize = 190;
                gapBetween1to2 = 390;
                gapBetween2to3 = 795;
                gapBetweenBoxGroup = 1.87f;
                musicSource[2].clip = bgMusicClip_slow;
                break;
            case 2:
                boxSize = 160;
                gapBetween1to2 = 330;
                gapBetween2to3 = 695;
                gapBetweenBoxGroup = 1.67f;
                musicSource[2].clip = bgMusicClip;
                break;
            case 3:
                boxSize = 100;
                gapBetween1to2 = 330;
                gapBetween2to3 = 695;
                gapBetweenBoxGroup = 1.67f;
                musicSource[2].clip = bgMusicClip;
                break;
        }
    }

    IEnumerator CountDown()
    {
        yield return new WaitForSeconds(1);
        text[0].text = "2";
        text[1].text = "2";
        yield return new WaitForSeconds(1);
        text[0].text = "1";
        text[1].text = "1";
        yield return new WaitForSeconds(1);
        text[0].text = "GO";
        text[1].text = "GO";
        yield return new WaitForSeconds(1);
        text[0].text = "";
        text[1].text = "";

        gameState = GameState.HoldAndWait;

        StartCoroutine("CreatePressBox");

        yield return new WaitForSeconds(.1f);

        SetActRangeByDifficulty(savedDifficulty);

        musicSource[2].Play();
    }

    IEnumerator CreatePressBox()
    {
        InstantiateBox(Vector3.zero, "box_1");
        InstantiateBox(new Vector3(0, gapBetween1to2, 0), "box_2"); 
        InstantiateBox(new Vector3(0, gapBetween2to3, 0), "box_3"); 

        yield return new WaitForSeconds(gapBetweenBoxGroup);

        StartCoroutine( "CreatePressBox");
    }

    void InstantiateBox(Vector3 pos, string label)
    {
        Image go = Instantiate(pressBox, createBoxPosition.transform.position + pos, pressBox.transform.rotation);
        go.transform.SetParent(canvas.transform);
        go.rectTransform.sizeDelta = new Vector2(boxSize, go.rectTransform.sizeDelta.y);
        BoxCollider2D boxColldier = go.GetComponent<BoxCollider2D>();
        boxColldier.size = new Vector2(boxSize, go.rectTransform.sizeDelta.y);

        if (label == "box_1" || label == "box_2")
            go.color = Color.red;
        else if (label == "box_3")
            go.color = Color.green;

        go.tag = label;
    }

    


    public void SetGameState(GameState state)
    {
        gameState = state;

        if (gameState == GameState.PickAction)
        {
            if (players[0].PlayerState != PlayerState.Ban && isPressCorrect[0, 0] && isPressCorrect[0, 1])
                players[0].CanChoose = true;
            if (players[1].PlayerState != PlayerState.Ban && isPressCorrect[1, 0] && isPressCorrect[1, 1])
                players[1].CanChoose = true;

            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].PlayerState == PlayerState.Ban || !isPressCorrect[i, 0] || !isPressCorrect[i, 1])
                {
                    for (int j = 0; j < 3; j++)
                    {
                        enabledButtons[j + i * 3] = false;
                    }
                }
                else
                {
                    enabledButtons[i * 3] = players[i].NumberOfBullets == 0 ? false : true;
                    enabledButtons[1 + i * 3] = true;
                    enabledButtons[2 + i * 3] = players[i].NumberOfBullets == 6 ? false : true;
                }

                isPressCorrect[i,0] = false;
                isPressCorrect[i,1] = false;
            }

            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null && enabledButtons[i])
                {
                    buttons[i].interactable = true;
                }
            }

        }
        else if (gameState == GameState.HoldAndWait)
        {
            activatePlayerState((int)players[0].PlayerState, 0);
            activatePlayerState((int)players[1].PlayerState, 1);

            CalculateGameResult();

            players[0].PlayerState = PlayerState.Idle;
            players[1].PlayerState = PlayerState.Idle;

            foreach (Button button in buttons)
            {
                if (button != null)
                {
                    button.interactable = false;
                }
            }

            
        }
    }

    private void activatePlayerState(int state, int playerIndex)
    {
        switch (state)
        {
            case 1:
                PlayerFire(playerIndex);
                break;
            case 2:
                PlayerDefense(playerIndex);
                break;
            case 3:
                PlayerReload(playerIndex);
                break;
        }
    }

    public void ChooseP1Action(int state)
    {
        ChooseAction(state, 0);
    }

    public void ChooseP2Action(int state)
    {
        ChooseAction(state, 1);
    }

    void ChooseAction(int state, int playerIndex)
    {
        if (gameState == GameState.PickAction && players[playerIndex].CanChoose)
        {
            players[playerIndex].PlayerState = (PlayerState)state;
            players[playerIndex].CanChoose = false;            
        }
    }

    void PlayerFire(int playerIndex)
    {
        if (players[playerIndex].NumberOfBullets > 0)
        {
            animator[playerIndex].SetTrigger("Fire");
            players[playerIndex].NumberOfBullets -= 1;
            musicSource[playerIndex].clip = clipPlayerFire;
            musicSource[playerIndex].Play();
        }
    }

    void PlayerDefense(int playerIndex)
    {
        animator[playerIndex].SetTrigger("Defense");
        musicSource[playerIndex].clip = clipPlayerDefense;
        musicSource[playerIndex].Play();
    }

    void PlayerReload(int playerIndex)
    {
        if (players[playerIndex].NumberOfBullets < 6)
        {
            animator[playerIndex].SetTrigger("Reload");
            players[playerIndex].NumberOfBullets += 1;
            musicSource[playerIndex].clip = clipPlayerReload;
            musicSource[playerIndex].Play();
        }
    }

    void PlayerHurt(int playerIndex)
    {
        players[playerIndex].Health -= 1;
        screenDamage[playerIndex].enabled = true;
        StartCoroutine("ScreenDamageFade", playerIndex);
        musicSource[playerIndex].clip = clipPlayerHurt;
        musicSource[playerIndex].Play();
        Instantiate(bloodSplatter, gameObjectPlayer[playerIndex].transform);
    }

    void PlayerDie(int playerIndex)
    {
        animator[playerIndex].SetTrigger("Die");
    }

    IEnumerator ScreenDamageFade(int playerIndex)
    {
        for (float i = 0; i <= 1; i += Time.deltaTime)
        {
            Color tempColor = screenDamage[playerIndex].color;
            tempColor.a = 1f - i;
            screenDamage[playerIndex].color = tempColor;

            yield return new WaitForSeconds(Time.deltaTime);
        }

        screenDamage[playerIndex].enabled = false;
    }

    GameResult CalculateGameResult()
    {
        ArrayComparer comperator = new ArrayComparer();

        PlayerState[] p_state = new PlayerState[2] { players[0].PlayerState, players[1].PlayerState };

        if( p1_win.Contains(p_state, comperator))
        {
            PlayerHurt(1);
        }
        else if(p2_win.Contains(p_state, comperator))
        {
            PlayerHurt(0);
        }

        for (int i = 0; i < 2; i++)
        {
            EnableDisablePlayerAttributes(i);
        }

        if (players[0].Health == 0)
        {
            ShowGameResult(1);
            return GameResult.P2_Win;
        }
        else if(players[1].Health == 0)
        {
            ShowGameResult(0);
            return GameResult.P1_Win;
        }
        else
        {
            return GameResult.Draw;
        }
    }

    void ShowGameResult(int winningPlayer)
    {
        gameState = GameState.GameFinished;

        text[winningPlayer].text = "You Win!";
        text[winningPlayer].gameObject.SetActive(true);

        int losingPlayer = (winningPlayer == 0) ? 1 : 0;
        text[losingPlayer].text = "You Lost!";
        text[losingPlayer].gameObject.SetActive(true);

        StopAllCoroutines();
    }

    public void IsPointerDown(int i)
    {
        if (gameState == GameState.HoldAndWait)
        {
            if (!isPressCorrect[i, 0] && lineCollider.GetComponent<PointerScript>().isColliding &&
                lineCollider.GetComponent<PointerScript>().boxType == "box_1")
            {
                isPressCorrect[i, 0] = true;
            }
            if (!isPressCorrect[i, 1] && lineCollider.GetComponent<PointerScript>().isColliding &&
                lineCollider.GetComponent<PointerScript>().boxType == "box_2")
            {
                isPressCorrect[i, 1] = true;
            }
            if (!lineCollider.GetComponent<PointerScript>().isColliding)
            {
                players[i].CanChoose = false;
                players[i].PlayerState = PlayerState.Ban;
            }
        }

    }

    private void helperFunction()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            players[1].HoldButton = true;
    }

    private void Update()
    {
        //if (Input.GetKey(KeyCode.Keypad1))
        //{
        //    PlayerPrefs.SetInt("Difficulty", 1);
        //}
        //if (Input.GetKey(KeyCode.Keypad2))
        //{
        //    PlayerPrefs.SetInt("Difficulty", 2);
        //}
        //if (Input.GetKey(KeyCode.Keypad3))
        //{
        //    PlayerPrefs.SetInt("Difficulty", 3);
        //}
        //PlayerPrefs.Save();
        //SetActRangeByDifficulty(PlayerPrefs.GetInt("Difficulty", 2));

        //helperFunction();

        if (gameState != GameState.GameFinished)
        {
            if (lineCollider.GetComponent<PointerScript>().isColliding && 
                lineCollider.GetComponent<PointerScript>().boxType.Equals("box_3") && gameState == GameState.HoldAndWait)
            {
                SetGameState(GameState.PickAction);
            }
            else if (!lineCollider.GetComponent<PointerScript>().isColliding && gameState == GameState.PickAction)
            {
                SetGameState(GameState.HoldAndWait);
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            InstantiateBox(new Vector3( 100, 100, 0), "box_1");
        }

    }

    void EnableDisablePlayerAttributes(int playerIndex)
    {
        for (int i = 0; i < 6; i++)
        {
            if(playerIndex == 0)
                bullets_p1[i].enabled = (i < players[playerIndex].NumberOfBullets) ? true : false;
            else
                bullets_p2[i].enabled = (i < players[playerIndex].NumberOfBullets) ? true : false;
        }

        for (int i = 0; i < heart_p1.Length; i++)
        {
            if(playerIndex == 0)
                heart_p1[i].enabled = (i < players[playerIndex].Health) ? true : false;
            else
                heart_p2[i].enabled = (i < players[playerIndex].Health) ? true : false;
        }
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}

class Player
{
    public Player()
    {
        PlayerState = PlayerState.Idle;
        CanChoose = true;
        HoldButton = false;
        NumberOfBullets = 1;
        Health = 3;
    }

    public bool CanChoose { get; set; }
    public bool HoldButton { get; set; }
    public int NumberOfBullets { get; set; }
    public int Buttons { get; set; }
    internal PlayerState PlayerState { get; set; }
    public int Health { get; set; }
}

class ArrayComparer : EqualityComparer<PlayerState[]>
{
    public override bool Equals(PlayerState[] x, PlayerState[] y)
    {
        if (x[0] == y[0] && x[1] == y[1])
            return true;
        else
            return false;
    }

    public override int GetHashCode(PlayerState[] obj)
    {
        throw new System.NotImplementedException();
    }
}
