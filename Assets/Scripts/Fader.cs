using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameColor { yellow, green, red, white }

public class Fader : MonoBehaviour {

    public QuizManager quiz;
    public Image fader;
    public Image faderFollower;

    public Color yellow;
    public Color green;
    public Color red;
    
    // Use this for initialization
	void Start () {
		
	}

    public void Fade(GameColor gc)
    {
        //fader.gameObject.SetActive(true);
        SetColor(gc);
        StartCoroutine(Util.MoveToPos(-quiz.outOfScreenX, quiz.outOfScreenX, fader.rectTransform, quiz.curves.smooth, 1));
    }

    public void FadeOut(GameColor gc)
    {
        //fader.gameObject.SetActive(true);
        SetColor(gc);
        StartCoroutine(Util.MoveToPos(quiz.outOfScreenX, -quiz.outOfScreenX, fader.rectTransform, quiz.curves.smooth, 1));
    }



    void SetColor(GameColor gc)
    {
        switch (gc)
        {
            case GameColor.green:
                fader.color = green;
                faderFollower.color = green;
                break;
            case GameColor.yellow:
                fader.color = yellow;
                faderFollower.color = yellow;
                break;
            case GameColor.red:
                fader.color = red;
                faderFollower.color = red;
                break;
            default:
                fader.color = Color.white;
                faderFollower.color = Color.white;
                break;
        }

    }


}
