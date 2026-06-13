using System.Globalization;
using System.Text.RegularExpressions;
using GukVoice.Models;

namespace GukVoice.Services;

public static class EqLogParser
{
    // ── Line wrapper ───────────────────────────────────────────────────────────
    // EQ format: [Mon Jan  1 00:00:00 2026] message body
    private static readonly Regex RxLine = new(
        @"^\[(\w{3} \w{3} [ \d]\d \d{2}:\d{2}:\d{2} \d{4})\] (.+)$",
        RegexOptions.Compiled);

    // ── Chat patterns ──────────────────────────────────────────────────────────
    // Others: "Name says, 'text'" / "Name shouts, 'text'" / "Name tells you, 'text'" etc.
    private static readonly Regex RxChatOther = new(
        @"^(.+?) (?:says?|shouts?|tells? you|tells? the group|says? out of character),? '(.+)'$",
        RegexOptions.Compiled);

    // Player "You say, 'text'" forms — speaker will be substituted with PlayerName
    private static readonly Regex RxChatYou = new(
        @"^You (?:say|shout|tell|say out of character|tell the group),? '(.+)'$",
        RegexOptions.Compiled);

    // ── Event patterns ─────────────────────────────────────────────────────────
    private static readonly Regex RxZone = new(
        @"^You have entered (.+)\.$", RegexOptions.Compiled);

    private static readonly Regex RxExp = new(
        @"^You gained (?:party |raid )?experience", RegexOptions.Compiled);

    private static readonly Regex RxLoot = new(
        @"^--(.+?) has looted (.+?) from .+", RegexOptions.Compiled);

    private static readonly Regex RxLootYou = new(
        @"^You receive .+? from", RegexOptions.Compiled);

    // ── Combat — damage dealt (melee verbs) ────────────────────────────────────
    private static readonly Regex RxDmgDealtMelee = new(
        @"^You (?:slash|pierce|crush|kick|punch|bite|bash|backstab|strike|claw|maul|gore|rend|frenzy on) (.+?) for (\d+) points? of damage\.$",
        RegexOptions.Compiled);

    // Combat — damage dealt (spell/non-melee)
    private static readonly Regex RxDmgDealtSpell = new(
        @"^You hit (.+?) for (\d+) points? of non-melee damage\.$",
        RegexOptions.Compiled);

    // Combat — damage taken (melee) — "YOU" is uppercase in EQ logs
    private static readonly Regex RxDmgTakenMelee = new(
        @"^(.+?) (?:slash|pierce|crush|kick|punch|bite|bash|backstab|hit|strike|claw|maul|gore|rend)s? YOU for (\d+) points? of damage\.$",
        RegexOptions.Compiled);

    // Combat — damage taken (spell)
    private static readonly Regex RxDmgTakenSpell = new(
        @"^(.+?) hit you for (\d+) points? of non-melee damage\.$",
        RegexOptions.Compiled);

    // Combat — kills
    private static readonly Regex RxYouSlew = new(
        @"^You have slain (.+)!$", RegexOptions.Compiled);

    private static readonly Regex RxSlainBy = new(
        @"^(.+?) has been slain by (.+?)!$", RegexOptions.Compiled);

    private static readonly Regex RxPlayerDied = new(
        @"^You have been slain by (.+?)!$", RegexOptions.Compiled);

    // ── Public entry point ─────────────────────────────────────────────────────

    public record ParseResult(LogEvent? LogEvent, CombatEvent? CombatEvent);

    public static ParseResult Parse(string rawLine, string playerName)
    {
        var m = RxLine.Match(rawLine);
        if (!m.Success) return new(null, null);

        var time = ParseTimestamp(m.Groups[1].Value);
        var body = m.Groups[2].Value;

        // Combat is checked first — those lines would also partially match chat
        var combat = TryCombat(body, time);
        if (combat != null) return new(null, combat);

        var logEvent = TryLogEvent(body, time, playerName);
        return new(logEvent, null);
    }

    // ── Log event parsing ──────────────────────────────────────────────────────

    private static LogEvent? TryLogEvent(string body, DateTime time, string playerName)
    {
        Match m;

        m = RxZone.Match(body);
        if (m.Success)
            return new LogEvent { Type = LogEventType.Zone, Time = time, Text = body };

        if (RxExp.IsMatch(body))
            return new LogEvent { Type = LogEventType.Experience, Time = time, Text = body };

        m = RxLoot.Match(body);
        if (m.Success)
            return new LogEvent { Type = LogEventType.Loot, Time = time, Text = body };

        if (RxLootYou.IsMatch(body))
            return new LogEvent { Type = LogEventType.Loot, Time = time, Text = body };

        m = RxChatOther.Match(body);
        if (m.Success)
            return new LogEvent { Type = LogEventType.Chat, Time = time,
                                  Speaker = m.Groups[1].Value, Text = m.Groups[2].Value };

        m = RxChatYou.Match(body);
        if (m.Success && !string.IsNullOrWhiteSpace(playerName))
            return new LogEvent { Type = LogEventType.Chat, Time = time,
                                  Speaker = playerName, Text = m.Groups[1].Value };

        return null;
    }

    // ── Combat parsing ─────────────────────────���───────────────────────────────

    private static CombatEvent? TryCombat(string body, DateTime time)
    {
        Match m;

        // Player death (check before RxSlainBy to avoid false match on "You")
        m = RxPlayerDied.Match(body);
        if (m.Success)
            return new CombatEvent { Type = CombatEventType.PlayerDeath, Time = time,
                                     Actor = m.Groups[1].Value };

        // You slew mob
        m = RxYouSlew.Match(body);
        if (m.Success)
            return new CombatEvent { Type = CombatEventType.MobDeath, Time = time,
                                     Target = m.Groups[1].Value };

        // X has been slain by Y
        m = RxSlainBy.Match(body);
        if (m.Success)
            return new CombatEvent { Type = CombatEventType.MobDeath, Time = time,
                                     Target = m.Groups[1].Value, Actor = m.Groups[2].Value };

        // Damage dealt — melee
        m = RxDmgDealtMelee.Match(body);
        if (m.Success && int.TryParse(m.Groups[2].Value, out int dmg))
            return new CombatEvent { Type = CombatEventType.DamageDealt, Time = time,
                                     Target = m.Groups[1].Value, Damage = dmg, Source = DamageSource.Melee };

        // Damage dealt — spell
        m = RxDmgDealtSpell.Match(body);
        if (m.Success && int.TryParse(m.Groups[2].Value, out dmg))
            return new CombatEvent { Type = CombatEventType.DamageDealt, Time = time,
                                     Target = m.Groups[1].Value, Damage = dmg, Source = DamageSource.Spell };

        // Damage taken — melee (YOU uppercase)
        m = RxDmgTakenMelee.Match(body);
        if (m.Success && int.TryParse(m.Groups[2].Value, out dmg))
            return new CombatEvent { Type = CombatEventType.DamageTaken, Time = time,
                                     Actor = m.Groups[1].Value, Damage = dmg, Source = DamageSource.Melee };

        // Damage taken — spell
        m = RxDmgTakenSpell.Match(body);
        if (m.Success && int.TryParse(m.Groups[2].Value, out dmg))
            return new CombatEvent { Type = CombatEventType.DamageTaken, Time = time,
                                     Actor = m.Groups[1].Value, Damage = dmg, Source = DamageSource.Spell };

        return null;
    }

    // ── Timestamp ──────────────────────────────────────────────────────────────

    private static DateTime ParseTimestamp(string ts)
    {
        // EQ pads single-digit days with a space: "Mon Jan  1 ..."
        var s = Regex.Replace(ts, @"\s+", " ").Trim();
        if (DateTime.TryParseExact(s, "ddd MMM d HH:mm:ss yyyy",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            return dt;
        return DateTime.Now;
    }
}
