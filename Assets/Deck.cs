using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour {

    public List<UIGame> games = new List<UIGame>();
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
        UIGame uigame = newgame.GetComponent<UIGame>();
        uigame.gameIAm = game.gameIAm;
        Sprite sprite = game.spriteToUse;
        uigame.Setup(uigame.gameIAm, sprite);
        games.Add(uigame);
    }

    public void RemoveCard(UIGame game)
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
