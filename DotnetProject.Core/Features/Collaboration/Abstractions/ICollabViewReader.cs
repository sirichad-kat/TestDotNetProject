
using FeedCommonLib.Application.Abstractions.Primitives;
namespace DotnetProject.Core.Features.Collaboration.Abstractions
{
    public interface ICollabViewReader
    {
        Task<Result<int>> GetNextValueAsync(string sequenceName, CancellationToken ct = default);
    }
}
