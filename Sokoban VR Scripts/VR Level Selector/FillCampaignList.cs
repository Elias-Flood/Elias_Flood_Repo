using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class FillCampaignList : MonoBehaviour
{
    [SerializeField] GameManager gameManager;

    private DataManager _myDataManager;
    [SerializeField] private int _displayedListElement;
    private int _listCount;

    [SerializeField] GameObject MenuCanvas;
    [SerializeField] GameObject MenuElement;

    private void Start()
    {
        _myDataManager = FindObjectOfType<DataManager>();
        _listCount = _myDataManager.Campaigns.Count-1;
        _displayedListElement = 0;

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            ChangeCampaignElement(0);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            NextOnList();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            PreviousOnList();
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            SendAndStartNewCampaign();
        }
    }
    public void DestroyAllElements()
    {
        for (int i = 0; i < MenuCanvas.transform.childCount; i++)
        {
            GameObject.Destroy(MenuCanvas.transform.GetChild(i).gameObject);
        }
    }

    public void ChangeCampaignElement(int ShowFromList)
    {
        Sprite _newThumbnail = Sprite.Create(_myDataManager.Campaigns[ShowFromList].thumbnail, new Rect(0, 0, _myDataManager.Campaigns[ShowFromList].thumbnail.width, _myDataManager.Campaigns[ShowFromList].thumbnail.height), new Vector2(0.5f, 0.5f));
        MenuElement.transform.GetChild(1).GetComponent<Image>().sprite = _newThumbnail;

        MenuElement.transform.GetChild(2).GetComponent<TMP_Text>().text = _myDataManager.Campaigns[ShowFromList].title;

        MenuElement.transform.GetChild(3).GetComponent<TMP_Text>().text = _myDataManager.Campaigns[ShowFromList].description;

        MenuElement.transform.GetChild(4).GetComponent<TMP_Text>().text = string.Format("Levels: {0}", _myDataManager.Campaigns[ShowFromList].puzzleMaps.Count);
    }

    public void TogglePlayButton(bool activeStatus)
    {
        MenuElement.transform.GetChild(5).gameObject.SetActive(activeStatus);
    }

    public void NextOnList()
    {
        ++_displayedListElement;
        if(_displayedListElement > _listCount)
            _displayedListElement = 0;
        ChangeCampaignElement(_displayedListElement);
    }
    public void PreviousOnList()
    {
        --_displayedListElement;
        if (_displayedListElement < 0)
            _displayedListElement = _listCount;
        ChangeCampaignElement(_displayedListElement);
    }

    public void SendAndStartNewCampaign()
    {
        int Puzzels = gameManager.PuzzleTextures.Length;

        gameManager.PuzzleTextures = _myDataManager.Campaigns[_displayedListElement].puzzleMaps.ToArray();

        /*for (int i = 0; i < _myDataManager.Campaigns[_displayedListElement].puzzelMaps.Count; i++)
        {
            gameManager.PuzzleTextures[i] = _myDataManager.Campaigns[_displayedListElement].puzzelMaps[i];
        }*/

        SceneManager.LoadScene(1);
    }
}
