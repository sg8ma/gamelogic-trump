using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HighLowSceneDirector : MonoBehaviour
{
    [SerializeField] CardsDirector cardsDirector;
    [SerializeField] GameObject buttonHigh;
    [SerializeField] GameObject buttonLow;
    [SerializeField] Text textInfo;
    List<CardController> cards;
    int cardIndex;
    int winCount;
    const float NextWaitTimer = 1.0f;

    void Start()
    {
        cards = cardsDirector.GetHighLowCards();
        for(int i = 0; i < cards.Count; i++)
        {
            cards[i].transform.position = new Vector3(0, 0, 0.15f);
            cards[i].FlipCard(false);
        }
        DealCards(); //2枚配布
    }

    void Update()
    {
        
    }

    void DealCards()
    {
        cards[cardIndex].transform.position = new Vector3(-0.05f, 0, 0);
        cards[cardIndex].GetComponent<CardController>().FlipCard();
        cards[cardIndex + 1].transform.position = new Vector3(0.05f, 0, 0);
        SetHighLowButtons(true);
    }

    void SetHighLowButtons(bool active)
    {
        buttonHigh.SetActive(true);
        buttonLow.SetActive(true);
    }

    public void OnClickHigh()
    {
        SetHighLowButtons(false);
        CheckHighLow(true);
    }

    public void OnClickLow()
    {
        SetHighLowButtons(false);
        CheckHighLow(false);
    }

    void CheckHighLow(bool high)
    {
        cards[cardIndex + 1].GetComponent<CardController>().FlipCard();
        string result = "LOSE... : ";
        int lno = cards[cardIndex].GetComponent<CardController>().No;
        int rno = cards[cardIndex + 1].GetComponent<CardController>().No;
        if(lno == rno)
        {
            result = "NO GAME : ";
        }
        else if (high) //Highを選んだ場合
        {
            if(lno < rno)
            {
                winCount++;
                result = "WIN!! : ";
                GetComponent<AudioSource>().Play();
            }
        }
        else //Lowを選んだ場合
        {
            if (lno > rno)
            {
                winCount++;
                result = "WIN!! : ";
                GetComponent<AudioSource>().Play();
            }
        }
        textInfo.text = result + winCount;
        StartCoroutine(nextCards());
    }

    IEnumerator nextCards()
    {
        yield return new WaitForSeconds(NextWaitTimer);//指定秒待つ
        cards[cardIndex].gameObject.SetActive(false);
        cards[cardIndex + 1].gameObject.SetActive(false);
        cardIndex += 2;
        if(cards.Count - 1 <= cardIndex)
        {
            textInfo.text = "終了: " + winCount;
        }
        else
        {
            DealCards();
        }
    }

    public void OnClickRestart()
    {
        SceneManager.LoadScene("HighLowScene");
    }
}
