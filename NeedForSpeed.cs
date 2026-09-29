class RemoteControlCar
{
    private int _speed;
    private int _batteryDrain;
    private int _distanceDriven;
    private int _batteryPercentage = 100;

    public RemoteControlCar(int speed, int batteryDrain)
    {
        _speed = speed;
        _batteryDrain = batteryDrain;
    }

    public bool BatteryDrained()
    {
        return _batteryPercentage < _batteryDrain;
    }

    public int DistanceDriven()
    {
        return _distanceDriven;
    }

    public void Drive()
    {
        if (!BatteryDrained())
        {
            _distanceDriven += _speed;
            _batteryPercentage -= _batteryDrain;
        }
    }

    public static RemoteControlCar Nitro()
    {
        return new RemoteControlCar(50, 4);
    }
}

class RaceTrack
{
    private int _distance;

    public RaceTrack(int distance)
    {
        _distance = distance;
    }

    public bool TryFinishTrack(RemoteControlCar car)
    {
        while (!car.BatteryDrained())
        {
            if (car.DistanceDriven() >= _distance)
            {
                return true;
            }

            car.Drive();
        }

        return car.DistanceDriven() >= _distance;
    }
}

class Program
{
    static void Main()
    {
        int speed = 5;
        int batteryDrain = 2;
        var car = new RemoteControlCar(speed, batteryDrain);
        car.Drive();
        Console.WriteLine(car.DistanceDriven());   // 5
        Console.WriteLine(car.BatteryDrained());   // False

        var nitro = RemoteControlCar.Nitro();
        nitro.Drive();
        Console.WriteLine(nitro.DistanceDriven()); // 50

        var track = new RaceTrack(100);
        var testCar = new RemoteControlCar(5, 2);
        Console.WriteLine(track.TryFinishTrack(testCar)); // True
    }
}
