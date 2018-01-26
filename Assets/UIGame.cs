using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UIGame : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

    public TextMeshProUGUI gamename;
    public Image gameIMG;
    public Game gameIAm;
    public RectTransform ownRect;
    public RectTransform rectT;
    public RectTransform shadow0; 
    public RectTransform shadow1;
    public RectTransform shadow2;
    public Image shadow0IMG;
    public Image shadow1IMG;
    public Image shadow2IMG;
    public Sprite spriteToUse;

    public QuizManager quiz;

    public Vector2 startPos;
    Vector2 offSetHeightPos;
    float shadowoffSetHeight;


    public float animationPlaySpeed = 2;

    bool animationGrace = false;
    bool pointerOn = false;

	// Use this for initialization
	void Start () {
        startPos = ownRect.anchoredPosition;
        offSetHeightPos = startPos + new Vector2(20f, 40f);
        shadowoffSetHeight = 40;
    }
	
    public void Setup(Game gb)
    {
        gameIAm = gb;
        gamename.text = gb.name;
        //Image!
        gamename.gameObject.SetActive(true);


        StartCoroutine(PullingImage());

    }

    public void Setup(Game gb, Sprite spr)
    {
        gameIAm = gb;
        gamename.text = gb.name;
        //Image!
        gamename.gameObject.SetActive(true);
        
        gameIMG.sprite = spr;
        spriteToUse = spr;
    }

    IEnumerator PullingImage()
    {
        yield return new WaitForSeconds(Random.Range(0.2f, 1.2f));
        using (WWW www = new WWW(gameIAm.image.medium_url))
        {
            yield return www;

            Texture2D texture = new Texture2D(www.texture.width, www.texture.height, TextureFormat.DXT1, false);

            // assign the downloaded image to sprite
            www.LoadImageIntoTexture(texture);
            Rect rec = new Rect(0, 0, texture.width, texture.height);
            spriteToUse = Sprite.Create(texture, rec, new Vector2(0.5f, 0.5f), 100);
            gameIMG.sprite = spriteToUse;
            print(gameObject.name + " pulled image from " + gameIAm.image.medium_url);

            //shadow0IMG.sprite = spriteToUse;
            //shadow1IMG.sprite = spriteToUse;
            //shadow2IMG.sprite = spriteToUse;


            www.Dispose();
        }
    }



    public void Win(bool correctGame)
    {
        if (correctGame)
        {
            StartCoroutine(Util.MoveToPos(offSetHeightPos, startPos, rectT, quiz.curves.downCurve, 1));
            StartCoroutine(Util.Spin(rectT, quiz.curves.downCurve, 1, -10, -360, false));

            StartCoroutine(Util.MoveToPos(new Vector2(0, shadowoffSetHeight), startPos, shadow0, quiz.curves.upCurve, 1));
            StartCoroutine(Util.Spin(shadow0, quiz.curves.upCurve, 1, 0, 360, false));
            StartCoroutine(Util.MoveToPos(new Vector2(-10, shadowoffSetHeight), startPos, shadow1, quiz.curves.upCurve, 1));
            StartCoroutine(Util.Spin(shadow1, quiz.curves.upCurve, 1, 10, 360, false));
            StartCoroutine(Util.MoveToPos(new Vector2(-20, shadowoffSetHeight), startPos, shadow2, quiz.curves.upCurve, 1));
            StartCoroutine(Util.Spin(shadow2, quiz.curves.upCurve, 1, 20, 360, false));
        }
        else
        {
          //  StartCoroutine(Util.MoveToPos(offSetHeightPos, offSetHeightPos - quiz.outOfScreenY, ownRect, quiz.curves.downCurve, 1));
         //   StartCoroutine(Util.MoveToPos(offSetHeightPos, offSetHeightPos - quiz.outOfScreenY, shadow0, quiz.curves.downCurve, 1));
          //  StartCoroutine(Util.MoveToPos(offSetHeightPos, offSetHeightPos - quiz.outOfScreenY, shadow1, quiz.curves.downCurve, 1));
          //  StartCoroutine(Util.MoveToPos(offSetHeightPos, offSetHeightPos - quiz.outOfScreenY, shadow2, quiz.curves.downCurve, 1));
            // StartCoroutine(Util.Spin(rectT, quiz.curves.downCurve, 1, -10, -360, false));
            gamename.gameObject.SetActive(false);
        }
    }

    public void Lose(bool selectedGame)
    {
        if (selectedGame)
        {
            StartCoroutine(Util.RotateAskew(rectT, quiz.curves.upCurve, 3, -10,30));

            StartCoroutine(Util.RotateAskew(shadow0, quiz.curves.upCurve, 3, 0, -30));
            StartCoroutine(Util.RotateAskew(shadow1, quiz.curves.upCurve, 3, 10, -40));
            StartCoroutine(Util.RotateAskew(shadow2, quiz.curves.upCurve, 3, 20, -50));

        }

    }




    //deck games need to be different than quiz games -- or at least, they should have this stuff ----- inherit? make a game class that's larger ? or make a DeckGame?
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!quiz.quizOn)
        {
            return;
        }
        quiz.TestAnswer(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!quiz.quizOn)
        {
            return;
        }
        pointerOn = true;
        if (!animationGrace)
        {
            PlayUpAnimation();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!quiz.quizOn)
        {
            return;
        }
        pointerOn = false;
        if (!animationGrace)
        {
            PlayDownAnimation();
        }
    }

    public void PlayUpAnimation()
    {
        //lift up
        StartCoroutine(Util.MoveToPos(startPos, offSetHeightPos, rectT, quiz.curves.upCurve, animationPlaySpeed));
        StartCoroutine(Util.Spin(rectT, quiz.curves.upCurve, animationPlaySpeed,0, -10, false));

        StartCoroutine(Util.MoveToPos(startPos, new Vector2(0,shadowoffSetHeight), shadow0, quiz.curves.upCurve, animationPlaySpeed));
     //   StartCoroutine(Util.Spin(shadow0, quiz.UpCurve, animationPlaySpeed, 0, 10, false));
        StartCoroutine(Util.MoveToPos(startPos, new Vector2(-10, shadowoffSetHeight), shadow1, quiz.curves.upCurve, animationPlaySpeed));
        StartCoroutine(Util.Spin(shadow1, quiz.curves.upCurve, animationPlaySpeed, 0, 10, false));
        StartCoroutine(Util.MoveToPos(startPos, new Vector2(-20, shadowoffSetHeight), shadow2, quiz.curves.upCurve, animationPlaySpeed));
        StartCoroutine(Util.Spin(shadow2, quiz.curves.upCurve, animationPlaySpeed, 0, 20, false));



        StartCoroutine(AnimationGrace(true));
    }
    
    public void PlayDownAnimation()
    {
        //go down.
        StartCoroutine(Util.MoveToPos(offSetHeightPos, startPos, rectT, quiz.curves.downCurve, animationPlaySpeed));
        StartCoroutine(Util.Spin(rectT, quiz.curves.downCurve, animationPlaySpeed,-10, 0, false)); //ok nvm that fixed it lol. Down curve should be smoother.    and I'd maybe like it to trigger always on mouse on rather than wait. rolling mouse over should feel nice!

        StartCoroutine(Util.MoveToPos(new Vector2(0, shadowoffSetHeight), startPos, shadow0, quiz.curves.downCurve, animationPlaySpeed));
      //  StartCoroutine(Util.Spin(shadow0, quiz.UpCurve, animationPlaySpeed, 10, 0, false));
        StartCoroutine(Util.MoveToPos(new Vector2(-10, shadowoffSetHeight), startPos, shadow1, quiz.curves.downCurve, animationPlaySpeed));
        StartCoroutine(Util.Spin(shadow1, quiz.curves.downCurve, animationPlaySpeed, 10, 0, false));
        StartCoroutine(Util.MoveToPos(new Vector2(-20, shadowoffSetHeight), startPos, shadow2, quiz.curves.downCurve, animationPlaySpeed));
        StartCoroutine(Util.Spin(shadow2, quiz.curves.downCurve, animationPlaySpeed, 20, 0, false));

        StartCoroutine(AnimationGrace(false));
    }

    IEnumerator AnimationGrace(bool upWasPlayed)
    {
        animationGrace = true;
        yield return new WaitForSeconds(1 / animationPlaySpeed);
        animationGrace = false;
        if (pointerOn)
        {
            if (!upWasPlayed)
            { 
                PlayUpAnimation();
            }
        }
        else
        {
            if (upWasPlayed)
            {
                PlayDownAnimation();
            }
        }

    }



    //grace period. Turn off animations when animation starts playing.
    //then, when done (requires callback-like thing? or just end of coroutine, check if mousepointer is inside field (with bool set in enter/exit?) 
    //if true, then run opposite animation. both ways.

}
