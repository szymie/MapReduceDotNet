using System;
using System.Collections.Generic;

namespace Server
{
	public class DtoPatientResponse
	{
		public DtoPatientResponse(IList<DtoPatient> patients) {
			Patients = patients;
		}

		public IList<DtoPatient> Patients { get; set; }
	}
}

