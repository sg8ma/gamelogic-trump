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
    [SerializeField] GameObject stock; //�R�D�y��
    [SerializeField] List<Transform> foundation; //�g�D�y��
    [SerializeField] List<Transform> column; //��D�y��
    [SerializeField] List<CardController> cards; //�S�D
    [SerializeField] List<CardController> stockCards; //�R�D
    [SerializeField] List<CardController> wasteCards; //�߂���D
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
            stockCards.Add(item);//�R�D�֒ǉ�
        }
        //��D
        int cardindex = 0;
        int columncount = 0;
        foreach(var item in column)
        {
            columncount++;//��D�ɉ����u����(������1,2,3,4,5,6...��)
            for(int i = 0; i < columncount; i++)
            {
                if (cards.Count - 1 < cardindex) break; //�J�[�h������Ȃ��Ȃ�����I��
                CardController card = cards[cardindex]; //�ǉ��J�[�h
                CardController parent = item.GetComponent<CardController>();//�e�I�u�W�F�N�g
                if (0 != i) parent = cards[cardindex - 1];//1�ԉ��ȊO��1�O�̃J�[�h�̎�O�ɒu��
                PutCard(parent, card);
                stockCards.Remove(card);//�ǉ������J�[�h���R�D����폜
                cardindex++;
            }
            cards[cardindex - 1].FlipCard();//�Ō�ɒǉ������J�[�h���߂���
        }
        StackStockCards();//�R�D����ׂ�
    }

    void Update()
    {
        if (isGameEnd) return;
        gameTimer += Time.deltaTime;
        textTimer.text = GetTimerText(gameTimer);
        if(Input.GetMouseButtonDown(0)) //�����ꂽ
        {
            SetSelectCard();
        }
        else if(Input.GetMouseButton(0)) //�h���b�O��
        {
            MoveCard();
        }
        else if(Input.GetMouseButtonUp(0)) //�����ꂽ
        {
            ReleaseCard();
        }
    }

    void PutCard(CardController parent, CardController child)
    {
        child.transform.parent = parent.transform; //�q�I�u�W�F�N�g�w��
        Vector3 pos = parent.transform.position; //�ړ���
        pos.y += StackCardHeight; //��ɂ��炷
        if(column.Contains(parent.transform.root) && !column.Contains(parent.transform)) //��D�̏ꍇ��z�����炷
        {
            pos.z -= StackCardWidth;
        }
        child.transform.position = pos;
        wasteCards.Remove(child); //�߂���D�̒��ɂ�������폜
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
            int min = (int)timer / 60;//�������v�Z
            string pmin = string.Format("{0:D2}", min);//00�̂悤�ȕ�����ɂ���
            string psec = string.Format("{0:D2}", sec);//00�̂悤�ȕ�����ɂ���
            ret = pmin + ":" + psec;
            oldSecond = sec;
        }
        return ret;
    }

    Vector3 GetScreenToWorldPosition()
    {
        Vector3 cameraposition = Input.mousePosition; //�}�E�X�̍��W
        cameraposition.z = Camera.main.transform.position.y;//�J������y���W�̐ݒ�
        Vector3 worldposition = Camera.main.ScreenToWorldPoint(cameraposition);//�X�N���[�����W->���[���h���W
        return worldposition;
    }

    //�߂���D
    void SortWasteCards()
    {
        float startx = stock.transform.position.x - CardController.Width * 2; //�R�D�̍������J�n�ʒu��
        for(int i = 0; i < wasteCards.Count; i++)
        {
            CardController card = wasteCards[i];
            float x = startx + i * StackCardWidth; //�������E��
            float y = i * StackCardHeight; //���������
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
        if(obj == stock)//�R�D���Ō�܂ł߂���ꂽ
        {
            //�߂���D�Ǝ̂ĎD���f�b�L�ɖ߂�
            stockCards.AddRange(wasteCards);
            foreach(var item in cards)
            {
                if (item.gameObject.activeSelf) continue;
                item.gameObject.SetActive(true);
                stockCards.Add(item);
            }
            wasteCards.Clear();
            cardsDirector.ShuffleCards(stockCards);//�V���b�t�����čēx���ׂ�
            StackStockCards();
        }
        if (!card || 0 > card.PlayerNo) return; //�y��̃J�[�h�͖���(PlayerNo = -1)
        if(card.isFrontUp) //�\�����̃J�[�h
        {
            if (wasteCards.Contains(card) && card != wasteCards[wasteCards.Count - 1]) return; //�߂���D��1�Ԏ�O�̂ݑI���\
            card.HandPosition = card.transform.position;
            selectCard = card;
            startPosition = GetScreenToWorldPosition();
        }
        else
        {
            if(1 > card.transform.childCount)//1�Ԏ�O�Ȃ���J
            {
                card.transform.DORotate(Vector3.zero, SortWasteCardTime)
                    .OnComplete(() => { card.FlipCard(); });
            }
            if(card.transform.root == stock.transform) //�R�D�J�[�h���߂���D�̏ꏊ��
            {
                if(3 < wasteCards.Count + 1) //4���ڈȍ~��1�ԌÂ��J�[�h���̂Ăĕ\��
                {
                    wasteCards[0].gameObject.SetActive(false);
                    wasteCards.RemoveAt(0);
                }
                //�R�D����߂���D�ֈړ�
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
        Vector3 diff = GetScreenToWorldPosition() - startPosition;//�������|�W�V�����̍���
        Vector3 pos = selectCard.transform.position + diff;
        pos.y = 0.01f;
        selectCard.transform.position = pos;
        startPosition = GetScreenToWorldPosition();//�|�W�V�������X�V
    }

    void ReleaseCard()
    {
        if (!selectCard) return;
        CardController frontcard = null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        foreach(RaycastHit hit in Physics.RaycastAll(ray))
        {
            CardController card = hit.transform.gameObject.GetComponent<CardController>();
            if (!card || card == selectCard) continue;//�ړ����̃J�[�h�̓X�L�b�v
            if(!frontcard || frontcard.transform.childCount > card.transform.childCount) //1�Ԏ�O�̎q�I�u�W�F�N�g�̏��Ȃ��J�[�h
            {
                frontcard = card;
            }
        }
        //�g�D�ɒu����J�[�h
        if (frontcard 
            && foundation.Contains(frontcard.transform.root)
            && 1 > selectCard.transform.childCount
            && frontcard.No + 1 == selectCard.No
            && frontcard.Suit == selectCard.Suit)
        {
            PutCard(frontcard, selectCard);
            bool fieldend = true;//�N���A����
            foreach(var item in column)
            {
                if (0 < item.childCount) fieldend = false;
            }
            isGameEnd = fieldend && 1 > wasteCards.Count && 1 > stockCards.Count;//��D�A�߂���D�A�R�D��1�����Ȃ���΃N���A
        }
        //��D�ɒu����J�[�h
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
