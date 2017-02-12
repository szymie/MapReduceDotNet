using System;
using ServiceStack.FluentValidation;
using ServiceStack.ServiceInterface;

namespace EntryPoint
{
	public class InputFileMetadataDtoValidator : AbstractValidator<InputFileMetadataDto>
	{
		public InputFileMetadataDtoValidator()
		{
			RuleSet(ApplyTo.Post, () =>
			{
				RuleFor(r => r.Description).NotEmpty();
			});
		}
	}
}
