﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameRun : MonoBehaviour
{

	// Management of sprites
	private Object[] backgrounds;
	private Object[] props;
	private Object[] chars;

	// Game management
	private GameObject enemyCards;
    private GameObject playerCards;
	private int [] enemyChars;	
	private Agent agent;

	private int NUM_ENEMY_CARDS = 3;
	private int NUM_CLASSES     = 3;
	private int DECK_SIZE       = 4;

	// Rewards
	private float RWD_ACTION_INVALID = -2.0f;
	private float RWD_HAND_LOST      = -1.0f;
	private float RWD_TIE            = -0.1f;
	private float RWD_HAND_WON       =  1.0f;

    //Counters
    private int WINS = 0;
    private int TIES = 0;
    private int LOSSES = 0;
    private int INVALIDS = 0;
    private int PLAYER_WINS = 0;
    private int ENEMY_WINS = 0;
    private int GAMES = 0;

	// Other UI elements
	private TextMeshPro textDeck;
    private TextMeshPro textAction;
    private TextMeshPro textAgentState;
    private TextMeshPro textTurn;
    private TextMeshPro textWin;
    private TextMeshPro textTie;
    private TextMeshPro textLose;
    private TextMeshPro textInvalid;
    private TextMeshPro textGame;
    private TextMeshPro textWinsEnemy;
    private TextMeshPro textWinsPlayer;

    // Start is called before the first frame update
    void Start()
    {


        ///////////////////////////////////////
        // Sprite management
        ///////////////////////////////////////

        // Load all prefabs
        backgrounds = Resources.LoadAll("Backgrounds/");
        props       = Resources.LoadAll("Props/");
        chars       = Resources.LoadAll("Chars/");


        ///////////////////////////////////////
        // UI management
        ///////////////////////////////////////
        textDeck = GameObject.Find("TextDeck").GetComponent<TextMeshPro>();
        textAction = GameObject.Find("TextAction").GetComponent<TextMeshPro>();
        textAgentState = GameObject.Find("TextAgent").GetComponent<TextMeshPro>();
        textTurn = GameObject.Find("TextTurn").GetComponent<TextMeshPro>();
        textWin = GameObject.Find("TextWin").GetComponent<TextMeshPro>();
        textTie = GameObject.Find("TextTie").GetComponent<TextMeshPro>();
        textLose = GameObject.Find("TextLose").GetComponent<TextMeshPro>();
        textInvalid = GameObject.Find("TextInvalid").GetComponent<TextMeshPro>();
        textGame = GameObject.Find("TextGame").GetComponent<TextMeshPro>();
        textWinsPlayer = GameObject.Find("TextWinsPlayer").GetComponent<TextMeshPro>();
        textWinsEnemy = GameObject.Find("TextWinsEnemy").GetComponent<TextMeshPro>();

        ///////////////////////////////////////
        // Game management
        ///////////////////////////////////////
        enemyCards = GameObject.Find("EnemyCards");
        enemyChars = new int[NUM_ENEMY_CARDS];

        agent      = GameObject.Find("AgentManager").GetComponent<Agent>();

        agent.Initialize();


        ///////////////////////////////////////
        // Start the game
        ///////////////////////////////////////
        StartCoroutine("GenerateTurn");


        ///////////////////////////////////////
        // Image generation
        ///////////////////////////////////////
        //renderTexture = gameObject.GetComponent<Camera>().targetTexture;

        //imgWidth = renderTexture.width;
        //imgHeight = renderTexture.height;

        playerCards = GameObject.Find("PlayerCards");


    }


    // Generate a card on a given transform
    // Return the label (0-2) of the card
    private int GenerateCard(Transform parent)
    {
    	int idx = Random.Range(0, backgrounds.Length);
    	Instantiate(backgrounds[idx], parent.position, Quaternion.identity, parent);


    	idx               = Random.Range(0, props.Length);
    	Vector3 position = new Vector3(Random.Range(-3.0f, 3.0f), Random.Range(-3.0f, 3.0f), -1.0f);
   	  	Instantiate(props[idx], parent.position+position, Quaternion.identity, parent);

    	idx         = Random.Range(0, chars.Length);
    	position    = new Vector3(Random.Range(-3.0f, 3.0f), Random.Range(-3.0f, 3.0f), -2.0f);    	
   	  	Instantiate(chars[idx], parent.position+position, Quaternion.identity, parent);

   	  	// Determine label of the character, return it
   	  	int label = 0;
   	  	if(chars[idx].name.StartsWith("frog")) label = 1;
   	  	else if(chars[idx].name.StartsWith("opossum")) label = 2;

    	return label;
    }

    private void GenerateCardofClass(Transform parent, int type)
    {
        int idx = Random.Range(0, backgrounds.Length);
        Instantiate(backgrounds[idx], parent.position, Quaternion.identity, parent);


        idx = Random.Range(0, props.Length);
        Vector3 position = new Vector3(Random.Range(-3.0f, 3.0f), Random.Range(-3.0f, 3.0f), -1.0f);
        Instantiate(props[idx], parent.position + position, Quaternion.identity, parent);

        if(type == 0)
        {
            idx = Random.Range(0, 5);
            position = new Vector3(Random.Range(-3.0f, 3.0f), Random.Range(-3.0f, 3.0f), -2.0f);
            Instantiate(chars[idx], parent.position + position, Quaternion.identity, parent);
        }
        else if (type == 1)
        {
            idx = Random.Range(6, 11);
            position = new Vector3(Random.Range(-3.0f, 3.0f), Random.Range(-3.0f, 3.0f), -2.0f);
            Instantiate(chars[idx], parent.position + position, Quaternion.identity, parent);
        }
        else if (type == 2)
        {
            idx = Random.Range(12, 17);
            position = new Vector3(Random.Range(-3.0f, 3.0f), Random.Range(-3.0f, 3.0f), -2.0f);
            Instantiate(chars[idx], parent.position + position, Quaternion.identity, parent);
        }
    }

    // Generate another turn
    IEnumerator GenerateTurn()
    {	
    	for(int turn=0; turn<100000; turn++) {

	        ///////////////////////////////////////
	        // Generate enemy cards
	        ///////////////////////////////////////

	    	// Destroy previous sprites (if any) and generate new cards
	    	int c = 0;
	    	foreach(Transform card in enemyCards.transform) {
	    		foreach(Transform sprite in card) {
	    			Destroy(sprite.gameObject);
	    		}

	    		enemyChars[c++] = GenerateCard(card);
	    	}


	        ///////////////////////////////////////
	        // Generate player deck
	        ///////////////////////////////////////
	        int [] deck   = GeneratePlayerDeck();
	        textDeck.text = "Deck: ";
	        foreach(int card in deck)
	        	textDeck.text += card.ToString() + "/";


	        ///////////////////////////////////////
	        // Tell the player to play
	        ///////////////////////////////////////

	        // IMPORTANT: wait until the frame is rendered so the player sees
	        //            the newly generated cards (otherwise it will see the previous ones)
	        yield return new WaitForEndOfFrame();

	        int [] action = agent.Play(deck, enemyChars, textAgentState);

            int i = 0;
            foreach (Transform card in playerCards.transform)
            {
                foreach (Transform sprite in card)
                {
                    Destroy(sprite.gameObject);
                }
                GenerateCardofClass(card, action[i]);
                i++;
            }

            textAction.text = "Action: ";
	        foreach(int a in action)
	        	textAction.text += a.ToString() + "/";

           


            ///////////////////////////////////////
            // Compute reward
            ///////////////////////////////////////
            float reward = ComputeReward(deck, action);

            

            Debug.Log("Turn/reward: " + turn.ToString() + "->" + reward.ToString());
            

            agent.GetReward(reward);


            ///////////////////////////////////////
            // Manage turns/games
            ///////////////////////////////////////


            if (turn % 15 + 1 == 1)
            {
                if(WINS > LOSSES + INVALIDS)
                {
                    PLAYER_WINS++;
                }
                else if(LOSSES + INVALIDS > WINS)
                {
                    ENEMY_WINS++;
                }

                WINS = 0;
                LOSSES = 0;
                TIES = 0;
                INVALIDS = 0;
                GAMES++;
            }

            if (reward == RWD_HAND_WON)
            {
                WINS++;
            }
            if (reward == RWD_HAND_LOST)
            {
                LOSSES++;
            }
            if (reward == RWD_TIE)
            {
                TIES++;
            }
            if (reward == RWD_ACTION_INVALID)
            {
                INVALIDS++;
            }

            textTurn.text = "R: " + (turn % 15 + 1).ToString();
            textWin.text = "W: " + WINS.ToString();
            textTie.text = "T: " + TIES.ToString();
            textLose.text = "L: " + LOSSES.ToString();
            textInvalid.text = "I: " + INVALIDS.ToString();
            textGame.text = "G: " + GAMES.ToString();
            textWinsEnemy.text = ENEMY_WINS.ToString();
            textWinsPlayer.text = PLAYER_WINS.ToString();


            yield return new WaitForSeconds(0.1f);

    	}

    }


    // Auxiliary methods
    private int [] GeneratePlayerDeck()
    {
    	int [] deck = new int [DECK_SIZE];

    	for(int i=0; i<DECK_SIZE; i++)
    	{
    		deck[i] = Random.Range(0, NUM_CLASSES);  // high limit is exclusive so [0, NUM_CLASSES-1]
    	}

    	return deck;
    }

    // Compute the result of the turn and return the associated reward 
    // given the cards selected by the agent (action)
   	// deck -> array with the number of cards of each class the player has
   	// action -> array with the class of each card played
    private float ComputeReward(int [] deck, int [] action)
    {
    	// First check if the action is valid given the player's deck
    	foreach(int card in action)
    	{
    		deck[card]--;
    		if(deck[card] < 0)
    			return RWD_ACTION_INVALID;
    	}


    	// Second see who wins
    	int score = 0;
    	for(int i=0; i<NUM_ENEMY_CARDS; i++)
    	{
    		if(action[i] != enemyChars[i])
    		{
    			if(action[i] > enemyChars[i] || action[i]==0 && enemyChars[i]==2)	
    				score++;
    			else
    				score--;
    		}
    		
    	}

    	if(score == 0) return RWD_TIE;
    	else if(score > 0) return RWD_HAND_WON;
    	else return RWD_HAND_LOST;
    }
}
