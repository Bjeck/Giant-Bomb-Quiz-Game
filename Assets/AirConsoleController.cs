using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NDream.AirConsole;
using Newtonsoft.Json.Linq;

public class AirConsoleController : MonoBehaviour {

    public QuizManager quiz;


    void Start()
    {
        AirConsole.instance.onMessage += OnMessage;
    }

    void OnMessage(int from, JToken data)
    {
        print("got thing "+data.ToString());
        AirConsole.instance.Message(from, "Full of pixels!");


        TranslateInputToGame(data);

    }

    public void MessageControllersGameNames(UIGame[] games)
    {
        var message = new
        {
            game1 = games[0].gameIAm.name,
            game2 = games[1].gameIAm.name,
            game3 = games[2].gameIAm.name
        };

        AirConsole.instance.Broadcast(message);
    }

    public void MessageControllers()
    {

        var message = new
        {
        };

        AirConsole.instance.Message(1, message);
    }


    void TranslateInputToGame(JToken data)
    {
        PickClass pc = data.ToObject<PickClass>();
        print(pc.gameNr);
        quiz.TestAnswer(quiz.uiGames[pc.gameNr - 1]);
    }


}

public class PickClass
{
    public int gameNr;
}
