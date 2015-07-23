using System;
using ICities;


namespace DistrictRCI
{
	public class DistrictRCIMod : IUserMod{
		public string Name{
			get{ return "District RCI"; }
		}

		public string Description{
			get{return "Enables the ability to view RCI demands for specific districts";}
		}
	}
}

