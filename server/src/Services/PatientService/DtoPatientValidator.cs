using System;
using ServiceStack.FluentValidation;
using ServiceStack.OrmLite;
using ServiceStack.ServiceInterface;

namespace Server
{
	public class DtoPatientValidator : BaseValidator<DtoPatient>
	{
		public DtoPatientValidator ()
		{
			RuleSet(ApplyTo.Get, () => RuleFor(r => r.Id).Must(i => i == null || PatientWithIdExistsForSessionOwner (i.Value)));
			RuleSet(ApplyTo.Post, () => RuleFor(r => r.Id).Must(i => i == null));
		}
	}
}

