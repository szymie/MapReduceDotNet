using System;
using ServiceStack.ServiceHost;

namespace Server
{
	[Route ("/api/patient", "GET POST")]
	[Route ("/api/patient/{Id}", "GET PUT DELETE")]
	public class DtoPatient : IReturn<DtoPatientResponse>
	{  
		public int? Id { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }	

		public DateTime BirthDate { get; set; }

		public Address HomeAddress { get; set; }

		public string ContactPhoneNumber { get; set; }

		public string Email { get; set; }

		public DateTime CreatedAt { get; set; }
	}
}

