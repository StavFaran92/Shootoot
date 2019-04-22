using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

enum GameState
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

    GameState gameState;

    Player[] players;
    
    public Button[] buttons;
    public bool[] enabledButtons;

    public Animator[] animator = new Animator[2];

    public Image waitBar;

    public Texture aTexture;

    bool canStart;

    public Image[] bullets_p1;
    public Image[] bullets_p2;

    public RectTransform box;

    public Text[] text;

    public Image[] heart_p1;
    public Image[] heart_p2;

    public Image[] screenDamage;

    public GameObject bloodSplatter;
    public GameObject[] gameObjectPlayer;

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

    public AudioSource[] musicSource;

    void Start()
    {
        Debug.ClearDeveloperConsole();

        canStart = false;
        gameState = GameState.InitGame;

        players = new Player[2];

        players[0] = new Player();
        players[1] = new Player();

        enabledButtons = new bool[6]{ true,true,true,true,true,true};


        if (waitBar != null)
        {
            waitBar.fillAmount = 0f;
            waitBar.color = Color.red;
        }
        
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

        canStart = true;

        
    }

    private void StartGame()
    {
        if (gameState == GameState.InitGame && canStart)
        {
            text[0].gameObject.SetActive(!players[0].HoldButton);
            text[1].gameObject.SetActive(!players[1].HoldButton);

            if (players[0].HoldButton && players[1].HoldButton)
            {
                gameState = GameState.HoldAndWait;

                musicSource[2].clip = bgMusicClip;
                musicSource[2].Play();

                StartCoroutine("FillBar", waitBar);
            }
        }
    }

    void SetGameState(GameState state)
    {
        gameState = state;

        if (gameState == GameState.PickAction)
        {
            if (players[0].PlayerState != PlayerState.Ban) players[0].CanChoose = true;
            if (players[1].PlayerState != PlayerState.Ban) players[1].CanChoose = true;

            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].PlayerState == PlayerState.Ban)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        enabledButtons[j + i*3] = false;
                    }
                }
                else
                {
                    enabledButtons[i * 3] = players[i].NumberOfBullets == 0 ? false : true;
                    enabledButtons[1 + i * 3] = true;
                    enabledButtons[2 + i * 3] = players[i].NumberOfBullets == 6 ? false : true;
                }
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

    IEnumerator FillBar(Image waitBar)
    {
        if (gameState != GameState.GameFinished)
        {
            for (float i = 0; i <= 1; i += Time.deltaTime / 3)
            {
                waitBar.fillAmount = i;
                yield return new WaitForSeconds(Time.deltaTime / 3);
            }
            waitBar.fillAmount = 1f;

            waitBar.color = Color.green;

            SetGameState(GameState.PickAction);

            yield return new WaitForSeconds(2);

            SetGameState(GameState.HoldAndWait);

            waitBar.fillAmount = 0f;

            waitBar.color = Color.red;

            StartCoroutine("FillBar", waitBar);
        }
    }

    GameResult CalculateGameResult()
    {
        

        ArrayComparer comperator = new ArrayComparer();

        PlayerState[] p_state = new PlayerState[2] { players[0].PlayerState, players[1].PlayerState };

        if( p1_win.Contains(p_state, comperator))
        {
            players[1].Health -= 1;
            PlayerHurt(1);
        }
        else if(p2_win.Contains(p_state, comperator))
        {
            players[0].Health -= 1;
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

        waitBar.fillAmount = 0;

        StopAllCoroutines();
    }

    public void IsPointerDown(int i)
    {
        players[i].HoldButton = true;
    }

    private void helperFunction()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            players[1].HoldButton = true;
    }

    public void IsPointerUp(int i)
    {
        players[i].HoldButton = false;
    }

    private void Update()
    {
        if (gameState == GameState.InitGame)
            StartGame();

        helperFunction();

        if (gameState != GameState.GameFinished)
        {
            for (int i = 0; i < players.Length; ++i)
            {
                if (!players[i].HoldButton && gameState == GameState.HoldAndWait && waitBar.fillAmount > .33f)
                {
                    players[i].CanChoose = false;
                    players[i].PlayerState = PlayerState.Ban;
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
