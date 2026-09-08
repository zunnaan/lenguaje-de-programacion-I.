using System;

var lasagna = new Lasagna();

Console.WriteLine("Minutos esperados en el horno: " + lasagna.ExpectedMinutesInOven());
Console.WriteLine("Minutos restantes (llevaba 30 min): " + lasagna.RemainingMinutesInOven(30));
Console.WriteLine("Tiempo de preparación (3 capas): " + lasagna.PreparationTimeInMinutes(3));
Console.WriteLine("Tiempo total transcurrido: " + lasagna.ElapsedTimeInMinutes(3, 30));

class Lasagna
{
    public int ExpectedMinutesInOven()
    {
        return 40;
    }

    public int RemainingMinutesInOven(int actualMinutes)
    {
        return ExpectedMinutesInOven() - actualMinutes;
    }

    public int PreparationTimeInMinutes(int layers)
    {
        return layers * 2;
    }

    public int ElapsedTimeInMinutes(int layers, int actualMinutesInOven)
    {
        return PreparationTimeInMinutes(layers) + actualMinutesInOven;
    }
}