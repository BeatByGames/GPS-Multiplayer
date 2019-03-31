using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.Multiplayer;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using System.Text;

public class authorization : MonoBehaviour, RealTimeMultiplayerListener
{

    public string namePlayer = "none name";
    public int players = 0;
    public string myID = "", participantsText = "", newParticipantsText = "";
    public GameObject startGame, leftGame, signIn, signOut, playersObj, searchPlayers, gameStarted,
        spawnCharacter;
    public Text curPlayers, myName, inputText, reliableTextButton, reciveMessage;
    public bool isSearchPlayers, isReliable, isReadyGame, isGameStarted;
    public GameObject[] characters = new GameObject[3];
    public Dropdown charactersValue;
    public GameObject player, enamy;
    public Transform mySpawnPoint, participantSpawnPoint;

    const int MinOpponents = 1, MaxOpponents = 1, GameVariant = 0;


    private void Awake()
    {
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder().Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.Activate();
    }
    public void SignIn () {
    
        signIn.SetActive(false);
        Social.localUser.Authenticate((bool success) => {
            if (success)
            {
                myName.text = "" + Social.localUser.userName;
                myName.gameObject.SetActive(true);
                signIn.SetActive(false);
                signOut.SetActive(true);
                spawnCharacter.SetActive(true);
            }
            else
            {
                signIn.SetActive(true);
            }
        });
    }

    public void SignOut()
    {
        signOut.SetActive(false);
        startGame.SetActive(false);
        myName.text = "";
        myName.gameObject.SetActive(false);
        playersObj.SetActive(false);
        PlayGamesPlatform.Instance.SignOut();
        signIn.SetActive(true);
        OnLeftRoom();
    }

    public void StartGame()
    {
        PlayGamesPlatform.Instance.RealTime.CreateQuickGame(MinOpponents, MaxOpponents, GameVariant, this);
        searchPlayers.SetActive(true);
        startGame.SetActive(false);
        leftGame.SetActive(true);
        playersObj.SetActive(true);
        isSearchPlayers = true;
    }

    public void LeftGame ()
    {
        PlayGamesPlatform.Instance.RealTime.LeaveRoom();
        OnLeftRoom();
    }

	void Update () {

        if (isSearchPlayers)
        {
            if (players == MaxOpponents + 1)
            {
                searchPlayers.SetActive(false);
                gameStarted.SetActive(true);
                isReadyGame = true;
                isSearchPlayers = false;
                PlayGamesPlatform.Instance.RealTime.SendMessageToAll(true,
                    Encoding.UTF8.GetBytes("EP#" + charactersValue.value + "#"
                    + player.transform.position.x + "^" + player.transform.position.y + "^" + player.transform.position.z));
            }
        }
    }

    public void OnRoomSetupProgress(float percent)
    {

    }

    public void OnRoomConnected(bool success)
    {
        if (success)
        {
            List<Participant> participants = PlayGamesPlatform.Instance.RealTime.GetConnectedParticipants();
            for (int i = 0; i < participants.Count; i++)
            {
                participantsText += participants[i].ParticipantId + " -   ";
            }
            Participant myself = PlayGamesPlatform.Instance.RealTime.GetSelf();
            myID = "My ID: " + myself.ParticipantId;
            players = participants.Count;
            curPlayers.text = players + "";
        }
        else
        {

        }
    }

    public void OnLeftRoom()
    {
        searchPlayers.SetActive(false);
        startGame.SetActive(true);
        leftGame.SetActive(false);
        gameStarted.SetActive(false);
    }

    public void OnParticipantLeft(Participant participant)
    {
        throw new System.NotImplementedException();
    }

    public void OnPeersConnected(string[] participantIds)
    {
        players = participantIds.Length;
        curPlayers.text = players + "";
    }

    public void OnPeersDisconnected(string[] participantIds)
    {
        players = participantIds.Length;
        curPlayers.text = players + "";
    }

    public void ReliableMessage()
    {
        isReliable = !isReliable;
        if (isReliable)
        {
            reliableTextButton.text = "Надежное сообщение";
        }
        else
        {
            reliableTextButton.text = "Ненадежное сообщение";
        }
    }

    public void SendMessage()
    {
        byte[] data;
        data = Encoding.UTF8.GetBytes(inputText.text);

        PlayGamesPlatform.Instance.RealTime.SendMessageToAll(isReliable, data);
        inputText.text = "";
    }

    public void OnRealTimeMessageReceived(bool isReliable, string senderId, byte[] data)
    {
        string reciveText;
        reciveText = reciveMessage.text = Encoding.UTF8.GetString(data);
        string[] parameters;
        parameters = reciveText.Split('#');

        if (parameters[0] == "EP")
        {
            int idCharacter = int.Parse(parameters[1]);
            string[] pos = parameters[2].Split('^');
            Vector3 newEnemyPos;
            newEnemyPos = new Vector3(float.Parse(pos[0]), participantSpawnPoint.position.y, float.Parse(pos[2]));
            if (enamy == null)
            {
                enamy = (GameObject)Instantiate(characters[idCharacter], newEnemyPos, Quaternion.Euler(0, 0, 0));
            }
            else
            {
                enamy.transform.position = newEnemyPos;
            }
        }
    }

    public void SpawnCharacter()
    {
        player = (GameObject)Instantiate(characters[charactersValue.value], mySpawnPoint.position, Quaternion.Euler(0, 0, 0));
        spawnCharacter.SetActive(false);
        startGame.SetActive(true);
    }

    public void RunCharacter ()
    {
        player.transform.position += new Vector3(Time.deltaTime * 2, 0, 0);
        PlayGamesPlatform.Instance.RealTime.SendMessageToAll(true,
            Encoding.UTF8.GetBytes("EP#" + charactersValue.value + "#"
            + player.transform.position.x + "^" + player.transform.position.y + "^" + player.transform.position.z));
    }
}


    