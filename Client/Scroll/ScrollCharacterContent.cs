using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollCharacterContent : MonoBehaviour
{
    public GameObject characterElement;

    RectTransform rectTransform = null;

    [SerializeField]
    public static int selectedCharacterIndex = 0;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
    }

    // Start is called before the first frame update
    void Start()
    {
        OrganizeChild();

        if(User.instance.characterDatas.Count > 0) {
            ScrollCharacterContent.selectedCharacterIndex = 0;
            SelectCharacter(0);
        } else {
            ScrollCharacterContent.selectedCharacterIndex = -1;
            SelectCharacter(-1);
        }
    }

    public static void SelectCharacter(int index) {
        if(index != -1) {
            CharacterData cd = User.instance.characterDatas[index];

            GameObject.Find("CanvasHolder").transform.GetChild(2).gameObject.SetActive(true);
            GameObject.Find("SelectedNickname").GetComponent<Text>().text = cd.nickname;
            GameObject.Find("SelectedLevel").GetComponent<Text>().text = "LV." + cd.level.ToString();

            int job = cd.job;
            string jobString = "Warrior";
            if(job == 0) {
                GameObject.Find("ModelHolder").transform.GetChild(0).gameObject.SetActive(true);
                GameObject.Find("ModelHolder").transform.GetChild(1).gameObject.SetActive(false);
            }
            else if(job == 1) {
                GameObject.Find("ModelHolder").transform.GetChild(0).gameObject.SetActive(false);
                GameObject.Find("ModelHolder").transform.GetChild(1).gameObject.SetActive(true);
                jobString = "Magician";
            }

            GameObject.Find("SelectedJob").GetComponent<Text>().text = jobString;
            User.instance.nickname = cd.nickname;
            User.instance.character_id = cd.id;
            User.instance.job = cd.job;
            User.instance.level = cd.level;
            User.instance.weapon_id = cd.weapon_id;
            User.instance.armor_id = cd.armor_id;
            User.instance.status_point = cd.status_point;
            User.instance.gold = cd.gold;
            User.instance.exp = cd.exp;
            User.instance.strong = cd.strong;
            User.instance.dexility = cd.dexility;
            User.instance.intellect = cd.intellect;
            User.instance.luck = cd.luck;

            User.instance.selectedCharacter = cd;
        } 
        else {
        }
    }

    public void OrganizeChild() {
        foreach (Transform child in transform) {
            GameObject.Destroy(child.gameObject);
        }

        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, User.instance.characterDatas.Count * 110.0f);
        rectTransform.localPosition = new Vector3(0.0f, -100.0f, 0.0f);

        for(int i = 0; i < User.instance.characterDatas.Count; i++) {
           CharacterData data = User.instance.characterDatas[i];

            GameObject go = Instantiate(characterElement, this.transform);

            string job = "Warrior";
            if(data.job == 1) {
                job = "Magician";
            }

            go.GetComponent<CharacterElement>().Set(data.id, data.nickname, data.level, job);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.localPosition = new Vector3(0.0f, + -5.0f - i * 110.0f, 0.0f);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
