﻿using Bogus;

namespace Deveel.Data {
	public class PersonFaker : Faker<Person> {
		public PersonFaker() {
			RuleFor(x => x.Id, f => Guid.NewGuid().ToString("N").OrNull(f));
			RuleFor(x => x.FirstName, f => f.Name.FirstName());
			RuleFor(x => x.LastName, f => f.Name.LastName());
			RuleFor(x => x.DateOfBirth, f => f.Date.Past(20));
			RuleFor(x => x.Email, f => f.Internet.Email().OrNull(f));
			RuleFor(x => x.Phone, f => f.Phone.PhoneNumber().OrNull(f));
		}
	}
}
