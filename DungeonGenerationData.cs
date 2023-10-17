using UnityEngine;

[CreateAssetMenu(fileName ="DungeonGenerationData.asset",menuName ="DungeonGenerationData/Dungeon Data")]
public class DungeonGenerationData : ScriptableObject
{
    public int numOfCrawlers;
    public int iterationMin;
    public int iterationMax;

}
