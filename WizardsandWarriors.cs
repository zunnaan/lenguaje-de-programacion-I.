abstract class Character
{
    private string _characterType;

    protected Character(string characterType)
    {
        _characterType = characterType;
    }

    public abstract int DamagePoints(Character target);

    public virtual bool Vulnerable()
    {
        return false;
    }

    public override string ToString()
    {
        return $"Character is a {_characterType}";
    }
}

class Warrior : Character
{
    public Warrior() : base("Warrior")
    {
    }

    public override int DamagePoints(Character target)
    {
        return target.Vulnerable() ? 10 : 6;
    }
}

class Wizard : Character
{
    private bool _spellPrepared;

    public Wizard() : base("Wizard")
    {
    }

    public override int DamagePoints(Character target)
    {
        return _spellPrepared ? 12 : 3;
    }

    public void PrepareSpell()
    {
        _spellPrepared = true;
    }

    public override bool Vulnerable()
    {
        return !_spellPrepared;
    }
}

class Program
{
    static void Main()
    {
        var warrior = new Warrior();
        var wizard = new Wizard();

        Console.WriteLine(warrior.ToString());          // Character is a Warrior
        Console.WriteLine(warrior.Vulnerable());         // False

        Console.WriteLine(wizard.Vulnerable());          // True (no ha preparado hechizo)
        wizard.PrepareSpell();
        Console.WriteLine(wizard.Vulnerable());          // False

        Console.WriteLine(wizard.DamagePoints(warrior)); // 12 (hechizo preparado)
        Console.WriteLine(warrior.DamagePoints(wizard));  // 6 (wizard ya no vulnerable)
    }
}
