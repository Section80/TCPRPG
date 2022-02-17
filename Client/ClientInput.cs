using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ClientInput : MonoBehaviour
{
    public static ClientInput instance;
    public static byte[] inputByte;

    public float x;
    public float y;

    public bool isReady = false;

    private void Awake() {
        inputByte = new byte[9];
        inputByte[0] = 7;

        instance = GetComponent<ClientInput>();
    }

    // Start is called before the first frame update
    void Start()
    {
        byte[] output = new byte[2];
        output[0] = 8;
        output[1] = 1;

        ClientSocket.instance.SendRequest(output);

        Room.instance.entityHolder.gameObject.SetActive(true);

        isReady = true;
    }

    private bool inputChanged = false;

    // Update is called once per frame
    void Update()
    {
        inputChanged = false;

        if(Input.GetKey(KeyCode.LeftArrow)) {
            if(inputByte[1] == 0) {
                inputChanged = true;
            }
            inputByte[1] = 1;
        } else {
            if(inputByte[1] == 1) {
                inputChanged = true;
            }
            inputByte[1] = 0;
        }

        if(Input.GetKey(KeyCode.RightArrow)) {
            if(inputByte[2] == 0) {
                inputChanged = true;
            }
            inputByte[2] = 1;
        } else {
            if(inputByte[2] == 1) {
                inputChanged = true;
            }
            inputByte[2] = 0;
        }

        if(Input.GetKey(KeyCode.UpArrow)) {
            if(inputByte[3] == 0) {
                inputChanged = true;
            }
            inputByte[3] = 1;
        } else {
            if(inputByte[3] == 1) {
                inputChanged = true;
            }
            inputByte[3] = 0;
        }

        if(Input.GetKey(KeyCode.DownArrow)) {
            if(inputByte[4] == 0) {
                inputChanged = true;
            }
            inputByte[4] = 1;
        } else {
            if(inputByte[4] == 1) {
                inputChanged = true;
            }
            inputByte[4] = 0;
        }

        if(Input.GetKey(KeyCode.Z)) {
            if (Room.instance.id != 0)
            {
                if (inputByte[5] == 0)
                {
                    inputChanged = true;
                }
                inputByte[5] = 1;
            }
        } else {
            if(inputByte[5] == 1) {
                inputChanged = true;
            }
            inputByte[5] = 0;
        }

        if(Input.GetKey(KeyCode.X)) {
            if(inputByte[6] == 0) {
                inputChanged = true;
            }
            inputByte[6] = 1;
        } else {
            if(inputByte[6] == 1) {
                inputChanged = true;
            }
            inputByte[6] = 0;
        }

        if(Input.GetKey(KeyCode.A)) {
            if(inputByte[7] == 0) {
                inputChanged = true;
            }
            inputByte[7] = 1;
        } else {
            if(inputByte[7] == 1) {
                inputChanged = true;
            }
            inputByte[7] = 0;
        }

        if(Input.GetKey(KeyCode.C)) {
            if (Room.instance.id != 0)
            {
                if (inputByte[8] == 0)
                {
                    inputChanged = true;
                }
                inputByte[8] = 1;
            }
        } else {
            if(inputByte[8] == 1) {
                inputChanged = true;
            }
            inputByte[8] = 0;
        }
        
        if(isReady) {
            if(inputChanged == true && ChattingInput.isFocused == false) {
                ClientSocket.instance.SendRequest(inputByte);
            }
        }
    }
}
