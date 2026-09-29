static class QuestLogic
{
    public static bool CanFastAttack(bool knightIsAwake)
    {
        return !knightIsAwake;
    }
    public static bool CanSpy(bool knightIsAwake, bool archerIsAwake, bool prisonerIsAwake)
    {
        return knightIsAwake || archerIsAwake || prisonerIsAwake;
    }
    public static bool CanSignalPrisoner(bool archerIsAwake, bool prisonerIsAwake)
    {
        return prisonerIsAwake && !archerIsAwake;
    }
    public static bool CanFreePrisoner(bool knightIsAwake, bool archerIsAwake, bool prisonerIsAwake, bool petDogIsPresent)
    {
        bool withDog = petDogIsPresent && !archerIsAwake;
        bool withoutDog = !petDogIsPresent && prisonerIsAwake && !knightIsAwake && !archerIsAwake;
        return withDog || withoutDog;
    }
}
class Program
{
    static void Main()
    {
        Console.WriteLine(QuestLogic.CanSpy(false, false, true));
        Console.WriteLine(QuestLogic.CanSignalPrisoner(true, false));
        Console.WriteLine(QuestLogic.CanFreePrisoner(false, false, true, true));
        Console.WriteLine(QuestLogic.CanFreePrisoner(false, true, true, true));
    }
}