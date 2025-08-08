using System.Collections;
using UnityEngine;

public class Dice : MonoBehaviour
{
    private Sprite[] diceSides;
    private SpriteRenderer rend;
    
    private bool coroutineAllowed = true;

    private void Start()
    {
        rend = GetComponent<SpriteRenderer>();
        diceSides = Resources.LoadAll<Sprite>("DiceSides/");
        rend.sprite = diceSides[5];
    }

    private void OnMouseDown()
    {
        if (!GameControl.gameOver && coroutineAllowed)
            StartCoroutine("RollTheDice");
    }

    private IEnumerator RollTheDice()
    {
        coroutineAllowed = false;
        int randomDiceSide = 0;

        for (int i = 0; i <= 20; i++)
        {
            randomDiceSide = Random.Range(0, 6);
            rend.sprite = diceSides[randomDiceSide];
            yield return new WaitForSeconds(0.05f);
        }

        int finalRoll = randomDiceSide + 1;

        // Cek jika dalam mode combat
        if (GameControl.IsInCombat())
        {
            Debug.Log("Combat Mode: Dice roll = " + finalRoll);
            GameControl.ProcessCombatRoll(finalRoll);
        }
        else
        {
            // Normal movement
            GameControl.diceSideThrown = finalRoll;

            if (GameControl.currentTurn == 1)
            {
                GameControl.MovePlayer(1);
                GameControl.currentTurn = 2;
            }
            else
            {
                GameControl.MovePlayer(2);
                GameControl.currentTurn = 1;
            }


    
        }

        coroutineAllowed = true;
    }
}
