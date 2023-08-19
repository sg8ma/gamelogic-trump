using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SolitaireSceneDirector : MonoBehaviour
{
    [SerializeField] CardsDirector cardsDirector;
    [SerializeField] Text textTimer;
    [SerializeField] GameObject stock; //山札土台
    [SerializeField] List<Transform> foundation; //組札土台
    [SerializeField] List<Transform> column; //場札土台
    [SerializeField] List<CardController> cards; //全札
    [SerializeField] List<CardController> stockCards; //山札
    [SerializeField] List<CardController> wasteCards; //めくり札
    CardController selectCard;
    Vector3 startPosition;
    bool isGameEnd;
    float gameTimer;
    int oldSecond;
    const float StackCardHeight = 0.0001f;
    const float StackCardWidth = 0.02f;
    const float SortWasteCardTime = 0.2f;

    void Start()
    {
        cards = cardsDirector.GetShuffleCards();
        wasteCards = new List<CardController>();
        stockCards = new List<CardController>();
        foreach(var item in cards)
        {
            item.PlayerNo = 0;
            item.FlipCard(false);
            stockCards.Add(item);//山札へ追加
        }
        //場札
        int cardindex = 0;
        int columncount = 0;
        foreach(var item in column)
        {
            columncount++;//場札に何枚置くか(左から1,2,3,4,5,6...枚)
            for(int i = 0; i < columncount; i++)
            {
                if (cards.Count - 1 < cardindex) break; //カードが足りなくなったら終了
                CardController card = cards[cardindex]; //追加カード
                CardController parent = item.GetComponent<CardController>();//親オブジェクト
                if (0 != i) parent = cards[cardindex - 1];//1番奥以外は1つ前のカードの手前に置く
                PutCard(parent, card);
                stockCards.Remove(card);//追加したカードを山札から削除
                cardindex++;
            }
            cards[cardindex - 1].FlipCard();//最後に追加したカードをめくる
        }
        StackStockCards();//山札を並べる
    }

    void Update()
    {
        if (isGameEnd) return;
        gameTimer += Time.deltaTime;
        textTimer.text = GetTimerText(gameTimer);
        if(Input.GetMouseButtonDown(0)) //押された
        {
            SetSelectCard();
        }
        else if(Input.GetMouseButton(0)) //ドラッグ中
        {
            MoveCard();
        }
        else if(Input.GetMouseButtonUp(0)) //離された
        {
            ReleaseCard();
        }
    }

    void PutCard(CardController parent, CardController child)
    {
        child.transform.parent = parent.transform; //子オブジェクト指定
        Vector3 pos = parent.transform.position; //移動先
        pos.y += StackCardHeight; //上にずらす
        if(column.Contains(parent.transform.root) && !column.Contains(parent.transform)) //場札の場合はzをずらす
        {
            pos.z -= StackCardWidth;
        }
        child.transform.position = pos;
        wasteCards.Remove(child); //めくり札の中にあったら削除
    }

    void StackStockCards()
    {
        for(int i = 0; i < stockCards.Count; i++)
        {
            CardController card = stockCards[i];
            card.FlipCard(false);
            Vector3 pos = stock.transform.position;
            pos.y += (i + 1) * StackCardHeight;
            card.transform.position = pos;
            card.transform.parent = stock.transform;
        }
    }

    string GetTimerText(float timer)
    {
        int sec = (int)timer % 60;
        string ret = textTimer.text;
        if (oldSecond != sec)
        {
            int min = (int)timer / 60;//分数を計算
            string pmin = string.Format("{0:D2}", min);//00のような文字列にする
            string psec = string.Format("{0:D2}", sec);//00のような文字列にする
            ret = pmin + ":" + psec;
            oldSecond = sec;
        }
        return ret;
    }

    Vector3 GetScreenToWorldPosition()
    {
        Vector3 cameraposition = Input.mousePosition; //マウスの座標
        cameraposition.z = Camera.main.transform.position.y;//カメラのy座標の設定
        Vector3 worldposition = Camera.main.ScreenToWorldPoint(cameraposition);//スクリーン座標->ワールド座標
        return worldposition;
    }

    //めくり札
    void SortWasteCards()
    {
        float startx = stock.transform.position.x - CardController.Width * 2; //山札の左側を開始位置に
        for(int i = 0; i < wasteCards.Count; i++)
        {
            CardController card = wasteCards[i];
            float x = startx + i * StackCardWidth; //枚数分右に
            float y = i * StackCardHeight; //枚数分上に
            card.transform.DOMove(new Vector3(x, y, stock.transform.position.z), SortWasteCardTime);
        }
    }

    void SetSelectCard()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;
        GameObject obj = hit.collider.gameObject;
        CardController card = obj.GetComponent<CardController>();
        selectCard = null;
        if(obj == stock)//山札が最後までめくられた
        {
            //めくり札と捨て札をデッキに戻す
            stockCards.AddRange(wasteCards);
            foreach(var item in cards)
            {
                if (item.gameObject.activeSelf) continue;
                item.gameObject.SetActive(true);
                stockCards.Add(item);
            }
            wasteCards.Clear();
            cardsDirector.ShuffleCards(stockCards);//シャッフルして再度並べる
            StackStockCards();
        }
        if (!card || 0 > card.PlayerNo) return; //土台のカードは無効(PlayerNo = -1)
        if(card.isFrontUp) //表向きのカード
        {
            if (wasteCards.Contains(card) && card != wasteCards[wasteCards.Count - 1]) return; //めくり札は1番手前のみ選択可能
            card.HandPosition = card.transform.position;
            selectCard = card;
            startPosition = GetScreenToWorldPosition();
        }
        else
        {
            if(1 > card.transform.childCount)//1番手前なら公開
            {
                card.transform.DORotate(Vector3.zero, SortWasteCardTime)
                    .OnComplete(() => { card.FlipCard(); });
            }
            if(card.transform.root == stock.transform) //山札カードをめくり札の場所へ
            {
                if(3 < wasteCards.Count + 1) //4枚目以降は1番古いカードを捨てて表示
                {
                    wasteCards[0].gameObject.SetActive(false);
                    wasteCards.RemoveAt(0);
                }
                //山札からめくり札へ移動
                stockCards.Remove(card);
                wasteCards.Add(card);
                SortWasteCards();
                StackStockCards();
            }
        }
    }

    void MoveCard()
    {
        if (!selectCard) return;
        Vector3 diff = GetScreenToWorldPosition() - startPosition;//動いたポジションの差分
        Vector3 pos = selectCard.transform.position + diff;
        pos.y = 0.01f;
        selectCard.transform.position = pos;
        startPosition = GetScreenToWorldPosition();//ポジションを更新
    }

    void ReleaseCard()
    {
        if (!selectCard) return;
        CardController frontcard = null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        foreach(RaycastHit hit in Physics.RaycastAll(ray))
        {
            CardController card = hit.transform.gameObject.GetComponent<CardController>();
            if (!card || card == selectCard) continue;//移動中のカードはスキップ
            if(!frontcard || frontcard.transform.childCount > card.transform.childCount) //1番手前の子オブジェクトの少ないカード
            {
                frontcard = card;
            }
        }
        //組札に置けるカード
        if (frontcard 
            && foundation.Contains(frontcard.transform.root)
            && 1 > selectCard.transform.childCount
            && frontcard.No + 1 == selectCard.No
            && frontcard.Suit == selectCard.Suit)
        {
            PutCard(frontcard, selectCard);
            bool fieldend = true;//クリア判定
            foreach(var item in column)
            {
                if (0 < item.childCount) fieldend = false;
            }
            isGameEnd = fieldend && 1 > wasteCards.Count && 1 > stockCards.Count;//場札、めくり札、山札が1枚もなければクリア
        }
        //場札に置けるカード
        else if (frontcard 
                && column.Contains(frontcard.transform.root)
                && 1 > frontcard.transform.childCount
                && frontcard.No - 1 == selectCard.No
                && frontcard.SuitColor != selectCard.SuitColor)
        { 
            PutCard(frontcard, selectCard);
        }
        else 
        {
            selectCard.transform.position = selectCard.HandPosition;
        }
    }

    public void OnClickRestart()
    {
        SceneManager.LoadScene("SolitaireScene");
    }
}
