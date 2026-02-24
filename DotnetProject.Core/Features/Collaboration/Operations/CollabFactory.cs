
using DotnetProject.Core.Shared;
using FeedCommonLib.Application.Abstractions.ResponseCodes;
using FeedCommonLib.Application.Abstractions.Primitives;
using Microsoft.Extensions.Logging;
using DotnetProject.Core.Features.UserInfo.DTO;

namespace DotnetProject.Core.Features.Collaboration.Operations
{
    public static class CollabFactory
    { 
        public static Result<AxonsCollabDTO> CreateCollab(
            ILogger logger,
            int? id,
            int? year,
            int? sprint,
            string? givenUser,
            string? givenFullname,
            string? starUser,
            string? startFullname,
            string? subTeam,
            string? remark)
        {
            var _data = new
            {
                Id = id,
                Year = year,
                Sprint = sprint,
                GivenUser = givenUser,
                GivenFullname = givenFullname,
                StarUser = starUser,
                StartFullname = startFullname,
                SubTeam = subTeam,
                Remark = remark
            };
            // Validation logic (ถ้ามี)
            if (string.IsNullOrWhiteSpace(givenUser))
            {
                StdResponse error = StdResponse.Create(FeatureErrors.GivenUserIsNull, data:_data);
                logger.Log(error, context: nameof(CreateCollab));
                return Result<AxonsCollabDTO>.Failure(error);
            }


            if (string.IsNullOrWhiteSpace(starUser))
            {
                StdResponse error = StdResponse.Create(FeatureErrors.StarUserIsNull, data: _data);
                logger.Log(error, context: nameof(CreateCollab));
                return Result<AxonsCollabDTO>.Failure(error);
            }

            if (givenUser == starUser)
            {
                StdResponse error = StdResponse.Create(FeatureErrors.CannotGiveStarToSelf, data: _data);
                logger.Log(error, context: nameof(CreateCollab));
                return Result<AxonsCollabDTO>.Failure(error);
            }

            if (id == null)
            {
                StdResponse error = StdResponse.Create(FeatureErrors.InvalidStarData, data: _data);
                logger.Log(error, context: nameof(CreateCollab), customMessage: "Validation failed: id is null.");
                return Result<AxonsCollabDTO>.Failure(error);
            }
            if (year == null)
            {
                StdResponse error = StdResponse.Create(FeatureErrors.InvalidStarData, data: _data);
                logger.Log(error, context: nameof(CreateCollab), customMessage: "Validation failed: year is null.");
                return Result<AxonsCollabDTO>.Failure(error);
            }


            // สร้าง entity
            var collab = new AxonsCollabDTO
            {
                Id = id.Value,
                Year = year,
                Sprint = sprint,
                GivenUser = givenUser,
                StarUser = starUser,
                GivenFullname = givenFullname,
                StarFullname = startFullname,
                Remark = remark,
                SubTeam = subTeam,
                GivenDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            return Result<AxonsCollabDTO>.Success(collab);
        }
 
    }
}
