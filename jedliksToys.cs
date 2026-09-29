class RemoteControlCar
{
    private int _distanceDriven;
    private int _batteryPercentage = 100;

    public static RemoteControlCar Buy()
    {
        return new RemoteControlCar();
    }

    public string DistanceDisplay()
    {
        return $"Driven {_distanceDriven} meters";
    }

    public string BatteryDisplay()
    {
        if (_batteryPercentage == 0)
        {
            return "Battery empty";
        }

        return $"Battery at {_batteryPercentage}%";
    }

    public void Drive()
    {
        if (_batteryPercentage > 0)
        {
            _distanceDriven += 20;
            _batteryPercentage--;
        }
    }
}

class Program
{
    static void Main()
    {
        var car = RemoteControlCar.Buy();
        Console.WriteLine(car.DistanceDisplay());  // Driven 0 meters
        Console.WriteLine(car.BatteryDisplay());   // Battery at 100%

        for (int i = 0; i < 3; i++)
        {
            car.Drive();
        }
        Console.WriteLine(car.DistanceDisplay());  // Driven 60 meters
        Console.WriteLine(car.BatteryDisplay());   // Battery at 97%

        for (int i = 0; i < 100; i++)
        {
            car.Drive();
        }
        Console.WriteLine(car.DistanceDisplay());  // Driven 2000 meters
        Console.WriteLine(car.BatteryDisplay());   // Battery empty
    }
}