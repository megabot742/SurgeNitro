public enum UIType
{
    Unknown = 0,
    Screen = 1,
    Popup = 2,
    Notify = 3,
    Overlap = 4
}
//CarInfo setting
public enum CarInfoMode
{
    View,      //Only view 
    SelectForRace, //Enbale setup select race
    Buy        //Enable setup select buy
}
public class CarInfoData
{
    public CarInfoMode Mode { get; set; }
    public CarParam Car {get; set;} 
}
//Camera setting
public enum MenuCameraType
{
    Home,
    CarInfo,
    CarView
}
//Result race data
public class ResultData
{
    public int position;
    public float bestTime;
}
//CarStatType
public enum CarStatType
{
    TopSpeed,     // Km/h
    Acceleration, // Seconds (thời gian tăng tốc 0-100)
    Handling,     // Power (0-1 hoặc độ bám đường)
    Nitro         // Seconds (thời gian nitro)
}
public class UpgradeData
{
    public CarStatType StatType { get; set; }  // Enum để biết tab nào
    public CarParam Car { get; set; }  // Xe đang nâng cấp
}
public class UpgradePayload
{
    public CarInfoData InfoData;         // Chứa Mode + Car đầy đủ
    public CarStatType InitialStatType;  // Tab nào sẽ mở đầu tiên
}