using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace SOG.Models
{
	public class MemberLogin
	{
		[Required]
		public string ContactNumber { get; set; }
		[Required]
		public string Password { get; set; }
		public MemberLogin Update(MemberLogin login)
		{
			ContactNumber = login.ContactNumber;
			Password = login.Password;
			return this;
		}


	}
}
