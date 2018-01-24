using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class GameDescription : MonoBehaviour, IPointerClickHandler {

    [SerializeField] QuizManager quiz;
    public RectTransform rect;
    TextMeshProUGUI descText;
    Canvas canvas;
    Camera camera;
    Game gameLink;

    void Awake()
    {
        descText = gameObject.GetComponent<TextMeshProUGUI>();
        canvas = gameObject.GetComponentInParent<Canvas>();
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            camera = null;
        }
        else
        {
            camera = canvas.worldCamera;
        }
    }

    public void Reset()
    {
        descText.text = "";
    }

    public void FillText(Game g)
    {
        gameLink = g;
        descText.text = g.name + " (" + quiz.apiHandler.GetDate(g) + ")\n" + g.deck + "\n" + "Read more about the game " + "<color=#0000FF><link=\"gameURL\">" + "here" + "</link></color>";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(descText, Input.mousePosition, camera);
        print("Clicked ");

        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = descText.textInfo.linkInfo[linkIndex];
            print(linkInfo.GetLinkID());

            switch (linkInfo.GetLinkID())
            {
                case "gameURL":
                    Application.OpenURL(gameLink.site_detail_url);
                    break;
            }
        }
    }

}

/*
 *  public class MyTextMeshLinkHandler : MonoBehaviour, IPointerClickHandler
 {
     TextMeshProUGUI textMeshPro;
     Canvas canvas;
     Camera camera;
 
     void Awake ()
     {
         textMeshPro = gameObject.GetComponent<TextMeshProUGUI> ();
         canvas = gameObject.GetComponentInParent<Canvas> ();
         if (canvas.renderMode == RenderMode.ScreenSpaceOverlay) {
             camera = null;
         } else {
             camera = canvas.worldCamera;
         }
     }
 
     public void OnPointerClick (PointerEventData eventData)
     {
         int linkIndex = TMP_TextUtilities.FindIntersectingLink (textMeshPro, Input.mousePosition, camera);
         if (linkIndex != -1) {
             TMP_LinkInfo linkInfo = textMeshPro.textInfo.linkInfo [linkIndex];
             switch (linkInfo.GetLinkID ()) {
             case "id_1":
                 Application.OpenURL ("http://answers.unity3d.com");
                 break;
             case "id_2":
                 //Do something else
                 break;
             }
         }
     }
 }

    */