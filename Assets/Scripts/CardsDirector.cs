using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardsDirector : MonoBehaviour
{
    [SerializeField] List<GameObject> prefabSpades;
    [SerializeField] List<GameObject> prefabClubs;
    [SerializeField] List<GameObject> prefabDiamonds;
    [SerializeField] List<GameObject> prefabHearts;
    [SerializeField] List<GameObject> prefabJokers;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public List<CardController> GetHighLowCards()
    {
        List<CardController> ret = new List<CardController>();
        ret.AddRange(CreateCards(SuitType.Spade));
        ret.AddRange(CreateCards(SuitType.Club));
        ret.AddRange(CreateCards(SuitType.Diamond));
        ret.AddRange(CreateCards(SuitType.Heart));
        ShuffleCards(ret);
        return ret;
    }

    public List<CardController> GetShuffleCards()
    {
        List<CardController> ret = new List<CardController>();
        ret.AddRange(CreateCards(SuitType.Spade));
        ret.AddRange(CreateCards(SuitType.Club));
        ret.AddRange(CreateCards(SuitType.Diamond));
        ret.AddRange(CreateCards(SuitType.Heart));
        ShuffleCards(ret);
        return ret;
    }

    public List<CardController> GetMemoryCards()
    {
        List<CardController> ret = new List<CardController>();
        ret.AddRange(CreateCards(SuitType.Spade, 10));
        ret.AddRange(CreateCards(SuitType.Diamond, 10));
        ShuffleCards(ret);
        return ret;
    }

    public void ShuffleCards(List<CardController> cards)
    {
        for(int i = 0; i < cards.Count; i++)
        {
            int rnd = Random.Range(0, cards.Count);
            CardController tmp = cards[i];
            cards[i] = cards[rnd];
            cards[rnd] = tmp;
        }
    }

    List<CardController> CreateCards(SuitType suittype, int count = -1)
    {
        List<CardController> ret = new List<CardController>();
        List<GameObject> prefabcards = prefabSpades;
        Color suitcolor = Color.black;
        if(SuitType.Club == suittype)
        {
            prefabcards = prefabClubs;
        }
        else if (SuitType.Diamond == suittype)
        {
            prefabcards = prefabDiamonds;
            suitcolor = Color.red;
        }
        else if (SuitType.Heart == suittype)
        {
            prefabcards = prefabHearts;
            suitcolor = Color.red;
        }
        else if (SuitType.Joker == suittype)
        {
            prefabcards = prefabJokers;
        }
        if(0 > count)
        {
            count = prefabcards.Count; //枚数に指定がなければすべてのカードを作成する
        }
        for(int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefabcards[i]);
            BoxCollider bc = obj.AddComponent<BoxCollider>();
            Rigidbody rb = obj.AddComponent<Rigidbody>();
            bc.isTrigger = true;
            rb.isKinematic = true;
            CardController ctrl = obj.AddComponent<CardController>();
            ctrl.Suit = suittype;
            ctrl.SuitColor = suitcolor;
            ctrl.PlayerNo = -1;
            ctrl.No = i + 1;
            ret.Add(ctrl);
        }
        return ret;
    }
}
