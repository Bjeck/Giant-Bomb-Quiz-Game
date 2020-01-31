//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using NDream.AirConsole;
//using Newtonsoft.Json.Linq;
//using System;

//public class AirConsoleController : MonoBehaviour {

//    [SerializeField] GameObject playerPrefab;
//    public QuizManager quiz;

//    public int amountOfPlayers;

//    public List<Player> players = new List<Player>();


//    void Start()
//    {
//        if (!quiz.MOBILEMODE)
//        {
//            Destroy(AirConsole.instance.gameObject);
//        }
//        AirConsole.instance.onMessage += OnMessage;
//        AirConsole.instance.onConnect += OnConnectDevice;
//        AirConsole.instance.onDisconnect += OnDisconnectDevice;
//        AirConsole.instance.onReady += OnAirConsoleReady;
//    }

//    private void OnAirConsoleReady(string code)
//    {
//        quiz.ReadyToPlay();
//    }

//    private void OnConnectDevice(int device_id)
//    {
//        amountOfPlayers++;
//        if (AirConsole.instance.GetActivePlayerDeviceIds.Count == 0)
//        {
//            if (AirConsole.instance.GetControllerDeviceIds().Count >= 1)
//            {
//                // can now start game 
//                // put up a prompt that we can.
//            }
//        }
//    }
    
//    private void OnDisconnectDevice(int device_id)
//    {
//        amountOfPlayers--;
//        int active_player = AirConsole.instance.ConvertDeviceIdToPlayerNumber(device_id);
//        print(active_player);
//        if (active_player != -1)
//        {
//            if (AirConsole.instance.GetControllerDeviceIds().Count == 0)
//            {
//                //probably pop something that says no device is connected..
//            }
//        }

//        //remove player from playerlist

//        if(players.Exists(x=>x.id == active_player)){       //TODO: maybe I shouldn't destroy them immediately. disconnect and wait for them to come back and store their data.
//            Player p = players.Find(x => x.id == active_player);
//            players.Remove(p);
//            Destroy(p.gameObject);
//        }

//    }

//    public void SetupGameStart()
//    {
//        AirConsole.instance.SetActivePlayers(amountOfPlayers); //count
//        print("started");
//        for (int i = 0; i < amountOfPlayers; i++)
//        {
//            GameObject playerObj = Instantiate(playerPrefab);
//            Player p = playerObj.GetComponent<Player>();
//            p.SetID(i);
//            players.Add(p);
//        }

//    }



//    public void MessageControllersGameNames(UIGame[] games)
//    {
//        var message = new
//        {
//            game1 = games[0].gameIAm.name,
//            game2 = games[1].gameIAm.name,
//            game3 = games[2].gameIAm.name
//        };

//        AirConsole.instance.Broadcast(message);
//    }

//    public void MessageControllers()
//    {

//        var message = new
//        {
//        };

//        AirConsole.instance.Message(1, message);
//    }


//    void OnMessage(int device_id, JToken data)
//    {
//        int active_player = AirConsole.instance.ConvertDeviceIdToPlayerNumber(device_id);

//        print("got thing from player " + data.ToString() + " " + active_player + " device id: "+device_id);

//        if (active_player != -1)
//        {
//            TranslateInputToGame(device_id, data);
//        }
//    }

//    void TranslateInputToGame(int device_id, JToken data)
//    {
//        PickClass pc = data.ToObject<PickClass>();
//        print(pc.gameNr);
//        quiz.MarkAnswerForPlayer(players[AirConsole.instance.ConvertDeviceIdToPlayerNumber(device_id)], quiz.uiGames[pc.gameNr - 1]);
//        //quiz.TestAnswer(quiz.uiGames[pc.gameNr - 1]);
//    }


//}

//public class PickClass
//{
//    public int gameNr;
//}
