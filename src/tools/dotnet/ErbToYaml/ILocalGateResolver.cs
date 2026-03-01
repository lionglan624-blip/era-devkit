using ErbParser.Ast;

namespace ErbToYaml;

/// <summary>
/// Interface for LOCAL gate resolution. Enables DI injection and F763 dynamic resolver extension.
/// </summary>
public interface ILocalGateResolver
{
    List<AstNode> Resolve(List<AstNode> ast);
}
