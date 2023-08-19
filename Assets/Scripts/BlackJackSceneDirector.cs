using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BlackJackSceneDirector : MonoBehaviour
{
    [SerializeField] CardsDirector cardsDirector; //共通カード管理クラス
    [SerializeField] GameObject buttonHit;
    [SerializeField] GameObject buttonStay;
    [SerializeField] Text textPlayerInfo;
    [SerializeField] Text textDealerInfo;
    List<CardController> cards; //使用するカード
    List<CardController> playerHand; //手札
    List<CardController> dealerHand; //手札
    int cardIndex; //山札
    const float NextWaitTime = 1; //待つ時間
    AudioSource audioPlayer;
    [SerializeField] AudioClip win;
    [SerializeField] AudioClip lose;


    void Start()
    {
        cards = cardsDirector.GetShuffleCards();
        foreach(var item in cards)
        {
            item.transform.position = new Vector3(100, 0, 0);//見えない場所に置く
            item.FlipCard(false);
        }
        playerHand = new List<CardController>();
        dealerHand = new List<CardController>();
        cardIndex = 0;
        CardController card; 
        card = HitCard(dealerHand);
        card = HitCard(dealerHand);
        card.FlipCard(); //2枚目のカードを公開
        HitCard(playerHand).FlipCard();//カード公開
        HitCard(playerHand).FlipCard();//カード公開
        textPlayerInfo.text = "" + GetScore(playerHand);
        audioPlayer = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    CardController HitCard(List<CardController> hand)
    {
        float x = -0.1f;
        float z = -0.05f;
        if(dealerHand == hand)//ディーラーの初期位置
        {
            z = 0.1f;
        }
        if(0 < hand.Count) //カードが有れば右に並べる
        {
            x = hand[hand.Count - 1].transform.position.x;
            z = hand[hand.Count - 1].transform.position.z;
        }
        CardController card = cards[cardIndex];
        card.transform.position = new Vector3(x + CardController.Width, 0, z);
        hand.Add(card);
        cardIndex++;
        return card;
    }

    int GetScore(List<CardController> hand)
    {
        int score = 0;
        List<CardController> ace = new List<CardController>();
        foreach(var item in hand)
        {
            int no = item.No;
            if(1 == no)
            {
                ace.Add(item);
            }
            else if(10 < no)
            {
                no = 10;
            }
            score += no;
        }
        foreach(var item in ace)
        {
            if((score + 10) < 22)
            {
                score += 10;
            }
        }
        return score;
    }

    public void OnClickHit()
    {
        //CardController card = HitCard(playerHand);
        //card.FlipCard();
        HitCard(playerHand).FlipCard();
        int score = GetScore(playerHand);
        textPlayerInfo.text = "" + score;
        if(21 < score)
        {
            textPlayerInfo.text = "バーストしました。敗北";
            buttonHit.gameObject.SetActive(false);
            buttonStay.gameObject.SetActive(false);
        }
    }

    public void OnClickStay()
    {
        buttonHit.gameObject.SetActive(false);
        buttonStay.gameObject.SetActive(false);
        dealerHand[0].FlipCard(); //伏せられている1枚目を公開
        int score = GetScore(dealerHand);
        textDealerInfo.text = "" + score;
        StartCoroutine(DealerHit());
    }

    IEnumerator DealerHit()
    {
        yield return new WaitForSeconds(NextWaitTime);
        int score = GetScore(dealerHand);
        if(16 >= score) //16以下はカードを引く
        {
            CardController card = HitCard(dealerHand);
            card.FlipCard();
            textDealerInfo.text = "" + GetScore(dealerHand);
        }
        score = GetScore(dealerHand);
        if (22 <= score) //22以上はバースト
        {
            textDealerInfo.text += "バースト";
            textPlayerInfo.text = "勝利";
            audioPlayer.PlayOneShot(win);
        }
        else if (17 <= score)//17～21は計算
        {
            string textPlayer = "勝利";
            if (GetScore(playerHand) < GetScore(dealerHand))
            {
                textPlayer = "敗北";
            }
            else if (GetScore(playerHand) == GetScore(dealerHand))
            {
                textPlayer = "引き分け";
            }
            textPlayerInfo.text = textPlayer;
            if (textPlayer.Contains("勝利"))
            {
                audioPlayer.PlayOneShot(win);
            }
            else if(textPlayer.Contains("敗北"))
            {
                audioPlayer.PlayOneShot(lose);
            }
        }
        else//16以下は再帰
        {
            StartCoroutine(DealerHit());
        }
    }

    public void OnClickRestart()
    {
        SceneManager.LoadScene("BlackJackScene");
    }
}
