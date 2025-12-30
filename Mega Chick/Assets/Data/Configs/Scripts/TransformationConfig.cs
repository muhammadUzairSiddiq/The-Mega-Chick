using UnityEngine;

/// <summary>
/// Configuration for transformation/megafication system.
/// Used across multiple modes (FFA, Zone, Carry, Hunter).
/// </summary>
[CreateAssetMenu(fileName = "TransformationConfig", menuName = "Mega Chick/Configs/Transformation Config")]
public class TransformationConfig : ScriptableObject
{
    [Header("Transformation Requirements")]
    [Tooltip("Points required to transform (bar fill, no number shown)")]
    [Min(1)]
    public int transformationPointsRequired = 20;
    
    [Tooltip("Enable transformation system (menu configurable on/off)")]
    public bool enableTransformation = true;
    
    [Header("Point Gains (Menu Configurable)")]
    [Tooltip("Points gained when getting attacked")]
    [Min(0)]
    public int pointsForAttacked = 2;
    
    [Tooltip("Points gained when getting knocked down")]
    [Min(0)]
    public int pointsForKnockedDown = 5;
    
    [Tooltip("Points gained when attacking and hitting")]
    [Min(0)]
    public int pointsForAttacking = 1;
    
    [Tooltip("Points gained when knocking down a player")]
    [Min(0)]
    public int pointsForKnockingDown = 3;
    
    [Header("Visual")]
    [Tooltip("Show transformation bar (without numbers)")]
    public bool showTransformationBar = true;
}

