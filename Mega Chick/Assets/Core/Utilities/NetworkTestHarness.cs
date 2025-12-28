using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simple test UI for network connection.
/// Why? Test networking without building full UI first.
/// Remove this later when real UI is built.
/// </summary>
public class NetworkTestHarness : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button connectButton;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;
    [SerializeField] private Button leaveRoomButton;
    [SerializeField] private TMP_InputField roomCodeInput;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI roomCodeText;
    
    private void Start()
    {
        // Setup buttons
        if (connectButton) connectButton.onClick.AddListener(OnConnectClicked);
        if (createRoomButton) createRoomButton.onClick.AddListener(OnCreateRoomClicked);
        if (joinRoomButton) joinRoomButton.onClick.AddListener(OnJoinRoomClicked);
        if (leaveRoomButton) leaveRoomButton.onClick.AddListener(OnLeaveRoomClicked);
        
        // Subscribe to events
        GameEventBus.OnConnectedToMaster += UpdateStatus;
        GameEventBus.OnJoinedLobby += UpdateStatus;
        GameEventBus.OnJoinedRoom += OnJoinedRoom;
        GameEventBus.OnLeftRoom += OnLeftRoom;
        
        UpdateStatus();
    }
    
    private void OnDestroy()
    {
        GameEventBus.OnConnectedToMaster -= UpdateStatus;
        GameEventBus.OnJoinedLobby -= UpdateStatus;
        GameEventBus.OnJoinedRoom -= OnJoinedRoom;
        GameEventBus.OnLeftRoom -= OnLeftRoom;
    }
    
    private void OnConnectClicked()
    {
        if (NetworkBootstrap.Instance != null)
        {
            NetworkBootstrap.Instance.Connect();
        }
    }
    
    private void OnCreateRoomClicked()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.CreateRoom();
        }
    }
    
    private void OnJoinRoomClicked()
    {
        if (RoomManager.Instance != null && roomCodeInput != null)
        {
            RoomManager.Instance.JoinRoom(roomCodeInput.text.ToUpper());
        }
    }
    
    private void OnLeaveRoomClicked()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.LeaveRoom();
        }
    }
    
    private void OnJoinedRoom()
    {
        if (roomCodeText != null && RoomManager.Instance != null)
        {
            roomCodeText.text = $"Room: {RoomManager.Instance.GetCurrentRoomCode()}";
        }
        UpdateStatus();
    }
    
    private void OnLeftRoom()
    {
        if (roomCodeText != null)
        {
            roomCodeText.text = "Not in room";
        }
        UpdateStatus();
    }
    
    private void UpdateStatus()
    {
        if (statusText == null) return;
        
        if (NetworkBootstrap.Instance == null)
        {
            statusText.text = "NetworkBootstrap not found!";
            return;
        }
        
        string status = "";
        if (NetworkBootstrap.Instance.IsConnected())
        {
            status += "Connected | ";
            if (NetworkBootstrap.Instance.IsInLobby()) status += "In Lobby | ";
            if (NetworkBootstrap.Instance.IsInRoom())
            {
                status += $"In Room ({RoomManager.Instance?.GetPlayerCount()}/{RoomManager.Instance?.GetMaxPlayers()})";
            }
        }
        else
        {
            status = "Disconnected";
        }
        
        statusText.text = status;
    }
}

