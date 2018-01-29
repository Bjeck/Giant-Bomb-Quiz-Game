using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Linq;

public class DeckGame : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler {

    public GameObject canvas;
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
    Vector2 offSetHeightPos = new Vector2(0,40f);
    float shadowoffSetHeight;


    public float animationPlaySpeed = 2;

    bool animationGrace = false;
    bool pointerOn = false;
    bool isDragging = false;
    

    public void Setup(Game gb, Sprite spr, QuizManager quiz)
    {
        canvas = GameObject.FindGameObjectWithTag("Canvas");

        this.quiz = quiz;

        gameIAm = gb;

        gameIMG.sprite = spr;
        spriteToUse = spr;

        startPos = ownRect.anchoredPosition;
    }



    public void OnBeginDrag(PointerEventData eventData)
    {
        //begin drag
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //check what we're on
        isDragging = false;

        GraphicRaycaster m_Raycaster;
        PointerEventData m_PointerEventData;
        EventSystem m_EventSystem;
        
        m_Raycaster = canvas.GetComponent<GraphicRaycaster>();
        m_EventSystem = canvas.GetComponent<EventSystem>();
        
        //Set up the new Pointer Event
        m_PointerEventData = new PointerEventData(m_EventSystem);
        m_PointerEventData.position = Input.mousePosition;
        
        List<RaycastResult> results = new List<RaycastResult>();

        m_Raycaster.Raycast(m_PointerEventData, results);
        foreach (RaycastResult result in results)
        {
            Debug.Log("Hit " + result.gameObject.name);
            if (result.gameObject.CompareTag("UIGame"))
            {
                GameComparison(result.gameObject.GetComponent<UIGame>().gameIAm);
            }
        }




    }

    public void GameComparison(Game g)
    {
        print("Comparison With " + g.name + " --- : ");
        string comparisonCorrect = "";
        foreach (KeyValuePair<QuizType, List<Feature>> lists in g.featureList)
        {
            if (g.featureList[lists.Key].Any(x => g.featureList[lists.Key].Any(y => y.Name == x.Name)))
            {
                comparisonCorrect += g.name + " also has " + lists.Key.ToString() + " ";

            }
            
        }
        print(comparisonCorrect);
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        //if (!quiz.quizOn)
        //{
        //    return;
        //}
        pointerOn = true;
        if (!animationGrace)
        {
            PlayUpAnimation();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
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

        StartCoroutine(Util.MoveToPos(startPos, new Vector2(0, 30), shadow0, quiz.curves.upCurve, animationPlaySpeed));
        StartCoroutine(Util.MoveToPos(startPos, new Vector2(0, 20), shadow1, quiz.curves.upCurve, animationPlaySpeed));
        StartCoroutine(Util.MoveToPos(startPos, new Vector2(0, 10), shadow2, quiz.curves.upCurve, animationPlaySpeed));
        
        StartCoroutine(AnimationGrace(true));
    }


    public void PlayDownAnimation()
    {
        //go down.
        StartCoroutine(Util.MoveToPos(offSetHeightPos, startPos, rectT, quiz.curves.downCurve, animationPlaySpeed));
        StartCoroutine(Util.MoveToPos(new Vector2(0, 30), startPos, shadow0, quiz.curves.downCurve, animationPlaySpeed));
        StartCoroutine(Util.MoveToPos(new Vector2(0, 20), startPos, shadow1, quiz.curves.downCurve, animationPlaySpeed));
        StartCoroutine(Util.MoveToPos(new Vector2(0, 10), startPos, shadow2, quiz.curves.downCurve, animationPlaySpeed));

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


}
