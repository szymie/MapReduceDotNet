using System;

namespace Server.Services
{
	public class ConfigFileUploadService : FileUploadService
	{
		public object Post(ConfigFileUploadDto request){
			return base._post(request);
		}

		protected override string getFileDirectory ()
		{
			return base.getFileDirectory() + "config/";
		}
	}

	public class AssemblyFileUploadService : FileUploadService
	{
		public object Post(AssemblyFileUploadDto request){
			return base._post(request);
		}

		protected override string getFileDirectory ()
		{
			return base.getFileDirectory() + "assembly/";
		}
	}

	public class DataFileUploadService : FileUploadService
	{
		public object Post(DataFileUploadDto request){
			return base._post(request);
		}

		protected override string getFileDirectory ()
		{
			return base.getFileDirectory() + "data/";
		}
	}
}

