﻿using ZeroSlope.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeroSlope.Service.Filters
{
	public class ModelstateValidationFilter : ActionFilterAttribute
	{
		private const string ErrorName = "RequestModel";
		private const string ErrorCode = "MSTATE001";
		private const string ErrorText = "Modelstate did not pass validation.";
		private const string ErrorDescription = "Unable to create request model. Most likely its not being constructed from [FromUri] / [FromBody] or not enough data supplied to create the object.";

		/// <summary>
		/// Handles model state validation and returns them in the same way server errors are returned.
		/// </summary>
		/// <param name="context"></param>
		/// <exception cref="HandledException"></exception>
		/// <inheritdoc />
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			var modelState = context.ModelState;

			// Do we have a request model? If we do and its null, add a modelstate error...
			if (context.ActionArguments.Any(kv => kv.Value == null))
			{
				context.ModelState.AddModelError(ErrorName, ErrorDescription);
			}

			// If the model state is not valid, mutate the error to our predictable error format.
			if (!modelState.IsValid)
			{
				var execptions = new List<HandledException>();
				foreach (var state in modelState)
				{
					foreach (var error in state.Value.Errors)
					{
						execptions.Add(new HandledException(ExceptionType.Validation, error.ErrorMessage, System.Net.HttpStatusCode.BadRequest, ErrorCode));
					}
				}

				// Throw the exception and lets the service exception filter capture it.
				throw new HandledException(ExceptionType.Validation, ErrorText, execptions);
			}
			base.OnActionExecuting(context);
		}
	}
}
