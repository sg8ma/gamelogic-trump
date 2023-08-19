using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MemorySceneDirector : MonoBehaviour
{
    [SerializeField] CardsDirector cardsDirector;
    [SerializeField] Text textTimer;
    List<CardController> cards;
    int width = 5;
    int height = 4;
    List<CardController> selectCards;//選んだカード
    int selectCountMax = 2;//選べる枚数
    bool isGameEnd;
    float gameTimer;
    int oldSecond;//前回の秒数

    void Start()
    {
        cards = cardsDirector.GetMemoryCards();
        Vector2 offset = new Vector2((width - 1) / 2.0f, (height - 1) / 2.0f);
        if(cards.Count < width * height)
        {
            Debug.LogError("カードが足りません");
        }
        for(int i = 0; i < width * height; i++)
        {
            float x = (i % width - offset.x) * CardController.Width; //表示位置
            float y = (i / width - offset.y) * CardController.Height; //表示位置
            cards[i].transform.position = new Vector3(x, 0, y); //場所
            cards[i].FlipCard(false);
        }
        selectCards = new List<CardController>();
        oldSecond = -1;

    }

    void Update()
    {
        if (isGameEnd) return;
        gameTimer += Time.deltaTime;
        textTimer.text = GetTimerText(gameTimer);
        if(Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out RaycastHit hit))
            {
                if (!CanOpen()) return;
                CardController card = hit.collider.gameObject.GetComponent<CardController>();
                if (!card || selectCards.Contains(card)) return;
                card.FlipCard();
                selectCards.Add(card);
            }
        }
    }

    string GetTimerText(float timer)
    {
        int sec = (int)timer % 60;
        string ret = textTimer.text;
        if(oldSecond != sec)
        {
            int min = (int)timer / 60;//分数を計算
            string pmin = string.Format("{0:D2}", min);//00のような文字列にする
            string psec = string.Format("{0:D2}", sec);//00のような文字列にする
            ret = pmin + ":" + psec;
            oldSecond = sec;
        }
        return ret;
    }

    bool CanOpen()
    {
        if (selectCards.Count < selectCountMax) return true;
        bool equal = true;
        foreach(var item in selectCards)
        {
            item.FlipCard(false);//カードを裏返しにする
            Debug.Log("Item.No: " + item.No);
            Debug.Log("selectCards[0].No: " + selectCards[0].No);
            if (item.No != selectCards[0].No)
            {
                equal = false; //1枚でも違ったらfalse
            }
        }
        if (equal)
        {
            foreach(var item in selectCards)
            {
                item.gameObject.SetActive(false); //揃ったカードを非表示にする
            }
            isGameEnd = true;
            foreach(var item in cards)
            {
                if(item.gameObject.activeSelf)
                {
                    isGameEnd = false; //1枚のカードでも表示されていればfalse
                    break;
                }
            }
            if(isGameEnd)
            {
                textTimer.text = "クリア" + GetTimerText(gameTimer);
            }
        }
        selectCards.Clear();
        return true;
    }
}
