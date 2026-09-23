//namespace aDataLib.Refactored;

//// TODO: Experimental --> GGf. noch nützlich...

//internal class DataDefinition(DataDictionary dataDictionary)
//{
//  public void DdlIndexCreate(DictIndex index)
//  {
//    if (index.ColumnsCount == 0)
//    {
//      return;
//    }
//    var indexTable = index.MyMotherTable;
//    if (indexTable == null)
//    {
//      return;
//    }

//    dataDictionary.lSql.Length = 0;
//    dataDictionary.lSql.Append(index.Unique ? "CREATE UNIQUE INDEX " : "CREATE INDEX ");
//    if (dataDictionary.DbType != DataBaseTypes.Oracle)
//    {
//      dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//      dataDictionary.lSql.Append(index.IndexName);
//      dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//    }
//    else
//    {
//      dataDictionary.lSql.Append(index.IndexName);
//      dataDictionary.lSql.Append("_");
//      dataDictionary.lSql.Append(indexTable.TableName);
//    }

//    dataDictionary.lSql.Append(" ON ");
//    if (indexTable.Schema.Length > 0 && dataDictionary.DbType != DataBaseTypes.MsAccess)
//    {
//      dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//      dataDictionary.lSql.Append(indexTable.Schema);
//      dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//      dataDictionary.lSql.Append(".");
//    }

//    dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//    dataDictionary.lSql.Append(indexTable.TableName);
//    dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//    dataDictionary.lSql.Append("\n ( ");
//    for (var i = 0; i < index.ColumnsCount; i++)
//    {
//      var dictIndexColumn = index[i];
//      if (dictIndexColumn == null)
//      {
//        continue;
//      }
//      if (i > 0)
//      {
//        dataDictionary.lSql.Append(",");
//      }

//      dataDictionary.lSql.Append(" ");
//      dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//      dataDictionary.lSql.Append(dictIndexColumn.ColName);
//      dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//    }
//    if (dataDictionary.DbType == DataBaseTypes.Oracle)
//    {
//      dataDictionary.lSql.Append(" )");
//      if (!index.Unique)
//      {
//        dataDictionary.lSql.Append(" WITH COMPRESS");
//      }
//    }
//    else
//    {
//      dataDictionary.lSql.Append(" );");
//    }

//    dataDictionary.ExecuteDdl();
//  }

//  public void DdlIndexDrop(DictIndex index)
//  {
//    if (index.ColumnsCount == 0)
//    {
//      return;
//    }
//    var indexTable = index.MyMotherTable;
//    if (indexTable == null)
//    {
//      return;
//    }

//    dataDictionary.lSql.Length = 0;
//    dataDictionary.lSql.Append("ALTER TABLE ");
//    if (indexTable.Schema.Length > 0 && dataDictionary.DbType != DataBaseTypes.MsAccess)
//    {
//      dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//      dataDictionary.lSql.Append(indexTable.Schema);
//      dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//      dataDictionary.lSql.Append(".");
//    }

//    dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//    dataDictionary.lSql.Append(indexTable.TableName);
//    dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//    dataDictionary.lSql.Append("\n");
//    switch (dataDictionary.DbType)
//    {
//      case DataBaseTypes.Asa7:
//      case DataBaseTypes.Asa8:
//      case DataBaseTypes.Asa9:
//      case DataBaseTypes.Asa10:
//      case DataBaseTypes.Asa11:
//        {
//          if (index.Primary)
//          {
//            dataDictionary.lSql.Append("DROP PRIMARY KEY ");
//          }
//          else
//          {
//            dataDictionary.lSql.Append("DROP INDEX ");
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//            dataDictionary.lSql.Append(indexTable.TableName);
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//            dataDictionary.lSql.Append(".");
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//            dataDictionary.lSql.Append(index.IndexName);
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//          }

//          break;
//        }
//      case DataBaseTypes.MsSqlServer when index.Primary:
//        dataDictionary.lSql.Append("DROP ");
//        dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//        dataDictionary.lSql.Append(index.IndexName);
//        dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//        break;
//      case DataBaseTypes.MsSqlServer:
//        dataDictionary.lSql.Length = 0;
//        dataDictionary.lSql.Append("DROP INDEX ");
//        dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//        dataDictionary.lSql.Append(indexTable.TableName);
//        dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//        dataDictionary.lSql.Append(".");
//        dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//        dataDictionary.lSql.Append(index.IndexName);
//        dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//        break;
//      case DataBaseTypes.MsAccess:
//        dataDictionary.lSql.Append("DROP CONSTRAINT ");
//        dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//        dataDictionary.lSql.Append(index.IndexName);
//        dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//        break;
//      case DataBaseTypes.Oracle:
//      case DataBaseTypes.MySql:
//      case DataBaseTypes.Maria:
//      case DataBaseTypes.NotDefined:
//        break;
//      default:
//        {
//          if (dataDictionary.DbType != DataBaseTypes.Oracle)
//          {
//          }

//          break;
//        }
//    }

//    dataDictionary.lSql.Append("\n");
//    if (dataDictionary.DbType != DataBaseTypes.Oracle)
//    {
//      dataDictionary.lSql.Append(";");
//    }

//    dataDictionary.ExecuteDdl();
//  }

//  public void DdlPrimaryKeyCreate(DictIndex index)
//  {
//    if (index.ColumnsCount == 0)
//    {
//      return;
//    }
//    var indexTable = index.MyMotherTable;
//    if (indexTable == null)
//    {
//      return;
//    }

//    dataDictionary.lSql.Length = 0;
//    dataDictionary.lSql.Append("ALTER TABLE ");
//    if (indexTable.Schema.Length > 0 && dataDictionary.DbType != DataBaseTypes.MsAccess)
//    {
//      dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//      dataDictionary.lSql.Append(indexTable.Schema);
//      dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//      dataDictionary.lSql.Append(".");
//    }

//    dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//    dataDictionary.lSql.Append(indexTable.TableName);
//    dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//    dataDictionary.lSql.Append("\n");
//    switch (dataDictionary.DbType)
//    {
//      case DataBaseTypes.Asa7:
//      case DataBaseTypes.Asa8:
//      case DataBaseTypes.Asa9:
//      case DataBaseTypes.Asa10:
//      case DataBaseTypes.Asa11:
//        dataDictionary.lSql.Append("ADD PRIMARY KEY ( ");
//        break;
//      case DataBaseTypes.MsSqlServer:
//        dataDictionary.lSql.Append("ADD CONSTRAINT ");
//        dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//        dataDictionary.lSql.Append(indexTable.TableName);
//        dataDictionary.lSql.Append("_PrimaryKey");
//        dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//        dataDictionary.lSql.Append(" PRIMARY KEY CLUSTERED ( ");
//        break;
//      case DataBaseTypes.MsAccess:
//        dataDictionary.lSql.Length = 0;
//        dataDictionary.lSql.Append("CREATE INDEX ");
//        dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//        dataDictionary.lSql.Append(index.IndexName);
//        dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//        dataDictionary.lSql.Append(" ON ");
//        dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//        dataDictionary.lSql.Append(indexTable.TableName);
//        dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//        dataDictionary.lSql.Append(" ( ");
//        break;
//      case DataBaseTypes.Oracle:
//        dataDictionary.lSql.Append("ADD ( CONSTRAINT ");
//        dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//        CHash.GetOracleBez(index.IndexName + "_" + indexTable.TableName);
//        dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//        dataDictionary.lSql.Append(" PRIMARY KEY ( ");
//        break;
//      case DataBaseTypes.MySql:
//      case DataBaseTypes.Maria:
//      case DataBaseTypes.NotDefined:
//      default:
//        break;
//    }

//    dataDictionary.lSql.Append("\n");
//    for (var i = 0; i < index.ColumnsCount; i++)
//    {
//      var dictIndexColumn = index[i];
//      if (dictIndexColumn == null)
//      {
//        continue;
//      }
//      if (i > 0)
//      {
//        dataDictionary.lSql.Append(",");
//      }

//      dataDictionary.lSql.Append(" ");
//      dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//      dataDictionary.lSql.Append(dictIndexColumn.ColName);
//      dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//    }

//    dataDictionary.lSql.Append("\n");
//    switch (dataDictionary.DbType)
//    {
//      case DataBaseTypes.Asa7:
//      case DataBaseTypes.Asa8:
//      case DataBaseTypes.Asa9:
//      case DataBaseTypes.Asa10:
//      case DataBaseTypes.Asa11:
//      case DataBaseTypes.MsSqlServer:
//        dataDictionary.lSql.Append(" );");
//        break;
//      case DataBaseTypes.MsAccess:
//        dataDictionary.lSql.Append(" )WITH PRIMARY;");
//        break;
//      case DataBaseTypes.Oracle:
//        dataDictionary.lSql.Append(" ))");
//        break;
//      case DataBaseTypes.MySql:
//      case DataBaseTypes.Maria:
//      case DataBaseTypes.NotDefined:
//      default:
//        break;
//    }

//    dataDictionary.ExecuteDdl();
//  }

//  public void DdlTableDrop(DictTable table)
//  {
//    dataDictionary.lSql.Length = 0;
//    dataDictionary.lSql.Append("DROP TABLE ");
//    if (table.Schema.Length > 0 && dataDictionary.DbType != DataBaseTypes.MsAccess)
//    {
//      dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//      dataDictionary.lSql.Append(table.Schema);
//      dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//      dataDictionary.lSql.Append(".");
//    }

//    dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//    dataDictionary.lSql.Append(table.TableName);
//    dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//    dataDictionary.lSql.Append("\n");
//    if (dataDictionary.DbType != DataBaseTypes.Oracle)
//    {
//      dataDictionary.lSql.Append(";");
//    }

//    dataDictionary.ExecuteDdl();
//  }

//  public void DdlColumnInsert(DictTable.DictColumn col)
//  {
//    var motherTable = col.MyMotherTable;
//    if (motherTable == null)
//    {
//      return;
//    }

//    dataDictionary.lSql.Length = 0;
//    dataDictionary.lSql.Append("ALTER TABLE ");
//    if (motherTable.Schema.Length > 0 && dataDictionary.DbType != DataBaseTypes.MsAccess)
//    {
//      dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//      dataDictionary.lSql.Append(motherTable.Schema);
//      dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//      dataDictionary.lSql.Append(".");
//    }

//    dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//    dataDictionary.lSql.Append(motherTable.TableName);
//    dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//    dataDictionary.lSql.Append("\n");
//    dataDictionary.lSql.Append(dataDictionary.DbType == DataBaseTypes.MsAccess ? "ADD COLUMN " : "ADD ");

//    dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//    dataDictionary.lSql.Append(col.ColName);
//    dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//    dataDictionary.lSql.Append(" ");
//    dataDictionary.lSql.Append(DdlGetFieldType(col));
//    dataDictionary.lSql.Append(col.Required ? " NOT NULL" : " NULL");
//    if (dataDictionary.DbType != DataBaseTypes.Oracle)
//    {
//      dataDictionary.lSql.Append(";");
//    }

//    dataDictionary.ExecuteDdl();
//  }

//  public void DdlColumnChange(DictTable.DictColumn col, bool changeToNotNull)
//  {
//    var motherTable = col.MyMotherTable;
//    if (motherTable == null)
//    {
//      return;
//    }
//    if (changeToNotNull && !DdlColumnInitValues(col))
//    {
//      return;
//    }

//    dataDictionary.lSql.Length = 0;
//    dataDictionary.lSql.Append("ALTER TABLE ");
//    if (motherTable.Schema.Length > 0 && dataDictionary.DbType != DataBaseTypes.MsAccess)
//    {
//      dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//      dataDictionary.lSql.Append(motherTable.Schema);
//      dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//      dataDictionary.lSql.Append(".");
//    }

//    dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//    dataDictionary.lSql.Append(motherTable.TableName);
//    dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//    dataDictionary.lSql.Append("\n");
//    switch (dataDictionary.DbType)
//    {
//      case DataBaseTypes.Asa7:
//      case DataBaseTypes.Asa8:
//      case DataBaseTypes.Asa9:
//      case DataBaseTypes.Asa10:
//      case DataBaseTypes.Asa11:
//        dataDictionary.lSql.Append("MODIFY ");
//        dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//        dataDictionary.lSql.Append(col.ColName);
//        dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//        dataDictionary.lSql.Append(" ");
//        dataDictionary.lSql.Append(DdlGetFieldType(col));
//        break;
//      case DataBaseTypes.MsSqlServer:
//      case DataBaseTypes.MsAccess:
//        dataDictionary.lSql.Append("ALTER COLUMN ");
//        dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//        dataDictionary.lSql.Append(col.ColName);
//        dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//        dataDictionary.lSql.Append(" ");
//        dataDictionary.lSql.Append(DdlGetFieldType(col));
//        break;
//      case DataBaseTypes.Oracle:
//        dataDictionary.lSql.Append("MODIFY ");
//        dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//        dataDictionary.lSql.Append(col.ColName);
//        dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//        dataDictionary.lSql.Append(" ");
//        dataDictionary.lSql.Append(DdlGetFieldType(col));
//        break;
//      case DataBaseTypes.MySql:
//      case DataBaseTypes.Maria:
//      case DataBaseTypes.NotDefined:
//        break;
//      default:
//        throw new ArgumentOutOfRangeException();
//    }

//    dataDictionary.lSql.Append(col.Required ? " NOT NULL" : " NULL");

//    dataDictionary.lSql.Append("\n");
//    if (dataDictionary.DbType != DataBaseTypes.Oracle)
//    {
//      dataDictionary.lSql.Append(";");
//    }

//    dataDictionary.ExecuteDdl();
//  }

//  public void DdlColumnDrop(DictTable.DictColumn col)
//  {
//    var motherTable = col.MyMotherTable;
//    if (motherTable == null)
//    {
//      return;
//    }

//    dataDictionary.lSql.Length = 0;
//    dataDictionary.lSql.Append("ALTER TABLE ");
//    if (motherTable.Schema.Length > 0 && dataDictionary.DbType != DataBaseTypes.MsAccess)
//    {
//      dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//      dataDictionary.lSql.Append(motherTable.Schema);
//      dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//      dataDictionary.lSql.Append(".");
//    }

//    dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//    dataDictionary.lSql.Append(motherTable.TableName);
//    dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//    dataDictionary.lSql.Append("\n");
//    if (dataDictionary.DbType == DataBaseTypes.MsAccess || dataDictionary.DbType == DataBaseTypes.Asa7 || dataDictionary.DbType == DataBaseTypes.Asa8 || dataDictionary.DbType == DataBaseTypes.Asa9 || dataDictionary.DbType == DataBaseTypes.Asa10 || dataDictionary.DbType == DataBaseTypes.Asa11)
//    {
//      dataDictionary.lSql.Append("DROP ");
//    }
//    else
//    {
//      dataDictionary.lSql.Append("DROP COLUMN");
//    }

//    dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//    dataDictionary.lSql.Append(col.ColName);
//    dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//    dataDictionary.lSql.Append(" ");
//    if (dataDictionary.DbType != DataBaseTypes.Oracle)
//    {
//      dataDictionary.lSql.Append(";");
//    }

//    dataDictionary.ExecuteDdl();
//  }

//  public void DdlRelationCreate(DictRelation rel)
//  {
//    dataDictionary.lSql.Length = 0;
//    dataDictionary.lSql.Append("ALTER TABLE ");
//    if (rel.MyMotherTable.Schema.Length > 0 && dataDictionary.DbType != DataBaseTypes.MsAccess)
//    {
//      dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//      dataDictionary.lSql.Append(rel.MyMotherTable.Schema);
//      dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//      dataDictionary.lSql.Append(".");
//    }

//    dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//    dataDictionary.lSql.Append(rel.ForeignTableName);
//    dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//    dataDictionary.lSql.Append("\n");
//    switch (dataDictionary.DbType)
//    {
//      case DataBaseTypes.Asa7:
//      case DataBaseTypes.Asa8:
//      case DataBaseTypes.Asa9:
//      case DataBaseTypes.Asa10:
//      case DataBaseTypes.Asa11:
//        {
//          dataDictionary.lSql.Append("ADD CONSTRAINT ");
//          dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//          dataDictionary.lSql.Append(rel.RelationName);
//          dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//          dataDictionary.lSql.Append("\n");
//          dataDictionary.lSql.Append("FOREIGN KEY ( ");
//          for (var i = 0; i < rel.ColumnsCount; i++)
//          {
//            var dictRelationColumn = rel[i];
//            if (dictRelationColumn == null)
//            {
//              continue;
//            }
//            if (i > 0)
//            {
//              dataDictionary.lSql.Append(",");
//            }

//            dataDictionary.lSql.Append(" ");
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//            dataDictionary.lSql.Append(dictRelationColumn.ForeignColName);
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//          }

//          dataDictionary.lSql.Append(" ) REFERENCES ");
//          if (rel.MyMotherTable.Schema.Length > 0)
//          {
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//            dataDictionary.lSql.Append(rel.MyMotherTable.Schema);
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//            dataDictionary.lSql.Append(".");
//          }

//          dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//          dataDictionary.lSql.Append(rel.TableName);
//          dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//          dataDictionary.lSql.Append(" ( ");
//          for (var j = 0; j < rel.ColumnsCount; j++)
//          {
//            var dictRelationColumn2 = rel[j];
//            if (dictRelationColumn2 == null)
//            {
//              continue;
//            }
//            if (j > 0)
//            {
//              dataDictionary.lSql.Append(",");
//            }

//            dataDictionary.lSql.Append(" ");
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//            dataDictionary.lSql.Append(dictRelationColumn2.ColName);
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//          }

//          dataDictionary.lSql.Append(" )\n");
//          if (rel.OnDeleteCascade && dataDictionary.DdlBuildDeleteCascade)
//          {
//            dataDictionary.lSql.Append("ON DELETE CASCADE ");
//          }
//          else
//          {
//            dataDictionary.lSql.Append("ON DELETE RESTRICT ");
//          }
//          if (rel.OnUpDateCascade && dataDictionary.DdlBuildUpDateCascade)
//          {
//            dataDictionary.lSql.Append("ON UPDATE CASCADE ");
//          }
//          else
//          {
//            dataDictionary.lSql.Append("ON UPDATE RESTRICT ");
//          }

//          dataDictionary.lSql.Append("\n;");
//          break;
//        }
//      case DataBaseTypes.MsSqlServer:
//        {
//          dataDictionary.lSql.Append("ADD CONSTRAINT ");
//          dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//          dataDictionary.lSql.Append(rel.RelationName);
//          dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//          dataDictionary.lSql.Append("\n");
//          dataDictionary.lSql.Append("FOREIGN KEY ( ");
//          for (var k = 0; k < rel.ColumnsCount; k++)
//          {
//            var dictRelationColumn3 = rel[k];
//            if (dictRelationColumn3 == null)
//            {
//              continue;
//            }
//            if (k > 0)
//            {
//              dataDictionary.lSql.Append(",");
//            }

//            dataDictionary.lSql.Append(" ");
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//            dataDictionary.lSql.Append(dictRelationColumn3.ForeignColName);
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//          }

//          dataDictionary.lSql.Append(" ) REFERENCES ");
//          if (rel.MyMotherTable.Schema.Length > 0)
//          {
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//            dataDictionary.lSql.Append(rel.MyMotherTable.Schema);
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//            dataDictionary.lSql.Append(".");
//          }

//          dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//          dataDictionary.lSql.Append(rel.TableName);
//          dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//          dataDictionary.lSql.Append(" ( ");
//          for (var l = 0; l < rel.ColumnsCount; l++)
//          {
//            var dictRelationColumn4 = rel[l];
//            if (dictRelationColumn4 == null)
//            {
//              continue;
//            }
//            if (l > 0)
//            {
//              dataDictionary.lSql.Append(",");
//            }

//            dataDictionary.lSql.Append(" ");
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//            dataDictionary.lSql.Append(dictRelationColumn4.ColName);
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//          }

//          dataDictionary.lSql.Append(" )\n");
//          if (dataDictionary.DdlBuildDeleteCascade && rel.OnDeleteCascade)
//          {
//            dataDictionary.lSql.Append("ON DELETE CASCADE ");
//          }
//          else
//          {
//            dataDictionary.lSql.Append("ON DELETE NO ACTION ");
//          }
//          if (dataDictionary.DdlBuildUpDateCascade && rel.OnUpDateCascade)
//          {
//            dataDictionary.lSql.Append("ON UPDATE CASCADE ");
//          }
//          else
//          {
//            dataDictionary.lSql.Append("ON UPDATE NO ACTION ");
//          }

//          dataDictionary.lSql.Append(" \n;");
//          break;
//        }
//      case DataBaseTypes.MsAccess:
//        {
//          dataDictionary.lSql.Append("ADD CONSTRAINT ");
//          dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//          dataDictionary.lSql.Append(rel.RelationName);
//          dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//          dataDictionary.lSql.Append("\n");
//          dataDictionary.lSql.Append("FOREIGN KEY NO INDEX ( ");
//          for (var m = 0; m < rel.ColumnsCount; m++)
//          {
//            var dictRelationColumn5 = rel[m];
//            if (dictRelationColumn5 == null)
//            {
//              continue;
//            }
//            if (m > 0)
//            {
//              dataDictionary.lSql.Append(",");
//            }

//            dataDictionary.lSql.Append(" ");
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//            dataDictionary.lSql.Append(dictRelationColumn5.ForeignColName);
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//          }

//          dataDictionary.lSql.Append(" ) REFERENCES ");
//          dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//          dataDictionary.lSql.Append(rel.TableName);
//          dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//          dataDictionary.lSql.Append(" ( ");
//          for (var n = 0; n < rel.ColumnsCount; n++)
//          {
//            var dictRelationColumn6 = rel[n];
//            if (dictRelationColumn6 == null)
//            {
//              continue;
//            }
//            if (n > 0)
//            {
//              dataDictionary.lSql.Append(",");
//            }

//            dataDictionary.lSql.Append(" ");
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//            dataDictionary.lSql.Append(dictRelationColumn6.ColName);
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//          }

//          dataDictionary.lSql.Append(" )\n");
//          if (dataDictionary.DdlBuildDeleteCascade && rel.OnDeleteCascade)
//          {
//            dataDictionary.lSql.Append("ON DELETE CASCADE ");
//          }
//          if (rel.OnUpDateCascade && dataDictionary.DdlBuildUpDateCascade)
//          {
//            dataDictionary.lSql.Append("ON UPDATE CASCADE ");
//          }

//          dataDictionary.lSql.Append("\n;");
//          break;
//        }
//      case DataBaseTypes.Oracle:
//        {
//          dataDictionary.lSql.Append("ADD ( CONSTRAINT ");
//          dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//          dataDictionary.lSql.Append(CHash.GetOracleBez(rel.RelationName));
//          dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//          dataDictionary.lSql.Append("\n");
//          dataDictionary.lSql.Append("FOREIGN KEY ( ");
//          for (var num = 0; num < rel.ColumnsCount; num++)
//          {
//            var dictRelationColumn7 = rel[num];
//            if (dictRelationColumn7 == null)
//            {
//              continue;
//            }
//            if (num > 0)
//            {
//              dataDictionary.lSql.Append(",");
//            }

//            dataDictionary.lSql.Append(" ");
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//            dataDictionary.lSql.Append(dictRelationColumn7.ForeignColName);
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//          }

//          dataDictionary.lSql.Append(" ) REFERENCES ");
//          if (rel.MyMotherTable.Schema.Length > 0)
//          {
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//            dataDictionary.lSql.Append(rel.MyMotherTable.Schema);
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//            dataDictionary.lSql.Append(".");
//          }

//          dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//          dataDictionary.lSql.Append(rel.TableName);
//          dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//          dataDictionary.lSql.Append(" ( ");
//          for (var num2 = 0; num2 < rel.ColumnsCount; num2++)
//          {
//            var dictRelationColumn8 = rel[num2];
//            if (dictRelationColumn8 == null)
//            {
//              continue;
//            }
//            if (num2 > 0)
//            {
//              dataDictionary.lSql.Append(",");
//            }

//            dataDictionary.lSql.Append(" ");
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//            dataDictionary.lSql.Append(dictRelationColumn8.ColName);
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//          }

//          dataDictionary.lSql.Append(" )\n");
//          if (dataDictionary.DdlBuildDeleteCascade && rel.OnDeleteCascade)
//          {
//            dataDictionary.lSql.Append("ON DELETE CASCADE ");
//          }

//          dataDictionary.lSql.Append("DEFERRABLE INITIALLY DEFERRED )\n");
//          break;
//        }
//      case DataBaseTypes.MySql:
//      case DataBaseTypes.Maria:
//      case DataBaseTypes.NotDefined:
//      default:
//        break;
//    }

//    dataDictionary.ExecuteDdl();
//  }

//  public void DdlRelationDrop(DictRelation rel)
//  {
//    dataDictionary.lSql.Length = 0;
//    dataDictionary.lSql.Append("ALTER TABLE ");
//    if (rel.MyMotherTable.Schema.Length > 0 && dataDictionary.DbType != DataBaseTypes.MsAccess)
//    {
//      dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//      dataDictionary.lSql.Append(rel.MyMotherTable.Schema);
//      dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//      dataDictionary.lSql.Append(".");
//    }

//    dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//    dataDictionary.lSql.Append(rel.ForeignTableName);
//    dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//    dataDictionary.lSql.Append("\n");
//    switch (dataDictionary.DbType)
//    {
//      case DataBaseTypes.Asa7:
//      case DataBaseTypes.Asa8:
//      case DataBaseTypes.Asa9:
//      case DataBaseTypes.Asa10:
//      case DataBaseTypes.Asa11:
//        dataDictionary.lSql.Append("DROP FOREIGN KEY ");
//        dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//        dataDictionary.lSql.Append(rel.RelationName);
//        dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//        dataDictionary.lSql.Append("\n");
//        break;
//      case DataBaseTypes.MsSqlServer:
//      case DataBaseTypes.MsAccess:
//        dataDictionary.lSql.Append("DROP CONSTRAINT ");
//        dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//        dataDictionary.lSql.Append(rel.RelationName);
//        dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//        dataDictionary.lSql.Append("\n");
//        break;
//      case DataBaseTypes.Oracle:
//        dataDictionary.lSql.Append("DROP FOREIGN KEY ");
//        dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//        dataDictionary.lSql.Append(CHash.GetOracleBez(rel.RelationName));
//        dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//        break;
//      case DataBaseTypes.MySql:
//      case DataBaseTypes.Maria:
//      case DataBaseTypes.NotDefined:
//      default:
//        break;
//    }

//    Utility.DbExecuteDdl(dataDictionary.lDbCmd, dataDictionary.lSql.ToString(), dataDictionary.DebugMode);
//  }

//  public void DdlTriggersCreate(DictTable table)
//  {
//    dataDictionary.lSql.Length = 0;
//    var flag = true;
//    var flag2 = false;
//    for (var i = 0; i < table.ReferencedByCount; i++)
//    {
//      var referencedBy = table.ReferencedBy(i);
//      if (referencedBy == null)
//      {
//        continue;
//      }
//      if (referencedBy.OnUpDateCascade)
//      {
//        flag = false;
//      }

//      if (!referencedBy.OnDeleteCascade) continue;
//      flag2 = true;
//      flag = false;
//    }
//    if (flag)
//    {
//      return;
//    }
//    switch (dataDictionary.DbType)
//    {
//      case DataBaseTypes.MsSqlServer:
//        {
//          if (flag2)
//          {
//            var value = "ambition_" + table.TableName.ToUpper() + "_delete";
//            dataDictionary.lSql.Append("CREATE TRIGGER ");
//            dataDictionary.lSql.Append(value);
//            dataDictionary.lSql.Append(" ON ");
//            if (table.Schema.Length > 0)
//            {
//              dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//              dataDictionary.lSql.Append(table.Schema);
//              dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//              dataDictionary.lSql.Append(".");
//            }

//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//            dataDictionary.lSql.Append(table.TableName);
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//            dataDictionary.lSql.Append("\n");
//            dataDictionary.lSql.Append(" INSTEAD OF DELETE AS\n");
//            dataDictionary.lSql.Append("SET NOCOUNT ON\n");
//            for (var j = 0; j < table.ReferencedByCount; j++)
//            {
//              var dictRelation = table.ReferencedBy(j);
//              if (dictRelation == null)
//              {
//                continue;
//              }
//              string value2;
//              if (table.Schema.Length > 0)
//              {
//                value2 = string.Concat(new[]
//                {
//                dataDictionary.DbCon.SqlBrackOpenSign,
//                table.Schema, dataDictionary.DbCon.SqlBrackCloseSign, dataDictionary.DbCon.SqlBrackOpenSign,
//                dictRelation.ForeignTableName, dataDictionary.DbCon.SqlBrackCloseSign
//                });
//              }
//              else
//              {
//                value2 = dataDictionary.DbCon.SqlBrackOpenSign + dictRelation.ForeignTableName + dataDictionary.DbCon.SqlBrackCloseSign;
//              }
//              if (dictRelation.OnDeleteCascade)
//              {
//                dataDictionary.lSql.Append("/* * LÖSCHWEITERGABE AN '");
//                dataDictionary.lSql.Append(value2);
//                dataDictionary.lSql.Append("' */\n");
//                dataDictionary.lSql.Append("DELETE ");
//                dataDictionary.lSql.Append(value2);
//                dataDictionary.lSql.Append(" FROM deleted, ");
//                dataDictionary.lSql.Append(value2);
//                dataDictionary.lSql.Append(" WHERE (deleted.");
//                var firstDeleteColumn = dictRelation[0];
//                if (firstDeleteColumn == null)
//                {
//                  continue;
//                }

//                dataDictionary.lSql.Append(firstDeleteColumn.ColName);
//                dataDictionary.lSql.Append(" = ");
//                dataDictionary.lSql.Append(value2);
//                dataDictionary.lSql.Append(".");
//                dataDictionary.lSql.Append(firstDeleteColumn.ForeignColName);
//                for (var k = 1; k < dictRelation.ColumnsCount; k++)
//                {
//                  var relationColumn = dictRelation[k];
//                  if (relationColumn == null)
//                  {
//                    continue;
//                  }

//                  dataDictionary.lSql.Append(" AND deleted.");
//                  dataDictionary.lSql.Append(relationColumn.ColName);
//                  dataDictionary.lSql.Append(" = ");
//                  dataDictionary.lSql.Append(value2);
//                  dataDictionary.lSql.Append(".");
//                  dataDictionary.lSql.Append(relationColumn.ForeignColName);
//                }

//                dataDictionary.lSql.Append(")\n\n");
//              }
//            }
//            var dictRelation2 = table.ReferencedBy(table.ReferencedByCount - 1);
//            if (dictRelation2 == null)
//            {
//              return;
//            }
//            string value3;
//            if (table.Schema.Length > 0)
//            {
//              value3 = string.Concat(new[]
//              {
//              dataDictionary.DbCon.SqlBrackOpenSign,
//              table.Schema, dataDictionary.DbCon.SqlBrackCloseSign, dataDictionary.DbCon.SqlBrackOpenSign,
//              dictRelation2.TableName, dataDictionary.DbCon.SqlBrackCloseSign
//              });
//            }
//            else
//            {
//              value3 = dataDictionary.DbCon.SqlBrackOpenSign + dictRelation2.TableName + dataDictionary.DbCon.SqlBrackCloseSign;
//            }

//            dataDictionary.lSql.Append("/* * EIGENE LÖSCHUNG '");
//            dataDictionary.lSql.Append(value3);
//            dataDictionary.lSql.Append("' */\n");
//            dataDictionary.lSql.Append("DELETE ");
//            dataDictionary.lSql.Append(value3);
//            dataDictionary.lSql.Append(" FROM deleted, ");
//            dataDictionary.lSql.Append(value3);
//            dataDictionary.lSql.Append(" WHERE (deleted.");
//            var firstOwnDeleteColumn = dictRelation2[0];
//            if (firstOwnDeleteColumn == null)
//            {
//              return;
//            }

//            dataDictionary.lSql.Append(firstOwnDeleteColumn.ColName);
//            dataDictionary.lSql.Append(" = ");
//            dataDictionary.lSql.Append(value3);
//            dataDictionary.lSql.Append(".");
//            dataDictionary.lSql.Append(firstOwnDeleteColumn.ColName);
//            for (var l = 1; l < dictRelation2.ColumnsCount; l++)
//            {
//              var relationColumn2 = dictRelation2[l];
//              if (relationColumn2 == null)
//              {
//                continue;
//              }

//              dataDictionary.lSql.Append(" AND deleted.");
//              dataDictionary.lSql.Append(relationColumn2.ColName);
//              dataDictionary.lSql.Append(" = ");
//              dataDictionary.lSql.Append(value3);
//              dataDictionary.lSql.Append(".");
//              dataDictionary.lSql.Append(relationColumn2.ColName);
//            }

//            dataDictionary.lSql.Append(")\n\n");
//          }
//          if (flag2)
//          {
//            dataDictionary.ExecuteDdl();
//          }
//          return;
//        }
//      case DataBaseTypes.Oracle when table.PrimKey != null:
//        {
//          string text;
//          if (table.Schema.Length > 0)
//          {
//            text = string.Concat(new[]
//            {
//            dataDictionary.DbCon.SqlBrackOpenSign,
//            table.Schema, dataDictionary.DbCon.SqlBrackCloseSign, dataDictionary.DbCon.SqlBrackOpenSign,
//            "TRI_",
//            table.TableName, dataDictionary.DbCon.SqlBrackCloseSign
//            });
//          }
//          else
//          {
//            text = dataDictionary.DbCon.SqlBrackOpenSign + "TRI_" + table.TableName + dataDictionary.DbCon.SqlBrackCloseSign;
//          }
//          text = CHash.GetOracleBez(text);
//          string value4;
//          if (table.Schema.Length > 0)
//          {
//            value4 = string.Concat(new[]
//            {
//            dataDictionary.DbCon.SqlBrackOpenSign,
//            table.Schema, dataDictionary.DbCon.SqlBrackCloseSign, dataDictionary.DbCon.SqlBrackOpenSign,
//            table.TableName, dataDictionary.DbCon.SqlBrackCloseSign
//            });
//          }
//          else
//          {
//            value4 = dataDictionary.DbCon.SqlBrackOpenSign + table.TableName + dataDictionary.DbCon.SqlBrackCloseSign;
//          }

//          dataDictionary.lSql.Append("CREATE OR REPLACE TRIGGER ");
//          dataDictionary.lSql.Append(text);
//          dataDictionary.lSql.Append(" AFTER UPDATE OF ");
//          if (table.PrimKey == null)
//          {
//            return;
//          }
//          for (var m = 0; m < table.PrimKey.ColumnsCount; m++)
//          {
//            var dictIndexColumn = table.PrimKey[m];
//            if (dictIndexColumn == null)
//            {
//              continue;
//            }
//            if (m > 0)
//            {
//              dataDictionary.lSql.Append(",");
//            }

//            dataDictionary.lSql.Append(" ");
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//            dataDictionary.lSql.Append(dictIndexColumn.ColName);
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//          }

//          dataDictionary.lSql.Append(" ON ");
//          dataDictionary.lSql.Append(value4);
//          dataDictionary.lSql.Append("\n");
//          dataDictionary.lSql.Append("FOR EACH ROW\n");
//          dataDictionary.lSql.Append("BEGIN\n");
//          dataDictionary.lSql.Append("  IF UPDATING AND ( ");
//          for (var n = 0; n < table.PrimKey.ColumnsCount; n++)
//          {
//            var dictIndexColumn2 = table.PrimKey[n];
//            if (dictIndexColumn2 == null)
//            {
//              continue;
//            }

//            dataDictionary.lSql.Append(n == 0 ? ":OLD." : "OR :OLD.");
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//            dataDictionary.lSql.Append(dictIndexColumn2.ColName);
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//            dataDictionary.lSql.Append(" != :NEW.");
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//            dataDictionary.lSql.Append(dictIndexColumn2.ColName);
//            dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//          }

//          dataDictionary.lSql.Append(") THEN\n");
//          for (var num = 0; num < table.ReferencedByCount; num++)
//          {
//            var dictRelation3 = table.ReferencedBy(num);
//            if (dictRelation3 == null)
//            {
//              continue;
//            }
//            if (dictRelation3.OnUpDateCascade)
//            {
//              dataDictionary.lSql.Append("    UPDATE ");
//              dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//              dataDictionary.lSql.Append(dictRelation3.ForeignTableName);
//              dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//              dataDictionary.lSql.Append(" SET\n");
//              for (var num2 = 0; num2 < dictRelation3.ColumnsCount; num2++)
//              {
//                dataDictionary.lSql.Append(num2 == 0 ? "      " : "    , ");
//                dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//                dataDictionary.lSql.Append(dictRelation3.ForeignTableName);
//                dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//                dataDictionary.lSql.Append(".");
//                var relationUpdateColumn = dictRelation3[num2];
//                if (relationUpdateColumn == null)
//                {
//                  continue;
//                }

//                dataDictionary.lSql.Append(relationUpdateColumn.ForeignColName);
//                dataDictionary.lSql.Append(" = :NEW.");
//                dataDictionary.lSql.Append(".");
//                dataDictionary.lSql.Append(relationUpdateColumn.ForeignColName);
//                dataDictionary.lSql.Append("\n");
//              }

//              dataDictionary.lSql.Append("    WHERE \n");
//              for (var num3 = 0; num3 < dictRelation3.ColumnsCount; num3++)
//              {
//                dataDictionary.lSql.Append(num3 == 0 ? "      " : "  AND ");
//                dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//                dataDictionary.lSql.Append(dictRelation3.ForeignTableName);
//                dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//                dataDictionary.lSql.Append(".");
//                var relationOldColumn = dictRelation3[num3];
//                if (relationOldColumn == null)
//                {
//                  continue;
//                }

//                dataDictionary.lSql.Append(relationOldColumn.ForeignColName);
//                dataDictionary.lSql.Append(" = :OLD.");
//                dataDictionary.lSql.Append(".");
//                dataDictionary.lSql.Append(relationOldColumn.ColName);
//                dataDictionary.lSql.Append("\n");
//              }
//            }
//          }

//          dataDictionary.lSql.Append("  END IF;\n");
//          dataDictionary.lSql.Append("  END;\n");
//          break;
//        }
//      case DataBaseTypes.Asa7:
//      case DataBaseTypes.MsAccess:
//      case DataBaseTypes.Asa8:
//      case DataBaseTypes.Asa9:
//      case DataBaseTypes.Asa10:
//      case DataBaseTypes.Asa11:
//      case DataBaseTypes.MySql:
//      case DataBaseTypes.Maria:
//      case DataBaseTypes.NotDefined:
//        break;
//      default:
//        throw new ArgumentOutOfRangeException();
//    }

//    dataDictionary.ExecuteDdl();
//  }

//  public void DdlTriggersDrop(DictTable table)
//  {
//    dataDictionary.lSql.Length = 0;
//    if (dataDictionary.DbType == DataBaseTypes.MsSqlServer)
//    {
//      var text = "ambition_" + table.TableName.ToUpper() + "_delete";
//      dataDictionary.lSql.Append("IF EXISTS (SELECT name FROM sysobjects WHERE name = '");
//      dataDictionary.lSql.Append(text);
//      dataDictionary.lSql.Append("' AND type = 'TR') DROP TRIGGER [");
//      dataDictionary.lSql.Append(text);
//      dataDictionary.lSql.Append("]");
//      Utility.DbExecuteDdl(dataDictionary.lDbCmd, dataDictionary.lSql.ToString(), dataDictionary.DebugMode);
//    }

//    dataDictionary.lSql.Length = 0;
//    if (dataDictionary.DbType == DataBaseTypes.MsSqlServer)
//    {
//      var text = "ambition_" + table.TableName.ToUpper() + "_insert";
//      dataDictionary.lSql.Append("IF EXISTS (SELECT name FROM sysobjects WHERE name = '");
//      dataDictionary.lSql.Append(text);
//      dataDictionary.lSql.Append("' AND type = 'TR') DROP TRIGGER [");
//      dataDictionary.lSql.Append(text);
//      dataDictionary.lSql.Append("]");
//      Utility.DbExecuteDdl(dataDictionary.lDbCmd, dataDictionary.lSql.ToString(), dataDictionary.DebugMode);
//    }

//    dataDictionary.lSql.Length = 0;
//    switch (dataDictionary.DbType)
//    {
//      case DataBaseTypes.MsSqlServer:
//        {
//          var text = "ambition_" + table.TableName.ToUpper() + "_update";
//          dataDictionary.lSql.Append("IF EXISTS (SELECT name FROM sysobjects WHERE name = '");
//          dataDictionary.lSql.Append(text);
//          dataDictionary.lSql.Append("' AND type = 'TR') DROP TRIGGER [");
//          dataDictionary.lSql.Append(text);
//          dataDictionary.lSql.Append("]");
//          Utility.DbExecuteDdl(dataDictionary.lDbCmd, dataDictionary.lSql.ToString(), dataDictionary.DebugMode);

//          break;
//        }
//      case DataBaseTypes.Oracle:
//        {
//          string text;
//          if (table.Schema.Length > 0)
//          {
//            text = string.Concat(new[]
//            {
//            dataDictionary.DbCon.SqlBrackOpenSign,
//            table.Schema, dataDictionary.DbCon.SqlBrackCloseSign, dataDictionary.DbCon.SqlBrackOpenSign,
//            "TRI_",
//            table.TableName, dataDictionary.DbCon.SqlBrackCloseSign
//            });
//          }
//          else
//          {
//            text = dataDictionary.DbCon.SqlBrackOpenSign + "TRI_" + table.TableName + dataDictionary.DbCon.SqlBrackCloseSign;
//          }
//          text = CHash.GetOracleBez(text);
//          dataDictionary.lSql.Append("DROP TRIGGER ");
//          dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackOpenSign);
//          dataDictionary.lSql.Append(text);
//          dataDictionary.lSql.Append(dataDictionary.DbCon.SqlBrackCloseSign);
//          dataDictionary.ExecuteDdl();
//          break;
//        }
//      case DataBaseTypes.Asa7:
//      case DataBaseTypes.MsAccess:
//      case DataBaseTypes.Asa8:
//      case DataBaseTypes.Asa9:
//      case DataBaseTypes.Asa10:
//      case DataBaseTypes.Asa11:
//      case DataBaseTypes.MySql:
//      case DataBaseTypes.Maria:
//      case DataBaseTypes.NotDefined:
//        break;
//      default:
//        throw new ArgumentOutOfRangeException();
//    }
//  }

//  private bool DdlColumnInitValues(DictTable.DictColumn col)
//  {
//    var motherTable = col.MyMotherTable;
//    if (motherTable == null)
//    {
//      return false;
//    }
//    var datDef = new DatDef(motherTable.Schema, motherTable.TableName, dataDictionary.DbCon);
//    var tableDef = datDef[0];
//    if (tableDef == null)
//    {
//      datDef.Dispose();
//      return false;
//    }
//    tableDef.FieldsAdd(col.ColName);
//    var fieldDef = tableDef[0];
//    if (fieldDef == null)
//    {
//      datDef.Dispose();
//      return false;
//    }

//    fieldDef.Value = col.FieldType switch
//    {
//      DbFieldType.Int16 or DbFieldType.Int32 or DbFieldType.Int64 or DbFieldType.Decimal => 0,
//      DbFieldType.Double => 0.0,
//      DbFieldType.DateTime => "01.01.1900 00:00:00",
//      DbFieldType.Binary => "",
//      _ => fieldDef.Value
//    };
//    datDef.CondsAdd(OpTypes.Where, motherTable.TableName, col.ColName, CompTypes.Equal, "Null");
//    datDef.DebugMode = dataDictionary.DebugMode;
//    var result = tableDef.WriteUpDate(CondTypes.DdCondition, false) > -1;
//    datDef.Dispose();
//    return result;
//  }

//  private string DdlGetFieldType(DictTable.DictColumn col)
//  {
//    var fieldType = col.FieldType;
//    switch (dataDictionary.DbType)
//    {
//      case DataBaseTypes.Asa7:
//      case DataBaseTypes.Asa8:
//      case DataBaseTypes.Asa9:
//      case DataBaseTypes.Asa10:
//      case DataBaseTypes.Asa11:
//        switch (fieldType)
//        {
//          case DbFieldType.Int16:
//            return "SMALLINT";
//          case DbFieldType.Int32:
//            return "INTEGER";
//          case DbFieldType.Double:
//            return "DOUBLE";
//          case DbFieldType.Int64:
//            return "VARNUMERIC";
//          case DbFieldType.Binary:
//            return "LONG BINARY";
//          case DbFieldType.DateTime:
//            return "TIMESTAMP";
//          case DbFieldType.String:
//            if (col.Length is > 0 and <= 32767)
//            {
//              return "CHAR(" + col.Length.ToString() + ")";
//            }
//            return "LONG VARCHAR";
//          case DbFieldType.Decimal:
//            return "DOUBLE";
//          case DbFieldType.Unknown:
//          case DbFieldType.DateOnly:
//          case DbFieldType.TimeOnly:
//          case DbFieldType.DateTimeOffset:
//          case DbFieldType.Boolean:
//          case DbFieldType.Guid:
//            break;
//          default:
//            throw new ArgumentOutOfRangeException();
//        }

//        break;
//      case DataBaseTypes.Oracle:
//        switch (fieldType)
//        {
//          case DbFieldType.Int16:
//            return "NUMBER(6,0)";
//          case DbFieldType.Int32:
//            return "NUMBER(11,0)";
//          case DbFieldType.Double:
//            return "FLOAT";
//          case DbFieldType.Int64:
//            return "NUMBER(20,0)";
//          case DbFieldType.Binary:
//            return "BLOB";
//          case DbFieldType.DateTime:
//            return "DATE";
//          case DbFieldType.String:
//            if (col.Length < 2001)
//            {
//              return "NCHAR(" + col.Length.ToString() + ")";
//            }
//            if (col.Length < 4001)
//            {
//              return "NVARCHAR2(" + col.Length.ToString() + ")";
//            }
//            return "CBLOB";
//          case DbFieldType.Decimal:
//            return "FLOAT";
//          case DbFieldType.Unknown:
//          case DbFieldType.DateOnly:
//          case DbFieldType.TimeOnly:
//          case DbFieldType.DateTimeOffset:
//          case DbFieldType.Boolean:
//          case DbFieldType.Guid:
//            break;
//          default:
//            throw new ArgumentOutOfRangeException();
//        }

//        break;
//      case DataBaseTypes.MsSqlServer:
//        switch (fieldType)
//        {
//          case DbFieldType.Int16:
//            return "SMALLINT";
//          case DbFieldType.Int32:
//            return "INT";
//          case DbFieldType.Double:
//            return "FLOAT";
//          case DbFieldType.Int64:
//            return "BIGINT";
//          case DbFieldType.Binary:
//            return "IMAGE";
//          case DbFieldType.DateTime:
//            return "DATETIME";
//          case DbFieldType.String:
//            return col.Length switch
//            {
//              0 => "NVARCHAR(4000)",
//              < 8001 => "NVARCHAR(" + col.Length.ToString() + ")",
//              _ => "NTEXT"
//            };
//          case DbFieldType.Decimal:
//            return "DECIMAL(18,2)";
//          case DbFieldType.Boolean:
//            return "BIT";
//          case DbFieldType.Unknown:
//            return "BINARY";
//          case DbFieldType.DateOnly:
//          case DbFieldType.TimeOnly:
//          case DbFieldType.DateTimeOffset:
//          case DbFieldType.Guid:
//            break;
//          default:
//            throw new ArgumentOutOfRangeException();
//        }

//        break;
//      case DataBaseTypes.MsAccess:
//        switch (fieldType)
//        {
//          case DbFieldType.Int16:
//            return "SMALLINT";
//          case DbFieldType.Int32:
//            return "INTEGER";
//          case DbFieldType.Double:
//            return "FLOAT";
//          case DbFieldType.Int64:
//            return "UNIQUEIDENTIFIER";
//          case DbFieldType.Binary:
//            return "IMAGE";
//          case DbFieldType.DateTime:
//            return "DATETIME";
//          case DbFieldType.String:
//            if (col.Length is > 0 and < 256)
//            {
//              return "TEXT(" + col.Length.ToString() + ")";
//            }
//            return "TEXT";
//          case DbFieldType.Boolean:
//            return "BIT";
//          case DbFieldType.Decimal:
//            return "DECIMAL";
//          case DbFieldType.Unknown:
//          case DbFieldType.DateOnly:
//          case DbFieldType.TimeOnly:
//          case DbFieldType.DateTimeOffset:
//          case DbFieldType.Guid:
//            break;
//          default:
//            throw new ArgumentOutOfRangeException();
//        }

//        break;
//      case DataBaseTypes.MySql:
//      case DataBaseTypes.Maria:
//      case DataBaseTypes.NotDefined:
//        break;
//      default:
//        throw new ArgumentOutOfRangeException();
//    }

//    return "<ERROR> READING DATATYPE";
//  }

//  //public void ddlTableCreate(DataDictionary.DictTable table)
//  //{
//  //  lSql.Length = 0;
//  //  lSql.Append("CREATE TABLE ");
//  //  if (table.Schema.Length > 0 && dbType != DataBaseTypes.MsAccess)
//  //  {
//  //    lSql.Append(DbCon.SqlBrackOpenSign);
//  //    lSql.Append(table.Schema);
//  //    lSql.Append(DbCon.SqlBrackCloseSign);
//  //    lSql.Append(".");
//  //  }
//  //  lSql.Append(DbCon.SqlBrackOpenSign);
//  //  lSql.Append(table.TableName);
//  //  lSql.Append(DbCon.SqlBrackCloseSign);
//  //  lSql.Append("\n");
//  //  if (table.ColumnsCount > 0)
//  //  {
//  //    lSql.Append("( ");
//  //    for (var i = 0; i < table.ColumnsCount; i++)
//  //    {
//  //      var dictColumn = table[i];
//  //      if (dictColumn == null)
//  //      {
//  //        continue;
//  //      }
//  //      if (i > 0)
//  //      {
//  //        lSql.Append(",");
//  //      }
//  //      lSql.Append(" ");
//  //      lSql.Append(DbCon.SqlBrackOpenSign);
//  //      lSql.Append(dictColumn.ColName);
//  //      lSql.Append(DbCon.SqlBrackCloseSign);
//  //      lSql.Append(" ");
//  //      lSql.Append(_dataDefinition.ddlGetFieldType(dictColumn));
//  //      if (dictColumn.Required)
//  //      {
//  //        lSql.Append(" NOT NULL");
//  //      }
//  //    }
//  //    lSql.Append(" )");
//  //  }
//  //  if (dbType != DataBaseTypes.Oracle)
//  //  {
//  //    lSql.Append(";");
//  //  }
//  //  ExecuteDdl();
//  //}

//}

