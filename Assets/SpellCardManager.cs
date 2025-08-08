using UnityEngine;
using UnityEngine.UI;

public class SpellCardManager : MonoBehaviour
{
    public Button attackCardButton;
    public Button freezeCardButton;
    public Button defenseCardButton;

    private void Start()
    {
        attackCardButton.onClick.AddListener(UseAttackCard);
        freezeCardButton.onClick.AddListener(UseFreezeCard);
        defenseCardButton.onClick.AddListener(UseDefenseCard);

        attackCardButton.interactable = false;
        freezeCardButton.interactable = false;
        defenseCardButton.interactable = false;
    }

    private void Update()
    {
        int current = GameControl.currentTurn;

        bool hasMoved = GameControl.HasPlayerMoved(current);

        // Hanya aktifkan tombol jika player sudah bergerak dan belum pakai kartunya
        attackCardButton.interactable = hasMoved;
        freezeCardButton.interactable = hasMoved;
        defenseCardButton.interactable = hasMoved;
    }

    public void UseAttackCard()
    {
        int current = GameControl.currentTurn;
        GameControl.TriggerCombatBySpell(current);
        Debug.Log("Attack card used! Combat triggered.");
        attackCardButton.interactable = false;
    }

    public void UseFreezeCard()
    {
        int targetPlayer = (GameControl.currentTurn == 1) ? 2 : 1;
        GameControl.FreezePlayer(targetPlayer);
        Debug.Log("Freeze card used on Player " + targetPlayer);
        freezeCardButton.interactable = false;
    }

    public void UseDefenseCard()
    {
        int playerID = GameControl.currentTurn;
        if (GameControl.IsPlayerFrozen(playerID))
        {
            GameControl.UnfreezePlayer(playerID);
            Debug.Log("Defense card used. Freeze effect removed.");
        }
        else
        {
            Debug.Log("Defense card used, but player is not frozen.");
        }
        defenseCardButton.interactable = false;
    }
}