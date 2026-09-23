using System.Data.Common;

using Microsoft.Data.SqlClient;

namespace aDataLib;

internal class Utility
{
  internal static int DbExecute(DbCommand pSqlCmd, string pSql, bool pDebugMode, ref string pDebugString)
  {
    int result;
    var text = "";
    try
    {
      if (pSql.Length > 0) pSqlCmd.CommandText = pSql;
      if (pDebugMode) text = text + "<debug mode> try dbExecute:\n" + pSql + "\n";
      result = pSqlCmd.ExecuteNonQuery();
      if (pDebugMode) text += "<debug mode> dbExecute: ok\n";
      pDebugString += text;
      Console.WriteLine(text);
    }
    catch (SqlException ex)
    {
      if (pDebugMode)
      {
        text = text + "<debug mode> sql error dbExecute:\n" + ex;
        Console.WriteLine(text);
      }

      pDebugString += text;
      throw;
    }
    catch (Exception ex2)
    {
      if (pDebugMode)
      {
        text = text + "<debug mode> error dbExecute:\n" + ex2;
        Console.WriteLine(text);
      }

      pDebugString += text;
      throw;
    }

    return result;
  }
}