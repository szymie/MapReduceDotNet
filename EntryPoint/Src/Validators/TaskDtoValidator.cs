using System;
using ServiceStack.FluentValidation;
using ServiceStack.ServiceInterface;

namespace EntryPoint
{
	public class TaskDtoValidator : BaseValidator<TaskDto>
	{
		public TaskDtoValidator()
		{
			RuleSet(ApplyTo.Post, () =>
			{
				RuleFor(r => r.AssemblyId).NotEmpty();
				RuleFor(r => r.AssemblyId).Must(assemblyId => exists<AssemblyMetadata>(assemblyId))
										  .WithMessage("Resource does not exist or you are not its owner");

				RuleFor(r => r.InputFileIds).NotEmpty();
			});
		}
	}
}
