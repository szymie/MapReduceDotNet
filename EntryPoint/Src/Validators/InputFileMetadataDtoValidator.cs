using System;
using ServiceStack.FluentValidation;
using ServiceStack.ServiceInterface;

namespace EntryPoint
{
	public class InputFileMetadataDtoValidator : AbstractValidator<InputFileMetadata>
	{
		public InputFileMetadataDtoValidator()
		{
			RuleSet(ApplyTo.Post, () => { RuleFor(r => r.Description).NotEmpty(); });
		}
	}
}
