﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;


public enum QuizType { Platform, Character, KilledCharacter, Concept, Location, Object, Theme, People, Developer, Publisher, Genre, DLC, Name, Image }

public class QuizManager : MonoBehaviour {

    public bool MOBILEMODE = false;

    public Curves curves;

    public GBAPIHandler apiHandler;

    public Deck deck;
    public Fader fader;

 //   public AirConsoleController airconsole;

    public List<Game> currentGames = new List<Game>();
    
    public RectTransform canvasRect;
    public RectTransform quizObj;

    public UIGame[] uiGames = new UIGame[3];
    public CanvasGroup cGroup;

    public Text airconsoleStatusText;

    public TextMeshProUGUI question;
    public QuizType quizType;
    public Feature featureAskedAboutInQuestion;
    public Game gameImageWasFrom;
    public GameDescription desc;
    public Image questionImage;
    public TextMeshProUGUI mashupGameText;

    public RectTransform correctAnswerMarker;

    public Vector2 outOfScreenX;
    public Vector2 outOfScreenY;
    public TextMeshProUGUI rawText;
    public bool quizOn = false;

    public Sprite defaultGameImage;

    public Game mashupGame;

    public int wrongChoiceCounter = 0;

    public int markedAnswers = 0; 

    public Queue<QuizType> previouslyAskedQuestions = new Queue<QuizType>();

    bool progressToNextQuestion = false;

    // Use this for initialization
    void Start()
    {
        outOfScreenX = new Vector2(canvasRect.sizeDelta.x, 0);
        outOfScreenY = new Vector2(0, canvasRect.sizeDelta.y);
        quizObj.gameObject.SetActive(false);

        StartCoroutine(WaitToStart());
    }

    // Update is called once per frame
    void Update () {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            progressToNextQuestion = true;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            rawText.gameObject.SetActive(!rawText.gameObject.activeInHierarchy);
        }
    }

    //called when airconsole is ready
    public void ReadyToPlay()
    {
        print("ready to play!");
        airconsoleStatusText.text = "ready";
    }


    IEnumerator WaitToStart()
    {
        yield return new WaitForSeconds(1);
        BeginQuiz();

    }

    public void BeginGame()
    {
        if (MOBILEMODE)
        {
        //    airconsole.SetupGameStart();
        }
        BeginQuiz();
    }

    public void BeginQuiz()
    {
        //pick three games for quiz
        //currentGames.Add(gb.results);
        quizObj.gameObject.SetActive(true);
        cGroup.interactable = true;
        desc.Reset();
        quizOn = true;
        markedAnswers = 0;
        correctAnswerMarker.gameObject.SetActive(false);
        questionImage.gameObject.SetActive(false);

        List<Game> gamesToPickFrom = new List<Game>();
        gamesToPickFrom = apiHandler.pulledGames;
        currentGames.Clear();

        wrongChoiceCounter = 0;

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
        if(deck.games.Count > 0)
        {
           // CompareGameToCurGames(deck.games[deck.games.Count - 1].gameIAm);    
        }

        List<QuizType> validTypes = new List<QuizType>();
        
        validTypes = TestForFeatures();
        if(TestForImages()) { validTypes.Add(QuizType.Image); }
        
        QuizType typ = FindFeatureToAskAbout(validTypes);
        print(typ.ToString());

        AddTypeToPreviousTypes(typ);

        if (typ == QuizType.Image)   //noot great
        {
            DisplayImageQuiz();
        }
        else
        {
            DisplayFeatureQuiz(typ);

        }

        StartCoroutine(Util.MoveToPos(question.rectTransform.anchoredPosition - outOfScreenX, question.rectTransform.anchoredPosition, question.rectTransform, curves.moveCurve, 2));

        for (int i = 0; i < 3; i++)
        {
            float delay = Random.Range(0, 0.5f);
            StartCoroutine(Util.MoveToPos(uiGames[i].ownRect.anchoredPosition + outOfScreenX, uiGames[i].ownRect.anchoredPosition, uiGames[i].ownRect, curves.moveCurve, 1.2f, delay));
            StartCoroutine(Util.WaitToEnable(uiGames[i].ownRect.gameObject, delay));
        }
    }


    QuizType FindFeatureToAskAbout(List<QuizType> listOfValidTypes)
    {
        List<QuizType> removedPreviousQuestions = listOfValidTypes.Except(previouslyAskedQuestions).ToList();

        if(removedPreviousQuestions.Count == 0)
        {
            return listOfValidTypes[Random.Range(0, listOfValidTypes.Count)]; //return random. We've asked about all these things before...
        }
        else
        {
            return removedPreviousQuestions[Random.Range(0, removedPreviousQuestions.Count)]; //return random. We've asked about all these things before...
        }
    }

    void AddTypeToPreviousTypes(QuizType typ)
    {
        previouslyAskedQuestions.Enqueue(typ);

        if(previouslyAskedQuestions.Count > 5)
        {
            previouslyAskedQuestions.Dequeue();
        }
    }


    public void DisplayImageQuiz()
    {
        quizType = QuizType.Image;

        List<Game> gamesToGuessFrom = new List<Game>();
        foreach (Game r in currentGames)
        {
            if (r.images != null)
            {
                if(r.images.Count > 0)
                {
                    gamesToGuessFrom.Add(r);
                }
            }
        }

        Game g = gamesToGuessFrom[Random.Range(0, gamesToGuessFrom.Count)];

        string imgURL = g.images[Random.Range(0, g.images.Count)].medium_url;
        StartCoroutine(PullImage(imgURL));


        string q = FindQuestionString(quizType);
        q = string.Format(q, g.name);
        PopulateQuestion(q, currentGames);

        gameImageWasFrom = g;
    }

    IEnumerator PullImage(string url)
    {
        using (WWW www = new WWW(url))
        {
            yield return www;
            Texture2D texture = new Texture2D(www.texture.width, www.texture.height, TextureFormat.DXT1, false);
            www.LoadImageIntoTexture(texture);
            Rect rec = new Rect(0, 0, texture.width, texture.height);
            questionImage.sprite = Sprite.Create(texture, rec, new Vector2(0.5f, 0.5f), 100);
            questionImage.gameObject.SetActive(true);

            www.Dispose();
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

        int index = Random.Range(0, featuresToGuessFrom.Count);

        Feature theOne = featuresToGuessFrom[index];
        featureAskedAboutInQuestion = theOne;
        print("found " + featuresToGuessFrom.Count + " " + type.ToString() + "s Picking "+theOne.Name);

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
    
    public bool TestForImages()
    {
        foreach(Game r in currentGames){
            if(r.images != null){
                if(r.images.Count > 0)
                {
                    return true;
                }
            }
        }
        return false;
    }


    public void PopulateQuestion(string q, List<Game> games)
    {
        question.text = q;

        for (int i = 0; i < 3; i++)
        {
            uiGames[i].Setup(games[i]);
        }

        if (MOBILEMODE)
        {
         //   airconsole.MessageControllersGameNames(uiGames);
        }

    }
    
    public void MarkAnswerForPlayer(Player player, UIGame answer)
    {
        player.SetGame(answer);
        print("player " + player.id + " just marked " + answer.gameIAm.name + " as their answer");
        markedAnswers++;

        //if(markedAnswers == airconsole.players.Count)
        //{
        //    LockAnswers();
        //}
    }

    


    //public void LockAnswers()
    //{
    //    for (int i = 0; i < airconsole.amountOfPlayers; i++)
    //    {
    //        if (TestAnswer(airconsole.players[i].game))
    //        {
    //            //they got the right answer! wooh!
    //            print("player " + i + " got the right answer with " + airconsole.players[i].game.gameIAm.name);
    //        }
    //        else
    //        {
    //            print("player " + i + " guessed wrong with " + airconsole.players[i].game.gameIAm.name);
    //        }
    //    }
    //}


    public bool TestAnswer(UIGame game)
    {
        if (MOBILEMODE)
        {
            return TestAnswerMobile(game);
        }
        else
        {
            TestAnswerDesktop(game);
            return true;
        }
    }

    //OLD this needs to be reworked to test all answers for all players. OR PLACED IN single-player mode.
    public void TestAnswerDesktop(UIGame game)
    {
        if (quizType == QuizType.Image)
        {
            if (game.gameIAm.name == gameImageWasFrom.name)
            {
                DoCorrectAnswer(game);
            }
        }
        else
        {
            foreach (KeyValuePair<QuizType, List<Feature>> f in game.gameIAm.featureList)
            {
                if (f.Key == quizType)
                {
                    if (f.Value.Contains(featureAskedAboutInQuestion))
                    {
                        print("Correct answer!");
                        DoCorrectAnswer(game);
                        return;
                    }
                }
            }
            print("wrong answer");
            //if we get here, it was wrong answer
            DoWrongAnswer(game);
        }
    }

    public bool TestAnswerMobile(UIGame game)
    {
        if (quizType == QuizType.Image)
        {
            if (game.gameIAm.name == gameImageWasFrom.name)
            {
                return true;
            }
        }
        else
        {
            foreach (KeyValuePair<QuizType, List<Feature>> f in game.gameIAm.featureList)
            {
                if (f.Key == quizType)
                {
                    if (f.Value.Contains(featureAskedAboutInQuestion))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }


    public void DoCorrectAnswer(UIGame game)
    {
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

        EndQuiz(game.gameIAm, true);

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
        progressToNextQuestion = false;
        
        deck.AddCard(GetCorrectAnswer());
        
        if (isWin)
        {
            AddFeatureToMashup(quizType, g);
            UpdateMashupGame();
        }
        else
        {

        }

        StartCoroutine(EndQuizRoll());
    }

    IEnumerator EndQuizRoll()
    {
        UIGame cor = GetCorrectAnswer();

        questionImage.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.2f);

        StartCoroutine(Util.MoveToPos(question.rectTransform.anchoredPosition, question.rectTransform.anchoredPosition + outOfScreenX, question.rectTransform, curves.moveCurve, 2));

        StartCoroutine(Util.Scale(Vector3.one, Vector3.one * 13, cor.shadow2, curves.upCurve, 0.8f));
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(Util.Scale(Vector3.one, Vector3.one * 13, cor.shadow1, curves.upCurve, 0.7f));
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(Util.Scale(Vector3.one, Vector3.one * 13, cor.shadow0, curves.upCurve, 0.6f));

        for (int i = 0; i < 3; i++)
        {
            float delay = Random.Range(0.1f, 0.2f);
            if (uiGames[i] != cor)
            {
                StartCoroutine(Util.MoveToPos(uiGames[i].ownRect.anchoredPosition, uiGames[i].ownRect.anchoredPosition - outOfScreenX, uiGames[i].ownRect, curves.moveCurve, 1.2f, delay));
            }
        }

        yield return new WaitForSeconds(0.3f);

        fader.Fade(GameColor.green);

        yield return new WaitForSeconds(0.7f);

        StartCoroutine(Util.MoveToPos(desc.rect.anchoredPosition + outOfScreenX, desc.rect.anchoredPosition, desc.rect, curves.moveCurve, 2));
        desc.gameObject.SetActive(true);
        desc.FillText(GetCorrectAnswer().gameIAm);

        

        yield return new WaitForSeconds(0.5f);


        //for (int i = 0; i < 3; i++)
        //{
        //    uiGames[i].ownRect.gameObject.SetActive(false);
        //    uiGames[i].ownRect.anchoredPosition += outOfScreenX;
        //}
        yield return new WaitUntil(() => progressToNextQuestion == true);


        StartCoroutine(Util.MoveToPos(desc.rect.anchoredPosition, desc.rect.anchoredPosition + outOfScreenX, desc.rect, curves.moveCurve, 1.5f));

        StartCoroutine(Util.MoveToPos(cor.ownRect.anchoredPosition, cor.ownRect.anchoredPosition - outOfScreenY, cor.ownRect, curves.hangingUndershoot, 1.5f));

        yield return new WaitForSeconds(0.75f);


        for (int i = 0; i < 3; i++)
        {
            uiGames[i].ownRect.gameObject.SetActive(false);
            uiGames[i].ownRect.anchoredPosition += outOfScreenX;
        }

        fader.FadeOut(GameColor.green);

        desc.rect.anchoredPosition -= outOfScreenX;
        desc.gameObject.SetActive(false);
        question.rectTransform.anchoredPosition -= outOfScreenX;
        question.text = "";


        yield return new WaitForSeconds(0.8f);

        BeginQuiz();
    }
     

    public void UpdateMashupGame()
    {
        string text = "Mashup Game is currently a game that ";

        int i = mashupGame.featureList.Count;
        foreach(KeyValuePair<QuizType,List<Feature>> f in mashupGame.featureList)
        {
            if(f.Value != null)
            {
                if (f.Value.Count > 0)
                {
                    if(text != "Mashup Game is currently a game that ")
                    {
                        text += ", and ";
                    }
                    text += FindDescriptorString(f.Key) + " ";
                    foreach(Feature ff in f.Value)
                    {
                        text += ff.Name;
                    }
                }
            }
        }
        text += ".";
        mashupGameText.text = text;
    }

    
    public UIGame GetCorrectAnswer()
    {
        if (quizType == QuizType.Image)
        {
            foreach (UIGame g in uiGames)
            {
                if (g.gameIAm.name == gameImageWasFrom.name)
                {
                    return g;
                }
            }
        }
        else
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


    public void CompareGameToCurGames(Game game)
    {
        print("Comparison With "+game.name + " --- : ");
        string comparisonCorrect = "";
        foreach(Game g in currentGames)
        {
           // s += g.name +":";
            print(g.name);
            foreach (KeyValuePair<QuizType,List<Feature>> lists in g.featureList)
            {
                if(g.featureList[lists.Key].Any(x => game.featureList[lists.Key].Any(y => y.Name == x.Name)))
                {
                    comparisonCorrect += g.name + " also has " + lists.Key.ToString() + " ";
                    
                }

                string song = "["+lists.Key.ToString()+"] : ";
                if (g.featureList.ContainsKey(lists.Key))
                {
                    foreach (Feature f in g.featureList[lists.Key])
                    {
                        song += f.Name + ", ";
                    }
                }
                song += " --- --- ";
                if (game.featureList.ContainsKey(lists.Key))
                {
                    foreach (Feature f in game.featureList[lists.Key])
                    {
                        song += f.Name + ", ";
                    }
                }
                print(song);

            }
        }
        print(comparisonCorrect);
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
                s = "Which of these games has a {0} theme?";
                break;
            case QuizType.Developer:
                s = "Which of these games was made by {0}?";
                break;
            case QuizType.Publisher:
                s = "Which of these games was published by {0}?";
                break;
            case QuizType.People:
                s = "{0} worked on or has a connection to one of these games. Which one?";
                break;
            case QuizType.Genre:
                s = "Which of these games fall under the {0} Genre?";
                break;
            case QuizType.DLC:
                s = "The DLC {0} was for which game?";
                break;
            case QuizType.KilledCharacter:
                s = "{0} died in one of these games. Which one?";
                break;
            case QuizType.Image:
                s = "Which game is this image from?";
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
                s = "came out for the";
                break;
            case QuizType.Character:
                s = "stars";
                break;
            case QuizType.Location:
                s = "takes place in";
                break;
            case QuizType.Concept:
                s = "includes";
                break;
            case QuizType.Object:
                s = "has";
                break;
            case QuizType.Theme:
                s = "brings themes of";
                break;
            case QuizType.Developer:
                s = "was made by";
                break;
            case QuizType.Publisher:
                s = "published by";
                break;
            case QuizType.Genre:
                s = "is a";
                break;
            case QuizType.People:
                s = "with";
                break;
            case QuizType.DLC:
                s = "future DLC includes";
                break;
            case QuizType.KilledCharacter:
                s = "these characters will die:";
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
