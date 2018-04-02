using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player : MonoBehaviour {
    
    public int id;

    public UIGame game;

    public int points;

	
    public void SetID(int id)
    {
        this.id = id;
    }
	
    public void SetGame(UIGame gam)
    {
        game = gam;
    }

    public void AddPoints(int pointsToAdd)
    {
        points += pointsToAdd;
    }

    public void ResetPoints()
    {
        points = 0;
    }

}
