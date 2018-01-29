using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour {

    public QuizManager quiz;

    public List<DeckGame> games = new List<DeckGame>();
    public Sprite sprite;
    public GameObject uigamePrefab;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

    }




    public void AddCard(UIGame game)
    {
        GameObject newgame = Instantiate(uigamePrefab, transform);
        DeckGame uigame = newgame.GetComponent<DeckGame>();
        uigame.gameIAm = game.gameIAm;
        Sprite sprite = game.spriteToUse;
        uigame.Setup(uigame.gameIAm, sprite, quiz);
        games.Add(uigame);
    }

    public void RemoveCard(DeckGame game)
    {
        if (games.Contains(game))
        {
            games.Remove(game);
        }
    }

    public void Draw()
    {

    }



}
