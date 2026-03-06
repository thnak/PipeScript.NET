namespace PipeScript.NET.Core;

public interface IPipelineStep
{
    IEnumerable<dynamic> Execute(IEnumerable<dynamic> data);
}
