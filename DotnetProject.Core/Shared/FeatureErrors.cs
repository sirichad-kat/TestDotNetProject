using FeedCommonLib.Application.Abstractions.ResponseCodes;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetProject.Core.Shared
{
    public static class FeatureErrors
    {
        #region "Collaboration"
        public static readonly Error CannotGiveStarToSelf = Errors.Register(
            "ERR301",
            "You cannot give a star to yourself.",
            StatusCodes.Status400BadRequest
        );

        public static readonly Error StarAlreadyGiven = Errors.Register(
            "ERR302",
            "You have already given a star to this person this month.",
            StatusCodes.Status409Conflict
        );

        public static readonly Error InvalidStarData = Errors.Register(
            "ERR303",
            "The provided star data is invalid.",
            StatusCodes.Status400BadRequest
        );

        public static readonly Error GivenUserIsNull = Errors.Register(
          "ERR304",
          "Given User is null or whitespace.",
          StatusCodes.Status400BadRequest
      );
        public static readonly Error StarUserIsNull = Errors.Register(
          "ERR305",
          "Star User is null or whitespace.",
          StatusCodes.Status400BadRequest
      );

        #endregion

        #region "ExternalService"
        public static readonly Error UrlExternalNotFound = Errors.Register(
         "ERR306",
         "External Service is not found.",
         StatusCodes.Status412PreconditionFailed
     );
        public static readonly Error ProductIsNull = Errors.Register(
        "ERR307",
        "Product data is invalid.",
        StatusCodes.Status400BadRequest
    );
        #endregion

    }
}
