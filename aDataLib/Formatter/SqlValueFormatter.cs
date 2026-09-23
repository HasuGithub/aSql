using System.Globalization;

namespace aDataLib.Formatter;

public static class SqlValueFormatter
{
  /// <summary>
  ///   Formatiert eine menschliche String-Eingabe basierend auf dem Ziel-Datentyp der Spalte in ein SQL-konformes Literal.
  /// </summary>
  /// <param name="userInput">Die rohe Eingabe aus der UI (z.B. Textbox).</param>
  /// <param name="rdbms">Das Ziel-Datenbanksystem.</param>
  /// <param name="targetType">Der erwartete Datentyp der Tabellenspalte.</param>
  /// <param name="useSimpleDate">Einfaches Datums-Format verwenden</param>
  public static string FormatUserValueForWhere(object? userInput, DataBaseTypes rdbms, DbFieldType targetType,
    bool useSimpleDate = false)
  {
    // 1. Wenn die Eingabe leer ist oder explizit "NULL" eingegeben wurde 
    if (userInput is null || string.IsNullOrWhiteSpace(userInput.ToString()) ||
        userInput.ToString()!.Trim().Equals("NULL", StringComparison.OrdinalIgnoreCase)) return "NULL";

    var trimmedInput = userInput.ToString()!.Trim();

    return targetType switch
    {
      // 2. Texte und Zeichenketten 
      DbFieldType.String => FormatString(trimmedInput, rdbms),

      // 3. Ganzzahlen (Validierung, um SQL-Injections via Zahlenfeldern zu blockieren) 
      DbFieldType.Int16 or DbFieldType.Int32 or DbFieldType.Int64 => long.TryParse(trimmedInput, NumberStyles.Integer,
        CultureInfo.InvariantCulture, out _)
        ? trimmedInput
        : throw new FormatException($"'{trimmedInput}' ist keine gültige Ganzzahl."),

      // 4. Dezimalzahlen (Erlaubt Punkt und Komma bei der Eingabe, erzwingt im SQL aber den invarianten Punkt) 
      DbFieldType.Decimal => ParseAndFormatDecimal(trimmedInput),

      // 5. Wahrheitswerte (Menschliche Eingaben wie true, false, 1, 0, ja, nein flexibel parsen) 
      DbFieldType.Boolean => FormatBoolean(trimmedInput, rdbms),

      // 6. Datums- und Zeitwerte 
      DbFieldType.DateTime => FormatDateTime(trimmedInput, useSimpleDate),
      DbFieldType.DateOnly => FormatDateOnly(trimmedInput, useSimpleDate),
      DbFieldType.TimeOnly => FormatTimeOnly(trimmedInput),
      DbFieldType.DateTimeOffset => FormatDateTimeOffset(trimmedInput, rdbms),

      // 7. Eindeutige IDs (GUIDs) 
      DbFieldType.Guid => FormatGuid(trimmedInput, rdbms),

      _ => throw new NotSupportedException($"Der Ziel-Datentyp {targetType} wird nicht unterstützt.")
    };
  }

  private static string FormatString(string input, DataBaseTypes rdbms)
  {
    var escaped = input.Replace("'", "''");
    return rdbms switch
    {
      _ => $"'{escaped}'"
    };
  }

  private static string ParseAndFormatDecimal(string input)
  {
    // Ersetzt Komma durch Punkt, falls der Anwender im deutschen Format (12,34) eingibt 
    var normalized = input.Replace(',', '.');

    return decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec)
      ? dec.ToString(CultureInfo.InvariantCulture)
      : throw new FormatException($"'{input}' ist keine gültige Dezimalzahl.");
  }

  private static string FormatBoolean(string input, DataBaseTypes rdbms)
  {
    var isTrue = input.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                 input.Equals("1") ||
                 input.Equals("ja", StringComparison.OrdinalIgnoreCase) ||
                 input.Equals("yes", StringComparison.OrdinalIgnoreCase);

    return rdbms switch
    {
      DataBaseTypes.Oracle or DataBaseTypes.MsSqlServer => isTrue ? "1" : "0",
      DataBaseTypes.MySql or DataBaseTypes.Maria => isTrue ? "true" : "false",
      _ => isTrue ? "1" : "0"
    };
  }

  private static string FormatDateTime(string input, bool useSimpleDate = false)
  {
    if (useSimpleDate) return FormatSimpleDate(input);

    // Flexibles Parsen gängiger Formate (ISO, Deutsch, etc.) 
    if (DateTime.TryParse(input, CultureInfo.CurrentCulture, out var dt) ||
        DateTime.TryParse(input, CultureInfo.InvariantCulture, out dt))
      return $"CAST('{dt:dd.MM.yyyy HH:mm:ss.fff}' AS DATE)";
    throw new FormatException($"'{input}' konnte nicht als Datum/Uhrzeit interpretiert werden.");
  }

  private static string FormatDateOnly(string input, bool useSimpleDate = false)
  {
    if (useSimpleDate) return FormatSimpleDate(input);


    if (!DateOnly.TryParse(input, CultureInfo.CurrentCulture, out var date) &&
        !DateOnly.TryParse(input, CultureInfo.InvariantCulture, out date))
      throw new FormatException($"'{input}' konnte nicht als reines Datum interpretiert werden.");

    return $"CAST('{date:dd.MM.yyyy}' AS DATE)";
  }

  private static string FormatSimpleDate(string input)
  {
    if (!DateOnly.TryParse(input, CultureInfo.CurrentCulture, out var date) &&
        !DateOnly.TryParse(input, CultureInfo.InvariantCulture, out date))
      throw new FormatException($"'{input}' konnte nicht als reines Datum interpretiert werden.");

    return $"CAST('{date:dd.MM.yyyy}' AS DATE)";
  }

  private static string FormatTimeOnly(string input)
  {
    if (!TimeOnly.TryParse(input, CultureInfo.CurrentCulture, out var time) &&
        !TimeOnly.TryParse(input, CultureInfo.InvariantCulture, out time))
      throw new FormatException($"'{input}' konnte nicht als Uhrzeit interpretiert werden.");

    return $"CAST('{time:HH:mm:ss.fff}' AS TIMESTAMP)";
  }

  private static string FormatDateTimeOffset(string input, DataBaseTypes rdbms)
  {
    if (DateTimeOffset.TryParse(input, CultureInfo.CurrentCulture, out var dto) ||
        DateTimeOffset.TryParse(input, CultureInfo.InvariantCulture, out dto))
      return rdbms switch
      {
        DataBaseTypes.Oracle =>
          $"TO_TIMESTAMP_TZ('{dto.ToString("yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.InvariantCulture)}', 'YYYY-MM-DD HH24:MI:SS.FF TZH:TZM')",
        _ => $"'{dto.ToString("yyyy-MM-dd HH:mm:ss.fff K", CultureInfo.InvariantCulture)}'"
      };
    throw new FormatException($"'{input}' konnte nicht als Datum mit Zeitzone interpretiert werden.");
  }

  private static string FormatGuid(string input, DataBaseTypes rdbms)
  {
    if (Guid.TryParse(input, out var guid))
      return rdbms switch
      {
        DataBaseTypes.Oracle => $"'{guid.ToString("N").ToUpperInvariant()}'",
        _ => $"'{guid}'"
      };
    throw new FormatException($"'{input}' ist keine gültige GUID.");
  }
}