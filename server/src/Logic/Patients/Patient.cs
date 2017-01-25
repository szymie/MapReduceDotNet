using System;
using ServiceStack.DataAnnotations;

namespace Server
{
	
	public class Patient
	{
		[AutoIncrement]
		public int? Id { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }	

		public DateTime BirthDate { get; set; }

		public Address HomeAddress { get; set; }

		public string ContactPhoneNumber { get; set; }

		public string Email { get; set; }

		public DateTime CreatedAt { get; set; }

		public int OwnerId { get; set; }
	}
}

