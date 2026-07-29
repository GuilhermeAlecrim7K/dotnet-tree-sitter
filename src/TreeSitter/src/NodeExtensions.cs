namespace TreeSitter;

public static class NodeExtensions
{
    public static IEnumerable<Node> Children(this Node node)
    {
        for (uint i = 0; i < node.ChildCount(); i++)
            yield return node.Child(i) ?? throw new InvalidOperationException($"Wasn't expecting child at index {i} to be null.");
    }

    public static IEnumerable<Node> NamedChildren(this Node node)
    {
        for (uint i = 0; i < node.NamedChildCount(); i++)
        {
            yield return node.NamedChild(i) ?? throw new InvalidOperationException($"Wasn't expecting named child at index {i} to be null.");
        }
    }

    /**
    public static IEnumerable<Node> ChildrenByFieldName(this Node node, string fieldName)
    {
        var fieldId = node.Tree.Language.FieldId(fieldName);

        if (fieldId == 0)
            yield break;

        var cursor = new TreeCursor(node);
        var ok = cursor.GotoFirstChild();

        while (ok)
        {
            if (cursor.CurrentField() == fieldName)
                yield return cursor.CurrentNode();

            ok = cursor.GotoNextSibling();
        }
    }
    */
}