public enum UIType
{
    Unknown = 0,
    Screen = 1,
    Popup = 2,
    Notify = 3,
    Overlap = 4
}
public enum CarInfoMode
{
    View,      // Luồng 1: Chỉ xem, không bật gì, back về Garage
    SelectForRace, // Luồng 2: Bật Setup, button Setup chọn xe đua, back về Garage
    Buy        // Luồng 3: Bật Buy, button Buy mua xe, back về Shop
}
public class CarInfoData
{
    public CarInfoMode Mode { get; set; }
    public string CarId { get; set; } // Optional: ID xe để load data
    // Thêm nếu cần: Action onConfirm, onBack (nhưng dùng event tốt hơn)
}

public enum MenuCameraType
{
    Home,
    CarInfo,
    CarView
}