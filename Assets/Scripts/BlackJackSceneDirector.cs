using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BlackJackSceneDirector : MonoBehaviour
{
    [SerializeField] CardsDirector cardsDirector; //���ʃJ�[�h�Ǘ��N���X
    [SerializeField] GameObject buttonHit;
    [SerializeField] GameObject buttonStay;
    [SerializeField] Text textPlayerInfo;
    [SerializeField] Text textDealerInfo;
    List<CardController> cards; //�g�p����J�[�h
    List<CardController> playerHand; //��D
    List<CardController> dealerHand; //��D
    int cardIndex; //�R�D
    const float NextWaitTime = 1; //�҂���
    AudioSource audioPlayer;
    [SerializeField] AudioClip win;
    [SerializeField] AudioClip lose;


    void Start()
    {
        cards = cardsDirector.GetShuffleCards();
        foreach(var item in cards)
        {
            item.transform.position = new Vector3(100, 0, 0);//�����Ȃ��ꏊ�ɒu��
            item.FlipCard(false);
        }
        playerHand = new List<CardController>();
        dealerHand = new List<CardController>();
        cardIndex = 0;
        CardController card; 
        card = HitCard(dealerHand);
        card = HitCard(dealerHand);
        card.FlipCard(); //2���ڂ̃J�[�h�����J
        HitCard(playerHand).FlipCard();//�J�[�h���J
        HitCard(playerHand).FlipCard();//�J�[�h���J
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
        if(dealerHand == hand)//�f�B�[���[�̏����ʒu
        {
            z = 0.1f;
        }
        if(0 < hand.Count) //�J�[�h���L��ΉE�ɕ��ׂ�
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
            textPlayerInfo.text = "�o�[�X�g���܂����B�s�k";
            buttonHit.gameObject.SetActive(false);
            buttonStay.gameObject.SetActive(false);
        }
    }

    public void OnClickStay()
    {
        buttonHit.gameObject.SetActive(false);
        buttonStay.gameObject.SetActive(false);
        dealerHand[0].FlipCard(); //�������Ă���1���ڂ����J
        int score = GetScore(dealerHand);
        textDealerInfo.text = "" + score;
        StartCoroutine(DealerHit());
    }

    IEnumerator DealerHit()
    {
        yield return new WaitForSeconds(NextWaitTime);
        int score = GetScore(dealerHand);
        if(16 >= score) //16�ȉ��̓J�[�h������
        {
            CardController card = HitCard(dealerHand);
            card.FlipCard();
            textDealerInfo.text = "" + GetScore(dealerHand);
        }
        score = GetScore(dealerHand);
        if (22 <= score) //22�ȏ�̓o�[�X�g
        {
            textDealerInfo.text += "�o�[�X�g";
            textPlayerInfo.text = "����";
            audioPlayer.PlayOneShot(win);
        }
        else if (17 <= score)//17�`21�͌v�Z
        {
            string textPlayer = "����";
            if (GetScore(playerHand) < GetScore(dealerHand))
            {
                textPlayer = "�s�k";
            }
            else if (GetScore(playerHand) == GetScore(dealerHand))
            {
                textPlayer = "��������";
            }
            textPlayerInfo.text = textPlayer;
            if (textPlayer.Contains("����"))
            {
                audioPlayer.PlayOneShot(win);
            }
            else if(textPlayer.Contains("�s�k"))
            {
                audioPlayer.PlayOneShot(lose);
            }
        }
        else//16�ȉ��͍ċA
        {
            StartCoroutine(DealerHit());
        }
    }

    public void OnClickRestart()
    {
        SceneManager.LoadScene("BlackJackScene");
    }
}
