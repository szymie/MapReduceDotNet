using System;
using System.Net;
using System.Linq;
using ServiceStack.Common;
using ServiceStack.Common.Web;
using ServiceStack.OrmLite;
using ServiceStack.ServiceInterface;

using System.Net;

namespace Server
{
	[Authenticate]
	public class PatientsService : BaseService
	{
		public PatientsService ()
		{
			Db.CreateTableIfNotExists<Patient> ();
		}

		[RequiredPermission("read")]
		public object Get(DtoPatient req) {

			var patients = req.Id == null
				? Db.Select<Patient> (p => p.OwnerId == GetCurrentAuthUserId ())
				: Db.Select<Patient> (p => p.OwnerId == GetCurrentAuthUserId () && p.Id == req.Id);

			var dtoPatients = patients.Select (p => new DtoPatient ().PopulateWith (p)).ToList ();

			return new DtoPatientResponse (dtoPatients);
		}

		public object Delete(DtoPatient req) {			
			Db.Delete<Patient> (p => p.Id == req.Id && p.OwnerId == GetCurrentAuthUserId ());
			return new HttpResult ("No content", HttpStatusCode.NoContent);
		}

		[RequiredPermission("editor")]
		public object Post(DtoPatient req) {

			var patient = new Patient ().PopulateWith (req);
			patient.OwnerId = GetCurrentAuthUserId ();

			Db.Insert<Patient>(patient);
			//base.Response.AddHeader (HttpHeaders.Location, "jakis url"); 
			return new HttpResult (null, HttpStatusCode.Created) {
				StatusDescription = "Created"
			};
		}

		public object Put(DtoPatient req) {

			var patient = new Patient ().PopulateWith (req);

			Db.Update<Patient> (patient, p => p.Id == patient.Id && p.OwnerId == GetCurrentAuthUserId ());

			return new HttpResult (null, HttpStatusCode.OK) {
				StatusDescription = "Updated"
			};
		}
	}
}

