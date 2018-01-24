using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PointVoucher.Plugin.LitJson;
using System.Linq;
using System.IO;


public class GBAPIHandler : MonoBehaviour {

    string APIkey = "bf0d917b2c66b1f7dab75d7807b70363508be5a2";

    public string GAMEURL = "http://www.giantbomb.com/api/game/3030-";
    public string GAMESURL = "http://www.giantbomb.com/api/games";

    public int MAXNR = 59518;
    public int amtOfGamesToPull = 10;

    string urlPost = "&format=JSON&limit=10&field_list=aliases,api_detail_url,characters,concepts,deck,developers,image,locations,name,objects,people,platforms,publishers,themes,expected_release_month,expected_release_quarter,expected_release_year,killed_characters,original_release_date,site_detail_url";
    //string urlPost = "&format=JSON&limit=10&field_list=api_detail_url,deck,name";
    //expected_release_day expected release moth/quarter/year  first appearance characters/concepts/locations/objects/people, killed_characters, original_release_date site_detail_url, 


    public string gamesJsonPath = "games";

    
    public List<Game> pulledGames = new List<Game>();

    
    // Use this for initialization
    void Start () {
        LoadGamesFromLocalFile();
        StartPullQueue(3, GotData);
    }
    

    public void LoadGamesFromLocalFile()
    {
        TextAsset asset = Resources.Load<TextAsset>("games");
      //  Debug.Log(asset.text);

        GameListLocal list = JsonMapper.ToObject<GameListLocal>(asset.text);
        if(list != null)
        {
            pulledGames.AddRange(list.Items);
        }

        print("loaded "+pulledGames.Count+" from json");

        for (int i = 0; i < pulledGames.Count; i++)
        {
            FillGame(pulledGames[i]);
        }

        
    }


    //public void PullManyFromGB(System.Action<bool, GBDataSingle> callback)
    //{
    //    int offset = Random.Range(0, (MAXNR - amtOfGamesToPull));
    //    StartCoroutine(PullFromGBCo(GAMESURL + "/?api_key=" + APIkey + "&offset=" + offset + urlPost, true, callback));
    //}

    public void GotData(bool b, GBDataSingle data)
    {
        FillGame(data.results);


        if (!pulledGames.Contains(data.results))
        {
            pulledGames.Add(data.results);
            StoreGameJson(data.results);
        }

    }


    public void PullSingleFromGB(System.Action<bool, GBDataSingle> callback)
    {
        StartCoroutine(PullFromGBCo(GAMEURL + Random.Range(1,MAXNR) + "/?api_key=" + APIkey + urlPost, false, callback));
    }

    public void StartPullQueue(float howMany, System.Action<bool, GBDataSingle> callback)
    {
        StartCoroutine(PullQueue(howMany, callback));
    }


    IEnumerator PullQueue(float howMany, System.Action<bool, GBDataSingle> callback)
    {
        for (int i = 0; i < howMany; i++)
        {
            PullSingleFromGB(callback);
            yield return new WaitForSeconds(1f);
        }
    }



    IEnumerator PullFromGBCo(string url, bool many, System.Action<bool, GBDataSingle> callback)
    {
      //  print("starting pull");
        using (WWW www = new WWW(url))
        {
            yield return www;
            if(www.error != null)
            {
                Debug.LogError("HTML ERROR " + www.error+" "+www.responseHeaders);

                yield break;
            }


         //   print(www.text);

            if(www.text.Contains("Object Not Found"))
            {
                Debug.LogError("Object not found!");
                yield break;
            }

            
            
            GBDataSingle data = new GBDataSingle();
            data = JsonMapper.ToObject<GBDataSingle>(www.text);
            if (ErrorHandling(data))
            {
                // pulledData.Add(data);
                //   print(data.error + " " + data.status_code);
                print(data.results.name);

                //  print(data.results.characters);

                callback(true, data);
            }
            // print(game.name);


            
        }
    }

    public void StoreGameJson(Game game)
    {
        //Write some text to the test.txt file
        //StreamWriter writer = new StreamWriter(gamesJsonPath, true);
        
        string json = JsonHelper.ToJson(pulledGames.ToArray());
        print("storing " + pulledGames.Count + " games");

        File.WriteAllText(gamesJsonPath, json);
        //  writer.Close();


#if UNITY_EDITOR
        //Re-import the file to update the reference in the editor
        UnityEditor.AssetDatabase.ImportAsset(gamesJsonPath);
#endif
        TextAsset asset = Resources.Load<TextAsset>("games");


        //Print the text from the file
       // Debug.Log(asset.text);

    }


    public bool ErrorHandling(GBData gbd)
    {

        if(gbd.error != "OK")
        {
            Debug.LogError("Error pulling data " + gbd.status_code);
            return false;
        }
        else
        {
            return true;
        }

    }



    public void FillGame(Game g)
    {
        g.featureList.Clear();

        if (g.platforms != null) { g.featureList.Add(QuizType.Platform, g.platforms.OfType<Feature>().ToList()); } 
        if (g.characters != null) { g.featureList.Add(QuizType.Character, g.characters.OfType<Feature>().ToList()); }
        if (g.concepts != null) { g.featureList.Add(QuizType.Concept, g.concepts.OfType<Feature>().ToList()); }
        if (g.locations != null) { g.featureList.Add(QuizType.Location, g.locations.OfType<Feature>().ToList()); }
        if (g.objects != null) { g.featureList.Add(QuizType.Object, g.objects.OfType<Feature>().ToList()); }
        if (g.themes != null) { g.featureList.Add(QuizType.Theme, g.themes.OfType<Feature>().ToList()); }
        if (g.people != null) { g.featureList.Add(QuizType.People, g.people.OfType<Feature>().ToList()); }
        if (g.developers != null) { g.featureList.Add(QuizType.Developer, g.developers.OfType<Feature>().ToList()); }
        if (g.publishers != null) { g.featureList.Add(QuizType.Publisher, g.publishers.OfType<Feature>().ToList()); }
    }



    public string GetDate(Game g)
    {
        string date = ""; //format of original release date is: 1999-01-14 00:00:00
        if (!string.IsNullOrEmpty(g.original_release_date))
        {
            date = g.original_release_date.Split(' ')[0];
            
        }
        else
        {
            //game is not out yet/doesn't have a release date. handle that differently.

            date = "Date Unknown";
        }

        return date;
    }

}


//expected_release_day expected release moth/quarter/year  first appearance characters/concepts/locations/objects/people, killed_characters, original_release_date site_detail_url, 


[System.Serializable]
public class GBData
{
    public string error;
    public int limit;
    public int offset;
    public int status_code;
}

[System.Serializable]
public class GBDataSingle : GBData
{
    public Game results = new Game();
}

[System.Serializable]
public class GBDataMany : GBData
{
    public List<Game> results = new List<Game>();
}


public class GameListLocal
{
    public List<Game> Items = new List<Game>();
}

[System.Serializable]
public class Game
{
    public object aliases;
    public string api_detail_url;
    public string site_detail_url;
    public string deck;
    public GBImage image;
    public string name;
    public List<Platform> platforms = new List<Platform>();
    public List<Character> characters = new List<Character>();
    public List<Concept> concepts = new List<Concept>();
    public List<Location> locations = new List<Location>();
    public List<Obj> objects = new List<Obj>();
    public List<Theme> themes = new List<Theme>();
    public List<Person> people = new List<Person>();
    public List<Developer> developers = new List<Developer>();
    public List<Publisher> publishers = new List<Publisher>();

    public List<FirstAppearancePerson> first_appearance_people = new List<FirstAppearancePerson>();
    public List<FirstAppearanceCharacter> first_appearance_characters = new List<FirstAppearanceCharacter>();
    public List<FirstAppearanceConcept> first_appearance_concepts = new List<FirstAppearanceConcept>();
    public List<FirstAppearanceObject> first_appearance_objects = new List<FirstAppearanceObject>();
    public List<FirstAppearanceLocation> first_appearance_locations = new List<FirstAppearanceLocation>();

    public List<KilledCharacter> killed_characters = new List<KilledCharacter>();
    public List<DLC> dlcs = new List<DLC>();

    public object expected_release_day;
    public object expected_release_month;
    public object expected_release_quarter;
    public object expected_release_year;
    public string original_release_date;

    [JsonIgnore]
    public Dictionary<QuizType, List<Feature>> featureList = new Dictionary<QuizType, List<Feature>>();
    //url,characters,concepts,deck,developers,image,locations,name,objects,people,platforms,publishers,themes";

}





[System.Serializable]
public abstract class Feature
{
   // private string name;
    // public string name;
    public abstract string Name { get; set; }
    
}

[System.Serializable]
public class Character : Feature
{
    public string api_detail_url;
    public string name; //this looks silly, but it's because the json needs load into the variable (public) and I need to access name correctly from the abstract class (hence the Name property)
    public override string Name { get { return name; } set { name = value; } }

}

[System.Serializable]
public class Platform : Feature
{
    public string api_detail_url;
    public string name;
    public override string Name { get { return name; } set { name = value; } }

}

[System.Serializable]
public class Concept : Feature
{
    public string api_detail_url;
    public string name;
    public override string Name { get { return name; } set { name = value; } }

}

[System.Serializable]
public class Location : Feature
{
    public string api_detail_url;
    public string name;
    public override string Name { get { return name; } set { name = value; } }
}

[System.Serializable]
public class Developer : Feature
{
    public string api_detail_url;
    public string name;
    public override string Name { get { return name; } set { name = value; } }
}

[System.Serializable]
public class Obj : Feature
{
    public string api_detail_url;
    public string name;
    public override string Name { get { return name; } set { name = value; } }
}

[System.Serializable]
public class Person : Feature
{
    public string api_detail_url;
    public string name;
    public override string Name { get { return name; } set { name = value; } }
}

[System.Serializable]
public class Publisher : Feature
{
    public string api_detail_url;
    public string name;
    public override string Name { get { return name; } set { name = value; } }
}

[System.Serializable]
public class Theme : Feature
{
    public string api_detail_url;
    public string name;
    public override string Name { get { return name; } set { name = value; } }
}

[System.Serializable]
public class GBImage
{
    public string icon_url;
    public string medium_url;
    public string screen_url;
    public string screen_large_url;
    public string small_url;
    public string super_url;
    public string thumb_url;
    public string tiny_url;
    public string original_url;
    public string image_tags;
}

[System.Serializable]
public class FirstAppearanceConcept
{
    public string api_detail_url;
    public int id;
    public string name;
    public string site_detail_url;
}

[System.Serializable]
public class FirstAppearancePerson
{
    public string api_detail_url;
    public int id;
    public string name;
    public string site_detail_url;
}

[System.Serializable]
public class FirstAppearanceCharacter
{
    public string api_detail_url;
    public int id;
    public string name;
    public string site_detail_url;
}

[System.Serializable]
public class FirstAppearanceObject
{
    public string api_detail_url;
    public int id;
    public string name;
    public string site_detail_url;
}

[System.Serializable]
public class FirstAppearanceLocation
{
    public string api_detail_url;
    public int id;
    public string name;
    public string site_detail_url;
}

[System.Serializable]
public class KilledCharacter
{
    public string api_detail_url;
    public string name;
  //  public override string Name { get { return name; } set { name = value; } }
}

public class DLC
{
    public string api_detail_url { get; set; }
    public int id { get; set; }
    public string name { get; set; }
    public string site_detail_url { get; set; }
}


public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}