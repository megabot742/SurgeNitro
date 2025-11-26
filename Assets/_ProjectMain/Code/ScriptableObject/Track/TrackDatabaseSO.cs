using UnityEngine;

[CreateAssetMenu]
public class TrackDatabaseSO : ScriptableObject
{
    public TrackSO[] tracksSO;

    public int TrackCount
    {
        get
        {
            return tracksSO.Length;
        }
    }

    public TrackSO GetTrackSO(int index)
    {
        return tracksSO[index];
    }
}
