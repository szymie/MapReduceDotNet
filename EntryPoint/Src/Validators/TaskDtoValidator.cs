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
				//RuleFor(r => r.AssemblyId).Must(assemblyId => existsAndIsOwnedByCurrentUser<AssemblyMetadata>(assemblyId))
				//						  .WithMessage("Assembly does not exist or you are not its owner");

				RuleFor(r => r.InputFileIds).NotEmpty();
				//RuleFor(r => r.InputFileIds).SetCollectionValidator(new InputFileIdValidator());

				RuleFor(r => r.M).NotEmpty();
				RuleFor(r => r.M).GreaterThan(0);

				RuleFor(r => r.R).NotEmpty();
				RuleFor(r => r.R).GreaterThan(0);
			});
		}

	}

	public class InputFileIdValidator : BaseValidator<int>
	{
		public InputFileIdValidator()
		{
			//RuleFor(id => id).Must(id => existsAndIsOwnedByCurrentUser<InputFileMetadata>(id))
			//                 .WithMessage("Input file does not exist or you are not its owner");
		}
	}
}
