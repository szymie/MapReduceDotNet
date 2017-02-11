using System;
using ServiceStack.FluentValidation;
using ServiceStack.ServiceInterface;

namespace EntryPoint
{
	public class AssemblyMetadataDtoValidator : AbstractValidator<AssemblyMetadataDto>
	{
		public AssemblyMetadataDtoValidator()
		{
			RuleSet(ApplyTo.Post, () =>
			{
				RuleFor(r => r.Description).NotEmpty();
				RuleFor(r => r.Name).NotEmpty();
				RuleFor(r => r.Namespace).NotEmpty();
				RuleFor(r => r.MapClassName).NotEmpty();
				RuleFor(r => r.ReduceClassName).NotEmpty();
			});
		}
	}
}
