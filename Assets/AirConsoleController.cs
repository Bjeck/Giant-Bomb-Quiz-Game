using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NDream.AirConsole;
using Newtonsoft.Json.Linq;
using System;

public class AirConsoleController : MonoBehaviour {

    public QuizManager quiz;

    public int amountOfPlayers;

    void Start()
    {
        AirConsole.instance.onMessage += OnMessage;
        AirConsole.instance.onConnect += OnConnectDevice;
        AirConsole.instance.onDisconnect += OnDisconnectDevice;
    }


    private void OnConnectDevice(int device_id)
    {
        amountOfPlayers++;
        if (AirConsole.instance.GetActivePlayerDeviceIds.Count == 0)
        {
            if (AirConsole.instance.GetControllerDeviceIds().Count >= 1)
            {
                // can now start game 
                //
            }
        }
    }
    
    private void OnDisconnectDevice(int device_id)
    {
        amountOfPlayers--;
        int active_player = AirConsole.instance.ConvertDeviceIdToPlayerNumber(device_id);
        print(active_player);
        if (active_player != -1)
        {
            if (AirConsole.instance.GetControllerDeviceIds().Count >= 2)
            {
               // StartGame(); ??
            }
            else
            {
                // uiText.text = "PLAYER LEFT - NEED MORE PLAYERS";
            }
        }
        
    }

    public void SetupGameStart()
    {
        AirConsole.instance.SetActivePlayers(amountOfPlayers); //count
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


    void OnMessage(int device_id, JToken data)
    {
        int active_player = AirConsole.instance.ConvertDeviceIdToPlayerNumber(device_id);

        print("got thing from player " + data.ToString() + " " + active_player);

        if (active_player != -1)
        {
            TranslateInputToGame(data);
        }
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
