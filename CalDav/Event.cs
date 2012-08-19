﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace CalDav {
	public class Event : ISerializeToICAL {
		private DateTime DTSTAMP = DateTime.UtcNow;

		public Event() {
			Attendees = new List<Contact>();
			Alarms = new List<Alarm>();
			Categories = new List<string>();
			Recurrences = new List<Recurrence>();
			Properties = new List<Tuple<string, string, System.Collections.Specialized.NameValueCollection>>();
		}

		public virtual ICollection<Contact> Attendees { get; set; }
		public virtual ICollection<Alarm> Alarms { get; set; }
		public virtual ICollection<string> Categories { get; set; }
		public virtual string Class { get; set; }
		public virtual DateTime? Created { get; set; }
		public virtual string Description { get; set; }
		public virtual bool IsAllDay { get; set; }
		public virtual DateTime? LastModified { get; set; }
		public virtual DateTime? Start { get; set; }
		public virtual DateTime? End { get; set; }
		public virtual string Location { get; set; }
		public virtual int? Priority { get; set; }
		public virtual string Status { get; set; }
		public virtual int? Sequence { get; set; }
		public virtual string Summary { get; set; }
		public virtual string Transparency { get; set; }
		public virtual string UID { get; set; }
		public virtual Uri Url { get; set; }
		public virtual Contact Organizer { get; set; }
		public virtual ICollection<Recurrence> Recurrences { get; set; }

		public ICollection<Tuple<string, string, System.Collections.Specialized.NameValueCollection>> Properties { get; set; }

		public void Deserialize(System.IO.TextReader rdr) {
			string name, value;
			var parameters = new System.Collections.Specialized.NameValueCollection();
			while (rdr.Property(out name, out value, parameters) && !string.IsNullOrEmpty(name)) {
				switch (name.ToUpper()) {
					case "BEGIN":
						switch (value) {
							case "VALARM":
								var a = new Alarm();
								a.Deserialize(rdr);
								Alarms.Add(a);
								break;
						}
						break;
					case "ATTENDEE":
						var contact = new Contact();
						contact.Deserialize(value, parameters);
						Attendees.Add(contact);
						break;
					case "CATEGORIES":
						Categories = value.SplitEscaped().ToList();
						break;
					case "CLASS": Class = value; break;
					case "CREATED": Created = value.ToDateTime(); break;
					case "DESCRIPTION": Description = value; break;
					case "DTEND": End = value.ToDateTime(); break;
					case "DTSTAMP": DTSTAMP = value.ToDateTime().GetValueOrDefault(); break;
					case "DTSTART": Start = value.ToDateTime(); break;
					case "LAST-MODIFIED": LastModified = value.ToDateTime(); break;
					case "LOCATION": Location = value; break;
					case "ORGANIZER":
						Organizer = new Contact();
						Organizer.Deserialize(value, parameters);
						break;
					case "PRIORITY": Priority = value.ToInt(); break;
					case "SEQUENCE": Sequence = value.ToInt(); break;
					case "STATUS": Status = value; break;
					case "TRANSPARENCY": Transparency = value; break;
					case "UID": UID = value; break;
					case "URL": Url = value.ToUri(); break;
					case "RRULE":
						var rule = new Recurrence();
						rule.Deserialize(null, parameters);
						Recurrences.Add(rule);
						break;
					case "END": return;
					default:
						Properties.Add(Tuple.Create(name, value, parameters));
						break;
				}
			}
		}

		public void Serialize(System.IO.TextWriter wrtr) {
			var d = new DDay.iCal.Event();
			wrtr.BeginBlock("VEVENT");
			if (Attendees != null)
				foreach (var attendee in Attendees)
					wrtr.Property("ATTENDEE", attendee);
			if (Categories != null && Categories.Count > 0)
				wrtr.Property("CATEGORIES", Categories);
			wrtr.Property("CLASS", Class);
			wrtr.Property("CREATED", Created);
			wrtr.Property("DESCRIPTION", Description);
			wrtr.Property("DTEND", IsAllDay ? (End ?? Start.Value).Date : End);
			wrtr.Property("DTSTAMP", DTSTAMP);
			wrtr.Property("DTSTART", IsAllDay ? (Start ?? End.Value).Date : Start);
			wrtr.Property("LAST-MODIFIED", LastModified);
			wrtr.Property("LOCATION", Location);
			wrtr.Property("ORGANIZER", Organizer);
			wrtr.Property("PRIORITY", Priority);
			wrtr.Property("SEQUENCE", Sequence);
			wrtr.Property("STATUS", Status);
			wrtr.Property("SUMMARY", Summary);
			wrtr.Property("TRANSPARENCY", Transparency);
			wrtr.Property("UID", UID);
			wrtr.Property("URL", Url);

			if (Alarms != null)
				foreach (var alarm in Alarms)
					alarm.Serialize(wrtr);
			wrtr.EndBlock("VEVENT");
		}
	}
}
