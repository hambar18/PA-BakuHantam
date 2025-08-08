using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GameControl : MonoBehaviour
{
    public static GameControl instance;
    private static GameObject whoWinsTextShadow, player1MoveText, player2MoveText;
    private static GameObject player1, player2;

    public static int diceSideThrown = 0;
    public static int player1StartWaypoint = 0;
    public static int player2StartWaypoint = 0;
    public static bool isCombatActive = false;
    public static bool hasMovedThisTurn = false;
    public static bool gameOver = false;
    public static int currentTurn = 1;

    private static bool[] isPlayerFrozen = new bool[2];

    private static Dictionary<int, int> snakeAndLadderMap = new Dictionary<int, int>()
    {
        { 2, 4 },
        { 6, 1 },
    };

    private static GameObject combatPanel;
    private static TextMeshProUGUI combatStatusText;
    private static TextMeshProUGUI combatResultText;
    private static Button combatRollButton;
    private static Image combatDiceImage;
    private static Sprite[] diceSides;

    private static bool inCombat = false;
    private static int combatRollP1 = -1;
    private static int combatRollP2 = -1;
    private static bool isP1TurnToRoll = true;
    private static bool isRolling = false;

    private void Awake()
    {
        instance = this;
    }

    private static void CheckForSnakeOrLadder(GameObject player, ref int playerStartWaypoint)
    {
        FollowThePath path = player.GetComponent<FollowThePath>();
        int currentPos = path.waypointIndex - 1;

        if (snakeAndLadderMap.ContainsKey(currentPos))
        {
            int newPos = snakeAndLadderMap[currentPos];
            path.waypointIndex = newPos;
            player.transform.position = path.waypoints[newPos].transform.position;
            playerStartWaypoint = newPos;
        }
    }

    void Start()
    {
        whoWinsTextShadow = GameObject.Find("WhoWinsText");
        player1MoveText = GameObject.Find("Player1MoveText");
        player2MoveText = GameObject.Find("Player2MoveText");

        player1 = GameObject.Find("Player1");
        player2 = GameObject.Find("Player2");

        player1.GetComponent<FollowThePath>().moveAllowed = false;
        player2.GetComponent<FollowThePath>().moveAllowed = false;

        whoWinsTextShadow.gameObject.SetActive(false);
        player1MoveText.gameObject.SetActive(true);
        player2MoveText.gameObject.SetActive(false);

        combatPanel = GameObject.Find("CombatPanel");
        combatStatusText = GameObject.Find("CombatStatusText").GetComponent<TextMeshProUGUI>();
        combatResultText = GameObject.Find("CombatResultText").GetComponent<TextMeshProUGUI>();
        combatRollButton = GameObject.Find("RollButton").GetComponent<Button>();
        combatDiceImage = GameObject.Find("CombatDiceImage").GetComponent<Image>();
        diceSides = Resources.LoadAll<Sprite>("DiceSides/");

        combatPanel.SetActive(false);
        combatRollButton.onClick.AddListener(OnRollButtonClicked);

        isPlayerFrozen[0] = false;
        isPlayerFrozen[1] = false;
    }

    void Update()
    {
        if (gameOver || inCombat) return;

        if (player1.GetComponent<FollowThePath>().waypointIndex >
            player1StartWaypoint + diceSideThrown)
        {
            player1.GetComponent<FollowThePath>().moveAllowed = false;
            player1MoveText.SetActive(false);
            player2MoveText.SetActive(true);
            player1StartWaypoint = player1.GetComponent<FollowThePath>().waypointIndex - 1;

            CheckForSnakeOrLadder(player1, ref player1StartWaypoint);

            currentTurn = 2;
            hasMovedThisTurn = true;
        }

        if (player2.GetComponent<FollowThePath>().waypointIndex >
            player2StartWaypoint + diceSideThrown)
        {
            player2.GetComponent<FollowThePath>().moveAllowed = false;
            player2MoveText.SetActive(false);
            player1MoveText.SetActive(true);
            player2StartWaypoint = player2.GetComponent<FollowThePath>().waypointIndex - 1;

            CheckForSnakeOrLadder(player2, ref player2StartWaypoint);

            currentTurn = 1;
            hasMovedThisTurn = true;
        }

        if (!inCombat &&
            player1StartWaypoint == player2StartWaypoint &&
            player1StartWaypoint > 0)
        {
            StartCombat(1);
        }

        if (player1.GetComponent<FollowThePath>().waypointIndex >= 7)
        {
            whoWinsTextShadow.SetActive(true);
            whoWinsTextShadow.GetComponent<Text>().text = "Player 1 Wins!";
            gameOver = true;
        }
        if (player2.GetComponent<FollowThePath>().waypointIndex >= 7)
        {
            whoWinsTextShadow.SetActive(true);
            player1MoveText.SetActive(false);
            player2MoveText.SetActive(false);
            whoWinsTextShadow.GetComponent<Text>().text = "Player 2 Wins!";
            gameOver = true;
        }
    }

    public static void MovePlayer(int playerToMove)
    {
        if (inCombat) return;

        hasMovedThisTurn = false;

        int index = playerToMove - 1;
        if (isPlayerFrozen[index])
        {
            Debug.Log("Player " + playerToMove + " is frozen and skips their move.");
            isPlayerFrozen[index] = false;
            hasMovedThisTurn = true;
            currentTurn = (playerToMove == 1) ? 2 : 1;
            return;
        }

        if (playerToMove == 1)
        {
            player1.GetComponent<FollowThePath>().moveAllowed = true;
        }
        else if (playerToMove == 2)
        {
            player2.GetComponent<FollowThePath>().moveAllowed = true;
        }
    }

    public static bool IsInCombat()
    {
        return inCombat;
    }

    public static int GetCurrentPlayerTurn()
    {
        return currentTurn;
    }

    public void OnRollButtonClicked()
    {
        if (!inCombat || isRolling) return;
        instance.StartCoroutine(RollDiceAndProcess());
    }

    private static IEnumerator RollDiceAndProcess()
    {
        isRolling = true;
        int finalRoll = 0;

        for (int i = 0; i < 10; i++)
        {
            int tempRoll = Random.Range(1, 7);
            combatDiceImage.sprite = diceSides[tempRoll - 1];
            yield return new WaitForSeconds(0.07f);
            finalRoll = tempRoll;
        }

        combatDiceImage.sprite = diceSides[finalRoll - 1];
        yield return new WaitForSeconds(0.4f);

        ProcessCombatRoll(finalRoll);
        isRolling = false;
    }

    public static void TriggerCombatBySpell(int attackerID)
    {
        if (!hasMovedThisTurn || inCombat || gameOver) return;
        StartCombat(attackerID);
    }

    public static void ProcessCombatRoll(int roll)
    {
        if (isP1TurnToRoll)
        {
            combatRollP1 = roll;
            isP1TurnToRoll = false;
            combatStatusText.text = "Combat! Player 2's turn to roll.";
        }
        else
        {
            combatRollP2 = roll;
            isP1TurnToRoll = true;
            combatResultText.text = $"P1 rolled {combatRollP1} vs P2 rolled {combatRollP2}";
            combatStatusText.text = "Resolving combat...";
            instance.StartCoroutine(DelayedResolveCombat());
        }
    }

    private static IEnumerator DelayedResolveCombat()
    {
        yield return new WaitForSeconds(1f);
        ResolveCombat();
    }

    private static void ResolveCombat()
    {
        if (combatRollP1 == combatRollP2)
        {
            combatStatusText.text = "Draw! Player 1's turn to roll again.";
            combatResultText.text = "";
            combatRollP1 = -1;
            combatRollP2 = -1;
            return;
        }

        GameObject loser;
        int loserID;
        int retreat;

        if (combatRollP1 > combatRollP2)
        {
            loser = player2;
            loserID = 2;
            retreat = combatRollP2;
        }
        else
        {
            loser = player1;
            loserID = 1;
            retreat = combatRollP1;
        }

        FollowThePath path = loser.GetComponent<FollowThePath>();
        int newPos = Mathf.Max(0, path.waypointIndex - 1 - retreat);
        path.ForceMoveTo(newPos);

        if (loserID == 1)
            player1StartWaypoint = newPos;
        else
            player2StartWaypoint = newPos;

        combatStatusText.text = $"Player {loserID} retreats {retreat} steps!";
        combatResultText.text += $"\nPlayer {loserID} loses!";
        inCombat = false;
        hasMovedThisTurn = false;
        combatPanel.SetActive(false);
    }

    private static void StartCombat(int startingPlayerID)
    {
        inCombat = true;
        isP1TurnToRoll = (startingPlayerID == 1);
        combatRollP1 = -1;
        combatRollP2 = -1;

        combatPanel.SetActive(true);
        combatStatusText.text = $"Combat! Player {startingPlayerID}'s turn to roll.";
        combatResultText.text = "";
    }

    // ========== Tambahan untuk Freeze Card ==========
    public static void FreezePlayer(int playerID)
    {
        int index = playerID - 1;
        if (index >= 0 && index < isPlayerFrozen.Length)
        {
            isPlayerFrozen[index] = true;
        }
    }

    public static bool CanUseAction()
    {
        return hasMovedThisTurn && !inCombat && !gameOver;
    }

    public static int GetOpponentPlayerID()
    {
        return (currentTurn == 1) ? 2 : 1;
    }

    // Tambahan agar SpellCardManager tidak error
    public static bool HasPlayerMoved(int playerID)
    {
        // Aktifkan spellcard hanya jika giliran player dan sudah bergerak
        return hasMovedThisTurn && currentTurn == playerID && !inCombat && !gameOver;
    }

    public static bool IsPlayerFrozen(int playerID)
    {
        int index = playerID - 1;
        if (index >= 0 && index < isPlayerFrozen.Length)
            return isPlayerFrozen[index];
        return false;
    }

    public static void UnfreezePlayer(int playerID)
    {
        int index = playerID - 1;
        if (index >= 0 && index < isPlayerFrozen.Length)
        {
            isPlayerFrozen[index] = false;
            Debug.Log("Player " + playerID + " is unfrozen!");
        }
    }
}
