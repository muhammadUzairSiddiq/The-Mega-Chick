# Fix Pink/Magenta Materials - Quick Guide

## Why Pink/Magenta?
Pink/magenta = **Missing or incompatible shader**. Materials are using Built-in shaders that don't work with URP.

## Quick Fix (2 Methods)

### Method 1: Use Conversion Tool (Easiest)

1. **Open Conversion Tool:**
   - Menu: **Mega Chick → Convert Materials to URP**

2. **Select Materials:**
   - In Project window, select all pink materials:
     - `Super_Chick.mat`
     - `Super_Chick_Chicken.mat`
     - `Super_Chick_Forest.mat`
     - `Super_Chick_Black.mat`
     - `Super_Chick_Gold.mat`
     - `Floor_beach.mat` (if exists)

3. **Convert:**
   - Click "Find Selected Materials"
   - Click "Convert All to URP Lit"
   - Done!

### Method 2: Manual Fix (If tool doesn't work)

1. **Select Material:**
   - Click on `Super_Chick.mat` in Project window

2. **Change Shader:**
   - In Inspector, click **Shader** dropdown
   - Select: **Universal Render Pipeline → Lit**

3. **Reassign Textures:**
   - Find texture files (usually in `Textures` folder)
   - Drag texture to **Base Map** slot
   - Adjust **Base Color** if needed

4. **Repeat** for all materials

## Verify Fix

- Materials should show proper colors (not pink)
- Characters should render correctly
- Check Console for shader errors

## If Still Pink

1. **Check Shader Errors:**
   - Window → Analysis → Shader Compiler
   - Look for errors

2. **Verify URP Setup:**
   - Edit → Project Settings → Graphics
   - Should show URP Asset assigned

3. **Reimport Materials:**
   - Right-click material → Reimport

## Why This Happens

- **Built-in shaders** don't work with URP
- **Custom shaders** from asset packs may not support URP
- **Missing shader files** = pink material

## Prevention

- Always use **URP-compatible** asset packs
- Convert materials immediately after import
- Use URP shaders: Lit, Unlit, Simple Lit

