﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonMessages.Authentication
{
	public class ValidateEmailTokenResponce : SessionSecuredResponce
	{
		public bool OK = false;
		public string Responce = string.Empty;
	}
}
