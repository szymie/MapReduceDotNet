using System;
using System.IO;
using System.Collections.Generic;

namespace MapReduceDotNetLib
{
	public class LineReader
	{
		private StreamReader streamReader = null;

		private int currentObject = 0;
		private List<S3ObjectMetadata> objects;

		public LineReader (S3ObjectMetadata s3ObjectMetadata)
		{			
			this.objects = new List<S3ObjectMetadata>(){s3ObjectMetadata};
		}

		public LineReader (List<S3ObjectMetadata> objects)
		{
			this.objects = objects;
		}
		

		public string readLine (){
			if (currentObject == objects.Count) {
				return null;
			}

			if (this.streamReader == null) {
				this.streamReader = new StreamReader (objects[currentObject].downStream());
			}

			string line = streamReader.ReadLine ();

			while (line == null) {
				currentObject++;
				if (currentObject == objects.Count) {
					return null;
				}

				this.streamReader = new StreamReader (objects[currentObject].downStream());
				line = streamReader.ReadLine ();
			}

			return line;
		}

		public void dispose ()
		{
			try{
			if(streamReader != null){
				streamReader.Dispose ();
			}
			}catch(Exception e){
			}
		}
	}
}

