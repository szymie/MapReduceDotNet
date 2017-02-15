using System;
namespace EntryPoint
{
	public interface Uploadable : Ownable
	{
		bool IsUploaded { get; set; }
	}
}
