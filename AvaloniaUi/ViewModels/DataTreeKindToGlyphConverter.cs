using System.Globalization;

using aDataLib;

using Avalonia.Data.Converters;

namespace aSql.ViewModels;

public sealed class DataTreeKindToGlyphConverter : IValueConverter
{
  public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    return value switch
    {
      DataTreeNode { Kind: DataTreeNodeKind.RelationsGroup } node => node.Name switch
      {
        "ForeignKeys" => "FK",
        "ReferencedBy" => "Ref",
        _ => "Rel"
      },
      DataTreeNode node => node.Kind switch
      {
        DataTreeNodeKind.Database => "DB",
        DataTreeNodeKind.Table => "T",
        DataTreeNodeKind.SqlTablesGroup => "T",
        DataTreeNodeKind.ColumnsGroup => "Cols",
        DataTreeNodeKind.Column => "C",
        DataTreeNodeKind.IndexesGroup => "Idx",
        DataTreeNodeKind.SqlOrderByGroup => "Srt",
        DataTreeNodeKind.Index => "IX",
        DataTreeNodeKind.IndexColumn => "IC",
        DataTreeNodeKind.Select => "S",
        DataTreeNodeKind.SqlJoinsGroup => "Joi",
        DataTreeNodeKind.SqlJoin => "Joi",
        DataTreeNodeKind.SqlWhereGroup => "W",
        DataTreeNodeKind.SqlGroupByGroup => "G",
        DataTreeNodeKind.SqlHavingGroup => "H",
        DataTreeNodeKind.Relation =>
          node.Tag is DictRelation column
            ? column.RelationType switch
            {
              RelationTypes.FKey => "F",
              RelationTypes.RefBy => "R",
              _ => ""
            }
            : "",
        DataTreeNodeKind.RelationColumn => node.Tag is DictRelation.DictRelationColumn
        {
          MyMotherRelation: not null
        } column
          ? column.MyMotherRelation.RelationType switch
          {
            RelationTypes.FKey => "FC",
            RelationTypes.RefBy => "RC",
            _ => ""
          }
          : "",
        _ => string.Empty
      },
      DataTreeNodeKind kind => kind switch
      {
        DataTreeNodeKind.Database => "DB",
        DataTreeNodeKind.Table => "T",
        DataTreeNodeKind.SqlTablesGroup => "T",
        DataTreeNodeKind.ColumnsGroup => "Cols",
        DataTreeNodeKind.Column => "C",
        DataTreeNodeKind.IndexesGroup => "Idx",
        DataTreeNodeKind.SqlOrderByGroup => "Srt",
        DataTreeNodeKind.Index => "IX",
        DataTreeNodeKind.IndexColumn => "IC",
        DataTreeNodeKind.Select => "S",
        DataTreeNodeKind.SqlJoinsGroup => "Joi",
        DataTreeNodeKind.SqlJoin => "Joi",
        DataTreeNodeKind.SqlWhereGroup => "W",
        DataTreeNodeKind.SqlGroupByGroup => "G",
        DataTreeNodeKind.SqlHavingGroup => "H",
        DataTreeNodeKind.RelationsGroup => "Rel",
        DataTreeNodeKind.Relation => "R",
        DataTreeNodeKind.RelationColumn => "RC",
        _ => string.Empty
      },
      _ => string.Empty
    };
  }

  public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    throw new NotSupportedException();
  }
}