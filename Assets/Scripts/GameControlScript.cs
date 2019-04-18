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
    Reload = 3
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

    public Animator[] animator = new Animator[2];

    [SerializeField]
    Image[] waitBar;

    public Texture aTexture;

    bool canStart;

    public Text[] text;

    List<PlayerState[]> p1_win = new List<PlayerState[]> {
            new PlayerState[2] { PlayerState.Fire, PlayerState.Idle },
            new PlayerState[2] { PlayerState.Fire, PlayerState.Reload }};

    List<PlayerState[]> p2_win = new List<PlayerState[]> {
            new PlayerState[2] { PlayerState.Idle, PlayerState.Fire },
            new PlayerState[2] { PlayerState.Reload, PlayerState.Fire }};



    void Start()
    {
        Debug.ClearDeveloperConsole();

        canStart = false;
        gameState = GameState.InitGame;

        players = new Player[2];

        players[0] = new Player();
        players[1] = new Player();

        foreach (Image bar in waitBar){

            if (bar != null)
            {
                bar.fillAmount = 0f;
                bar.color = Color.red;
            }
        }
        foreach (Button button in buttons)
        {
            if (button != null)
                button.interactable = false;
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

                StartCoroutine("FillBar", waitBar[0]);
                StartCoroutine("FillBar", waitBar[1]);
            }
        }
    }

    void SetGameState(GameState state)
    {
        gameState = state;

        if (gameState == GameState.PickAction)
        {
            players[0].CanChoose = true;
            players[1].CanChoose = true;

            foreach (Button button in buttons)
            {
                if (button != null)
                    button.interactable = true;
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
                    button.interactable = false;
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

    private void ChooseAction(int state, int playerIndex)
    {
        if (gameState == GameState.PickAction && players[playerIndex].CanChoose)
        {
            players[playerIndex].PlayerState = (PlayerState)state;
            players[playerIndex].CanChoose = false;            
        }
    }

    void PlayerFire(int playerIndex)
    {
        animator[playerIndex].SetTrigger("Fire");
    }

    void PlayerDefense(int playerIndex)
    {
        animator[playerIndex].SetTrigger("Defense");
    }

    void PlayerReload(int playerIndex)
    {
        players[playerIndex].NumberOfBullets = Mathf.Min(6, players[playerIndex].NumberOfBullets + 1);
    }

    void PlayerDie(int playerIndex)
    {
        animator[playerIndex].SetTrigger("Die");
    }

    IEnumerator FillBar(Image waitBar)
    {
        if (gameState != GameState.GameFinished)
        {
            for (float i = 0; i <= 1; i += Time.deltaTime / 2)
            {
                waitBar.fillAmount = i;
                yield return new WaitForSeconds(Time.deltaTime / 2);
            }
            waitBar.fillAmount = 1f;

            waitBar.color = Color.green;

            SetGameState(GameState.PickAction);

            yield return new WaitForSeconds(1);

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
            ShowGameResult(0);
            return GameResult.P1_Win;
        }
        else if(p2_win.Contains(p_state, comperator))
        {
            ShowGameResult(1);
            return GameResult.P2_Win;
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

        foreach (Image bar in waitBar)
            bar.fillAmount = 0;

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
                if (!players[i].HoldButton && gameState == GameState.HoldAndWait && waitBar[i].fillAmount > .5f)
                {
                    ShowGameResult(i == 0 ? 1 : 0);
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
            
    }

    void OnGUI()
    {
        if (!aTexture)
        {
            Debug.LogError("Assign a Texture in the inspector.");
            return;
        }

        for (int i = 0; i < players[0].NumberOfBullets; i++)
        {
            GUI.DrawTexture(new Rect(705 + Mathf.Cos(Mathf.PI + Mathf.PI / 3 * i) * 38, 622 + Mathf.Sin(Mathf.PI + Mathf.PI / 3 * i) * 38, 30, 30), aTexture, ScaleMode.ScaleToFit);
        }

        for (int i = 0; i < players[1].NumberOfBullets; i++)
        {
            GUI.DrawTexture(new Rect(571+Mathf.Cos(Mathf.PI/3*i) * 38, 67+Mathf.Sin(Mathf.PI / 3 * i) * 38, 30, 30), aTexture, ScaleMode.ScaleToFit);
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
    }

    public bool CanChoose { get; set; }
    public bool HoldButton { get; set; }
    public int NumberOfBullets { get; set; }
    internal PlayerState PlayerState { get; set; }
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
