using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProfileSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] Button button1;
    [SerializeField] TextMeshProUGUI text1;

    [SerializeField] Button button2;
    [SerializeField] TextMeshProUGUI text2;

    [SerializeField] TextMeshProUGUI rowText;

    List<GameObject> list = new List<GameObject>();

    Action function1;
    Action function2;

    string payload;
    string timeAgo;
    string option1text;
    string option2text;

    public void Initialize(string payload, Action function1, Action function2, string option1text, string option2text) {
        this.function1 = function1;
        this.function2 = function2;

        this.option1text = option1text;
        this.option2text = option2text;
   
        this.payload = payload;

        SetDefaultSate();

        list.Add(button1.gameObject);
        list.Add(text1.gameObject);
        list.Add(button2.gameObject);
        list.Add(text2.gameObject);

        SetActive(list, false);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        SetDefaultSate();
        SetActive(list, true); 
    }

    public void OnPointerExit(PointerEventData eventData) {
        SetDefaultSate();
        SetActive(list, false);
    }


    void SetActive(List<GameObject> list, bool state) {
        foreach (GameObject go in list) {
            go.SetActive(state);
        }
    }

    void SetDefaultSate() {
        EventSystem.current.SetSelectedGameObject(null);

        button1.onClick.RemoveAllListeners();
        button2.onClick.RemoveAllListeners();

        text1.text = option1text;
        text2.text = option2text;
        rowText.text = payload;

        button1.onClick.AddListener(() => function1());
        button2.onClick.AddListener(() => SetDeleteState());
    }

    void SetDeleteState() {
        EventSystem.current.SetSelectedGameObject(null);

        button1.onClick.RemoveAllListeners();
        button2.onClick.RemoveAllListeners();

        text1.text = "Yes";
        text2.text = "No";
        rowText.text = "Please confirm: ";

        button1.onClick.AddListener(() => function2());
        button2.onClick.AddListener(() => SetDefaultSate());
    }
}
