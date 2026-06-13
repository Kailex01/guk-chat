namespace GukVoice.ViewModels;

public class FightRecord
{
    public string   Target           { get; init; } = "";
    public DateTime StartTime        { get; init; }
    public DateTime EndTime          { get; init; }
    public int      MeleeDmgDealt    { get; init; }
    public int      SpellDmgDealt    { get; init; }
    public int      MeleeDmgTaken    { get; init; }
    public int      SpellDmgTaken    { get; init; }
    public bool     Won              { get; init; }

    public int    TotalDealt        => MeleeDmgDealt + SpellDmgDealt;
    public int    TotalTaken        => MeleeDmgTaken + SpellDmgTaken;
    public double DurationSeconds   => Math.Max(1, (EndTime - StartTime).TotalSeconds);
    public int    DpsOut            => (int)(TotalDealt / DurationSeconds);
    public int    DpsIn             => (int)(TotalTaken / DurationSeconds);

    public string Duration => $"{(int)DurationSeconds}s";
    public string Result   => Won ? "WIN" : "DIED";
    public string Summary  =>
        $"{Target}  {TotalDealt:N0} dealt / {TotalTaken:N0} taken  |  {DpsOut} DPS out  |  {Duration}  [{Result}]";
}
