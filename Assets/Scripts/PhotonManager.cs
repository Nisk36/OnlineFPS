using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;//�����̍쐬�ɕK�v


public class PhotonManager : MonoBehaviourPunCallbacks
{
    public static PhotonManager instance;

    public GameObject loadingPanel;
    public Text loadingText;

    public GameObject buttons;

    public GameObject createRoomPanel;
    public Text createdRoomName;

    public GameObject roomPanel;
    public Text roomName;
    public GameObject startButton;

    public GameObject errorPanel;
    public Text errorText;

    public GameObject roomListPanel;

    public Room originalRoomButton;

    public GameObject roomButtonContent;

    Dictionary<string, RoomInfo> roomsDic = new Dictionary<string, RoomInfo>();
    private List<Room> allRoomButtons = new List<Room>();

    public Text playerNameText;
    private List<Text> allPlayerNames = new List<Text>();

    public GameObject playerNameContent;

    public GameObject nameInputPanel;
    public Text placeholderText;
    public InputField nameInput;
    private bool setName;


    public string playLevel;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        CloseMenuUI();

        loadingPanel.SetActive(true);
        loadingText.text = "�T�[�o�[�ɐڑ���...";

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    private void Update()
    {
        Debug.Log(PhotonNetwork.IsConnected);
    }


    public void CloseMenuUI()
    {
        buttons.SetActive(false);

        loadingPanel.SetActive(false);

        createRoomPanel.SetActive(false);

        roomPanel.SetActive(false);

        errorPanel.SetActive(false);

        roomListPanel.SetActive(false);

        nameInputPanel.SetActive(false);
    }

    public void DisplayLobbyUI()
    {
        CloseMenuUI();
        buttons.SetActive(true);
    }

    //�}�X�^�[�T�[�o�[�ɐڑ����ꂽ�Ƃ��ɌĂ΂��֐�
    public override void OnConnectedToMaster()
    {
        Debug.Log("�}�X�^�[�ɐڑ�");
        //���r�[�ɐڑ�
        PhotonNetwork.JoinLobby();
        loadingText.text = "���r�[�ɐڑ���";

        //Master Client�Ɠ������x�������[�h
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    //���r�[�ڑ����ɌĂ΂��֐�
    public override void OnJoinedLobby()
    {
        DisplayLobbyUI();
        //roomDic������
        roomsDic.Clear();

        PhotonNetwork.NickName = "Player" + Random.RandomRange(0, 1000).ToString();

        ConfirmationName();
    }

    public void DisplayCreateRoomUI()
    {
        CloseMenuUI();
        Debug.Log("CreateRoom");
        createRoomPanel.SetActive(true);
    }

    //���[���쐬�{�^���p�̊֐�
    public void CreateRoomButton()
    {
        if (!string.IsNullOrEmpty(createdRoomName.text))
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 8;//�����łł�20�l

            PhotonNetwork.CreateRoom(createdRoomName.text, options);
            Debug.Log(createdRoomName.text);

            CloseMenuUI();

            loadingPanel.SetActive(true);
            loadingText.text = "���[���쐬��...";
        }
    }

    //���[���Q�����ɌĂ΂��֐�
    public override void OnJoinedRoom()
    {
        Debug.Log("joined room");

        CloseMenuUI();
        roomPanel.SetActive(true);
        Debug.Log(PhotonNetwork.CurrentRoom.Name);
        roomName.text = PhotonNetwork.CurrentRoom.Name;//�Q�����Ă��郋�[���̖��O�擾

        GetAllPlayers();

        //���[���}�X�^�[���ǂ������肵�ă{�^���\��
        CheckRoomMaster();

    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();

        CloseMenuUI();
        loadingText.text = "���[���ޏo��...";
        loadingPanel.SetActive(true);
    }

    //���[�����痣�ꂽ���ɌĂ΂��֐�
    public override void OnLeftRoom()
    {
        DisplayLobbyUI();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        CloseMenuUI();
        errorText.text = "Failed to create room." + message;

        errorPanel.SetActive(true);
    }

    public void FindRoom()
    {
        CloseMenuUI();
        roomListPanel.SetActive(true);

    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)//���[�����X�g�ɍX�V�����������ɌĂ΂��֐�
    {
        //���[���{�^��UI������
        RoomUIInitialize();
        UpdateRoomDic(roomList);
    }

    public void UpdateRoomDic(List<RoomInfo> roomList)
    {
        for(int i = 0; i < roomList.Count; i++)
        {
            RoomInfo info = roomList[i];

            if (info.RemovedFromList)//�����Ƃ���������Ture���Ԃ��Ă���
            {
                roomsDic.Remove(info.Name);
            }
            else
            {
                roomsDic[info.Name] = info;
            }
        }

        RoomsDicDisplay(roomsDic);
    }

    public void RoomsDicDisplay(Dictionary<string,RoomInfo> roomsDic)
    {
        foreach(var roomInfo in roomsDic)
        {
            //�{�^������
            Room newButton = Instantiate(originalRoomButton);
            //���������{�^���Ƀ��[������ݒ�
            newButton.RegisterRoomDetails(roomInfo.Value);
            //�e�̐ݒ�
            newButton.transform.SetParent(roomButtonContent.transform);

            allRoomButtons.Add(newButton);
        }
    }

    public void RoomUIInitialize()
    {
        foreach(Room roomButton in allRoomButtons)
        {
            Destroy(roomButton.gameObject);
        }
        //���X�g�̏�����
        allRoomButtons.Clear();
    }

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);

        CloseMenuUI();
        loadingText.text = "���[���Q����";
        loadingPanel.SetActive(true);
    }

    public void GetAllPlayers()
    {
        InitializePlayerList();

        DisplayPlayers();
    }

    public void InitializePlayerList()
    {
        foreach(var playerName in allPlayerNames)
        {
            Destroy(playerName.gameObject);
        }

        allPlayerNames.Clear();
    }

    public void DisplayPlayers()
    {
        //���[���ɎQ�����Ă���l����UI�쐬
        foreach(var players in PhotonNetwork.PlayerList)
        {
            //UI����
            GeneratePlayerText(players);
        }
    }

    public void GeneratePlayerText(Player players)
    {
        Text newPlayerText = Instantiate(playerNameText);
        newPlayerText.text = players.NickName;
        newPlayerText.transform.SetParent(playerNameContent.transform);

        allPlayerNames.Add(newPlayerText);
    }


    public void ConfirmationName()
    {
        if (!setName)//�N����false
        {
            CloseMenuUI();
            nameInputPanel.SetActive(true);

            if (PlayerPrefs.HasKey("playerName"))//�ߋ��̃v���C�ŕۑ��ς݂�������(playerName��PlayerPrefs��Key)
            {
                placeholderText.text = PlayerPrefs.GetString("playerName");
                nameInput.text = PlayerPrefs.GetString("playerName");
            }
        }
        else//���[���ɓ����Ĕ��������Ƃ��ɍēx���͖h��
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("playerName");
        }
    }

    //���O�o�^
    public void SetName()
    {
        if (!string.IsNullOrEmpty(nameInput.text))
        {
            //���[�U���o�^
            PhotonNetwork.NickName = nameInput.text;

            //�ۑ�
            PlayerPrefs.SetString("playerName", nameInput.text);

            DisplayLobbyUI();

            setName = true;
        }
    }

    //�v���C���[�����[���ɓ�������(override)
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        GeneratePlayerText(newPlayer);
    }

    //�v���C���[�����[���𗣂�邩�A��A�N�e�B�u�ɂȂ�����(overide)
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        GetAllPlayers();
    }

    public void CheckRoomMaster()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }
    }

    //�}�X�^�[���؂�ւ������(override)
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
    }

    //�J�ڊ֐�
    public void PlayGame()
    {
        PhotonNetwork.LoadLevel(playLevel);
    }
}