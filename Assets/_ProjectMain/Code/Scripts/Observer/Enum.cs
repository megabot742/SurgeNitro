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
    public string CarId { get; set; } // Optional: ID xe để load data
    // Thêm nếu cần: Action onConfirm, onBack (nhưng dùng event tốt hơn)
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