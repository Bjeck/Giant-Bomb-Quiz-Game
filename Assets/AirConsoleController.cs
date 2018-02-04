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
