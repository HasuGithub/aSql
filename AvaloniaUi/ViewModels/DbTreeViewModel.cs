using System.Collections.ObjectModel;

using aDataLib;

namespace aSql.ViewModels;

public sealed class DbTreeViewModel : ViewModelBase
{
  public ObservableCollection<DataTreeNode> Roots { get; } = [];

  public void BuildFrom(DbConnect? dbConnect)
  {
    Roots.Clear();

    if (dbConnect == null) return;

    var dic = dbConnect.DataDic;
    if (!dic.Build()) return;

    var dbRoot = new DataTreeNode(
      dic.DbName,
      DataTreeNodeKind.Database,
      null)
    {
      IsExpanded = true
    };

    for (var i = 0; i < dic.TablesCount; i++)
    {
      var table = dic[i];
      if (table == null) continue;
      var tableNode = new DataTreeNode(table.TableName, DataTreeNodeKind.Table, table);

      // Columns
      if (table.ColumnsCount > 0)
      {
        var columnsNode = new DataTreeNode("Columns", DataTreeNodeKind.ColumnsGroup, null);
        for (var c = 0; c < table.ColumnsCount; c++)
        {
          var col = table[c];
          if (col == null) continue;
          var readonlyText = col.IsReadOnly ? " (readonly) " : string.Empty;
          var header = col.ColName + " {" + col.FieldType.ToString().ToLower() + readonlyText + "}";
          if (col.Required) header = "*" + header;

          columnsNode.Children.Add(new DataTreeNode(header, DataTreeNodeKind.Column, col));
        }

        tableNode.Children.Add(columnsNode);
      }

      // Indexes
      if (table.IndexesCount > 0)
      {
        var indexesNode = new DataTreeNode("Indexes", DataTreeNodeKind.IndexesGroup, null);
        for (var idx = 0; idx < table.IndexesCount; idx++)
        {
          var index = table.Indexes(idx);
          if (index == null) continue;
          var indexNode = new DataTreeNode(index.IndexName, DataTreeNodeKind.Index, index);
          for (var ic = 0; ic < index.ColumnsCount; ic++)
          {
            var idxCol = index[ic];
            if (idxCol == null) continue;
            indexNode.Children.Add(new DataTreeNode(idxCol.ColName, DataTreeNodeKind.IndexColumn, idxCol));
          }

          indexesNode.Children.Add(indexNode);
        }

        tableNode.Children.Add(indexesNode);
      }

      // ForeignKeys
      if (table.ForeignKeysCount > 0)
      {
        var fkGroup = new DataTreeNode("ForeignKeys", DataTreeNodeKind.RelationsGroup, null);
        for (var r = 0; r < table.ForeignKeysCount; r++)
        {
          var rel = table.ForeignKeys(r);
          if (rel == null) continue;
          var suffix = rel switch
          {
            { OnUpDateCascade: true, OnDeleteCascade: true } => "{du}",
            { OnDeleteCascade: true } => "{d}",
            { OnUpDateCascade: true } => "{u}",
            _ => string.Empty
          };

          var relationDisplayName = string.Concat(
            rel.TableName,
            " --> ",
            rel.ForeignTableName,
            suffix);
          var relNode = new DataTreeNode(relationDisplayName, DataTreeNodeKind.Relation, rel, rel.RelationName)
          {
            Parent = fkGroup
          };
          for (var rc = 0; rc < rel.ColumnsCount; rc++)
          {
            var col = rel[rc];
            if (col == null) continue;
            var text = string.Concat(
              rel.TableName,
              ".",
              col.ColName,
              " -> ",
              rel.ForeignTableName,
              ".",
              col.ForeignColName);

            relNode.Children.Add(new DataTreeNode(text, DataTreeNodeKind.RelationColumn, col));
          }

          fkGroup.Children.Add(relNode);
        }

        tableNode.Children.Add(fkGroup);
      }

      // ReferencedBy
      if (table.ReferencedByCount > 0)
      {
        var refByGroup = new DataTreeNode("ReferencedBy", DataTreeNodeKind.RelationsGroup, null);
        for (var r = 0; r < table.ReferencedByCount; r++)
        {
          var rel = table.ReferencedBy(r);
          if (rel == null) continue;
          var suffix = string.Empty;
          if (rel is { OnUpDateCascade: true, OnDeleteCascade: true })
            suffix = "{du}";
          else if (rel.OnDeleteCascade)
            suffix = "{d}";
          else if (rel.OnUpDateCascade) suffix = "{u}";

          var relationDisplayName = string.Concat(
            rel.TableName,
            " --> ",
            rel.ForeignTableName,
            suffix);
          var relNode = new DataTreeNode(relationDisplayName, DataTreeNodeKind.Relation, rel, rel.RelationName)
          {
            Parent = refByGroup
          };

          for (var rc = 0; rc < rel.ColumnsCount; rc++)
          {
            var col = rel[rc];
            if (col == null) continue;
            var text = string.Concat(
              rel.TableName,
              ".",
              col.ColName,
              " -> ",
              rel.ForeignTableName,
              ".",
              col.ForeignColName);

            relNode.Children.Add(new DataTreeNode(text, DataTreeNodeKind.RelationColumn, col));
          }

          refByGroup.Children.Add(relNode);
        }

        tableNode.Children.Add(refByGroup);
      }

      dbRoot.Children.Add(tableNode);
    }

    Roots.Add(dbRoot);
  }
}