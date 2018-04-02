using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CenterComparing
{
	public static class ExtensionFunction
	{
		public static double GetResolution
		(this double pxlen, double reallen)
		=> reallen / pxlen;




	}
}
