using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//dir
//  meta.txt
//      puzzelcampaign
//      <title>
//      <description>
//  thumbnail.png (512x512px) exempel
//  puzzel_1.png (5x5px)
//  puzzel_2.png
//  puzzel_n.png

public class PuzzleData
{
    public string title;
    public string description;
    public Texture2D thumbnail;
    public List<Texture2D> puzzleMaps = new List<Texture2D>();
}

public class DataManager : MonoBehaviour
{
    public List<PuzzleData> Campaigns = new List<PuzzleData>();

    private void Awake()
    {
        //Directory.GetDirectories(Application.persistentDataPath + "/Campaigns");
        Debug.Log(Application.persistentDataPath + "/Campaigns");
        string[] dataPaths = { Application.persistentDataPath + "/Campaigns" /*"/Users/pette/AppData/LocalLow/Zero Index/SokobanVR/Campaigns"*/ };

        foreach (string path in dataPaths)
        {
            if (Directory.Exists(path))
            {
                string[] dirs = Directory.GetDirectories(path);

                foreach (string dir in dirs)
                {
                    if (File.Exists($"{dir}/meta.txt"))
                    {
                        string[] meta = File.ReadAllLines($"{dir}/meta.txt");
                        PuzzleData campaign = new PuzzleData();
                        if (meta[0] == "puzzlecampaign")
                        {
                            campaign.title = meta[1];
                            campaign.description = meta[2];

                            Texture2D thumbnail = new Texture2D(512, 512);
                            if (File.Exists($"{dir}/thumbnail.png"))
                                thumbnail.LoadImage(File.ReadAllBytes($"{dir}/thumbnail.png"));

                            campaign.thumbnail = thumbnail;

                            foreach (string file in Directory.GetFiles(dir))
                            {
                                if (campaign.puzzleMaps.Count >= 30)
                                    break; // max 30 puzzles in one campaign for example

                                if (Path.GetExtension(file) == ".png" && Path.GetFileNameWithoutExtension(file).Contains("puzzle_"))
                                {
                                    Texture2D puzzle = new Texture2D(5, 5);
                                    puzzle.LoadImage(File.ReadAllBytes(file));
                                    campaign.puzzleMaps.Add(puzzle);
                                    Debug.Log(campaign.puzzleMaps.Count);
                                }

                            }
                        }

                        if (!string.IsNullOrEmpty(campaign.title))
                            Campaigns.Add(campaign); Debug.Log("Campaign Added");
                    }
                }
            }
        }
    }
}