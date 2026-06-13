namespace GukVoice.Models;

public enum CombatEventType { DamageDealt, DamageTaken, MobDeath, PlayerDeath }
public enum DamageSource    { Melee, Spell }

public class CombatEvent
{
    public CombatEventType Type   { get; init; }
    public DateTime        Time   { get; init; }
    public string          Actor  { get; init; } = "";
    public string          Target { get; init; } = "";
    public int             Damage { get; init; }
    public DamageSource    Source { get; init; }
}
