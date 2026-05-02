using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Color Swap/Level Data")]
public class LevelData : ScriptableObject
{
    public string levelName = "Level";
    public int parMoves = 1;

    public List<NodeData> nodes = new List<NodeData>();
    public List<EdgeData> edges = new List<EdgeData>();
}