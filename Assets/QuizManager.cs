using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;


public enum QuizType { Platform, Character, Concept, Location, Object, Theme, People, Developer, Publisher, Name }

public class QuizManager : MonoBehaviour {

    public Curves curves;

    public GBAPIHandler apiHandler;
    public List<Game> currentGames = new List<Game>();

    public RectTransform canvasRect;
    public RectTransform quizObj;

    public UIGame[] uiGames = new UIGame[3];
    public CanvasGroup cGroup;

    public TextMeshProUGUI question;
    public QuizType quizType;
    public Feature featureAskedAboutInQuestion;
    public GameDescription desc;
    public TextMeshProUGUI mashupGameText;

    public RectTransform correctAnswerMarker;

    public Vector2 outOfScreenX;
    public Vector2 outOfScreenY;
    public TextMeshProUGUI rawText;
    public bool quizOn = false;

    public Sprite defaultGameImage;

    public Game mashupGame;

    public int wrongChoiceCounter = 0;
    
	// Use this for initialization
	void Start () {
        outOfScreenX = new Vector2(canvasRect.sizeDelta.x, 0);
        outOfScreenY = new Vector2(0, canvasRect.sizeDelta.y);
        quizObj.gameObject.SetActive(false);
        
    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.Space) && !quizOn)
        {
            BeginQuiz();
        }
	}
    


    public void BeginQuiz()
    {
        //pick three games for quiz
        //currentGames.Add(gb.results);
        quizObj.gameObject.SetActive(true);
        cGroup.interactable = true;
        desc.Reset();
        quizOn = true;
        correctAnswerMarker.gameObject.SetActive(false);

        List<Game> gamesToPickFrom = new List<Game>();
        gamesToPickFrom = apiHandler.pulledGames;
        currentGames.Clear();

        int[] rands = new int[3];
        for (int i = 0; i < 3; i++)
        {
            rands[i] = Random.Range(0, gamesToPickFrom.Count);
            currentGames.Add(gamesToPickFrom[rands[i]]);
            gamesToPickFrom.Remove(gamesToPickFrom[rands[i]]);

            uiGames[i].ownRect.gameObject.SetActive(false);
            uiGames[i].ownRect.anchoredPosition = uiGames[i].startPos;
            uiGames[i].rectT.anchoredPosition = uiGames[i].startPos;
            uiGames[i].gameIMG.sprite = defaultGameImage;
        }

        DisplayQuiz();
    }


    public void DisplayQuiz()
    {
        ShowRawDebugText();

        List<QuizType> validTypes = new List<QuizType>();
        
        validTypes = TestForFeatures();
        
        print("Can Pick from " + validTypes.Count + " categories");


        QuizType typ = validTypes[Random.Range(0, validTypes.Count)];
        //need to have some check in that these games can actually do this. so if the three games have no platforms, don't pick platform. etc.
        
        print(typ.ToString());

        DisplayFeatureQuiz(typ);

        StartCoroutine(Util.MoveToPos(question.rectTransform.anchoredPosition - outOfScreenX, question.rectTransform.anchoredPosition, question.rectTransform, curves.moveCurve, 2));

        for (int i = 0; i < 3; i++)
        {
            float delay = Random.Range(0, 0.5f);
            StartCoroutine(Util.MoveToPos(uiGames[i].ownRect.anchoredPosition + outOfScreenX, uiGames[i].ownRect.anchoredPosition, uiGames[i].ownRect, curves.moveCurve, 1.2f, delay));
            StartCoroutine(Util.WaitToEnable(uiGames[i].ownRect.gameObject, delay));
        }


    }


    public void DisplayFeatureQuiz(QuizType type)
    {
        quizType = type;

        List<Feature> featuresToGuessFrom = new List<Feature>();
        //        featuresToGuessFrom.AddRange()

        foreach (Game r in currentGames)
        {
            if (r.featureList.ContainsKey(type))
            {
                featuresToGuessFrom.AddRange(r.featureList[type]);
            }
        }
        print("found " + featuresToGuessFrom.Count + " " + type.ToString() + "s Picking one.");

        int index = Random.Range(0, featuresToGuessFrom.Count);

        Feature theOne = featuresToGuessFrom[index];
        featureAskedAboutInQuestion = theOne;

        print("Picked " + theOne.Name);

        string q = FindQuestionString(quizType);
        q = string.Format(q, theOne.Name);
        PopulateQuestion(q, currentGames);
    }


    public List<QuizType> TestForFeatures()
    {
        List<QuizType> typesToReturn = new List<QuizType>();

        foreach (Game r in currentGames)
        {
            foreach (KeyValuePair<QuizType, List<Feature>> kvp in r.featureList)
            {
                if (kvp.Value != null)
                {
                    if(kvp.Value.Count > 0)
                    {
                        if (!typesToReturn.Contains(kvp.Key))
                        {
                            typesToReturn.Add(kvp.Key);
                        }
                    }
                }
            }
        }
        return typesToReturn;
    }
    


    public void PopulateQuestion(string q, List<Game> games)
    {
        question.text = q;

        for (int i = 0; i < 3; i++)
        {
            uiGames[i].Setup(games[i]);
        }

    }
    

    public void TestAnswer(UIGame game)
    {
        print("Test answer");

        foreach(KeyValuePair<QuizType,List<Feature>> f in game.gameIAm.featureList)
        {
            if(f.Key == quizType)
            {
                if (f.Value.Contains(featureAskedAboutInQuestion))
                {
                    print("Correct answer!");
                    DoCorrectAnswer(game);
                    return;
                }
            }
        }
        //if we get here, it was wrong answer
        DoWrongAnswer(game);
    }

    
    public void DoCorrectAnswer(UIGame game)
    {
        EndQuiz(game.gameIAm, true);

        foreach (UIGame g in uiGames)
        {
            if(g == game)
            {
                g.Win(true);
            }
            else
            {
                g.Win(false);
            }
        }
        // correctAnswerMarker.gameObject.SetActive(true);
        // correctAnswerMarker.anchoredPosition = game.GetComponent<RectTransform>().anchoredPosition;


    }

    public void DoWrongAnswer(UIGame selectedGame)
    {
        selectedGame.Lose(true);

        if (wrongChoiceCounter >= 1)
        {
            EndQuiz(selectedGame.gameIAm, false);
        }
        else
        {
            wrongChoiceCounter++;
        }

    }


    public void EndQuiz(Game g, bool isWin)
    {
        cGroup.interactable = false;
        quizOn = false;

        desc.FillText(GetCorrectAnswer().gameIAm);
        StartCoroutine(Util.MoveToPos(desc.rect.anchoredPosition + outOfScreenX, desc.rect.anchoredPosition, desc.rect, curves.moveCurve, 2));
        
        if (isWin)
        {
            AddFeatureToMashup(quizType, g);
            UpdateMashupGame();
        }
        else
        {

        }
    }


    public void UpdateMashupGame()
    {
        string text = "";

        foreach(KeyValuePair<QuizType,List<Feature>> f in mashupGame.featureList)
        {
            if(f.Value != null)
            {
                if (f.Value.Count > 0)
                {
                    text += FindDescriptorString(f.Key) + " ";
                    foreach(Feature ff in f.Value)
                    {
                        text += ff.Name + ",";
                    }
                    text += " and ";
                }
            }
        }


        mashupGameText.text = text;
    }




    public UIGame GetCorrectAnswer()
    {
        for (int i = 0; i < 3; i++)
        {
            foreach (KeyValuePair<QuizType, List<Feature>> f in uiGames[i].gameIAm.featureList)
            {
                if (f.Key == quizType)
                {
                    if (f.Value.Contains(featureAskedAboutInQuestion))
                    {
                        return uiGames[i];
                    }
                }
            }
        }
        Debug.LogError("Found No Correct answer. Something must have gone wrong");
        return null;
    }


    //DEBUG - ----------------------------


    public void ShowRawDebugText()
    {
        rawText.text = "Games shown:\n";
        foreach (Game g in currentGames)
        {

            rawText.text += g.name;
            
            if (g.objects != null)
            {
                rawText.text += "| objs: " + g.objects.Count;
            }
            
            if (g.characters != null)
            {
                rawText.text += "| chars: " + g.characters.Count;
            }
            
            if (g.concepts != null)
            {
                rawText.text += "| concs: " + g.concepts.Count;
            }
            
            if (g.locations != null)
            {
                rawText.text += "| locs: " + g.locations.Count;
            }
            
            if (g.themes != null)
            {
                rawText.text += "| thms: " + g.themes.Count;
            }
            
            if (g.platforms != null)
            {
                rawText.text += "| plats: " + g.platforms.Count;
            }
            
            if (g.people != null)
            {
                rawText.text += "| ppl: " + g.people.Count;
            }

            if (g.developers != null)
            {
                rawText.text += "| devs: " + g.developers.Count;
            }

            if (g.publishers != null)
            {
                rawText.text += "| pubs: " + g.publishers.Count;
            }

            //      string urlPost = aliases,api_detail_url,characters,concepts,deck,developers,image,locations,name,objects,people,platforms,publishers,themes";


            rawText.text += "\n";
        }

    }



    public string FindQuestionString(QuizType type)
    {
        string s = "";
        switch (type)
        {
            case QuizType.Platform:
                s = "Which of these games came out for the {0}?";
                break;
            case QuizType.Character:
                s = "In which of these games is {0} a character?";
                break;
            case QuizType.Location:
                s = "Which one of these games take place in {0}?";
                break;
            case QuizType.Concept:
                s = "One of these games use {0}. Which one?";
                break;
            case QuizType.Object:
                s = "Which of these games has a {0}?";
                break;
            case QuizType.Theme:
                s = "Which of these games is about {0}?";
                break;
            case QuizType.Developer:
                s = "Which of these games was made by {0}?";
                break;
            case QuizType.Publisher:
                s = "Which of these games was published by {0}?";
                break;
            case QuizType.People:
                s = "{0} has something to do with one of these games. Which one?";
                break;
        }

        return s;
    }


    public string FindDescriptorString(QuizType type)
    {
        string s = "";
        switch (type)
        {
            case QuizType.Platform:
                s = "Came out for the";
                break;
            case QuizType.Character:
                s = "Has the great characters of";
                break;
            case QuizType.Location:
                s = "Takes place in";
                break;
            case QuizType.Concept:
                s = "Includes these wondrous concepts:";
                break;
            case QuizType.Object:
                s = "Has these objects:";
                break;
            case QuizType.Theme:
                s = "has themes of";
                break;
            case QuizType.Developer:
                s = "was made by";
                break;
            case QuizType.Publisher:
                s = "published by";
                break;
            case QuizType.People:
                s = "these people touched it at least";
                break;
        }

        return s;
    }



    public QuizType FeatureToType(Feature f) //not used, dunnot what to use it for yet...
    {
        QuizType q;

        if (f is Platform)
        {
            q = QuizType.Platform;
        }
        else if (f is Character)
        {
            q = QuizType.Character;
        }
        else if (f is Concept)
        {
            q = QuizType.Concept;
        }
        else if (f is Location)
        {
            q = QuizType.Location;
        }
        else if (f is Obj)
        {
            q = QuizType.Object;
        }
        else if (f is Theme)
        {
            q = QuizType.Theme;
        }
        else if (f is Person)
        {
            q = QuizType.People;
        }
        else if (f is Developer)
        {
            q = QuizType.Developer;
        }
        else if (f is Publisher)
        {
            q = QuizType.Publisher;
        }
        else
        {
            q = QuizType.Name;
        }
        return q;
    }

    //public List<Feature> TypeToFeature(QuizType q, Game g)
    //{
    //    Feature f;

    //    switch (q)
    //    {
    //        case QuizType.Platform:
    //            return g.platforms.OfType<Feature>().ToList();
    //            break;
    //    }
    //}


    public void AddFeatureToMashup(QuizType q, Game g)
    {
        switch (q)
        {
            case QuizType.Platform:
                mashupGame.platforms.Add(featureAskedAboutInQuestion as Platform);
                break;
            case QuizType.Character:
                mashupGame.characters.Add(featureAskedAboutInQuestion as Character);
                break;
            case QuizType.Concept:
                mashupGame.concepts.Add(featureAskedAboutInQuestion as Concept);
                break;
            case QuizType.Developer:
                mashupGame.developers.Add(featureAskedAboutInQuestion as Developer);
                break;
            case QuizType.Location:
                mashupGame.locations.Add(featureAskedAboutInQuestion as Location);
                break;
            case QuizType.Object:
                mashupGame.objects.Add(featureAskedAboutInQuestion as Obj);
                break;
            case QuizType.People:
                mashupGame.people.Add(featureAskedAboutInQuestion as Person);
                break;
            case QuizType.Publisher:
                mashupGame.publishers.Add(featureAskedAboutInQuestion as Publisher);
                break;
            case QuizType.Theme:
                mashupGame.themes.Add(featureAskedAboutInQuestion as Theme);
                break;
        }
        apiHandler.FillGame(mashupGame);
    }



}
