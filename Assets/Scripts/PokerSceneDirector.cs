using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PokerSceneDirector : MonoBehaviour
{
    [SerializeField] CardsDirector cardsDirector;
    [SerializeField] Button buttonBetCoin;
    [SerializeField] Button buttonPlay;
    [SerializeField] Button buttonChange;
    [SerializeField] Text textGameInfo;
    [SerializeField] Text textRate;
    Text textButtonBetCoin;
    Text textButtonChange;
    [SerializeField]  List<CardController> cards;
    [SerializeField]  List<CardController> hand;
    [SerializeField]  List<CardController> selectCards;
    int dealCardCount = 0;
    [SerializeField] int playerCoin;
    [SerializeField] int cardChangeCountMax;
    int betCoin = 1;
    int cardChangeCount;
    int straightFlushRate = 10;
    int fourCardRate = 8;
    int fullHouseRate = 6;
    int flushRate = 5;
    int straightRate = 4;
    int threeCardRate = 3;
    int twoPairRate = 2;
    int onePairRate = 1;
    const float SortHandTime = 0.5f;

    void Start()
    {
        cards = cardsDirector.GetShuffleCards();
        hand = new List<CardController>();
        selectCards = new List<CardController>();
        textButtonBetCoin = buttonBetCoin.GetComponentInChildren<Text>();
        textButtonChange = buttonChange.GetComponentInChildren<Text>();
        RestartGame(false);
        UpdateTexts();
        SetButtonInPlay(false);
    }

    void Update()
    {
        if(Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out RaycastHit hit))
            {
                CardController card = hit.collider.gameObject.GetComponent<CardController>();
                SetSelectCard(card);
            }
        }
    }

    CardController AddHand()
    {
        CardController card = cards[dealCardCount++];
        hand.Add(card);
        return card;
    }

    void OpenHand(CardController card)
    {
        card.transform.DORotate(Vector3.zero, SortHandTime)
            .OnComplete(() => { card.FlipCard(); }); // 回転アニメーション
    }

    void SortHand()
    {
        float x = -CardController.Width * 2; //初期位置
        foreach(var item in hand)
        {
            Vector3 pos = new Vector3(x, 0, 0);
            item.transform.DOMove(pos, SortHandTime); //表示位置へアニメーションして移動
            x += CardController.Width; //1枚カード分ずらす
        }
    }

    void RestartGame(bool deal = true)
    {
        hand.Clear();
        selectCards.Clear();
        cardChangeCount = cardChangeCountMax;
        dealCardCount = 0;
        cardsDirector.ShuffleCards(cards);
        foreach(var item in cards)
        {
            item.gameObject.SetActive(true);
            item.FlipCard(false);
            item.transform.position = new Vector3(0, 0, 0.18f);
        }
        if (!deal) return;
        for (int i = 0; i < 5; i++)
        {
            OpenHand(AddHand());
        }
        SortHand();
    }

    void UpdateTexts()
    {
        textButtonBetCoin.text = "手持ちコイン" + playerCoin;
        textGameInfo.text = "BET枚数" + betCoin;
        textRate.text = "ストレートフラッシュ " + (straightFlushRate * betCoin) + "\n"
            + "フォーカード " + (fourCardRate * betCoin) + "\n"
            + "フルハウス " + (fullHouseRate * betCoin) + "\n"
            + "フラッシュ " + (flushRate * betCoin) + "\n"
            + "ストレート " + (straightFlushRate * betCoin) + "\n"
            + "スリーカード " + (threeCardRate * betCoin) + "\n"
            + "ツーペア " + (twoPairRate * betCoin) + "\n"
            + "ワンペア " + (onePairRate * betCoin) + "\n";
    }

    void SetButtonInPlay(bool disp = true)
    {
        textButtonChange.text = "終了";
        buttonChange.gameObject.SetActive(disp);
        buttonBetCoin.gameObject.SetActive(!disp);
        buttonPlay.gameObject.SetActive(!disp);
    }

    public void OnClickBetCoin()
    {
        if (1 > playerCoin) return;
        playerCoin--;
        betCoin++;
        UpdateTexts();
    }

    public void OnClickPlay()
    {
        RestartGame();
        SetButtonInPlay();
        UpdateTexts();
    }

    void SetSelectCard(CardController card)
    {
        if (!card || !card.isFrontUp) return;
        Vector3 pos = card.transform.position;
        if(selectCards.Contains(card))
        {
            pos.z -= 0.02f;
            selectCards.Remove(card);
        }
        else if(cards.Count > dealCardCount + selectCards.Count)
        {
            pos.z += 0.02f;
            selectCards.Add(card);
        }
        card.transform.position = pos;
        textButtonChange.text = "交換";
        if(1 > selectCards.Count)
        {
            textButtonChange.text = "終了";
        }
    }

    public void OnClickChange()
    {
        if(1 > selectCards.Count)
        {
            cardChangeCount = 0;
        }
        foreach(var item in selectCards)
        {
            item.gameObject.SetActive(false);
            hand.Remove(item);
            OpenHand(AddHand());
        }
        selectCards.Clear();
        SortHand();
        SetButtonInPlay();
        cardChangeCount--;
        if(1 > cardChangeCount)
        {
            CheckHandRank();
        }
    }

    void CheckHandRank()
    {
        bool flush = true;
        SuitType suit = hand[0].Suit;
        foreach(var item in hand)
        {
            if(suit != item.Suit) //1枚でもマークが違えば
            {
                flush = false;
                break;
            }
        }
        bool straight = false;
        for(int i = 0; i < hand.Count; i++)
        {
            int straightcount = 0;
            int cardno = hand[i].No;
            for(int j = 0; j < hand.Count; j++) //1枚目から自分の番号+1でループ
            {
                if (i == j) continue;
                int targetno = cardno + 1;
                if (13 < targetno) targetno = 1; // 13の次は1
                if(targetno == hand[j].No)
                {
                    straightcount++;
                    cardno = hand[j].No;
                    j = -1;
                }
            }
            if(4 <= straightcount) //連続したカードが4個以上
            {
                straight = true;
                break;
            }
        }
        int pair = 0;
        bool threecard = false;
        bool fourcard = false;
        List<CardController> checkcards = new List<CardController>();
        for(int i = 0; i < hand.Count; i++)
        {
            if (checkcards.Contains(hand[i])) continue; //役になっていないカードをスキップ
            int samenocount = 0;
            int cardno = hand[i].No;
            for(int j = 0; j < hand.Count; j++)
            {
                if (i == j) continue;
                if(cardno == hand[j].No) //同じ番号があれば
                {
                    samenocount++;
                    checkcards.Add(hand[j]);
                }
            }
            if(1 == samenocount)
            {
                pair++;
            }
            else if(2 == samenocount)
            {
                threecard = true;
            }
            else if(3 == samenocount)
            {
                fourcard = true;
            }
        }
        bool fullhouse = false;
        if(1 == pair && threecard)
        {
            fullhouse = true;
        }
        bool straightflush = false;
        if(flush && straight)
        {
            straightflush = true;
        }
        int addcoin = 0;
        string infotext = "役無し..";
        if(straightflush)
        {
            addcoin = straightFlushRate * betCoin;
            infotext = "ストレートフラッシュ!!";
        }
        else if(fourcard)
        {
            addcoin = fourCardRate * betCoin;
            infotext = "フォーカード!!";
        }
        else if (fullhouse)
        {
            addcoin = fullHouseRate * betCoin;
            infotext = "フルハウス!!";
        }
        else if (flush)
        {
            addcoin = flushRate * betCoin;
            infotext = "フラッシュ!!";
        }
        else if (straight)
        {
            addcoin = straightRate * betCoin;
            infotext = "ストレート!!";
        }
        else if (threecard)
        {
            addcoin = threeCardRate * betCoin;
            infotext = "スリーカード!!";
        }
        else if (2 == pair)
        {
            addcoin = twoPairRate * betCoin;
            infotext = "ツーペア!!";
        }
        else if (1 == pair)
        {
            addcoin = onePairRate * betCoin;
            infotext = "ワンペア!!";
        }
        playerCoin += addcoin;
        UpdateTexts();
        textGameInfo.text = infotext + addcoin;
        betCoin = 0;
        SetButtonInPlay(false);
    }
}
