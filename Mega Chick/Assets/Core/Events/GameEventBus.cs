using System;
using UnityEngine;

/// <summary>
/// Central event bus for game events.
/// Why? Decouples systems - PlayerController doesn't need to know about ScoreManager.
/// </summary>
public static class GameEventBus
{
    // Connection Events
    public static event Action OnConnectedToMaster;
    public static event Action OnDisconnected;
    public static event Action OnJoinedLobby;
    public static event Action OnLeftLobby;
    
    // Room Events
    public static event Action OnJoinedRoom;
    public static event Action OnLeftRoom;
    public static event Action<string> OnPlayerEnteredRoom; // actorNumber
    public static event Action<string> OnPlayerLeftRoom; // actorNumber
    
    // Match Flow Events
    public static event Action OnMatchStateChanged; // Use MatchFlowController for state
    public static event Action OnCountdownStarted;
    public static event Action OnMatchStarted;
    public static event Action OnMatchEnded;
    
    // Player Events
    public static event Action<int> OnPlayerSpawned; // actorNumber
    public static event Action<int> OnPlayerKOed; // actorNumber
    public static event Action<int> OnPlayerRespawned; // actorNumber
    
    // Invocation methods (call these to fire events)
    public static void FireConnectedToMaster() => OnConnectedToMaster?.Invoke();
    public static void FireDisconnected() => OnDisconnected?.Invoke();
    public static void FireJoinedLobby() => OnJoinedLobby?.Invoke();
    public static void FireLeftLobby() => OnLeftLobby?.Invoke();
    public static void FireJoinedRoom() => OnJoinedRoom?.Invoke();
    public static void FireLeftRoom() => OnLeftRoom?.Invoke();
    public static void FirePlayerEnteredRoom(string actorNumber) => OnPlayerEnteredRoom?.Invoke(actorNumber);
    public static void FirePlayerLeftRoom(string actorNumber) => OnPlayerLeftRoom?.Invoke(actorNumber);
    public static void FireMatchStateChanged() => OnMatchStateChanged?.Invoke();
    public static void FireCountdownStarted() => OnCountdownStarted?.Invoke();
    public static void FireMatchStarted() => OnMatchStarted?.Invoke();
    public static void FireMatchEnded() => OnMatchEnded?.Invoke();
    public static void FirePlayerSpawned(int actorNumber) => OnPlayerSpawned?.Invoke(actorNumber);
    public static void FirePlayerKOed(int actorNumber) => OnPlayerKOed?.Invoke(actorNumber);
    public static void FirePlayerRespawned(int actorNumber) => OnPlayerRespawned?.Invoke(actorNumber);
}

