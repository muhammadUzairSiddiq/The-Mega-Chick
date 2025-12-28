# Step 02 - Photon Integration ✅

## What Was Created

1. **GameEventBus.cs** - Central event system
2. **NetworkBootstrap.cs** - Photon connection manager
3. **RoomManager.cs** - Room creation/joining
4. **NetworkTestHarness.cs** - Test UI helper

## Quick Setup (3 Steps)

### 1. Install Photon PUN2
- Unity will auto-install from manifest.json
- OR: Window → Package Manager → Add package from git URL:
  ```
  https://github.com/ExitGames/PhotonUnityNetworking.git?path=/PhotonUnityNetworking
  ```

### 2. Get Photon App ID
- Go to: https://dashboard.photonengine.com/
- Create account → Create app → Copy App ID
- Window → Photon Unity Networking → Highlight Server Settings
- Paste App ID
- Set Region (e.g., "us", "eu")

### 3. Create Test Scene
- Create empty GameObject → Add `NetworkBootstrap` component
- Assign `NetworkConfig` asset to NetworkBootstrap
- Create another GameObject → Add `RoomManager` component
- Assign `NetworkConfig` asset to RoomManager
- (Optional) Add `NetworkTestHarness` for test buttons

## Why This Architecture?

**GameEventBus (Static Events)**
- ✅ Pros: Decoupled, easy to subscribe, no references needed
- ❌ Cons: Can't see subscribers in Inspector, memory leaks if not unsubscribed

**NetworkBootstrap (Singleton)**
- ✅ Pros: One connection manager, accessible anywhere, DontDestroyOnLoad
- ❌ Cons: Global state, harder to test

**RoomManager (Separate from Bootstrap)**
- ✅ Pros: Single responsibility, easy to test rooms separately
- ❌ Cons: Two managers to coordinate

## Test It

1. Press Play
2. Click "Connect" button (if using TestHarness)
3. Click "Create Room" → See room code
4. Open second Unity instance → Click "Join Room" with code
5. Both should see each other in room!

## Next: Step 03 - Match Flow Controller

