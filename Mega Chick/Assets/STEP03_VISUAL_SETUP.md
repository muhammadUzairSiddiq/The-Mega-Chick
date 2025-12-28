# Step 03 - Visual Test Setup

## Quick Setup (5 minutes)

### 1. Create Test Scene
- Create new scene: `Assets/Maps/Scenes/TestMatchFlow.unity`
- Or use existing SampleScene

### 2. Create UI Canvas
- GameObject → UI → Canvas
- Set Canvas Scaler: Scale With Screen Size
- Reference Resolution: 1920x1080

### 3. Add MatchFlowController
- Create empty GameObject: "Match Flow Controller"
- Add component: `MatchFlowController`
- Assign `MatchConfig` asset to Config field

### 4. Create Test UI
- Create empty GameObject under Canvas: "Match Flow Test UI"
- Add component: `MatchFlowTestUI`

### 5. Setup UI Elements

#### State Display Panel
- Create Panel: "State Display"
- Add TextMeshPro (TMP): "Current State Text"
- Add TextMeshPro: "Elapsed Time Text"
- Add TextMeshPro: "Remaining Time Text"
- Assign to MatchFlowTestUI component

#### Buttons
- Create Button: "Start Match" → Assign to Start Match Button
- Create Button: "End Match" → Assign to End Match Button
- Create Button: "Return to Lobby" → Assign to Return to Lobby Button

#### Countdown Display
- Create Panel: "Countdown Panel" (centered, large)
- Add TextMeshPro: "Countdown Text" (size 100, centered)
- Assign to MatchFlowTestUI component
- Initially: Set active = false

#### Timer Display
- Create Panel: "Timer Panel" (top-right)
- Add TextMeshPro: "Timer Text" (size 50)
- Assign to MatchFlowTestUI component
- Initially: Set active = false

### 6. Test It!
1. Press Play
2. Click "Start Match" → See countdown (3..2..1..GO)
3. Watch timer count down
4. Click "End Match" → See results
5. Click "Return to Lobby" → Back to lobby

## What You'll See

- **Lobby State:** Buttons enabled, no countdown/timer
- **Countdown:** Large numbers (3..2..1..GO) in center
- **Playing:** Timer in top-right counting down
- **Results:** Results state, return button enabled

## Quick Alternative (Minimal)

If you just want to see it work quickly:

1. Add `MatchFlowController` to scene
2. Assign `MatchConfig`
3. In Play mode, open Console
4. Call from code:
   ```csharp
   MatchFlowController.Instance.StartMatch();
   ```
5. Watch Console logs for state changes

## Why Visual Test?

- **See it work:** Visual feedback confirms system works
- **Debug easier:** See state changes in real-time
- **Test flow:** Verify countdown → playing → results works
- **Find bugs:** Visual issues easier to spot

